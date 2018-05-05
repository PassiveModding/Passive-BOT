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
        public string MentionAllMessage = null;
        public string NoInviteMessage = null;


        public ulong GuildId { get; set; } //
        public string GuildName { get; set; } //
        public string Prefix { get; set; } = Configuration.Load.Pre; //


        public bool antiraid { get; set; } = false;
        public bool chatwithmention { get; set; } = true;
        public ulong DjRoleId { get; set; } // restrict the music module to a specific role
        public ulong MutedRole { get; set; } = 0;
        public ulong ModeratorRoleId { get; set; } = 0;
        public PartnerShip PartnerSetup { get; set; } = new PartnerShip();


        //TRANSLATION
        //public bool Premium { get; set; } = false;

        //public DateTime TimeOffset { get; set; }
        //public int Characters { get; set; } = 0;


        public List<ulong> RoleList { get; set; } =
            new List<ulong>(); // a list of roles that users can join via command


        public string Rss { get; set; } = "0"; // rss feed url

        public ulong RssChannel { get; set; } // channel to post custom rss feeds to
        //public List<Twitch> TwitchChannels { get; set; } = new List<Twitch>();
        //public ulong TwitchPostChannel { get; set; } = 0;


        public List<Tags.Tagging> Dict { get; set; } = new List<Tags.Tagging>(); // tags module

        public List<string> Blacklist { get; set; } = new List<string>(); // keyword blacklist
        public string BlacklistMessage { get; set; } = "";
        public bool BlacklistBetterFilter { get; set; } = true;
        public bool Invite { get; set; } = false; // blacklist for discord invites
        public List<ulong> InviteExcempt { get; set; } = new List<ulong>();

        public bool MentionAll { get; set; } = false; //blacklist for @everyone and @here 
        public List<ulong> MentionallExcempt { get; set; } = new List<ulong>();

        public bool NoSpam { get; set; } = false;
        public List<string> AntiSpamSkip { get; set; } = new List<string>();
        public bool RemoveMassMention { get; set; } = false;
        public bool RemoveIPs { get; set; } = false;

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

        public static GuildConfig Load(ulong id, IGuild contextGuild)
        {
            var file = Path.Combine(Appdir, $"setup/server/{id}.json");
            if (!File.Exists(file)) Setup(contextGuild);
            return JsonConvert.DeserializeObject<GuildConfig>(File.ReadAllText(file));
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
            if (!File.Exists(Path.Combine(Appdir, $"setup/server/{guild.Id}.json"))) Setup(guild);

            var file = Path.Combine(Appdir, $"setup/server/{guild.Id}.json");
            return JsonConvert.DeserializeObject<GuildConfig>(File.ReadAllText(file));
        }

        public static string SetWMessage(IGuild guild, string input)
        {
            var file = Path.Combine(Appdir, $"setup/server/{guild.Id}.json");
            if (File.Exists(file))
            {
                var jsonObj = GetServer(guild);
                jsonObj.WelcomeMessage = input;
                jsonObj.WelcomeEvent = true;
                SaveServer(jsonObj);
            }
            else
            {
                return "please run the setup command before using configuration commands";
            }

            return null;
        }

        public static string SetWChannel(IGuild guild, ulong channel)
        {
            var file = Path.Combine(Appdir, $"setup/server/{guild.Id}.json");
            if (File.Exists(file))
            {
                var jsonObj = GetServer(guild);
                jsonObj.WelcomeChannel = channel;
                SaveServer(jsonObj);
            }
            else
            {
                return "please run the setup command before using configuration commands";
            }

            return null;
        }

        public static string SetError(IGuild guild, bool status)
        {
            var file = Path.Combine(Appdir, $"setup/server/{guild.Id}.json");
            if (File.Exists(file))
            {
                var jsonObj = GetServer(guild);
                jsonObj.ErrorLog = status;
                SaveServer(jsonObj);
            }
            else
            {
                return "please run the setup command before using configuration commands";
            }

            return null;
        }


        public static string SetWelcomeStatus(IGuild guild, bool status)
        {
            var file = Path.Combine(Appdir, $"setup/server/{guild.Id}.json");
            if (File.Exists(file))
            {
                var jsonObj = GetServer(guild);
                jsonObj.WelcomeEvent = status;
                SaveServer(jsonObj);
            }
            else
            {
                return "please run the setup command before using configuration commands";
            }

            return null;
        }

        public static string SetDj(IGuild guild, ulong role)
        {
            var file = Path.Combine(Appdir, $"setup/server/{guild.Id}.json");
            if (File.Exists(file))
            {
                var jsonObj = GetServer(guild);
                jsonObj.DjRoleId = role;
                SaveServer(jsonObj);
            }
            else
            {
                return "please run the setup command before using configuration commands";
            }

            return null;
        }

        public static string RssSet(IGuild guild, ulong chan, string url, bool add)
        {
            var file = Path.Combine(Appdir, $"setup/server/{guild.Id}.json");
            if (File.Exists(file))
            {
                var jsonObj = GetServer(guild);

                if (add)
                {
                    jsonObj.Rss = url;
                    jsonObj.RssChannel = chan;
                }
                else
                {
                    jsonObj.Rss = "0";
                }

                SaveServer(jsonObj);
            }
            else
            {
                return "please run the setup command before using configuration commands";
            }

            return null;
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
            public bool enabled { get; set; } = true;
            public GamblingSet settings { get; set; } = new GamblingSet();
            public class GamblingSet
            {
                public string CurrencyName { get; set; } = "Coins";
            }

            public TheStore Store = new TheStore(); 
            public class TheStore
            {

                public List<Storeitem> ShowItems = new List<Storeitem>();
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


                    //Remove item from store w/o actually removing it.
                    public bool Hidden { get; set; } = false;
                }
            }


            public List<user> Users = new List<user>();
            public class user
            {
                public ulong userID { get; set; }
                public int coins { get; set; } = 200;
                public bool banned { get; set; } = false;
                public int totalpaidout { get; set; } = 0;
                public int totalbet { get; set; } = 0;

                public List<item> Inventory = new List<item>();
                public class item
                {
                    public int ItemID { get; set; }
                    public int quantity { get; set; } = 0;
                }
            }
        }
    }
}