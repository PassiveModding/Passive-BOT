using System;
using System.Collections.Generic;
using System.Text;

namespace PassiveBOT.Services
{
    using Discord.WebSocket;

    using DiscordBotsList.Api.Adapter.Discord.Net;

    using PassiveBOT.Models;

    public class DBLApiService
    {
        private readonly DiscordShardedClient Client;

        private readonly ConfigModel Config;

        public DBLApiService(DiscordShardedClient client, ConfigModel config)
        {
            Client = client;
            Config = config;
        }

        public bool Initialized { get; set; } = false;

        public ShardedDiscordNetDblApi DBLApi { get; set; }

        public void Initialize()
        {
            if (Config.DiscordBotsListApi != null)
            {
                try
                {
                    DBLApi = new ShardedDiscordNetDblApi(Client, Config.DiscordBotsListApi);
                    DBLApi.CreateListener();
                    Initialized = true;
                }
                catch
                {
                    //
                }
            }
        }
    }
}
