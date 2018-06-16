namespace PassiveBOT.Models
{
    using System;
    using System.Collections.Generic;

    using PassiveBOT.Handlers;

    /// <summary>
    /// The guild model.
    /// </summary>
    public class GuildModel
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public ulong ID { get; set; }

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        public GuildSetup Settings { get; set; } = new GuildSetup();

        /// <summary>
        /// Gets or sets the partner setup
        /// </summary>
        public PartnerSetup Partner { get; set; } = new PartnerSetup();

        /// <summary>
        /// Gets or sets the levels setup
        /// </summary>
        public LevelSetup Levels { get; set; } = new LevelSetup();

        /// <summary>
        /// Gets or sets the Custom Command Access
        /// </summary>
        public CommandAccess Disabled { get; set; } = new CommandAccess();

        /// <summary>
        /// Gets or sets the events setup
        /// </summary>
        public EventSetup Events { get; set; } = new EventSetup();

        /// <summary>
        /// Gets or sets the custom channel setup
        /// </summary>
        public CustomChannels CustomChannel { get; set; } = new CustomChannels();

        /// <summary>
        /// Gets or sets the tags setup
        /// </summary>
        public TagSetup Tags { get; set; } = new TagSetup();

        /// <summary>
        /// Gets or sets the moderation setup
        /// </summary>
        public ModerationSetup Moderation { get; set; } = new ModerationSetup();

        /// <summary>
        /// The saves the config
        /// </summary>
        public void Save()
        {
            using (var session = DatabaseHandler.Store.OpenSession())
            {
                session.Store(this, ID.ToString());
                session.SaveChanges();
            }
        }

        /// <summary>
        /// The custom channels setup
        /// </summary>
        public class CustomChannels
        {
            /// <summary>
            /// Gets or sets the media channels.
            /// </summary>
            public List<MediaChannel> MediaChannels { get; set; } = new List<MediaChannel>();

            /// <summary>
            /// Gets or sets the auto message channels.
            /// </summary>
            public List<AutoMessageChannel> AutoMessageChannels { get; set; } = new List<AutoMessageChannel>();

            /// <summary>
            /// The media channel.
            /// </summary>
            public class MediaChannel
            {
                /// <summary>
                /// Gets or sets the channel id.
                /// </summary>
                public ulong ChannelID { get; set; }

                /// <summary>
                /// Gets or sets a value indicating whether enabled.
                /// </summary>
                public bool Enabled { get; set; } = false;

                /// <summary>
                /// Gets or sets the exempt roles.
                /// </summary>
                public List<ulong> ExemptRoles { get; set; } = new List<ulong>();
            }

            /// <summary>
            /// The auto message channel.
            /// </summary>
            public class AutoMessageChannel
            {
                /// <summary>
                /// Gets or sets the channel id.
                /// </summary>
                public ulong ChannelID { get; set; }

                /// <summary>
                /// Gets or sets a value indicating whether enabled.
                /// </summary>
                public bool Enabled { get; set; } = false;

                /// <summary>
                /// Gets or sets the count.
                /// </summary>
                public int Count { get; set; } = 0;

                /// <summary>
                /// Gets or sets the limit.
                /// </summary>
                public int Limit { get; set; } = 100;

                /// <summary>
                /// Gets or sets the message.
                /// </summary>
                public string Message { get; set; }
            }
        }

        /// <summary>
        /// The moderation setup.
        /// </summary>
        public class ModerationSetup
        {
            /// <summary>
            /// Gets or sets the admin role ids.
            /// </summary>
            public List<ulong> AdminRoleIDs { get; set; } = new List<ulong>();

            /// <summary>
            /// Gets or sets the mod role ids.
            /// </summary>
            public List<ulong> ModRoleIDs { get; set; } = new List<ulong>();

            /// <summary>
            /// Gets or sets the sub role ids.
            /// </summary>
            public List<ulong> SubRoleIDs { get; set; } = new List<ulong>();
        }

        /// <summary>
        /// The command access setup
        /// </summary>
        public class CommandAccess
        {
            /// <summary>
            /// Gets or sets the customized permission.
            /// </summary>
            public List<CustomPermission> CustomizedPermission { get; set; } = new List<CustomPermission>();

            /// <summary>
            /// The custom permission.
            /// </summary>
            public class CustomPermission
            {
                /// <summary>
                /// The access type.
                /// </summary>
                public enum AccessType
                {
                    /// <summary>
                    /// Server Owner
                    /// </summary>
                    ServerOwner,

                    /// <summary>
                    /// Administrator Users
                    /// </summary>
                    Admin,

                    /// <summary>
                    /// Moderator Users
                    /// </summary>
                    Moderator
                }

                /// <summary>
                /// Gets or sets a value indicating whether it is a command.
                /// </summary>
                public bool IsCommand { get; set; } = true;

                /// <summary>
                /// Gets or sets the name.
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                /// Gets or sets the access type.
                /// </summary>
                public AccessType Setting { get; set; } = AccessType.Admin;
            }
        }

        /// <summary>
        /// The event setup.
        /// </summary>
        public class EventSetup
        {
            /// <summary>
            /// Gets or sets the welcome event
            /// </summary>
            public Event Welcome { get; set; } = new Event
            {
                Message = "Has Joined the Server!"
            };

            /// <summary>
            /// Gets or sets the goodbye event
            /// </summary>
            public Event Goodbye { get; set; } = new Event
            {
                Message = "Has Left the Server!"
            };

            /// <summary>
            /// The event overview
            /// </summary>
            public class Event
            {
                /// <summary>
                /// Gets or sets the channel id.
                /// </summary>
                public ulong ChannelID { get; set; }

                /// <summary>
                /// Gets or sets the message.
                /// </summary>
                public string Message { get; set; }

                /// <summary>
                /// Gets or sets a value indicating whether it is enabled.
                /// </summary>
                public bool Enabled { get; set; } = false;

                /// <summary>
                /// Gets or sets a value indicating whether to send dms.
                /// </summary>
                public bool SendDMs { get; set; } = false;

                /// <summary>
                /// Gets or sets a value indicating whether to show user count.
                /// </summary>
                public bool UserCount { get; set; } = true;
            }
        }

        /// <summary>
        /// The level setup.
        /// </summary>
        public class LevelSetup
        {
            /// <summary>
            /// Gets or sets the settings.
            /// </summary>
            public LevelSettings Settings { get; set; } = new LevelSettings();

            /// <summary>
            /// Gets or sets the reward roles.
            /// </summary>
            public List<LevelReward> RewardRoles { get; set; } = new List<LevelReward>();

            /// <summary>
            /// Gets or sets the users.
            /// </summary>
            public List<LevelUser> Users { get; set; } = new List<LevelUser>();

            /// <summary>
            /// The level settings.
            /// </summary>
            public class LevelSettings
            {
                /// <summary>
                /// Gets or sets a value indicating whether leveling is enabled.
                /// </summary>
                public bool Enabled { get; set; } = false;

                /// <summary>
                /// Gets or sets a value indicating whether use log channel.
                /// </summary>
                public bool UseLogChannel { get; set; } = false;

                /// <summary>
                /// Gets or sets the log channel id.
                /// </summary>
                public ulong LogChannelID { get; set; }

                /// <summary>
                /// Gets or sets a value indicating whether to dm level ups.
                /// </summary>
                public bool DMLevelUps { get; set; } = false;

                /// <summary>
                /// Gets or sets a value indicating whether reply level up messages.
                /// </summary>
                public bool ReplyLevelUps { get; set; } = true;

                /// <summary>
                /// Gets or sets a value indicating whether to increment level rewards.
                /// </summary>
                public bool IncrementLevelRewards { get; set; } = false;
            }

            /// <summary>
            /// The level reward.
            /// </summary>
            public class LevelReward
            {
                /// <summary>
                /// Gets or sets the requirement level to receive the role
                /// </summary>
                public int Requirement { get; set; }

                /// <summary>
                /// Gets or sets the role id.
                /// </summary>
                public ulong RoleID { get; set; }
            }

            /// <summary>
            /// The level user.
            /// </summary>
            public class LevelUser
            {
                /// <summary>
                /// Gets or sets the user id.
                /// </summary>
                public ulong UserID { get; set; }

                /// <summary>
                /// Gets or sets the level.
                /// </summary>
                public int Level { get; set; } = 1;

                /// <summary>
                /// Gets or sets the xp.
                /// </summary>
                public int XP { get; set; } = 0;

                /// <summary>
                /// Gets or sets the last update.
                /// </summary>
                public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// The partner setup.
        /// </summary>
        public class PartnerSetup
        {
            /// <summary>
            /// Gets or sets the settings.
            /// </summary>
            public PartnerSettings Settings { get; set; } = new PartnerSettings();

            /// <summary>
            /// Gets or sets the stats.
            /// </summary>
            public PartnerStats Stats { get; set; } = new PartnerStats();

            /// <summary>
            /// Gets or sets the message.
            /// </summary>
            public PartnerMessage Message { get; set; } = new PartnerMessage();

            /// <summary>
            /// The partner stats.
            /// </summary>
            public class PartnerStats
            {
                /// <summary>
                /// Gets or sets the users reached.
                /// </summary>
                public int UsersReached { get; set; } = 0;

                /// <summary>
                /// Gets or sets the servers reached.
                /// </summary>
                public int ServersReached { get; set; } = 0;
            }

            /// <summary>
            /// The partner settings.
            /// </summary>
            public class PartnerSettings
            {
                /// <summary>
                /// Gets or sets a value indicating whether they are banned.
                /// </summary>
                public bool Banned { get; set; } = false;

                /// <summary>
                /// Gets or sets a value indicating whether it is enabled.
                /// </summary>
                public bool Enabled { get; set; } = false;

                /// <summary>
                /// Gets or sets the channel id.
                /// </summary>
                public ulong ChannelID { get; set; }
            }

            /// <summary>
            /// The partner message.
            /// </summary>
            public class PartnerMessage
            {
                /// <summary>
                /// Gets or sets the content.
                /// </summary>
                public string Content { get; set; } = null;

                /// <summary>
                /// Gets or sets the image url.
                /// </summary>
                public string ImageUrl { get; set; } = null;

                /// <summary>
                /// Gets or sets a value indicating whether user count.
                /// </summary>
                public bool UserCount { get; set; } = false;

                /// <summary>
                /// Gets or sets a value indicating whether use thumb.
                /// </summary>
                public bool UseThumb { get; set; } = false;

                /// <summary>
                /// Gets or sets the color.
                /// </summary>
                public RGB Color { get; set; } = new RGB();

                /// <summary>
                /// The rgb.
                /// </summary>
                public class RGB
                {
                    /// <summary>
                    /// Gets or sets the red value
                    /// </summary>
                    public int R { get; set; }

                    /// <summary>
                    /// Gets or sets the green value
                    /// </summary>
                    public int G { get; set; }

                    /// <summary>
                    /// Gets or sets the blue value
                    /// </summary>
                    public int B { get; set; }
                }
            }
        }

        /// <summary>
        /// The tag setup.
        /// </summary>
        public class TagSetup
        {
            /// <summary>
            /// Gets or sets the settings.
            /// </summary>
            public TagSettings Settings { get; set; } = new TagSettings();

            /// <summary>
            /// Gets or sets the tags.
            /// </summary>
            public List<Tag> Tags { get; set; } = new List<Tag>();

            /// <summary>
            /// The tag settings.
            /// </summary>
            public class TagSettings
            {
                /// <summary>
                /// Gets or sets a value indicating whether tags are enabled.
                /// </summary>
                public bool Enabled { get; set; } = true;

                /// <summary>
                /// Gets or sets a value indicating whether only server admins can create tags.
                /// </summary>
                public bool AdminOnly { get; set; } = false;
            }

            /// <summary>
            /// The tag.
            /// </summary>
            public class Tag
            {
                /// <summary>
                /// Gets or sets the name.
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                /// Gets or sets the content.
                /// </summary>
                public string Content { get; set; }

                /// <summary>
                /// Gets or sets the use count
                /// </summary>
                public int Uses { get; set; } = 0;

                /// <summary>
                /// Gets or sets the creator id.
                /// </summary>
                public ulong CreatorID { get; set; }

                /// <summary>
                /// Gets or sets the creator name.
                /// </summary>
                public string OwnerName { get; set; }
            }
        }

        /// <summary>
        /// The guild setup.
        /// </summary>
        public class GuildSetup
        {
            /// <summary>
            /// Gets or sets the prefix.
            /// </summary>
            public PrefixSetup Prefix { get; set; } = new PrefixSetup();

            /// <summary>
            /// Gets or sets the translate.
            /// </summary>
            public TranslateSetup Translate { get; set; } = new TranslateSetup();

            /// <summary>
            /// Gets or sets the nsfw settings
            /// </summary>
            public NsfwSetup Nsfw { get; set; } = new NsfwSetup();

            /// <summary>
            /// The prefix setup.
            /// </summary>
            public class PrefixSetup
            {
                /// <summary>
                /// Gets or sets a value indicating whether to deny the @ mention prefix
                /// </summary>
                public bool DenyMentionPrefix { get; set; } = false;

                /// <summary>
                /// Gets or sets a value indicating whether to deny the default bot prefix.
                /// </summary>
                public bool DenyDefaultPrefix { get; set; } = false;

                /// <summary>
                /// Gets or sets the custom guild prefix.
                /// </summary>
                public string CustomPrefix { get; set; } = null;
            }

            /// <summary>
            /// The nsfw setup.
            /// </summary>
            public class NsfwSetup
            {
                /// <summary>
                /// Gets or sets a value indicating whether enabled.
                /// </summary>
                public bool Enabled { get; set; } = true;
            }

            /// <summary>
            /// The translate setup.
            /// </summary>
            public class TranslateSetup
            {
                /// <summary>
                /// Gets or sets a value indicating whether easy translate with reactions
                /// </summary>
                public bool EasyTranslate { get; set; } = false;

                /// <summary>
                /// Gets or sets a value indicating whether to dm translations.
                /// </summary>
                public bool DMTranslations { get; set; } = false;

                /// <summary>
                /// Gets or sets the custom pairs.
                /// </summary>
                public List<TranslationSet> CustomPairs { get; set; } = new List<TranslationSet>();

                /// <summary>
                /// The translation set.
                /// </summary>
                public class TranslationSet
                {
                    /// <summary>
                    /// Gets or sets the emote matches.
                    /// </summary>
                    public List<string> EmoteMatches { get; set; } = new List<string>();

                    /// <summary>
                    /// Gets or sets the language.
                    /// </summary>
                    public LanguageMap.LanguageCode Language { get; set; }
                }
            }
        }
    }
}