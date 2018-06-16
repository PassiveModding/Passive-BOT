using System;
using System.Collections.Generic;
using System.Text;

namespace PassiveBOT.Models.DepreciatedGuildModels
{
    using PassiveBOT.Handlers;

    // Guild Model at this commit
    // https://github.com/PassiveModding/Passive-BOT/commit/7271092f1095b95c4f9b6c92b213a23f81b9e325


    public class GuildModel
    {
        /// <summary>
        ///     The Server ID
        /// </summary>
        public ulong ID { get; set; }

        /// <summary>
        ///     General Setup of the Server
        /// </summary>
        public gsettings Settings { get; set; } = new gsettings();

        /// <summary>
        ///     Partner Program Setup
        /// </summary>
        public partner Partner { get; set; } = new partner();

        /// <summary>
        ///     Levels Setup, userdata and rewards
        /// </summary>
        public levelling Levels { get; set; } = new levelling();

        /// <summary>
        ///     Disabled Commands and Modules
        /// </summary>
        public hidden Disabled { get; set; } = new hidden();

        /// <summary>
        ///     Channel Auto-Messaging
        /// </summary>
        public autoMessage AutoMessage { get; set; } = new autoMessage();

        /// <summary>
        ///     Join and Leave Events
        /// </summary>
        public events Events { get; set; } = new events();

        public CustomChannels CustomChannel { get; set; } = new CustomChannels();
        public class CustomChannels
        {
            public List<MediaChannel> MediaChannels { get; set; } = new List<MediaChannel>();
            public class MediaChannel
            {
                public ulong ChannelID { get; set; }
                public bool Enabled { get; set; } = false;
                public List<ulong> ExcemptRoles { get; set; } = new List<ulong>();
            }
        }

        public tags Tags { get; set; } = new tags();

        /// <summary>
        ///     Moderation for Bot Setup in guilds
        /// </summary>
        public moderation Moderation { get; set; } = new moderation();

        public void Save()
        {
            using (var session = DatabaseHandler.Store.OpenSession())
            {
                session.Store(this, ID.ToString());
                session.SaveChanges();
            }
        }

        public class moderation
        {
            /// <summary>
            ///     A list of Role IDs that are checked against for admins
            /// </summary>
            public List<ulong> AdminRoleIDs { get; set; } = new List<ulong>();

            /// <summary>
            ///     A list of Role IDs that are checked against for Moderators
            /// </summary>
            public List<ulong> ModRoleIDs { get; set; } = new List<ulong>();

            /// <summary>
            ///     A list of Role IDs that are publically available for users to subscribe to
            /// </summary>
            public List<ulong> SubRoleIDs { get; set; } = new List<ulong>();
        }


        public class autoMessage
        {
            /// <summary>
            ///     A List of Channels that use Auto Messaging
            /// </summary>
            public List<amChannel> AutoMessageChannels { get; set; } = new List<amChannel>();

            public class amChannel
            {
                /// <summary>
                ///     The Channel ID that we will send an automessage to
                /// </summary>
                public ulong ChannelID { get; set; }

                /// <summary>
                ///     True = send messages
                ///     False = Do not Send Messages
                /// </summary>
                public bool Enabled { get; set; } = false;

                /// <summary>
                ///     The Amount of messages sent since last AutoMessage
                /// </summary>
                public int Count { get; set; } = 0;

                /// <summary>
                ///     The Amount of messages required before next AutoMessage
                /// </summary>
                public int Limit { get; set; } = 100;

                /// <summary>
                ///     The Message to be sent
                /// </summary>
                public string Message { get; set; }
            }
        }

        public class hidden
        {
            /// <summary>
            ///     List of Modules that have been configured
            /// </summary>
            public List<hiddentype> Modules { get; set; } = new List<hiddentype>();

            /// <summary>
            ///     List of Commands that have been configured
            /// </summary>
            public List<hiddentype> Commands { get; set; } = new List<hiddentype>();

            public class hiddentype
            {
                /// <summary>
                ///     Name of configured item
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                ///     WhiteListed Role IDs
                /// </summary>
                public List<ulong> WhiteList { get; set; } = new List<ulong>();

                /// <summary>
                ///     Blacklisted Role IDs
                /// </summary>
                public List<ulong> Blacklist { get; set; } = new List<ulong>();

                /// <summary>
                ///     True = No Access unless server owner or whitelisted
                ///     False = Full Access unless blacklisted
                /// </summary>
                public bool Disabled { get; set; } = false;
            }
        }

        public class events
        {
            /// <summary>
            ///     Welcome Event
            /// </summary>
            public _event Welcome { get; set; } = new _event
            {
                Message = "Has Joined the Server!"
            };

            /// <summary>
            ///     GoodBye Event
            /// </summary>
            public _event Goodbye { get; set; } = new _event
            {
                Message = "Has Left the Server!"
            };

            public class _event
            {
                /// <summary>
                ///     The channel the Event message will be sent to
                /// </summary>
                public ulong ChannelID { get; set; }

                /// <summary>
                ///     The Event Message
                /// </summary>
                public string Message { get; set; }

                /// <summary>
                ///     True = Send Event Messages
                ///     False = Do not send event Messages
                /// </summary>
                public bool Enabled { get; set; } = false;

                public bool SendDMs { get; set; } = false;

                public bool UserCount { get; set; } = true;
            }
        }

        public class levelling
        {
            /// <summary>
            ///     Levelling Settings and setup
            /// </summary>
            public lsettings Settings { get; set; } = new lsettings();

