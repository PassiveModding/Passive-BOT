namespace PassiveBOT.Models
{
    using System.Collections.Generic;

    using PassiveBOT.Handlers;

    /// <summary>
    ///     A model that holds bot usage statistics and other info
    /// </summary>
    public class StatModel
    {
        /// <summary>
        ///     Gets or sets the command stats.
        /// </summary>
        public List<CommandStat> CommandStats { get; set; } = new List<CommandStat>();

        /*
                /// <summary>
                /// Gets or sets the message stats.
                /// </summary>
                public List<MessageStat> MessageStats { get; set; } = new List<MessageStat>();
                */

        /// <summary>
        ///     Gets or sets the message count.
        /// </summary>
        public int MessageCount { get; set; } = 0;

        /// <summary>
        ///     The saves the config
        /// </summary>
        /// <returns>
        ///     The <see cref="HomeModel" />.
        /// </returns>
        public static StatModel Load()
        {
            using (var session = DatabaseHandler.Store.OpenSession())
            {
                var model = session.Load<StatModel>("Statistics");
                if (model == null)
                {
                    model = new StatModel();
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
                session.Store(this, "Statistics");
                session.SaveChanges();
            }
        }

        /*
        /// <summary>
        /// Message statistics
        /// </summary>
        public class MessageStat
        {
            
            /// <summary>
            /// Gets or sets the message length.
            /// </summary>
            public int MessageLength { get; set; }

            /// <summary>
            /// Gets or sets the message owner.
            /// </summary>
            public ulong MessageOwner { get; set; }

            /// <summary>
            /// Gets or sets the message guild.
            /// </summary>
            public ulong MessageGuild { get; set; }
            
        }
        */

        /// <summary>
        ///     Command Usage Statistics
        /// </summary>
        public class CommandStat
        {
            /// <summary>
            ///     Gets or sets the command guilds.
            /// </summary>
            public List<CommandGuild> CommandGuilds { get; set; } = new List<CommandGuild>();

            /// <summary>
            ///     Gets or sets the command name.
            /// </summary>
            public string CommandName { get; set; }

            /// <summary>
            ///     Gets or sets the command users.
            /// </summary>
            public List<CommandUser> CommandUsers { get; set; } = new List<CommandUser>();

            /// <summary>
            ///     Gets or sets the command uses.
            /// </summary>
            public int CommandUses { get; set; }

            /// <summary>
            ///     Gets or sets the error count
            /// </summary>
            public int ErrorCount { get; set; }

            /// <summary>
            ///     The command guild.
            /// </summary>
            public class CommandGuild
            {
                /// <summary>
                ///     Gets or sets the guild id.
                /// </summary>
                public ulong GuildId { get; set; }

                /// <summary>
                ///     Gets or sets the uses.
                /// </summary>
                public int Uses { get; set; } = 0;
            }

            /// <summary>
            ///     The command user.
            /// </summary>
            public class CommandUser
            {
                /// <summary>
                ///     Gets or sets the user id.
                /// </summary>
                public ulong UserId { get; set; }

                /// <summary>
                ///     Gets or sets the uses.
                /// </summary>
                public int Uses { get; set; } = 0;
            }
        }
    }
}