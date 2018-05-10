using System;
using System.Collections.Generic;
using System.IO;
using Discord;
using Newtonsoft.Json;
using PassiveBOT.Commands.ServerSetup;

namespace PassiveBOT.Configuration
{
    public class GuildConfig
    {
        [JsonIgnore] public static readonly string Appdir = AppContext.BaseDirectory;


        public ulong GuildId { get; set; } //
        public string GuildName { get; set; } //
        public string Prefix { get; set; } = Load.Pre; //

        public bool chatwithmention { get; set; } = true;

        public PartnerShip PartnerSetup { get; set; } = new PartnerShip();

        public roleConfigurations RoleConfigurations { get; set; } = new roleConfigurations();

        public List<Tags.Tagging> Dict { get; set; } = new List<Tags.Tagging>(); // tags module

        public antispams Antispams { get; set; } = new antispams();

        public visibilityconfig Visibilityconfig { get; set; } = new visibilityconfig();

        public bool ErrorLog { get; set; } // allows for responses with errors 

        public bool GoodbyeEvent { get; set; } = false;
        public string GoodbyeMessage { get; set; } = "Has Left the Server :(";
        public ulong GoodByeChannel { get; set; } = 0;
        public bool WelcomeEvent { get; set; } // toggles welcome messages for new users
        public string WelcomeMessage { get; set; } = "Welcome to Our Server!!!"; // the welcome message
        public ulong WelcomeChannel { get; set; } // welcome messages in a channel
        public bool EventLogging { get; set; } = false;
        public ulong EventChannel { get; set; } = 0;

        public GiveAway Comp { get; set; } = new GiveAway();


        public ulong Starboard { get; set; } = 0;

        public bool LogModCommands { get; set; } = false;
        public ulong ModLogChannel { get; set; } = 0;
        public List<Warns> Warnings { get; set; } = new List<Warns>();
        public List<Kicks> Kicking { get; set; } = new List<Kicks>();
        public List<Bans> Banning { get; set; } = new List<Bans>();
        public List<autochannels> AutoMessage { get; set; } = new List<autochannels>();


        public levelling Levels { get; set; } = new levelling();
        public gambling Gambling { get; set; } = new gambling();

        public static void SaveServer(GuildConfig config)
        {
            var file = Path.Combine(Appdir, $"setup/server/{config.GuildId}.json");
            var output = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(file, output);
        }

        public static void Setup(IGuild guild)
        {
            if (File.Exists(Path.Combine(Appdir, $"setup/server/{guild.Id}.json"))) return;
            var cfg = new GuildConfig
            {
                GuildId = guild.Id,
                GuildName = guild.Name
            };

            SaveServer(cfg);
        }

        public static GuildConfig GetServer(IGuild guild)
        {
            if (guild == null) return null;
            if (!File.Exists(Path.Combine(Appdir, $"setup/server/{guild.Id}.json"))) Setup(guild);

            var file = Path.Combine(Appdir, $"setup/server/{guild.Id}.json");
            return JsonConvert.DeserializeObject<GuildConfig>(File.ReadAllText(file));
        }

        public class visibilityconfig
        {
            public List<string> BlacklistedModules { get; set; } = new List<string>();
            public List<string> BlacklistedCommands { get; set; } = new List<string>();
        }

        public class roleConfigurations
        {
            // a list of roles that users can join via command
            public List<ulong> SubRoleList { get; set; } = new List<ulong>();


            //public ulong DjRoleId { get; set; } // restrict the music module to a specific role
            public ulong MutedRole { get; set; } = 0;

            //public ulong ModeratorRoleId { get; set; } = 0;
            public List<ulong> ModeratorRoleList { get; set; } = new List<ulong>();
            public List<ulong> AdminRoleList { get; set; } = new List<ulong>();
        }


        public class antispams
        {
            public blacklist Blacklist { get; set; } = new blacklist();
            public antispam Antispam { get; set; } = new antispam();
            public advertising Advertising { get; set; } = new advertising();
            public mention Mention { get; set; } = new mention();
            public privacy Privacy { get; set; } = new privacy();
            public toxicity Toxicity { get; set; } = new toxicity();

            public List<IgnoreRole> IngoreRoles { get; set; } = new List<IgnoreRole>();

            public class toxicity
            {
                public bool UsePerspective { get; set; } = false;
                public int ToxicityThreshHold { get; set; } = 90;
            }


            public class antispam
            {
                //remove repetitive messages and messages posted in quick succession
                public bool NoSpam { get; set; } = false;

                //words to skip while using antispam
                public List<string> AntiSpamSkip { get; set; } = new List<string>();

                //Toggle wether or not to use antispam on bot commands
                public bool IgnoreCommandMessages { get; set; } = true;

                public bool antiraid { get; set; } = false;
            }

            public class blacklist
            {
                //the blacklist word groupings
                public List<BlacklistWords> BlacklistWordSet { get; set; } = new List<BlacklistWords>();
                public string DefaultBlacklistMessage { get; set; } = "";

                //toggle wether or not to filter diatrics and replace certain numbers with their letter counterparts etc.
                public bool BlacklistBetterFilter { get; set; } = true;

