using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PassiveBOT.Configuration
{
    public class Config
    {
        [JsonIgnore]
        public static readonly string appdir = AppContext.BaseDirectory;

        public string Prefix { get; set; } = "";
        public string Token { get; set; } = "";
        public string Debug { get; set; } = "";

        public void Save(string dir = "cfg/config.json")
        {
            string file = Path.Combine(appdir, dir);
            File.WriteAllText(file, ToJson());
        }

        public static Config Load(string dir = "cfg/config.json")
        {
            string file = Path.Combine(appdir, dir);
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(file));
        }

        public string ToJson()
    => JsonConvert.SerializeObject(this, Formatting.Indented);

        public static void CheckExistence()
        {
            Handlers.LogHandler.LogAsync("Run (Y for run, N for setup Config)");
            Console.Write("Y or N: ");
            var res = Console.ReadLine();
            if (res == "N")
                File.Delete("cfg/config.json");
            if (res == "n")
                File.Delete("cfg/config.json");

            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "cfg")))
            {
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "cfg"));
            }

            if (!File.Exists(ConfigPath))
            {
                var cfg = new Config();

                Handlers.LogHandler.LogAsync(prefix);
                Console.Write("Prefix: ");
                cfg.Prefix = Console.ReadLine();

                Handlers.LogHandler.LogAsync("Would you like to log debug?");
                Console.Write("Y or N: ");
                cfg.Debug = Console.ReadLine();
tokeninput:
                Handlers.LogHandler.LogAsync(token);
                Console.Write("Token: ");
                cfg.Token = Console.ReadLine();
                if (cfg.Token.Length == 59)
                    Handlers.LogHandler.LogAsync($"Token Accepted!");
                else
                {
                    Handlers.LogHandler.LogErrorAsync($"Incorrect input", $"Inavlid Token!");
                    goto tokeninput;
                }

                cfg.Save();
            }
            Handlers.LogHandler.LogAsync($"Configuration Loaded!");
            Handlers.LogHandler.LogAsync($"Prefix: {Config.Load().Prefix}");
            Handlers.LogHandler.LogAsync($"Debug: {Config.Load().Debug}");
            Handlers.LogHandler.LogAsync($"Token Length: {Config.Load().Token.Length} (should be 59)");
        }
        public static string token = @"After you input your token, a config will be generated at 'cfg\\config.json'";
        public static string prefix = @"Please enter a prefix for the bot eg. '+' (do not include the '' outside of the prefix)";
        public static string ConfigPath = Path.Combine(AppContext.BaseDirectory, "cfg/config.json");
    }
}
