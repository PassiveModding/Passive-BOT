using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PassiveBOT.Services
{
    public class Config
    {
        [JsonIgnore]
        public static readonly string appdir = AppContext.BaseDirectory;

        public string Prefix { get; set; } = "";
        public string Token { get; set; } = "";

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
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "cfg")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "cfg"));

            if (!File.Exists(ConfigPath))
            {
                var cfg = new Config();
                Console.WriteLine(prefix);
                Console.Write("Prefix: ");
                cfg.Prefix = Console.ReadLine();

                Console.WriteLine(token);
                Console.Write("Token: ");
                cfg.Token = Console.ReadLine();

                cfg.Save();
            }
            Console.WriteLine("Configuration successfully loaded!");
        }
        public static string token = @"After you input your token, a config will be generated at 'cfg\\config.json'.";
        public static string prefix = @"Please enter a prefix for the bot eg. '.' or '+' (do not include the '' outside of the prefix)";
        public static string ConfigPath = Path.Combine(AppContext.BaseDirectory, "cfg/config.json");
    }
}
