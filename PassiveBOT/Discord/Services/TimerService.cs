namespace PassiveBOT.Discord.Services
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using global::Discord;
    using global::Discord.WebSocket;

    using Microsoft.Extensions.DependencyInjection;

    using PassiveBOT.Discord.Extensions.PassiveBOT;
    using PassiveBOT.Handlers;
    using PassiveBOT.Models;

    /// <summary>
    /// The timer service.
    /// </summary>
    public class TimerService
    {        
        /// <summary>
        /// The _timer.
        /// </summary>
        private readonly Timer timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerService"/> class.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        public TimerService(DiscordShardedClient client, IServiceProvider provider)
        {
            Provider = provider;
            ShardedClient = client;
            timer = new Timer(async _ =>
                {
                    try
                    {
                        await Partner();
                    }
                    catch (Exception e)
                    {
                        LogHandler.LogMessage("Partner Error:\n" +
                                              $"{e}", LogSeverity.Error);
                    }

                    try
                    {
                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                        GC.WaitForPendingFinalizers();
                    }
                    catch (Exception e)
                    {
                        LogHandler.LogMessage(e.ToString(), LogSeverity.Error);
                    }

                    LastFireTime = DateTime.UtcNow;
                },
                null, TimeSpan.Zero, TimeSpan.FromMinutes(FirePeriod));
        }

        /// <summary>
        /// Gets or sets the last fire time.
        /// </summary>
        public static DateTime LastFireTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets the fire period.
        /// </summary>
        public static int FirePeriod { get; set; } = 30;

        /// <summary>
        /// Gets the provider.
        /// </summary>
        public IServiceProvider Provider { get; }

        /// <summary>
        /// Gets or sets the sharded client.
        /// </summary>
        public DiscordShardedClient ShardedClient { get; set; }

        /// <summary>
        /// The partner.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task Partner()
        {
            var guildModels = Provider.GetRequiredService<DatabaseHandler>().Query<GuildModel>().Where(x => !x.Partner.Settings.Banned &&
                                                                   x.Partner.Settings.Enabled && x.Partner.Settings.ChannelID != 0 &&
                                                                   x.Partner.Message.Content != null &&
                                                                   ShardedClient.GetChannel(x.Partner.Settings.ChannelID) != null &&
                                                                   ShardedClient.Guilds.Any(g => g.Id == x.ID))
                .ToList();
            var reduces_GuildList = guildModels.ToList();
            foreach (var receiverConfig in guildModels)
            {
                try
                {
                    var receiverGuild = ShardedClient.GetGuild(receiverConfig.ID);
                    if (!(ShardedClient.GetChannel(receiverConfig.Partner.Settings.ChannelID) is SocketTextChannel textChannel))
                    {
                        continue;
                    }

                    var messageGuild = reduces_GuildList.OrderByDescending(x => Provider.GetRequiredService<Random>().Next()).FirstOrDefault(x => x.ID != receiverGuild.Id);
                    if (messageGuild == null)
                    {
                        continue;
                    }

                    if (!(ShardedClient.GetChannel(messageGuild.Partner.Settings.ChannelID) is SocketTextChannel messageChannel))
                    {
                        continue;
                    }

                    if ((decimal)messageChannel.Users.Count / messageChannel.Guild.Users.Count * 100 < 90)
                    {
                        await messageChannel.SendMessageAsync(string.Empty, false, new EmbedBuilder
                        {
                            Description = "NOTICE:\n" +
                                          "This server's partner message was not shared to any other guilds because this channel's visibility is less than 90%\n" +
                                          "Please change the role settings of this channel to ensure all roles have the `read messages` permission"
                        }.Build());
                    }
                    else
                    {
                        try
                        {
                            await textChannel.SendMessageAsync(string.Empty, false, PartnerHelper.GenerateMessage(messageGuild, messageChannel.Guild).Build());
                            messageGuild.Partner.Stats.ServersReached++;
                            messageGuild.Partner.Stats.UsersReached = messageGuild.Partner.Stats.UsersReached + receiverGuild.MemberCount;
                            messageGuild.Save();
                            await Task.Delay(500);
                        }
                        catch (Exception e)
                        {
                            LogHandler.LogMessage("Partner Message Send Error in:\n" +
                                                  $"S:{receiverGuild.Name} [{receiverGuild.Id}] : C:{textChannel.Name}\n" +
                                                  $"{e}", LogSeverity.Error);
                        }
                    }

                    reduces_GuildList.Remove(messageGuild);
                }
                catch (Exception e)
                {
                    LogHandler.LogMessage($"Partner Event Error for Guild: [{receiverConfig.ID}]\n" +
                                          $"{e}", LogSeverity.Error);
                }
            }
        }

        /// <summary>
        /// The stop.
        /// </summary>
        public void Stop()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// The restart.
        /// </summary>
        public void Restart()
        {
            timer.Change(TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(FirePeriod));
        }

        /// <summary>
        /// The change rate.
        /// </summary>
        /// <param name="newPeriod">
        /// The newPeriod.
        /// </param>
        public void ChangeRate(int newPeriod = 60)
        {
            FirePeriod = newPeriod;
            timer.Change(TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(FirePeriod));
        }
    }
}