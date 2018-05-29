using System;
using System.Collections.Generic;
using System.IO;
using Discord;
using Newtonsoft.Json;
using PassiveBOT.Handlers;

namespace PassiveBOT.Models
{
    public class GuildModel_Depreciated
    {
        [JsonIgnore] public static readonly string Appdir = AppContext.BaseDirectory;


        public ulong GuildId { get; set; } //
        public string GuildName { get; set; } //
        public string Prefix { get; set; } = CommandHandler.Config.Prefix;

        public bool chatwithmention { get; set; } = true;

        public PartnerShip PartnerSetup { get; set; } = new PartnerShip();

        public roleConfigurations RoleConfigurations { get; set; } = new roleConfigurations();

        public List<Tagging> Dict { get; set; } = new List<Tagging>(); // tags module
        public visibilityconfig Visibilityconfig { get; set; } = new visibilityconfig();

        public bool ErrorLog { get; set; } // allows for responses with errors 

        public bool GoodbyeEvent { get; set; } = false;
        public string GoodbyeMessage { get; set; } = "Has Left the Server :(";
        public ulong GoodByeChannel { get; set; } = 0;
        public bool WelcomeEvent { get; set; } // toggles welcome messages for new users
        public string WelcomeMessage { get; set; } = "Welcome to Our Server!!!"; // the welcome message
        public ulong WelcomeChannel { get; set; } // welcome messages in a channel

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

        public static void SaveServer(GuildModel_Depreciated config)
        {
            var file = Path.Combine(Appdir, $"setup/server/{config.GuildId}.json");
            var output = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(file, output);
        }

        public static void Setup(IGuild guild)
        {
            if (File.Exists(Path.Combine(Appdir, $"setup/server/{guild.Id}.json"))) return;
            var cfg = new GuildModel_Depreciated
            {
                GuildId = guild.Id,
                GuildName = guild.Name
            };

            SaveServer(cfg);
        }

        public static GuildModel_Depreciated GetServer(IGuild guild)
        {
            if (guild == null) return null;
            if (!File.Exists(Path.Combine(Appdir, $"setup/server/{guild.Id}.json"))) return null;

            var file = Path.Combine(Appdir, $"setup/server/{guild.Id}.json");
            return JsonConvert.DeserializeObject<GuildModel_Depreciated>(File.ReadAllText(file));
        }

        public class Tagging
        {
            public string Tagname { get; set; }
            public string Content { get; set; }
            public ulong Creator { get; set; }
            public int uses { get; set; }
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
            public ColorRoles ColorRoleList { get; set; } = new ColorRoles();


            //public ulong ModeratorRoleId { get; set; } = 0;
            public List<ulong> ModeratorRoleList { get; set; } = new List<ulong>();
            public List<ulong> AdminRoleList { get; set; } = new List<ulong>();

            public class ColorRoles
            {
                public enum Colours
                {
                    blue,
                    green,
                    red,
                    purple,
                    yellow,
                    cyan,
                    pink,
                    orange,
                    brown
                }

                public bool AllowCustomColorRoles { get; set; } = false;
                public Color Color { get; set; } = new Color();
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