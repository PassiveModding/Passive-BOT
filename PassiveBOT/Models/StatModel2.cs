namespace PassiveBOT.Models
{
    using System.Collections.Generic;

    public class StatModel2
    {
        public int MessagesReceived { get; set; } = 0;

        public Dictionary<string, CommandStats> Stats { get; set; } = new Dictionary<string, CommandStats>();

        public class CommandStats
        {
            public int Executions { get; set; } = 0;

            public int Errors { get; set; } = 0;

            public Dictionary<ulong, int> CommandUsers { get; set; } = new Dictionary<ulong, int>();

            public Dictionary<ulong, int> CommandGuilds { get; set; } = new Dictionary<ulong, int>();

            public Dictionary<ulong, string> CommandErrors { get; set; } = new Dictionary<ulong, string>();
        }
    }
}