                public class BlacklistWords
                {
                    //Words for the specified blacklist message
                    public List<string> WordList { get; set; } = new List<string>();

                    //Custom response for certain words.
                    public string BlacklistResponse { get; set; } = null;
                }
            }

            public class advertising
            {
                public string NoInviteMessage { get; set; } = null;

                //blacklist for discord invites
                public bool Invite { get; set; } = false;
            }

            public class mention
            {
                public string MentionAllMessage { get; set; } = null;

                //blacklist for @everyone and @here 
                public bool MentionAll { get; set; } = false;

                //Remove 5+ mentions of roles or users
                public bool RemoveMassMention { get; set; } = false;
            }

            public class privacy
            {
                //remove all ip addresses posted in the format x.x.x.x
                public bool RemoveIPs { get; set; } = false;
            }

            public class IgnoreRole
            {
                public ulong RoleID { get; set; }

                //false = filter
                //true = bypass filter
                public bool AntiSpam { get; set; } = false;
                public bool Blacklist { get; set; } = false;
                public bool Advertising { get; set; } = false;
                public bool Mention { get; set; } = false;
                public bool Privacy { get; set; } = false;
                public bool Toxicity { get; set; } = false;
            }
        }

        public class levelling
        {
            public bool LevellingEnabled { get; set; } = true;

            public bool UseLevelMessages { get; set; } = true;

            public bool IncrementLevelRewards { get; set; } = true;

            public bool UseLevelChannel { get; set; } = false;
            public ulong LevellingChannel { get; set; }

            public List<Level> LevelRoles { get; set; } = new List<Level>();
            public List<user> Users { get; set; } = new List<user>();

            public class Level
            {
                public int LevelToEnter { get; set; }
                public ulong RoleID { get; set; }
            }

            public class user
            {
                public ulong userID { get; set; }
                public int xp { get; set; } = 0;
                public int level { get; set; } = 1;
                public bool banned { get; set; } = false;
            }
        }

        public class PartnerShip
        {
            public bool IsPartner { get; set; } = false;
            public ulong PartherChannel { get; set; } = 0;
            public string Message { get; set; } = null;
            public bool banned { get; set; } = false;
            public string ImageUrl { get; set; } = null;
            public bool showusercount { get; set; }
        }

        public class autochannels
        {
            public bool enabled { get; set; } = false;
            public ulong channelID { get; set; }
            public int messages { get; set; } = 0;
            public string automessage { get; set; } = "PassiveBOT";
            public int sendlimit { get; set; } = 50;
        }

        public class Twitch
        {
            public string Username { get; set; }
            public bool LastCheckedStatus { get; set; } = false;
        }

        public class GiveAway
        {
            public string Message { get; set; }
            public List<ulong> Users { get; set; } = new List<ulong>();
            public ulong Creator { get; set; }
        }

        public class Warns
        {
            public string User { get; set; }
            public string Reason { get; set; }
            public string Moderator { get; set; }
            public ulong UserId { get; set; }
        }

        public class Kicks
        {
            public string User { get; set; }
            public string Reason { get; set; }
            public string Moderator { get; set; }
            public ulong UserId { get; set; }
        }

        public class Bans
        {
            public string User { get; set; }
            public string Reason { get; set; }
            public string Moderator { get; set; }
            public ulong UserId { get; set; }
        }

        public class gambling
        {
            public TheStore Store { get; set; } = new TheStore();


            public List<user> Users { get; set; } = new List<user>();
            public bool enabled { get; set; } = true;
            public GamblingSet settings { get; set; } = new GamblingSet();

            public class GamblingSet
            {
                public string CurrencyName { get; set; } = "Coins";
            }

            public class TheStore
            {
                public List<Storeitem> ShowItems { get; set; } = new List<Storeitem>();

                public class Storeitem
                {
                    public string ItemName { get; set; }
                    public int ItemID { get; set; } = 0;

                    //Possible Stats of items?
                    public int Attack { get; set; } = 0;
                    public int Defense { get; set; } = 0;

                    public int cost { get; set; } = 0;

                    //Ensure when using this, we use -1 as unlimited quantity.
                    //This means that if we do have a limited quantity do not let it go below zero.
                    public int quantity { get; set; } = -1;


                    //Keep a log of how many have been purchased ever.
                    public int total_purchased { get; set; } = 0;

                    public ulong InitialCreatorID { get; set; }


                    public bool HasDurability { get; set; } = false;
                    public int Durability { get; set; } = 100;
                    public int DurabilityModifier { get; set; } = 5;

                    //Remove item from store w/o actually removing it.
                    public bool Hidden { get; set; } = false;
                }
            }

            public class user
            {
                public List<item> Inventory { get; set; } = new List<item>();
                public ulong userID { get; set; }
                public int coins { get; set; } = 200;
                public bool banned { get; set; } = false;
                public int totalpaidout { get; set; } = 0;
                public int totalbet { get; set; } = 0;

                public class item
                {
                    public int ItemID { get; set; }

                    public int Durability { get; set; } = 100;

                    public int quantity { get; set; } = 0;
                }
            }
        }
    }
}