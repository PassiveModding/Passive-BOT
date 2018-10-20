namespace PassiveBOT.Extensions
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Discord;

    using global::PassiveBOT.Handlers;

    using Raven.Client.Documents;

    public class TranslateLimits
    {
        /// <summary>
        ///     The _timer.
        /// </summary>
        private readonly Timer timer;

        /// <summary>
        ///     Gets or sets the last fire time.
        /// </summary>
        public static DateTime LastFireTime { get; set; } = DateTime.MinValue;

        private bool initialized = false;

        public TranslateLimits(IDocumentStore store)
        {
            Store = store;
            timer = new Timer(
                async _ =>
                    {
                        try
                        {
                            await ClearDailyAsync();
                        }
                        catch (Exception e)
                        {
                            LogHandler.LogMessage("Reset Error:\n" + $"{e}", LogSeverity.Error);
                        }
                        LastFireTime = DateTime.UtcNow;
                    },
                null,
                TimeSpan.Zero,
                TimeSpan.FromHours(24));
        }

        public void Initialize()
        {
            using (var session = Store.OpenSession())
            {
                var doc = session.Load<TranslateLimits>("TranslateLimits");

                if (doc != null)
                {
                    Users = doc.Users;
                    Guilds = doc.Guilds;
                    Keys = doc.Keys;
                }
            }

            initialized = true;
            timer.Change(TimeSpan.Zero, TimeSpan.FromHours(24));
        }

        public void Save()
        {
            using (var session = Store.OpenSession())
            {
                try
                {
                    session.Store(this, "TranslateLimits");
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
        private readonly IDocumentStore Store;

        public Task ClearDailyAsync()
        {
            if (!initialized)
            {
                return Task.CompletedTask;
            }

            foreach (var user in Users)
            {
                user.Value.DailyTranslations = 0;
            }

            foreach (var guild in Guilds)
            {
                guild.Value.DailyTranslations = 0;
            }

            Save();
            return Task.CompletedTask;
        }

        public enum ResponseStatus
        {
            DefaultSuccess,
            UserUpgradedSuccess,
            UserLimitExceeded,
            GuildLimitExceeded
        }

        public async Task<ResponseStatus> UpdateAsync(ulong guildId, ulong userId)
        {
           var userResult = await UpdateUserAsync(userId);

            Save();

            if (userResult == ResponseStatus.UserLimitExceeded)
            {
                return ResponseStatus.UserLimitExceeded;
            }

            if (userResult == ResponseStatus.DefaultSuccess)
            {
                var guildResult = await UpdateGuildAsync(guildId);

                if (guildResult == ResponseStatus.GuildLimitExceeded)
                {
                    return ResponseStatus.GuildLimitExceeded;
                }
            }

            if (userResult == ResponseStatus.UserUpgradedSuccess)
            {
                return ResponseStatus.UserUpgradedSuccess;
            }

            return ResponseStatus.DefaultSuccess;
        }

        public List<UnRedeemedKey> Keys { get; set; } = new List<UnRedeemedKey>();

        public class KeyRedemptionObject
        {
            public bool Success { get; set; }

            public DateTime ValidUntil { get; set; } = DateTime.UtcNow;
        }


        public Task<KeyRedemptionObject> RedeemKey(ulong userId, string key)
        {
            var keyObj = Keys.FirstOrDefault(x => x.Key == key);
            if (keyObj != null)
            {
                if (Users.TryGetValue(userId, out User user))
                {
                    user.Upgrades.Add(new RedeemedKey
                                          {
                                              Key = keyObj.Key,
                                              Expiry = DateTime.UtcNow + keyObj.ValidFor
                                          });

                    Keys = Keys.Where(x => x.Key != key).ToList();
                    Save();
                    return Task.FromResult(new KeyRedemptionObject
                               {
                                   Success = true,
                                   ValidUntil = DateTime.UtcNow + keyObj.ValidFor
                               });
                }

                return Task.FromResult(new KeyRedemptionObject
                                           {
                                               Success = false,
                                               ValidUntil = DateTime.UtcNow
                                           });
            }

            return Task.FromResult(new KeyRedemptionObject
                                       {
                                           Success = false,
                                           ValidUntil = DateTime.UtcNow
                                       });
        }

        public Task<ResponseStatus> UpdateUserAsync(ulong userId)
        {
            if (Users.TryGetValue(userId, out var User))
            {
                if (User.DailyTranslations > 100)
                {
                    if (User.Upgrades.Any(x => x.Expiry >= DateTime.UtcNow))
                    {
                        User.DailyTranslations++;
                        User.TotalTranslations++;
                        return Task.FromResult(ResponseStatus.UserUpgradedSuccess);
                    }

                    return Task.FromResult(ResponseStatus.UserLimitExceeded);
                }

                User.DailyTranslations++;
                User.TotalTranslations++;
            }
            else
            {
                Users.TryAdd(userId, new User { UserId = userId });
            }

            return Task.FromResult(ResponseStatus.DefaultSuccess);
        }

        public Task<ResponseStatus> UpdateGuildAsync(ulong guildId)
        {
            if (Guilds.TryGetValue(guildId, out var guild))
            {
                if (guild.DailyTranslations > 2000)
                {
                    return Task.FromResult(ResponseStatus.GuildLimitExceeded);
                }

                guild.DailyTranslations++;
                guild.TotalTranslations++;
            }
            else
            {
                Guilds.TryAdd(guildId, new Guild { GuildId = guildId });
            }

            return Task.FromResult(ResponseStatus.DefaultSuccess);
        }

        public ConcurrentDictionary<ulong, User> Users { get; set; } = new ConcurrentDictionary<ulong, User>();

        public ConcurrentDictionary<ulong, Guild> Guilds { get; set; } = new ConcurrentDictionary<ulong, Guild>();

        public class UnRedeemedKey
        {
            public string Key { get; set; }

            public TimeSpan ValidFor { get; set; } = TimeSpan.FromDays(28);
        }

        public class RedeemedKey
        {
            public string Key { get; set; }

            public DateTime Expiry { get; set; }
        }

        public class User
        {
            public ulong UserId { get; set; }

            public int DailyTranslations { get; set; }

            public int TotalTranslations { get; set; }

            public List<RedeemedKey> Upgrades { get; set; } = new List<RedeemedKey>();
        }

        public class Guild
        {
            public ulong GuildId { get; set; }

            public int DailyTranslations { get; set; }

            public int TotalTranslations { get; set; }
        }
    }
}
