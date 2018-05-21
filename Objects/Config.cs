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

        public string Prefix { get; set; } = ".p ";
        public string Token { get; set; } = "Token";
        public string Debug { get; set; } = "N";
        public bool AutoRun { get; set; }

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
            bool auto;
            try
            {
                auto = Load().AutoRun;
            }
            catch
            {
                auto = false;
            }

            if (auto)
            {
            }
            else
            {
                LogHandler.LogMessage("Run (Y for run, N for setup Config)");

                Console.Write("Y or N: ");
                var res = Console.ReadLine();
                if (res == "N" || res == "n")
                    File.Delete("setup/config/config.json");

                if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "setup/config")))
                    Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "setup/config"));
            }


            if (!File.Exists(ConfigPath))
            {
                var cfg = new Config();

                LogHandler.LogMessage(
                    @"Please enter a prefix for the bot eg. '+' (do not include the '' outside of the prefix)");
                Console.Write("Prefix: ");
                cfg.Prefix = Console.ReadLine();
                //Configuration.Load.Pre = cfg.Prefix;

                LogHandler.LogMessage("Would you like to log debug?");
                Console.Write("Yes or No: ");
                var type = Console.ReadLine();
                if (type != null && (type.StartsWith("y") || type.StartsWith("Y")))
                    type = "Y";
                else
                    type = "N";
                cfg.Debug = type;

                LogHandler.LogMessage(
                    @"After you input your token, a config will be generated at 'setup/config/config.json'");
                Console.Write("Token: ");
                cfg.Token = Console.ReadLine();


                LogHandler.LogMessage("Would you like to AutoRun the bot from now on? Y/N");
                var type2 = Console.ReadLine();
                if (type2 != null && (type2.StartsWith("y") || type2.StartsWith("Y")))
                    cfg.AutoRun = true;
                else
                    cfg.AutoRun = false;

                cfg.Save();
            }

            LogHandler.LogMessage("Config Loaded!");
            LogHandler.LogMessage($"Prefix: {Load().Prefix}");
            LogHandler.LogMessage($"Debug: {Load().Debug}");
            LogHandler.LogMessage($"Token Length: {Load().Token.Length} (should be 59)");
            LogHandler.LogMessage($"Autorun: {Load().AutoRun}");
        }
    }
}