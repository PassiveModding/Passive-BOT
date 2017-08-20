using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Discord;

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
        public bool ErrorLog { get; set; }
        public ulong Roles { get; set; }
        public string Rss { get; set; }
        public ulong RssChannel { get; set; }
        public Dictionary<string, string> Tags { get; set; }

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

        public static void Setup(IGuild guild)
        {
            var id = guild.Id;
            var name = guild.Name;
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, $"setup/server/{id}")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, $"setup/server/{id}"));

            if (File.Exists(Path.Combine(Appdir, $"setup/server/{id}/config.json"))) return;
            var cfg = new GuildConfig
            {
                GuildId = id,
                GuildName = name,
                WelcomeMessage = "Welcome to Our Server!!",
                ErrorLog = false,
                Roles = 0
            };

            cfg.Save(id);
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

        public static string SetError(ulong id, bool status)
        {
            var file = Path.Combine(Appdir, $"setup/server/{id}/config.json");
            if (File.Exists(file))
            {
                dynamic jsonObj = JsonConvert.DeserializeObject(File.ReadAllText(file));
                jsonObj.ErrorLog = status;
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

        public static string SetDj(ulong id, ulong role)
        {
            var file = Path.Combine(Appdir, $"setup/server/{id}/config.json");
            if (File.Exists(file))
            {
                dynamic jsonObj = JsonConvert.DeserializeObject(File.ReadAllText(file));
                jsonObj.DjRoleId = role;
                string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
                File.WriteAllText(file, output);
            }
            else
            {
                return "please run the setup command before using configuration commands";
            }
            return null;
        }

        public static string Subrole(ulong id, ulong role, bool add)
        {
            var file = Path.Combine(Appdir, $"setup/server/{id}/config.json");
            if (File.Exists(file))
            {
                dynamic jsonObj = JsonConvert.DeserializeObject(File.ReadAllText(file));
                var list = new List<ulong>();

                if (add)
                    list.Add(role);

                if (list.Count == 1)
                    jsonObj.Roles = role;
                else if (list.Count == 0)
                    jsonObj.Roles = 0;

                Console.WriteLine(1);
                string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
                File.WriteAllText(file, output);
                Console.WriteLine(1);
            }
            else
            {
                return "please run the setup command before using configuration commands";
            }
            return null;
        }

        public static string RssSet(ulong id, ulong chan, string url, bool add)
        {
            var file = Path.Combine(Appdir, $"setup/server/{id}/config.json");
            if (File.Exists(file))
            {
                dynamic jsonObj = JsonConvert.DeserializeObject(File.ReadAllText(file));

                if (add)
                {
                    jsonObj.Rss = url;
                    jsonObj.RssChannel = chan;
                }
                else
                {
                    jsonObj.Rss = 0;
                }

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