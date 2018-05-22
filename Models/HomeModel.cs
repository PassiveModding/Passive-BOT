using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