            /// <summary>
            ///     Level Up Rewards
            /// </summary>
            public List<levelreward> RewardRoles { get; set; } = new List<levelreward>();

            /// <summary>
            ///     List of all users that have been initialised in levelling
            /// </summary>
            public List<luser> Users { get; set; } = new List<luser>();

            public class lsettings
            {
                /// <summary>
                ///     True = Use Levelling
                ///     False = Levelling is disabled
                /// </summary>
                public bool Enabled { get; set; } = false;

                /// <summary>
                ///     True = Log all Level up messages in a specific channel
                /// </summary>
                public bool UseLogChannel { get; set; } = false;

                /// <summary>
                ///     Channel ID used for level logging
                /// </summary>
                public ulong LogChannelID { get; set; }

                /// <summary>
                ///     True = Send users a private message when they level up
                /// </summary>
                public bool DMLevelUps { get; set; } = false;

                /// <summary>
                ///     Reply in the current channel with level up message
                /// </summary>
                public bool ReplyLevelUps { get; set; } = true;

                /// <summary>
                ///     Reply in the current channel with level up message
                /// </summary>
                public bool IncrementLevelRewards { get; set; } = false;
            }

            public class levelreward
            {
                /// <summary>
                ///     Level Requirement to receive reward
                /// </summary>
                public int Requirement { get; set; }

                /// <summary>
                ///     ID of role to receive for level up
                /// </summary>
                public ulong RoleID { get; set; }
            }

            public class luser
            {
                /// <summary>
                ///     User ID
                /// </summary>
                public ulong UserID { get; set; }

                /// <summary>
                ///     User Level
                /// </summary>
                public int Level { get; set; } = 1;

                /// <summary>
                ///     User total XP
                /// </summary>
                public int XP { get; set; } = 0;

                public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
            }
        }

        public class partner
        {
            /// <summary>
            ///     Partner Settings
            /// </summary>
            public psettings Settings { get; set; } = new psettings();

            /// <summary>
            ///     Partner Stats
            /// </summary>
            public pstats Stats { get; set; } = new pstats();

            /// <summary>
            ///     Partner Message
            /// </summary>
            public message Message { get; set; } = new message();

            public class pstats
            {
                /// <summary>
                ///     Total amount of users reached via partner program
                /// </summary>
                public int UsersReached { get; set; } = 0;

                /// <summary>
                ///     total amount of servers reached via partner program
                /// </summary>
                public int ServersReached { get; set; } = 0;
            }

            public class psettings
            {
                /// <summary>
                ///     Bot admin Toggled, True = Partner Messages will no longer be sent at all.
                /// </summary>
                public bool Banned { get; set; } = false;

                /// <summary>
                ///     True = Using Partner Program
                /// </summary>
                public bool Enabled { get; set; } = false;

                /// <summary>
                ///     ID Of partner message channel, where messages will be sent to
                /// </summary>
                public ulong ChannelID { get; set; }
            }

            public class message
            {
                /// <summary>
                ///     Main Text body of partner message
                /// </summary>
                public string Content { get; set; } = null;

                /// <summary>
                ///     Optional Image for Partner message
                /// </summary>
                public string ImageUrl { get; set; } = null;

                /// <summary>
                ///     Toggle whether or not to show server user count in partner message
                /// </summary>
                public bool UserCount { get; set; } = false;

                /// <summary>
                ///     Toggle whether or not to show server icon in partner message
                /// </summary>
                public bool UseThumb { get; set; } = false;

                /// <summary>
                ///     The Colour used for the embed message
                /// </summary>
                public rgb Color { get; set; } = new rgb();

                public class rgb
                {
                    public int R { get; set; }
                    public int G { get; set; }
                    public int B { get; set; }
                }
            }
        }

        public class tags
        {
            public TSettings Settings { get; set; } = new TSettings();
            public List<tag> Tags { get; set; } = new List<tag>();

            public class TSettings
            {
                public bool Enabled { get; set; } = true;
                public bool AdminOnly { get; set; } = false;
            }

            public class tag
            {
                public string Name { get; set; }
                public string Content { get; set; }
                public int Uses { get; set; } = 0;

                public ulong CreatorID { get; set; }
                public string OwnerName { get; set; }
            }
        }

        public class gsettings
        {
            /// <summary>
            ///     Bot Custom Prefix config
            /// </summary>
            public prefix Prefix { get; set; } = new prefix();

            public translate Translate { get; set; } = new translate();
            public colorroles ColorRoles { get; set; } = new colorroles();

            public class prefix
            {
                /// <summary>
                ///     Deny the @BOTNAME prefix for commands
                /// </summary>
                public bool DenyMentionPrefix { get; set; } = false;

                /// <summary>
                ///     Deny the default bot prefix in commands
                /// </summary>
                public bool DenyDefaultPrefix { get; set; } = false;

                /// <summary>
                ///     Server's Own cutom prefix for bot commands
                /// </summary>
                public string CustomPrefix { get; set; } = null;
            }

            public class translate
            {
                public bool EasyTranslate { get; set; } = false;
                public bool DMTranslations { get; set; } = false;
                public List<TObject> Custompairs { get; set; } = new List<TObject>();

                public class TObject
                {
                    public List<string> EmoteMatches { get; set; } = new List<string>();
                    public LanguageMap.LanguageCode Language { get; set; }
                }
            }

            public class colorroles
            {
                public bool Enabled { get; set; } = false;
            }
        }
    }
}