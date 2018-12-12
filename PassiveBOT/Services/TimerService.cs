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
            var senderIds = ShardedClient.Guilds.Select(x => x.Id).Where(
                x =>
                    {
                        var inf = PartnerService.GetPartnerInfo(x);
                        return VerifyPartnerAsync(inf).Result;
                    }).OrderByDescending(x => rnd.Next()).ToList();

            Console.WriteLine("Beginning partner run");
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

                    if (receiverConfig.Settings.Banned || !receiverConfig.Settings.Enabled)
                    {
                        senderIds.Remove(receiverGuild.Id);
                        continue;
                    }

                    LogHandler.LogMessage($"Running Partner for {receiverGuild.Id}", LogSeverity.Verbose);

                    PartnerService.PartnerInfo messageGuildModel = null;

                    // Find a channel to send the message from!
                    foreach (var id in senderIds)
                    {
                        if (id == receiverGuild.Id)
                        {
                            continue;
                        }

                        messageGuildModel = PartnerService.GetPartnerInfo(id);
                    }

                    if (messageGuildModel == null)
                    {
                        return Task.CompletedTask;
                    }

                    senderIds.Remove(messageGuildModel.GuildId);

                    SendPartnerMessage(messageGuildModel, receiverConfig);

                    LogHandler.LogMessage($"Matched Partner for {receiverGuild.Id} => Guild [{messageGuildModel.GuildId}]", LogSeverity.Verbose);
                }
                catch (Exception e)
                {
                    LogHandler.LogMessage($"Partner Event Error for Guild: [{receiverGuild.Id}]\n" + $"{e}", LogSeverity.Error);
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            return Task.CompletedTask;
        }

        public async Task<bool> VerifyPartnerAsync(PartnerService.PartnerInfo partnerInfo = null)
        {
            Console.WriteLine($"Verifying partner {partnerInfo?.GuildId}");
            if (partnerInfo?.Settings == null || partnerInfo.Message == null)
            {
                return false;
            }

            if (!partnerInfo.Settings.Enabled || partnerInfo.Settings.Banned)
            {
                return false;
            }

            try
            {
                SocketGuild guild = ShardedClient.GetGuild(partnerInfo.GuildId);
                SocketTextChannel channel = guild?.GetTextChannel(partnerInfo.Settings.ChannelId);
                if (channel == null)
                {
                    return false;
                }

                if (channel.IsNsfw)
                {                
                    try
                    {
                        await channel.SendMessageAsync(string.Empty, false, new EmbedBuilder { Description = "NOTICE:\nPartner channels must not be marked as NSFW" }.Build());
                    }
                    catch
                    {
                        // Ignored
                    }

                    return false;
                }

                await guild.DownloadUsersAsync();
                if (((decimal)channel.Users.Count(x => !x.IsBot && !x.IsWebhook) / guild.Users.Count(x => !x.IsBot && !x.IsWebhook) * 100) < 90)
                {
                    try
                    {
                        await channel.SendMessageAsync(string.Empty, false, new EmbedBuilder { Description = "NOTICE:\nA minimum of 90% of this guilds users (not including bots/webhooks) must be able to view this channel in order to use the partner program" }.Build());
                    }
                    catch
                    {
                        // Ignored
                    }

                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }

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
        public async void SendPartnerMessage(PartnerService.PartnerInfo messageGuildModel, PartnerService.PartnerInfo receiverConfig)
        {
            var messageGuild = ShardedClient.GetGuild(messageGuildModel.GuildId);
            var messageChannel = messageGuild?.GetTextChannel(messageGuildModel.Settings.ChannelId);
            var receiverChannel = ShardedClient.GetGuild(receiverConfig.GuildId)?.GetTextChannel(receiverConfig.Settings.ChannelId);
            if (messageGuild == null || messageChannel == null || receiverChannel == null)
            {
                return;
            }

            var partnerMessage = partnerHelper.GenerateMessage(messageGuildModel, messageChannel.Guild).Build();

            try
            {
                await receiverChannel.SendMessageAsync(string.Empty, false, partnerMessage);
                messageGuildModel.Stats.ServersReached++;
                messageGuildModel.Stats.UsersReached += receiverChannel.Guild.MemberCount;
                messageGuildModel.Save();
            }
            catch (Exception e)
            {
                LogHandler.LogMessage("AUTOBAN: Partner Message Send Error in:\n" + $"S:{receiverChannel.Guild.Name} [{receiverChannel.Guild.Id}] : C:{receiverChannel.Name}\n" + $"{e}", LogSeverity.Error);

                await partnerHelper.PartnerLogAsync(ShardedClient, receiverConfig, new EmbedFieldBuilder { Name = "Partner Server Auto Banned", Value = "Unable to send message to channel\n" + $"S:{receiverChannel.Guild.Name} [{receiverChannel.Guild.Id}] : C:{receiverChannel.Name}" });
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