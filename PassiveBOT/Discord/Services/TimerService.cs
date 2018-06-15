namespace PassiveBOT.Discord.Services
{
    using System;
    using System.Collections.Generic;
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
            var guildModels = Provider.GetRequiredService<DatabaseHandler>().Query<GuildModel>().Where(x =>

                    // Ensure the partner is not banned
                   !x.Partner.Settings.Banned &&

                   // Ensure they have enabled the partner service and set a channel
                   x.Partner.Settings.Enabled && x.Partner.Settings.ChannelID != 0 &&

                   // Ensure they have set a message
                   x.Partner.Message.Content != null &&

                   // Ensure the channel actually exists 
                   ShardedClient.GetChannel(x.Partner.Settings.ChannelID) != null)
                .Select(x => new KeyValuePair<GuildModel, SocketTextChannel>(x, ShardedClient.GetChannel(x.Partner.Settings.ChannelID) as SocketTextChannel))
                .ToList();

            // Randomize the guilds so that repeats each time are minimal.
            var reduces_GuildList = guildModels.OrderByDescending(x => Provider.GetRequiredService<Random>().Next()).ToList();
            foreach (var pair in guildModels)
            {
                try
                {
                    var receiverConfig = pair.Key;
                    var receiverGuild = ShardedClient.GetGuild(receiverConfig.ID);
                    var textChannel = pair.Value;

                    var messageConfig = reduces_GuildList.First(x => x.Key.ID != receiverGuild.Id);
                    var messageGuild = messageConfig.Key;
                    var messageChannel = messageConfig.Value;

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
                        var partnerMessage = PartnerHelper.GenerateMessage(messageGuild, messageChannel.Guild).Build();
                        try
                        {
                            await textChannel.SendMessageAsync(string.Empty, false, partnerMessage);
                            messageGuild.Partner.Stats.ServersReached++;
                            messageGuild.Partner.Stats.UsersReached = messageGuild.Partner.Stats.UsersReached + receiverGuild.MemberCount;
                            messageGuild.Save();
                            await Task.Delay(500);
                        }
                        catch (Exception e)
                        {
                            LogHandler.LogMessage("AUTOBAN: Partner Message Send Error in:\n" +
                                                  $"S:{receiverGuild.Name} [{receiverGuild.Id}] : C:{textChannel.Name}\n" +
                                                  $"{e}", LogSeverity.Error);

                            await PartnerHelper.PartnerLog(ShardedClient, receiverConfig, new EmbedFieldBuilder
                                                                                              {
                                                                                                  Name = "Partner Server Auto Banned",
                                                                                                  Value = "Unable to send message to channel\n" + $"S:{receiverGuild.Name} [{receiverGuild.Id}] : C:{textChannel.Name}"
                                                                                              });

                            // Auto-Ban servers which deny permissions to send
                            receiverConfig.Partner.Settings.Banned = true;
                        }
                    }

                    reduces_GuildList.Remove(messageConfig);
                }
                catch (Exception e)
                {
                    LogHandler.LogMessage($"Partner Event Error for Guild: [{pair.Key.ID}]\n" +
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