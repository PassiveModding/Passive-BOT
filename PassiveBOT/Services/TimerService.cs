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

        private readonly PartnerHelper partnerHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerService"/> class.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="partnerService">
        /// The partner Service.
        /// </param>
        /// <param name="pHelper">
        /// The p Helper.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        public TimerService(DiscordShardedClient client, PartnerService partnerService, PartnerHelper pHelper, IServiceProvider provider)
        {
            partnerHelper = pHelper;
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

                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                        GC.WaitForPendingFinalizers();

                        LastFireTime = DateTime.UtcNow;
                    },
                null,
                TimeSpan.Zero,
                TimeSpan.FromMinutes(FirePeriod));
        }

        /// <summary>
        ///     Gets or sets the fire period.
        /// </summary>
        public static int FirePeriod { get; set; } = 60;

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
            var rnd = Provider.GetRequiredService<Random>();
            var senderIds = ShardedClient.Guilds.Select(x => x.Id).OrderByDescending(x => rnd.Next()).ToList();
            PartnerStats.UpdatePartneredGuilds = 0;
            PartnerStats.UpdateReachableMembers = 0;
            foreach (var receiverGuild in ShardedClient.Guilds)
            {
                try
                {
                    var receiverConfig = PartnerService.GetPartnerInfo(receiverGuild.Id);
                    if (receiverConfig == null)
                    {
                        senderIds.Remove(receiverGuild.Id);
                        continue;
                    }

                    if (receiverConfig.Settings.Banned || !receiverConfig.Settings.Enabled || !(ShardedClient.GetChannel(receiverConfig.Settings.ChannelId) is SocketTextChannel receiverChannel))
                    {
                        senderIds.Remove(receiverGuild.Id);
                        continue;
                    }

                    LogHandler.LogMessage($"Running Partner for {receiverGuild.Id}", LogSeverity.Verbose);

                    PartnerService.PartnerInfo messageGuildModel = null;
                    SocketTextChannel messageChannel = null;

                    // Find a channel to send the message from!
                    foreach (var id in senderIds)
                    {
                        if (id == receiverGuild.Id)
                        {
                            continue;
                        }

                        var model = PartnerService.GetPartnerInfo(id);
                        if (model == null)
                        {
                            continue;
                        }

                        if (model.Settings.Banned || !model.Settings.Enabled || !(ShardedClient.GetChannel(model.Settings.ChannelId) is SocketTextChannel mChannel))
                        {
                            continue;
                        }
                        
                        messageGuildModel = model;
                        messageChannel = mChannel;
                        break;
                    }

                    if (messageGuildModel == null)
                    {
                        return Task.CompletedTask;
                    }

                    senderIds.Remove(messageGuildModel.GuildId);

                    SendPartnerMessage(messageChannel, messageGuildModel, receiverChannel, receiverGuild, receiverConfig);

                    LogHandler.LogMessage($"Matched Partner for {receiverGuild.Id} => Guild [{messageGuildModel.GuildId}]", LogSeverity.Verbose);
                }
                catch (Exception e)
                {
                    LogHandler.LogMessage($"Partner Event Error for Guild: [{receiverGuild.Id}]\n" + $"{e}", LogSeverity.Error);
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

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
            if ((((decimal)messageChannel.Users.Count / messageChannel.Guild.Users.Count) * 100) < 90)
            {
                try
                {
                    await messageChannel.SendMessageAsync(string.Empty, false, new EmbedBuilder { Description = "NOTICE:\n" + "This server's partner message was not shared to any other guilds because this channel's visibility is less than 90%\n" + "Please change the role settings of this channel to ensure all roles have the `read messages` permission" }.Build());
                }
                catch
                {
                    // Ignored
                }

                receiverConfig.Settings.Enabled = false;
                receiverConfig.Save();
                return;
            }

            if (receiverChannel.IsNsfw)
            {
                try
                {
                    await messageChannel.SendMessageAsync(string.Empty, false, new EmbedBuilder { Description = "NOTICE:\n" + "Partner channels must not be marked as NSFW" }.Build());
                }
                catch
                {
                    // Ignored
                }

                receiverConfig.Settings.Enabled = false;
                receiverConfig.Save();
                return;
            }

            var partnerMessage = partnerHelper.GenerateMessage(messageGuildModel, messageChannel.Guild).Build();

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

                await partnerHelper.PartnerLogAsync(ShardedClient, receiverConfig, new EmbedFieldBuilder { Name = "Partner Server Auto Banned", Value = "Unable to send message to channel\n" + $"S:{receiverGuild.Name} [{receiverGuild.Id}] : C:{receiverChannel.Name}" });

                // Auto-Ban servers which deny permissions to send
                receiverConfig.Settings.Banned = true;
                receiverConfig.Settings.Enabled = false;
                receiverConfig.Save();
            }
        }

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