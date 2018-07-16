namespace PassiveBOT.Services
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using Raven.Client.Documents;

    public class ChannelService
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ChannelService" /> class.
        /// </summary>
        /// <param name="store">
        ///     The store.
        /// </param>
        public ChannelService(IDocumentStore store)
        {
            Store = store;
        }

        /// <summary>
        ///     Gets or sets the store.
        /// </summary>
        private static IDocumentStore Store { get; set; }

        public CustomChannels GetCustomChannels(ulong guildId)
        {
            using (var session = Store.OpenSession())
            {
                return session.Load<CustomChannels>($"{guildId}-Channels") ?? new CustomChannels(guildId);
            }
        }

        public class CustomChannels
        {
            public CustomChannels(ulong guildId)
            {
                GuildId = guildId;
            }

            /// <summary>
            ///     Gets or sets the auto message channels.
            /// </summary>
            public ConcurrentDictionary<ulong, AutoMessageChannel> AutoMessageChannels { get; set; } = new ConcurrentDictionary<ulong, AutoMessageChannel>();

            public ulong GuildId { get; }

            /// <summary>
            ///     Gets or sets the media channels.
            /// </summary>
            public ConcurrentDictionary<ulong, MediaChannel> MediaChannels { get; set; } = new ConcurrentDictionary<ulong, MediaChannel>();

            public void Save()
            {
                using (var session = Store.OpenSession())
                {
                    session.Store(this, $"{GuildId}-Channels");
                    session.SaveChanges();
                }
            }

            /// <summary>
            ///     The auto message channel.
            /// </summary>
            public class AutoMessageChannel
            {
                /// <summary>
                ///     Gets or sets the channel id.
                /// </summary>
                public ulong ChannelID { get; set; }

                /// <summary>
                ///     Gets or sets the count.
                /// </summary>
                public int Count { get; set; } = 0;

                /// <summary>
                ///     Gets or sets a value indicating whether enabled.
                /// </summary>
                public bool Enabled { get; set; } = false;

                /// <summary>
                ///     Gets or sets the limit.
                /// </summary>
                public int Limit { get; set; } = 100;

                /// <summary>
                ///     Gets or sets the message.
                /// </summary>
                public string Message { get; set; }
            }

            /// <summary>
            ///     The media channel.
            /// </summary>
            public class MediaChannel
            {
                /// <summary>
                ///     Gets or sets the channel id.
                /// </summary>
                public ulong ChannelID { get; set; }

                /// <summary>
                ///     Gets or sets a value indicating whether enabled.
                /// </summary>
                public bool Enabled { get; set; } = false;

                /// <summary>
                ///     Gets or sets the exempt roles.
                /// </summary>
                public List<ulong> ExemptRoles { get; set; } = new List<ulong>();
            }
        }
    }
}