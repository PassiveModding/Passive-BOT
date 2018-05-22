using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace PassiveBOT.Models
{
    public class HomeModel
    {
        public ulong ID { get; set; }
        public logging Logging { get; set; } = new logging();

        public class logging
        {
            public bool LogPartnerChanges { get; set; } = false;
            public ulong PartnerLogChannel { get; set; }
        }



        public void Save(string dir = "setup/home.json")
        {
            var file = Path.Combine(AppContext.BaseDirectory, dir);
            File.WriteAllText(file, ToJson());
        }

        public static HomeModel Load(string dir = "setup/home.json")
        {
            var file = Path.Combine(AppContext.BaseDirectory, dir);
            return File.Exists(file) ? JsonConvert.DeserializeObject<HomeModel>(File.ReadAllText(file)) : new HomeModel();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

    }
}
