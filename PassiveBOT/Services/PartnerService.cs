namespace PassiveBOT.Services
{
    using System.Collections.Concurrent;

    using Raven.Client.Documents;

    /// <summary>
    ///     The partner service.
    /// </summary>
    public class PartnerService
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PartnerService" /> class.
        /// </summary>
        /// <param name="store">
        ///     The store.
        /// </param>
        public PartnerService(IDocumentStore store)
        {
            Store = store;
        }

        /// <summary>
        ///     Gets or sets the store.
        /// </summary>
        private static IDocumentStore Store { get; set; }

        /// <summary>
        /// Gets a partner info config via guild ID
        /// </summary>
        /// <param name="guildId">
        /// The guild id.
        /// </param>
        /// <param name="forced">
        /// The forced.
        /// </param>
        /// <returns>
        /// The <see cref="PartnerInfo"/>.
        /// </returns>
        public PartnerInfo GetPartnerInfo(ulong guildId, bool forced = false)
        {
            if (!forced)
            {
                if (PartnerStatuses.ContainsKey(guildId))
                {
                    PartnerStatuses.TryGetValue(guildId, out var status);
                    if (status == false)
                    {
                        return null;
                    }
                }
            }

            using (var session = Store.OpenSession())
            {
                var res = session.Load<PartnerInfo>($"{guildId}-Partner") ?? new PartnerInfo(guildId);
                PartnerStatuses.TryRemove(guildId, out _);
                PartnerStatuses.TryAdd(guildId, res.Settings.Enabled && !res.Settings.Banned);

                return res;
            }
        }

        /// <summary>
        /// A dictionary of guildID's and statuses that tell whether a guild uses the partner service or not
        /// </summary>
        private static readonly ConcurrentDictionary<ulong, bool> PartnerStatuses = new ConcurrentDictionary<ulong, bool>();
        
        public void OverWrite(PartnerInfo newObj)
        {
            using (var session = Store.OpenSession())
            {
                session.Store(newObj, $"{newObj.GuildId}-Partner");
                PartnerStatuses.TryRemove(newObj.GuildId, out _);
                session.SaveChanges();
            }
        }

        /// <summary>
        ///     The partner info.
        /// </summary>
        public class PartnerInfo
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="PartnerInfo" /> class.
            /// </summary>
            /// <param name="guildId">
            ///     The guild id.
            /// </param>
            public PartnerInfo(ulong guildId)
            {
                GuildId = guildId;
            }

            /// <summary>
            ///     Gets or sets the guild id.
            /// </summary>
            public ulong GuildId { get; set; }

            /// <summary>
            ///     Gets or sets the message.
            /// </summary>
            public PartnerMessage Message { get; set; } = new PartnerMessage();

            /// <summary>
            ///     Gets or sets the settings.
            /// </summary>
            public PartnerSettings Settings { get; set; } = new PartnerSettings();

            /// <summary>
            ///     Gets or sets the stats.
            /// </summary>
            public PartnerStats Stats { get; set; } = new PartnerStats();

            /// <summary>
            ///     Saves the current partner info
            /// </summary>
            public void Save()
            {
                using (var session = Store.OpenSession())
                {
                    session.Store(this, $"{GuildId}-Partner");
                    PartnerStatuses.TryRemove(GuildId, out _);
                    PartnerStatuses.TryAdd(GuildId, Settings.Enabled && !Settings.Banned);
                    session.SaveChanges();
                }
            }

            /// <summary>
            ///     The partner message.
            /// </summary>
            public class PartnerMessage
            {
                /// <summary>
                ///     Gets or sets the color.
                /// </summary>
                public RGB Color { get; set; } = new RGB();

                /// <summary>
                ///     Gets or sets the content.
                /// </summary>
                public string Content { get; set; } = null;

                /// <summary>
                ///     Gets or sets the image url.
                /// </summary>
                public string ImageUrl { get; set; } = null;

                /// <summary>
                ///     Gets or sets a value indicating whether user count.
                /// </summary>
                public bool UserCount { get; set; } = false;

                /// <summary>
                ///     Gets or sets a value indicating whether use thumb.
                /// </summary>
                public bool UseThumb { get; set; } = false;

                /// <summary>
                /// Gets or sets the partner Invite
                /// </summary>
                public string Invite { get; set; }

                /// <summary>
                ///     The rgb.
                /// </summary>
                public class RGB
                {
                    /// <summary>
                    ///     Gets or sets the blue value
                    /// </summary>
                    public int B { get; set; }

                    /// <summary>
                    ///     Gets or sets the green value
                    /// </summary>
                    public int G { get; set; }

                    /// <summary>
                    ///     Gets or sets the red value
                    /// </summary>
                    public int R { get; set; }
                }
            }

            /// <summary>
            ///     The partner settings.
            /// </summary>
            public class PartnerSettings
            {
                /// <summary>
                ///     Gets or sets a value indicating whether they are banned.
                /// </summary>
                public bool Banned { get; set; } = false;

                /// <summary>
                ///     Gets or sets the channel id.
                /// </summary>
                public ulong ChannelId { get; set; }

                /// <summary>
                ///     Gets or sets a value indicating whether it is enabled.
                /// </summary>
                public bool Enabled { get; set; } = false;
            }

            /// <summary>
            ///     The partner stats.
            /// </summary>
            public class PartnerStats
            {
                /// <summary>
                ///     Gets or sets the servers reached.
                /// </summary>
                public int ServersReached { get; set; } = 0;

                /// <summary>
                ///     Gets or sets the users reached.
                /// </summary>
                public int UsersReached { get; set; } = 0;
            }
        }
    }
}