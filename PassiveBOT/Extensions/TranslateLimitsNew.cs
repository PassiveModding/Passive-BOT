namespace PassiveBOT.Extensions
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Raven.Client.Documents;

    public class TranslateLimitsNew
    {
        public TranslateLimitsNew(IDocumentStore store)
        {
            this.store = store;
        }

        private string documentName = "TranslateLimitsTwo";

        public void Initialize()
        {
            using (var session = store.OpenSession())
            {
                var doc = session.Load<TranslateLimitsNew>(documentName);

                if (doc != null)
                {
                    Guilds = doc.Guilds;
                    Keys = doc.Keys;
                }
            }
        }

        public void Save()
        {
            using (var session = store.OpenSession())
            {
                try
                {
                    session.Store(this, documentName);
                    session.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        /// <summary>
        ///     Gets or sets the store.
        /// </summary>
        private readonly IDocumentStore store;
       
        public enum ResponseStatus
        {
            GuildLimitExceeded,
            GuildSucceded,
            GuildLimitExceededByMessage,
            Error
        }

        public async Task<ResponseStatus> UpdateAsync(ulong guildId, string message)
        {
            var guildResult = await UpdateGuildAsync(guildId, message);
            Save();
            return guildResult;
        }

        public List<GuildKey> Keys { get; set; } = new List<GuildKey>();

        public class KeyRedemptionObject
        {
            public bool Success { get; set; }

            public int ValidFor { get; set; }
        }

        private readonly KeyRedemptionObject failKeyRedemption = new KeyRedemptionObject
                                                           {
                                                               Success = false,
                                                               ValidFor = 0
                                                           };

        public Task<KeyRedemptionObject> RedeemKeyAsync(ulong guildId, string key)
        {
            var keyObj = Keys.FirstOrDefault(x => x.Key == key);
            if (keyObj != null)
            {
                if (Guilds.TryGetValue(guildId, out Guild guild))
                {
                    guild.Upgrades.Add(new GuildKey
                                          {
                                              Key = keyObj.Key,
                                              ValidFor = keyObj.ValidFor
                                          });

                    Keys = Keys.Where(x => x.Key != key).ToList();
                    Save();
                    return Task.FromResult(new KeyRedemptionObject
                               {
                                   Success = true,
                                   ValidFor = keyObj.ValidFor
                               });
                }

                return Task.FromResult(failKeyRedemption);
            }

            return Task.FromResult(failKeyRedemption);
        }

        private Task<ResponseStatus> UpdateGuildAsync(ulong guildId, string message)
        {
            if (Guilds.TryGetValue(guildId, out var guild))
            {
                int max = guild.MaxCharacters();
                if (guild.TotalCharacters >= max)
                {
                    return Task.FromResult(ResponseStatus.GuildLimitExceeded);
                }

                if (guild.TotalCharacters + message.Length > max)
                { 
                    return Task.FromResult(ResponseStatus.GuildLimitExceededByMessage);
                }

                guild.TotalCharacters += message.Length;
                return Task.FromResult(ResponseStatus.GuildSucceded);
            }
            else
            {
                Guilds.TryAdd(guildId, new Guild { GuildId = guildId });
            }

            return Task.FromResult(ResponseStatus.GuildLimitExceeded);
        }

        public ConcurrentDictionary<ulong, Guild> Guilds { get; set; } = new ConcurrentDictionary<ulong, Guild>();

        public class GuildKey
        {
            public string Key { get; set; }

            public int ValidFor { get; set; }
        }

        public class Guild
        {
            public ulong GuildId { get; set; }

            public int TotalCharacters { get; set; }

            public List<GuildKey> Upgrades { get; set; } = new List<GuildKey>();

            public int MaxCharacters()
            {
                return Upgrades.Sum(x => x.ValidFor);
            }

            public int RemainingCharacters()
            {
                return MaxCharacters() - TotalCharacters;
            }

            public bool CanTranslate(int characters)
            {
                if (TotalCharacters + characters > MaxCharacters())
                {
                    return false;
                }

                return true;
            }
        }
    }
}
