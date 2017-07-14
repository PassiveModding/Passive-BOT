using System;
using System.IO;
using Newtonsoft.Json;
using PassiveBOT.Handlers;

namespace PassiveBOT.Configuration
{
    public class Config
    {
        [JsonIgnore] public static readonly string Appdir = AppContext.BaseDirectory;

        public static string ConfigPath = Path.Combine(AppContext.BaseDirectory, "setup/config/config.json");

        public string Prefix { get; set; } = "";
        public string Token { get; set; } = "";
        public string Debug { get; set; } = "";

        public void Save(string dir = "setup/config/config.json")
        {
            var file = Path.Combine(Appdir, dir);
            File.WriteAllText(file, ToJson());
        }

        public static Config Load(string dir = "setup/config/config.json")
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
            ColourLog.In1Run("Run (Y for run, N for setup Config)");

            Console.Write("Y or N: ");
            var res = Console.ReadLine();
            if (res == "N" || res == "n")
                File.Delete("setup/config/config.json");

            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "setup/config")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "setup/config"));

            if (!File.Exists(ConfigPath))
            {
                var cfg = new Config();

                ColourLog.In1Run(
                    @"Please enter a prefix for the bot eg. '+' (do not include the '' outside of the prefix)");
                Console.Write("Prefix: ");
                cfg.Prefix = Console.ReadLine();
                Configuration.Load.Pre = cfg.Prefix;

                ColourLog.In1Run("Would you like to log debug?");
                Console.Write("Yes or No: ");
                var type = Console.ReadLine();
                if (type.StartsWith("y") || type.StartsWith("Y"))
                    type = "Y";
                else
                    type = "N";
                cfg.Debug = type;

                ColourLog.In1Run(
                    @"After you input your token, a config will be generated at 'setup/config/config.json'");
                Console.Write("Token: ");
                cfg.Token = Console.ReadLine();

                cfg.Save();
            }
            else
            {
                Configuration.Load.Pre = Load().Prefix;
            }
            ColourLog.In1Run("Config Loaded!");
            ColourLog.In1Run($"Prefix: {Load().Prefix}");
            ColourLog.In1Run($"Debug: {Load().Debug}");
            ColourLog.In1Run($"Token Length: {Load().Token.Length} (should be 59)");
        }
    }
}