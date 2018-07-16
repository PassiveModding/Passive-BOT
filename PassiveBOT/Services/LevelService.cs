namespace PassiveBOT.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using Raven.Client.Documents;

    public class LevelService
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LevelService" /> class.
        /// </summary>
        /// <param name="store">
        ///     The store.
        /// </param>
        public LevelService(IDocumentStore store)
        {
            Store = store;
        }

        /// <summary>
        ///     Gets or sets the store.
        /// </summary>
        private static IDocumentStore Store { get; set; }

        public LevelSetup GetLevelSetup(ulong guildId)
        {
            using (var session = Store.OpenSession())
            {
                return session.Load<LevelSetup>($"{guildId}-Levels") ?? new LevelSetup(guildId);
            }
        }
        
        public void OverWrite(LevelSetup newObj)
        {
            using (var session = Store.OpenSession())
            {
                session.Store(newObj, $"{newObj.GuildId}-Levels");
                session.SaveChanges();
            }
        }

        /// <summary>
        ///     The level setup.
        /// </summary>
        public class LevelSetup
        {
            public LevelSetup(ulong guildId)
            {
                GuildId = guildId;
            }

            public ulong GuildId { get; set; }

            /// <summary>
            ///     Gets or sets the reward roles.
            /// </summary>
            public List<LevelReward> RewardRoles { get; set; } = new List<LevelReward>();

            /// <summary>
            ///     Gets or sets the settings.
            /// </summary>
            public LevelSettings Settings { get; set; } = new LevelSettings();

            /// <summary>
            ///     Gets or sets the users.
            /// </summary>
            public ConcurrentDictionary<ulong, LevelUser> Users { get; set; } = new ConcurrentDictionary<ulong, LevelUser>();

            public void Save()
            {
                using (var session = Store.OpenSession())
                {
                    session.Store(this, $"{GuildId}-Levels");
                    session.SaveChanges();
                }
            }

            /// <summary>
            ///     The level reward.
            /// </summary>
            public class LevelReward
            {
                /// <summary>
                ///     Gets or sets the requirement level to receive the role
                /// </summary>
                public int Requirement { get; set; }

                /// <summary>
                ///     Gets or sets the role id.
                /// </summary>
                public ulong RoleID { get; set; }
            }

            /// <summary>
            ///     The level settings.
            /// </summary>
            public class LevelSettings
            {
                /// <summary>
                ///     Gets or sets a value indicating whether to dm level ups.
                /// </summary>
                public bool DMLevelUps { get; set; } = false;

                /// <summary>
                ///     Gets or sets a value indicating whether leveling is enabled.
                /// </summary>
                public bool Enabled { get; set; } = false;

                /// <summary>
                ///     Gets or sets a value indicating whether to increment level rewards.
                /// </summary>
                public bool IncrementLevelRewards { get; set; } = false;

                /// <summary>
                ///     Gets or sets the log channel id.
                /// </summary>
                public ulong LogChannelID { get; set; }

                /// <summary>
                ///     Gets or sets a value indicating whether reply level up messages.
                /// </summary>
                public bool ReplyLevelUps { get; set; } = true;

                /// <summary>
                ///     Gets or sets a value indicating whether use log channel.
                /// </summary>
                public bool UseLogChannel { get; set; } = false;
            }

            /// <summary>
            ///     The level user.
            /// </summary>
            public class LevelUser
            {
                public LevelUser(ulong userId)
                {
                    UserID = userId;
                }

                /// <summary>
                ///     Gets or sets the last update.
                /// </summary>
                public DateTime LastUpdate { get; set; } = DateTime.UtcNow;

                /// <summary>
                ///     Gets or sets the level.
                /// </summary>
                public int Level { get; set; } = 1;

                /// <summary>
                ///     Gets the user id.
                /// </summary>
                public ulong UserID { get; }

                /// <summary>
                ///     Gets or sets the xp.
                /// </summary>
                public int XP { get; set; } = 0;
            }
        }
    }
}