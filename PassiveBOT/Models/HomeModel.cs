namespace PassiveBOT.Models
{
    using System.Collections.Generic;

    using PassiveBOT.Handlers;

    /// <summary>
    ///     The home model.
    /// </summary>
    public class HomeModel
    {
        /// <summary>
        ///     Gets or sets the blacklist.
        /// </summary>
        public BlacklistConfig Blacklist { get; set; } = new BlacklistConfig();

        /// <summary>
        ///     Gets or sets the bot moderator.
        /// </summary>
        public ulong BotModerator { get; set; } = 0;

        /// <summary>
        ///     Gets or sets the guild ID.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        ///     Gets or sets the home invite.
        /// </summary>
        public string HomeInvite { get; set; } = "https://discord.me/passive";

        /// <summary>
        ///     Gets or sets the logging setup
        /// </summary>
        public LoggingSetup Logging { get; set; } = new LoggingSetup();

        /// <summary>
        ///     The saves the config
        /// </summary>
        /// <returns>
        ///     The <see cref="HomeModel" />.
        /// </returns>
        public static HomeModel Load()
        {
            using (var session = DatabaseHandler.Store.OpenSession())
            {
                var model = session.Load<HomeModel>("HomeServer");
                if (model == null)
                {
                    model = new HomeModel();
                    model.Save();
                }

                return model;
            }
        }

        /// <summary>
        ///     The saves the config
        /// </summary>
        public void Save()
        {
            using (var session = DatabaseHandler.Store.OpenSession())
            {
                session.Store(this, "HomeServer");
                session.SaveChanges();
            }
        }

        /// <summary>
        ///     The blacklist.
        /// </summary>
        public class BlacklistConfig
        {
            /// <summary>
            ///     Gets or sets the blacklisted guilds.
            /// </summary>
            public List<ulong> BlacklistedGuilds { get; set; } = new List<ulong>();

            /// <summary>
            ///     Gets or sets the blacklisted users.
            /// </summary>
            public List<ulong> BlacklistedUsers { get; set; } = new List<ulong>();
        }

        /// <summary>
        ///     The logging.
        /// </summary>
        public class LoggingSetup
        {
            /// <summary>
            ///     Gets or sets a value indicating whether log partner changes.
            /// </summary>
            public bool LogPartnerChanges { get; set; } = false;

            /// <summary>
            ///     Gets or sets the partner log channel.
            /// </summary>
            public ulong PartnerLogChannel { get; set; }
        }
    }
}