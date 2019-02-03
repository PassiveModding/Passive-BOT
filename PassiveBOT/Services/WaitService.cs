using System;
using System.Collections.Generic;
using System.Text;

namespace PassiveBOT.Services
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Discord;
    using Discord.WebSocket;

    using Raven.Client.Documents;

    public class WaitService
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HomeService" /> class.
        /// </summary>
        /// <param name="store">
        ///     The store.
        /// </param>
        public WaitService(IDocumentStore store, DiscordShardedClient client)
        {
            Store = store;
            Client = client;
            timer = new Timer(TimerEvent, null, 1000, 30000);
        }
        
        private Timer timer;

        private readonly DiscordShardedClient Client;

        private bool Initialized = false;

        public List<ReminderModel> ReminderModels { get; set; } = new List<ReminderModel>();
        public List<TempRoleModel> TempRoleModels { get; set; } = new List<TempRoleModel>();

        /// <summary>
        ///     Gets or sets the store.
        /// </summary>
        private static IDocumentStore Store { get; set; }

        public void Initialize()
        {
            using (var session = Store.OpenSession())
            {
                ReminderModels = session.Query<ReminderModel>().ToList();
                TempRoleModels = session.Query<TempRoleModel>().ToList();
            }

            if (Client.Shards.Any(x => x.Guilds.Any()))
            {
                Initialized = true;
            }
        }

        public async void TimerEvent(object _)
        {
            if (!Initialized)
            {
                return;
            }

            foreach (var model in TempRoleModels.Where(x => x.ExpiresOn < DateTime.UtcNow.AddMinutes(10)).ToList())
            {
                if (model.ExpiresOn < DateTime.UtcNow)
                {
                    TempRoleModels = TempRoleModels.Where(x => x.Id == model.Id).ToList();

                    using (var session = Store.OpenSession())
                    {
                        var query = session.Query<TempRoleModel>().Where(x => x.Id == model.Id && x.UserId == model.UserId);

                        if (query.Any())
                        {
                            ReminderModels = ReminderModels.Where(x => x.Id != query.First().Id).ToList();
                            session.Delete(query.First());
                            session.SaveChanges();
                        }
                    }

                    var guild = Client.GetGuild(model.GuildId);
                    var role = guild?.GetRole(model.RoleId);
                    var user = guild?.GetUser(model.UserId);

                    if (guild != null && role != null && user != null)
                    {
                        try
                        {
                            await user.RemoveRoleAsync(role);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                    }
                }                
            }

            foreach (var model in ReminderModels.Where(x => x.ExpiresOn < DateTime.UtcNow.AddMinutes(10)).ToList())
            {
                if (model.ExpiresOn < DateTime.UtcNow)
                {
                    ReminderModels = ReminderModels.Where(x => x.Id != model.Id).ToList();
                    DelReminder(model.UserId, model.Id);
                    var guild = Client.GetGuild(model.GuildId);
                    var channel = guild?.GetTextChannel(model.ChannelId);
                    var user = Client.GetUser(model.UserId);
                    if (user != null)
                    {
                        var embed = new EmbedBuilder
                                        {
                                            Description = $"{model.Message}",
                                            Footer =
                                                new EmbedFooterBuilder
                                                    {
                                                        Text =
                                                            $"Created: {model.CreatedOn.ToShortDateString()}",
                                                        IconUrl = user.GetAvatarUrl()
                                                    },
                                            Color = Color.Blue,
                                            Title = "Reminder"
                                        };

                        if (channel != null)
                        {
                            try
                            {
                                await channel.SendMessageAsync($"{user.Mention}", false, embed.Build());
                            }
                            catch
                            {
                                try
                                {
                                    await user.SendMessageAsync($"{user.Mention}", false, embed.Build());
                                }
                                catch
                                {
                                    //
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                await user.SendMessageAsync($"{user.Mention}", false, embed.Build());
                            }
                            catch
                            {
                                //
                            }
                        }
                    }
                }
            }
        }

        public TempRoleModel AddTempRole(ulong guildId, ulong userId, ulong roleId, TimeSpan span)
        {
            using (var session = Store.OpenSession())
            {
                var query = session.Query<TempRoleModel>().ToList();
                int count;
                if (query.Any())
                {
                    count = int.Parse(query.Max(x => x.Id)) + 1;
                }
                else
                {
                    count = 1;
                }

                var newTemp = new TempRoleModel(userId, guildId, roleId, span)
                                      {
                                          Id = $"TempRole-{count}"
                                      };

                session.Store(newTemp);
                session.SaveChanges();

                TempRoleModels.Add(newTemp);
                return newTemp;
            }
        }

        public ReminderModel AddReminder(ulong guildId, ulong userId, ulong channelId, string message, TimeSpan span)
        {
            using (var session = Store.OpenSession())
            {
                var query = session.Query<ReminderModel>().ToList();
                int count;
                if (query.Any())
                {
                    count = int.Parse(query.Max(x => x.Id)) + 1;
                }
                else
                {
                    count = 1;
                }

                var newReminder = new ReminderModel(userId, guildId, message, channelId, span)
                                      {
                                          Id = $"Reminder-{count}"
                                      };

                session.Store(newReminder);
                session.SaveChanges();

                ReminderModels.Add(newReminder);
                return newReminder;
            }
        }

        public bool DelReminder(ulong userId, string reminderId)
        {
            if (int.TryParse(reminderId, out var res))
            {
                return DelReminder(userId, res);
            }

            return false;
        }

        public bool DelReminder(ulong userId, int reminderId)
        {
            using (var session = Store.OpenSession())
            {
                var query = session.Query<ReminderModel>().Where(x => x.Id == reminderId.ToString() && x.UserId == userId);

                if (query.Any())
                {
                    ReminderModels = ReminderModels.Where(x => x.Id != query.First().Id).ToList();
                    session.Delete(query.First());
                    session.SaveChanges();
                    return true;
                }

                return false;
            }
        }
        
        public List<ReminderModel> GetReminders(ulong userId)
        {
            using (var session = Store.OpenSession())
            {
                var query = session.Query<ReminderModel>().Where(x => x.UserId == userId);
                return query.ToList();
            }
        }

        public class TempRoleModel
        {
            public TempRoleModel(ulong userId, ulong guildId, ulong roleId, TimeSpan expiresAfter)
            {
                UserId = userId;
                GuildId = guildId;
                RoleId = roleId;
                ExpiresOn = DateTime.UtcNow + expiresAfter;
            }

            public string Id { get; set; }

            public ulong UserId { get; set; }

            public ulong GuildId { get; set; }

            public ulong RoleId { get; set; }

            public DateTime ExpiresOn { get; set; }
        }

        public class ReminderModel
        {
            public ReminderModel(ulong userId, ulong guildId, string message, ulong channelId, TimeSpan expiresAfter)
            {
                UserId = userId;
                GuildId = guildId;
                ChannelId = channelId;
                Message = message;
                CreatedOn = DateTime.UtcNow;
                ExpiresOn = DateTime.UtcNow + expiresAfter;
            }
            
            public string Id { get; set; }

            public ulong UserId { get; set; }

            public ulong GuildId { get; set; }        

            public string Message { get; set; }

            public ulong ChannelId { get; set; }

            public DateTime ExpiresOn { get; set; }

            public DateTime CreatedOn { get; set; }        
        }

    }
}
