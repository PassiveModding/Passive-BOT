namespace PassiveBOT.Services
{
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
        ///     Gets a partner info config via guild ID
        /// </summary>
        /// <param name="guildId">
        ///     The guild id.
        /// </param>
        /// <returns>
        ///     The <see cref="PartnerInfo" />.
        /// </returns>
        public PartnerInfo GetPartnerInfo(ulong guildId)
        {
            using (var session = Store.OpenSession())
            {
                return session.Load<PartnerInfo>($"{guildId}-Partner") ?? new PartnerInfo(guildId);
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
            ///     Gets the guild id.
            /// </summary>
            public ulong GuildId { get; }

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