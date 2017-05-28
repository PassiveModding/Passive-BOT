using System;
using System.IO;
using Newtonsoft.Json;
using PassiveBOT.Handlers;

namespace PassiveBOT.Configuration
{
    public class Config
    {
        [JsonIgnore] public static readonly string Appdir = AppContext.BaseDirectory;

        public static string ConfigPath = Path.Combine(AppContext.BaseDirectory, "cfg/config.json");

        public string Prefix { get; set; } = "";
        public string Token { get; set; } = "";
        public string Debug { get; set; } = "";

        public void Save(string dir = "cfg/config.json")
        {
            var file = Path.Combine(Appdir, dir);
            File.WriteAllText(file, ToJson());
        }

        public static Config Load(string dir = "cfg/config.json")
        {
            var file = Path.Combine(Appdir, dir);
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(file));
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static void CheckExistence()
        {
            ColourLog.ColourInfo("Run (Y for run, N for setup Config)");

            Console.Write("Y or N: ");
            var res = Console.ReadLine();
            if (res == "N" || res == "n")
                File.Delete("cfg/config.json");

            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "cfg")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "cfg"));

            if (!File.Exists(ConfigPath))
            {
                var cfg = new Config();

                ColourLog.ColourInfo(
                    @"Please enter a prefix for the bot eg. '+' (do not include the '' outside of the prefix)");
                Console.Write("Prefix: ");
                cfg.Prefix = Console.ReadLine();

                ColourLog.ColourInfo("Would you like to log debug?");
                Console.Write("Y or N: ");
                cfg.Debug = Console.ReadLine();

                ColourLog.ColourInfo(@"After you input your token, a config will be generated at 'cfg\\config.json'");
                Console.Write("Token: ");
                cfg.Token = Console.ReadLine();

                cfg.Save();
            }
            ColourLog.ColourInfo("Config Loaded!");
            ColourLog.ColourInfo($"Prefix: {Load().Prefix}");
            ColourLog.ColourInfo($"Debug: {Load().Debug}");
            ColourLog.ColourInfo($"Token Length: {Load().Token.Length} (should be 59)");
        }
    }
}