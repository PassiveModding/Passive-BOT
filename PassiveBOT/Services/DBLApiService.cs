using System;
using System.Collections.Generic;
using System.Text;

namespace PassiveBOT.Services
{
    using System.Threading.Tasks;

    using Discord.WebSocket;

    using DiscordBotsList.Api.Adapter.Discord.Net;

    using PassiveBOT.Models;

    public class DBLApiService
    {
        private DiscordShardedClient Client { get; }

        private ConfigModel Config { get; }

        public DBLApiService(DiscordShardedClient client, ConfigModel config)
        {
            Client = client;
            Config = config;
        }

        public bool Initialized { get; set; }

        public ShardedDiscordNetDblApi DBLApi { get; set; }

        public async Task<bool> InitializeAsync()
        {
            if (Config.DiscordBotsListApi != null && Client != null && Client.CurrentUser != null)
            {
                try
                {
                    DBLApi = new ShardedDiscordNetDblApi(Client, Config.DiscordBotsListApi);
                    DBLApi?.CreateListener();
                    Initialized = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            return Initialized;
        }
    }
}
