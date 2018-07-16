namespace PassiveBOT.Models
{
    using System.Collections.Concurrent;

    public class StatModel2
    {
        public int MessagesReceived { get; set; } = 0;

        public ConcurrentDictionary<string, CommandStats> Stats { get; set; } = new ConcurrentDictionary<string, CommandStats>();

        public class CommandStats
        {
            public ConcurrentDictionary<ulong, string> CommandErrors { get; set; } = new ConcurrentDictionary<ulong, string>();

            public ConcurrentDictionary<ulong, int> CommandGuilds { get; set; } = new ConcurrentDictionary<ulong, int>();

            public ConcurrentDictionary<ulong, int> CommandUsers { get; set; } = new ConcurrentDictionary<ulong, int>();

            public int Errors { get; set; } = 0;

            public int Executions { get; set; } = 0;
        }
    }
}