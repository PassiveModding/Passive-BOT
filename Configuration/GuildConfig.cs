using System;
using System.IO;
using Newtonsoft.Json;

namespace PassiveBOT.Configuration
{
    public class GuildConfig
    {
        [JsonIgnore] public static readonly string Appdir = AppContext.BaseDirectory;

        public bool WelcomeEvent { get; set; }
        public ulong GuildId { get; set; }
        public string GuildName { get; set; }
        public string WelcomeMessage { get; set; }
        public ulong WelcomeChannel { get; set; }
        public ulong DjRoleId { get; set; }

        public void Save(ulong id)
        {
            var file = Path.Combine(Appdir, $"setup/server/{id}/config.json");
            File.WriteAllText(file, ToJson());
        }

        public static GuildConfig Load(ulong id)
        {
            var file = Path.Combine(Appdir, $"setup/server/{id}/config.json");
            return JsonConvert.DeserializeObject<GuildConfig>(File.ReadAllText(file));
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static void Setup(ulong id, string name)
        {
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, $"setup/server/{id}")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, $"setup/server/{id}"));

            if (File.Exists(Path.Combine(Appdir, $"setup/server/{id}/config.json"))) return;
            var cfg = new GuildConfig
            {
                GuildId = id,
                GuildName = name
            };

            cfg.Save(id);
        }

        public static string Read(ulong id)
        {
            var file = Path.Combine(Appdir, $"setup/server/{id}/config.json");
            if (!File.Exists(file)) return null;
            var p = Load(id);
            var output = $"Guild ID: {p.GuildId}\n" +
                         $"Guild Name: {p.GuildName}\n" +
                         $"Welcome Event: {p.WelcomeEvent}\n" +
                         $"Welcome Channel: {p.WelcomeChannel}\n" +
                         $"Welcome Message: {p.WelcomeMessage}\n" +
                         $"DJ Role: {p.DjRoleId}";
            return output;
        }

        public static string SetWMessage(ulong id, string input)
        {
            var file = Path.Combine(Appdir, $"setup/server/{id}/config.json");
            if (File.Exists(file))
            {
                dynamic jsonObj = JsonConvert.DeserializeObject(File.ReadAllText(file));
                jsonObj.WelcomeMessage = input;
                jsonObj.WelcomeEvent = true;
                string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
                File.WriteAllText(file, output);
            }
            else
            {
                return "please run the setup command before using configuration commands";
            }
            return null;
        }

        public static string SetWChannel(ulong id, ulong channel)
        {
            var file = Path.Combine(Appdir, $"setup/server/{id}/config.json");
            if (File.Exists(file))
            {
                dynamic jsonObj = JsonConvert.DeserializeObject(File.ReadAllText(file));
                jsonObj.WelcomeChannel = channel;
                string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
                File.WriteAllText(file, output);
            }
            else
            {
                return "please run the setup command before using configuration commands";
            }
            return null;
        }

        public static string SetWelcomeStatus(ulong id, bool status)
        {
            var file = Path.Combine(Appdir, $"setup/server/{id}/config.json");
            if (File.Exists(file))
            {
                dynamic jsonObj = JsonConvert.DeserializeObject(File.ReadAllText(file));
                jsonObj.WelcomeEvent = status;
                string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
                File.WriteAllText(file, output);
            }
            else
            {
                return "please run the setup command before using configuration commands";
            }
            return null;
        }
    }
}