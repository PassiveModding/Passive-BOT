using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace PassiveBOT.Configuration
{
    public class Homeserver
    {
        [JsonIgnore] public static readonly string Appdir = AppContext.BaseDirectory;


        public ulong GuildId { get; set; } //
        public string GuildName { get; set; } //
        public ulong Suggestion { get; set; } = 0;
        public ulong Error { get; set; } = 0;
        public ulong PartnerUpdates { get; set; } = 0;
        public ulong BotModerator { get; set; } = 0;
        public List<globalban> GlobalBans { get; set; } = new List<globalban>();
        public class globalban
        {
            public string Name { get; set; } = "";
            public ulong ID { get; set; } = 0;
        }

        public static void SaveHome(Homeserver config)
        {
            var file = Path.Combine(Appdir, "setup/config/home.json");
            var output = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(file, output);
        }

        public static Homeserver Load()
        {
            var file = Path.Combine(Appdir, "setup/config/home.json");
            return JsonConvert.DeserializeObject<Homeserver>(File.ReadAllText(file));
        }
    }
}