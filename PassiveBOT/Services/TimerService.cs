namespace PassiveBOT.Services
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Discord;
    using Discord.WebSocket;

    using Microsoft.Extensions.DependencyInjection;

    using PassiveBOT.Extensions.PassiveBOT;
    using PassiveBOT.Handlers;

    /// <summary>
    ///     The timer service.
    /// </summary>
    public class TimerService
    {
        /// <summary>
        ///     The _timer.
        /// </summary>
        private readonly Timer timer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimerService" /> class.
        /// </summary>
        /// <param name="client">
        ///     The client.
        /// </param>
        /// <param name="provider">
        ///     The provider.
        /// </param>
        public TimerService(DiscordShardedClient client, PartnerService partnerService, IServiceProvider provider)
        {
            PartnerService = partnerService;
            Provider = provider;
            ShardedClient = client;
            timer = new Timer(
                async _ =>
                    {
                        try
                        {
                            await PartnerAsync();
                        }
                        catch (Exception e)
                        {
                            LogHandler.LogMessage("Partner Error:\n" + $"{e}", LogSeverity.Error);
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
                null,
                TimeSpan.Zero,
                TimeSpan.FromMinutes(FirePeriod));
        }

        /// <summary>
        ///     Gets or sets the fire period.
        /// </summary>
        public static int FirePeriod { get; set; } = 30;

        /// <summary>
        ///     Gets or sets the last fire time.
        /// </summary>
        public static DateTime LastFireTime { get; set; } = DateTime.MinValue;

        /// <summary>
        ///     Gets or sets the partner stats.
        /// </summary>
        public PartnerStatistics PartnerStats { get; set; } = new PartnerStatistics();

        /// <summary>
        ///     Gets the provider.
        /// </summary>
        public IServiceProvider Provider { get; }

        /// <summary>
        ///     Gets or sets the sharded client.
        /// </summary>
        public DiscordShardedClient ShardedClient { get; set; }

        private PartnerService PartnerService { get; }

        /// <summary>
        ///     Changes the rate at which the timer fires
        /// </summary>
        /// <param name="newPeriod">
        ///     The newPeriod.
        /// </param>
        public void ChangeRate(int newPeriod = 60)
        {
            FirePeriod = newPeriod;
            timer.Change(TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(FirePeriod));
        }

        /// <summary>
        ///     The partner message event
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        public Task PartnerAsync()
        {
            var senderIds = ShardedClient.Guilds.Select(x => x.Id).ToList();
            var handler = Provider.GetRequiredService<DatabaseHandler>();
            PartnerStats.UpdatePartneredGuilds = 0;
            PartnerStats.UpdateReachableMembers = 0;
            foreach (var receiverGuild in ShardedClient.Guilds)
            {
                try
                {
                    var receiverConfig = PartnerService.GetPartnerInfo(receiverGuild.Id);
                    if (receiverConfig == null || receiverConfig.Settings.Banned || !receiverConfig.Settings.Enabled || string.IsNullOrWhiteSpace(receiverConfig.Message.Content) || !(ShardedClient.GetChannel(receiverConfig.Settings.ChannelId) is SocketTextChannel receiverChannel))
                    {
                        senderIds.Remove(receiverGuild.Id);
                        continue;
                    }

                    LogHandler.LogMessage($"Running Partner for {receiverGuild.Id}", LogSeverity.Verbose);

                    PartnerService.PartnerInfo messageGuildModel = null;
                    SocketTextChannel messageChannel = null;
                    foreach (var id in senderIds.OrderByDescending(x => Provider.GetRequiredService<Random>().Next()).ToList())
                    {
                        if (id == receiverGuild.Id)
                        {
                            continue;
                        }

                        var model = PartnerService.GetPartnerInfo(id);
                        if (model == null || model.Settings.Banned || !model.Settings.Enabled || string.IsNullOrWhiteSpace(model.Message.Content) || !(ShardedClient.GetChannel(model.Settings.ChannelId) is SocketTextChannel mChannel))
                        {
                            senderIds.Remove(id);
                            continue;
                        }

                        senderIds.Remove(id);
                        messageGuildModel = model;
                        messageChannel = mChannel;
                        break;
                    }

                    if (messageGuildModel == null)
                    {
                        return Task.CompletedTask;
                    }

                    SendPartnerMessage(messageChannel, messageGuildModel, receiverChannel, receiverGuild, receiverConfig);

                    LogHandler.LogMessage($"Matched Partner for {receiverGuild.Id} => Guild [{messageGuildModel.GuildId}]", LogSeverity.Verbose);
                }
                catch (Exception e)
                {
                    LogHandler.LogMessage($"Partner Event Error for Guild: [{receiverGuild.Id}]\n" + $"{e}", LogSeverity.Error);
                }
            }

            PartnerStats.PartneredGuilds = PartnerStats.UpdatePartneredGuilds;
            PartnerStats.ReachableMembers = PartnerStats.UpdateReachableMembers;
            LogHandler.LogMessage($"Partner Event Completed: {PartnerStats.PartneredGuilds} Guild {PartnerStats.ReachableMembers} Members");
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Restarts the timer
        /// </summary>
        public void Restart()
        {
            timer.Change(TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(FirePeriod));
        }

        /// <summary>
        ///     The send partner message.
        /// </summary>
        /// <param name="messageChannel">
        ///     The message channel.
        /// </param>
        /// <param name="messageGuildModel">
        ///     The message guild model.
        /// </param>
        /// <param name="receiverChannel">
        ///     The receiver channel.
        /// </param>
        /// <param name="receiverGuild">
        ///     The receiver guild.
        /// </param>
        /// <param name="receiverConfig">
        ///     The receiver config.
        /// </param>
        public async void SendPartnerMessage(SocketTextChannel messageChannel, PartnerService.PartnerInfo messageGuildModel, SocketTextChannel receiverChannel, SocketGuild receiverGuild, PartnerService.PartnerInfo receiverConfig)
        {
            if ((decimal)messageChannel.Users.Count / messageChannel.Guild.Users.Count * 100 < 90)
            {
                await messageChannel.SendMessageAsync(string.Empty, false, new EmbedBuilder { Description = "NOTICE:\n" + "This server's partner message was not shared to any other guilds because this channel's visibility is less than 90%\n" + "Please change the role settings of this channel to ensure all roles have the `read messages` permission" }.Build());
                receiverConfig.Settings.Enabled = false;
                receiverConfig.Save();
                return;
            }

            if (receiverChannel.IsNsfw)
            {
                await messageChannel.SendMessageAsync(string.Empty, false, new EmbedBuilder { Description = "NOTICE:\n" + "Partner channels must not be marked as NSFW" }.Build());
                receiverConfig.Settings.Enabled = false;
                receiverConfig.Save();
                return;
            }

            var partnerMessage = PartnerHelper.GenerateMessage(messageGuildModel, messageChannel.Guild).Build();

            try
            {
                await receiverChannel.SendMessageAsync(string.Empty, false, partnerMessage);
                messageGuildModel.Stats.ServersReached++;
                messageGuildModel.Stats.UsersReached += receiverGuild.MemberCount;
                messageGuildModel.Save();
                PartnerStats.UpdateReachableMembers += receiverChannel.Users.Count;
                PartnerStats.UpdatePartneredGuilds++;
            }
            catch (Exception e)
            {
                LogHandler.LogMessage("AUTOBAN: Partner Message Send Error in:\n" + $"S:{receiverGuild.Name} [{receiverGuild.Id}] : C:{receiverChannel.Name}\n" + $"{e}", LogSeverity.Error);

                await PartnerHelper.PartnerLogAsync(ShardedClient, receiverConfig, new EmbedFieldBuilder { Name = "Partner Server Auto Banned", Value = "Unable to send message to channel\n" + $"S:{receiverGuild.Name} [{receiverGuild.Id}] : C:{receiverChannel.Name}" });

                // Auto-Ban servers which deny permissions to send
                receiverConfig.Settings.Banned = true;
                receiverConfig.Settings.Enabled = false;
                receiverConfig.Save();
            }
        }

        /*
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

            PartnerStats.PartneredGuilds = guildModels.Count;
            PartnerStats.ReachableMembers = guildModels.Sum(x => x.Value.Users.Count);

            // Randomize the guilds so that repeats each time are minimal.
            var reduced_GuildList = guildModels.Select(x => x.Key.ID).OrderByDescending(x => Provider.GetRequiredService<Random>().Next()).ToList();
            foreach (var pair in guildModels)
            {
                try
                {
                    var receiverConfig = pair.Key;
                    var receiverGuild = ShardedClient.GetGuild(receiverConfig.ID);
                    var textChannel = pair.Value;

                    var messageConfig = guildModels.First(g => g.Key.ID == reduced_GuildList.First(x => x != receiverGuild.Id));
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
                            messageGuild.Partner.Stats.UsersReached += receiverGuild.MemberCount;
                            messageGuild.Save();
                            await Task.Delay(500);
                        }
                        catch (Exception e)
                        {
                            LogHandler.LogMessage("AUTOBAN: Partner Message Send Error in:\n" +
                                                  $"S:{receiverGuild.Name} [{receiverGuild.Id}] : C:{textChannel.Name}\n" +
                                                  $"{e}", LogSeverity.Error);

                            await PartnerHelper.PartnerLog(ShardedClient,
                                                           receiverConfig,
                                                           new EmbedFieldBuilder
                                                           {
                                                               Name = "Partner Server Auto Banned",
                                                               Value = "Unable to send message to channel\n" + $"S:{receiverGuild.Name} [{receiverGuild.Id}] : C:{textChannel.Name}"
                                                           });

                            // Auto-Ban servers which deny permissions to send
                            receiverConfig.Partner.Settings.Banned = true;
                            receiverConfig.Partner.Settings.Enabled = false;
                            receiverConfig.Save();
                        }
                    }

                    reduced_GuildList.Remove(messageConfig.Key.ID);
                }
                catch (Exception e)
                {
                    LogHandler.LogMessage($"Partner Event Error for Guild: [{pair.Key.ID}]\n" +
                                          $"{e}", LogSeverity.Error);
                }
            }
        }*/

        /// <summary>
        ///     Stops the timer
        /// </summary>
        public void Stop()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        ///     The partner statistics.
        /// </summary>
        public class PartnerStatistics
        {
            /// <summary>
            ///     Gets or sets the partnered guilds count
            /// </summary>
            public int PartneredGuilds { get; set; }

            /// <summary>
            ///     Gets or sets the reachable members count
            /// </summary>
            public int ReachableMembers { get; set; }

            /// <summary>
            ///     Gets or sets the up-datable partnered guilds count
            /// </summary>
            public int UpdatePartneredGuilds { get; set; }

            /// <summary>
            ///     Gets or sets the up-datable reachable members count
            /// </summary>
            public int UpdateReachableMembers { get; set; }
        }
    }
}