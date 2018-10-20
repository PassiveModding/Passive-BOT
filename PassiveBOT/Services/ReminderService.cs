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

    public class ReminderService
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HomeService" /> class.
        /// </summary>
        /// <param name="store">
        ///     The store.
        /// </param>
        public ReminderService(IDocumentStore store, DiscordShardedClient client)
        {
            Store = store;
            Client = client;
            timer = new Timer(TimerEvent, null, 1000, 30000);
        }

        private bool Initialized = false;

        public void Initialize()
        {
            using (var session = Store.OpenSession())
            {
                ReminderModels = session.Query<ReminderModel>().ToList();
            }

            if (Client.Shards.Any(x => x.Guilds.Any()))
            {
                Initialized = true;
            }
        }

        private Timer timer;

        private readonly DiscordShardedClient Client;

        public async void TimerEvent(object _)
        {
            if (!Initialized)
            {
                return;
            }

            foreach (var model in ReminderModels.Where(x => x.ExpiresOn.DateTime < DateTime.UtcNow.AddMinutes(10)).ToList())
            {
                if (model.ExpiresOn < DateTimeOffset.UtcNow)
                {
                    ReminderModels = ReminderModels.Where(x => x.Id != model.Id).ToList();
                    DelReminder(model.UserId, model.Id);
                    var guild = Client.GetGuild(model.GuildId);
                    var channel = guild?.GetTextChannel(model.ChannelId);
                    var user = Client.GetUser(model.UserId);
                    if (channel != null && user != null)
                    {
                        var embed = new EmbedBuilder
                                            {
                                                Description = $"{model.Message}",
                                                Footer = new EmbedFooterBuilder
                                                             {
                                                                 Text = $"Created: {model.CreatedOn.DateTime.ToShortDateString()}",
                                                                 IconUrl = user.GetAvatarUrl()
                                                             },
                                                Color = Color.Blue,
                                                Title = "Reminder"
                                            };

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
                }
            }
        }

        public List<ReminderModel> ReminderModels { get; set; } = new List<ReminderModel>();

        /// <summary>
        ///     Gets or sets the store.
        /// </summary>
        private static IDocumentStore Store { get; set; }

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

                var newReminder = new ReminderModel
                                      {
                                          Id = count.ToString(),
                                          ChannelId = channelId,
                                          CreatedOn = DateTime.UtcNow,
                                          ExpiresOn = DateTimeOffset.UtcNow.Add(span),
                                          GuildId = guildId,
                                          Message = message,
                                          UserId = userId
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

        public class ReminderModel
        {
            
            public string Id { get; set; }

            public ulong UserId { get; set; }

            public ulong GuildId { get; set; }        

            public string Message { get; set; }

            public ulong ChannelId { get; set; }

            public DateTimeOffset ExpiresOn { get; set; }

            public DateTimeOffset CreatedOn { get; set; }        
        }

    }
}
