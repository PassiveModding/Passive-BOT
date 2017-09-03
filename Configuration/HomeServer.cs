using System;
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

        public static void SaveHome(Homeserver config)
        {
            var file = Path.Combine(Appdir, $"setup/config/home.json");
            var output = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(file, output);
        }

        public static Homeserver Load()
        {
            var file = Path.Combine(Appdir, $"setup/config/home.json");
            return JsonConvert.DeserializeObject<Homeserver>(File.ReadAllText(file));
        }
    }
}