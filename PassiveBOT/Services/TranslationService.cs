namespace PassiveBOT.Services
{
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    using PassiveBOT.Handlers;
    using PassiveBOT.Models;

    public class TranslationService
    {
        private static readonly ConcurrentDictionary<ulong, GuildModel.GuildSetup.TranslateSetup> Cache = new ConcurrentDictionary<ulong, GuildModel.GuildSetup.TranslateSetup>();

        public GuildModel.GuildSetup.TranslateSetup GetSetup(ulong guildId)
        {
            Cache.TryGetValue(guildId, out var setup);
            if (setup == null)
            {
                setup = dbHandler.Execute<GuildModel>(DatabaseHandler.Operation.LOAD, null, guildId.ToString())?.Settings.Translate;
                Cache.TryAdd(guildId, setup);
            }

            return setup;
        }

        public Task UpdateSetupAsync(ulong guildId, GuildModel.GuildSetup.TranslateSetup setup)
        {
            Cache.TryRemove(guildId, out _);
            Cache.TryAdd(guildId, setup);
            return Task.CompletedTask;
        }

        private readonly DatabaseHandler dbHandler;

        public TranslationService(DatabaseHandler handler)
        {
            dbHandler = handler;
        }
    }
}
