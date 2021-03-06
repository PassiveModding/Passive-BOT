﻿namespace PassiveBOT.Modules
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Addons.Interactive;
    using Discord.Addons.PrefixService;
    using Discord.Commands;
    using Discord.WebSocket;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json;

    using PassiveBOT.Context;
    using PassiveBOT.Extensions;
    using PassiveBOT.Extensions.PassiveBOT;
    using PassiveBOT.Handlers;
    using PassiveBOT.Models;
    using PassiveBOT.Services;

    using Raven.Client.Documents;

    [Group("Dev")]
    [Summary("Bot Developer ONLY Commands")]
    [RequireContext(ContextType.Guild)]
    [RequireOwner]
    public class Developer : Base
    {
        private readonly PrefixService prefixService;

        /// <summary>
        ///     The timer service.
        /// </summary>
        private readonly TimerService timerService;

        private HttpClient client;

        private readonly TranslateLimitsNew Limits;

        private readonly DBLApiService DblApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="Developer"/> class.
        /// </summary>
        /// <param name="service">
        /// The service.
        /// </param>
        /// <param name="partnerService">
        /// The partner Service.
        /// </param>
        /// <param name="prefix">
        /// The prefix.
        /// </param>
        /// <param name="httpClient">
        /// The http Client.
        /// </param>
        /// <param name="limits">
        /// The limits.
        /// </param>
        public Developer(TimerService service, PartnerService partnerService, PartnerHelper pHelper, IDocumentStore store, PrefixService prefix, HttpClient httpClient, TranslateLimitsNew limits, DBLApiService dblApi)
        {
            timerService = service;
            prefixService = prefix;
            PartnerService = partnerService;
            docStore = store;
            client = httpClient;
            Limits = limits;
            PartnerHelper = pHelper;
            DblApi = dblApi;
        }

        private IDocumentStore docStore { get; set; }

        private PartnerService PartnerService { get; }

        private PartnerHelper PartnerHelper { get; }

        [Command("CreateKeys")]
        public async Task CreateKeysAsync(int keyCount, int characters)
        {
            if (keyCount > 100)
            {
                await SimpleEmbedAsync("Cannot Create more than 100 keys at a time");
                return;
            }

            await InlineReactionReplyAsync(
                new ReactionCallbackData(
                    null,
                    new EmbedBuilder
                        {
                            Description =
                                $"Do you wish to create {keyCount} keys, each with {characters} characters?"
                        }.Build(),
                    true,
                    true,
                    TimeSpan.FromSeconds(30)).WithCallback(
                    new Emoji("✅"),
                    async (c, r) =>
                        {
                            var sb = new StringBuilder();
                            for (var i = 0; i < keyCount; i++)
                            {
                                var token =
                                    $"{GenerateRandomNo()}-{GenerateRandomNo()}-{GenerateRandomNo()}-{GenerateRandomNo()}";
                                if (Limits.Keys.Any(x => x.Key == token))
                                {
                                    continue;
                                }

                                Limits.Keys.Add(
                                    new TranslateLimitsNew.GuildKey { Key = token, ValidFor = characters });
                                sb.AppendLine(token);
                            }

                            Limits.Save();
                            await SimpleEmbedAsync("Complete");
                            await SimpleEmbedAsync($"New Tokens\n```\n{sb.ToString()}\n```");
                            sb.Clear();
                        }).WithCallback(
                    new Emoji("❎"),
                    (c, r) => SimpleEmbedAsync("Exited Token Task")));
        }

        private readonly Random random = new Random();

        public string GenerateRandomNo()
        {
            return random.Next(0, 9999).ToString("D4");
        }

        [Command("GetIP")]
        [Summary("Displays ip based on http client")]
        public async Task SetProxyAsync()
        {
            var str = await client.GetStringAsync("http://api.ipify.org/");
            await SimpleEmbedAsync(str);
        }

        /// <summary>
        ///     Displays command usage stats
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("CommandStats")]
        [Summary("Displays command usage stats")]
        public Task CommandStatsAsync()
        {
            var model = StatModel.Load();
            var ordered = model.CommandStats.OrderByDescending(x => x.CommandUses).ToList();
            var pages = ordered.SplitList(20).Select(x => new PaginatedMessage.Page { Description = string.Join("\n", x.Select(cmd => $"`{cmd.CommandName}` - Uses: {cmd.CommandUses} | Errors: {cmd.ErrorCount} | Users: {cmd.CommandUsers.Count} | Guilds: {cmd.CommandGuilds.Count}")) }).ToList();
            var pager = new PaginatedMessage { Title = "Command Usage", Pages = pages };
            return PagedReplyAsync(pager, new ReactionList { Backward = true, First = true, Forward = true, Info = true, Last = true, Trash = true });
        }

        /// <summary>
        ///     Downloads the stored json config of the guild
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("DownloadConfig")]
        [Summary("Downloads the config file of the guild")]
        public async Task DBDownloadAsync()
        {
            var database = Context.Server;
            var serialized = JsonConvert.SerializeObject(database, Formatting.Indented);

            var uniEncoding = new UnicodeEncoding();
            using (Stream ms = new MemoryStream())
            {
                var sw = new StreamWriter(ms, uniEncoding);
                try
                {
                    await sw.WriteAsync(serialized).ConfigureAwait(false);
                    await sw.FlushAsync().ConfigureAwait(false);
                    ms.Seek(0, SeekOrigin.Begin);

                    // You can send files from a stream in discord too, This allows us to avoid having to read and write directly from a file for this command.
                    await Context.Channel.SendFileAsync(ms, $"{Context.Guild.Name}[{Context.Guild.Id}] BotConfig.json");
                }
                finally
                {
                    sw.Dispose();
                }
            }
        }

        /// <summary>
        ///     gets an invite to the specified server
        /// </summary>
        /// <param name="guildID">
        ///     The guild id.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("GetInvite")]
        [Summary("Gets an invite to the specified server")]
        public async Task GetInviteAsync(ulong guildID)
        {
            string invite = null;
            var target = Context.Client.GetGuild(guildID);
            if (target == null)
            {
                throw new Exception("Server is unavailable");
            }

            foreach (var inv in await target.GetInvitesAsync())
            {
                if (inv.IsRevoked)
                {
                    continue;
                }

                invite = $"{inv.Url} Grabbed from channel: {inv.ChannelName}";;
            }

            if (invite == null)
            {
                foreach (var channel in target.TextChannels)
                {
                    try
                    {
                        var inv = await channel.CreateInviteAsync();
                        invite = $"{inv.Url} Generated for channel: {channel.Name}";
                        break;
                    }
                    catch
                    {
                        // Ignored
                    }
                }
            }

            if (invite == null)
            {
                throw new Exception("Invite unable to be created");
            }

            await ReplyAsync(invite);
        }

        /// <summary>
        ///     The partner_ restart.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("Partner_Restart")]
        [Summary("Restart the partner service")]
        public Task Partner_RestartAsync()
        {
            timerService.Restart();
            return ReplyAsync("Timer (re)started.");
        }

        /// <summary>
        ///     The partner_ stop.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("Partner_Stop")]
        [Summary("Stop the partner service")]
        public Task Partner_StopAsync()
        {
            timerService.Stop();
            return ReplyAsync("Timer stopped.");
        }

        /// <summary>
        ///     The partner_ trigger.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("Partner_Trigger", RunMode = RunMode.Async)]
        [Summary("Trigger the partner service")]
        public Task Partner_TriggerAsync()
        {
            return timerService.PartnerAsync();
        }

        [Command("Birthday_Trigger", RunMode = RunMode.Async)]
        [Summary("Trigger the partner service")]
        public Task Birthday_TriggerAsync()
        {
            return timerService.BirthdayAsync();
        }

        [Command("SetDefaultPrefix")]
        public Task SetDefaultAsync(string prefix)
        {
            prefixService.SetDefaultPrefix(prefix);
            return SimpleEmbedAsync($"Success, prefix has been set to {prefix}");
        }

        /// <summary>
        ///     Set the total amount of shards for the bot
        /// </summary>
        /// <param name="shards">
        ///     The shard count
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("SetShards")]
        [Summary("Set total amount of shards for the bot")]
        public Task SetShardsAsync(int shards)
        {
            // Here we can access the service provider via our custom context.
            var config = Context.Provider.GetRequiredService<ConfigModel>();
            config.Shards = shards;
            Context.Provider.GetRequiredService<DatabaseHandler>().Execute<ConfigModel>(DatabaseHandler.Operation.SAVE, config, "Config");
            return SimpleEmbedAsync(
                $"Shard Count updated to: {shards}\nThis will be effective after a restart.\n" +

                // Note, 2500 Guilds is the max amount per shard, so this should be updated based on around 2000 as if you hit the 2500 limit discord will ban the account associated.
                $"Recommended shard count: {(Context.Client.Guilds.Count / 2000 < 1 ? 1 : Context.Client.Guilds.Count / 2000)}");
        }

        [Command("SetTranslationStoreUrl")]
        [Summary("Set the displayed url for translation store limits")]
        public Task SetTStoreUrlAsync(string url = null)
        {
            // Here we can access the service provider via our custom context.
            var config = Context.Provider.GetRequiredService<ConfigModel>();
            config.TranslateStoreUrl = url;
            Context.Provider.GetRequiredService<DatabaseHandler>().Execute<ConfigModel>(DatabaseHandler.Operation.SAVE, config, "Config");
            return SimpleEmbedAsync(
                $"Store URL is now: {url}");
        }

        [Command("TranslationCharacterSupply")]
        [Summary("Displays how many total characters have been translated and how many is the limit")]
        public Task TranslationCharacterSupplyAsync()
        {
            var totalTranslated = Limits.Guilds.Sum(x => x.Value.TotalCharacters);
            var totalLimit = Limits.Guilds.Sum(x => x.Value.MaxCharacters());

            return SimpleEmbedAsync($"Total Translated: {totalTranslated}\n" + 
                                    $"Total Limit: {totalLimit}\n");
        }

        [Command("TranslateRankings")]
        [Summary("Displays guild translation information")]
        public Task TranslateRankingsAsync()
        {
            var pages = new List<PaginatedMessage.Page>();

            foreach (var group in Limits.Guilds.OrderByDescending(x => x.Value.TotalCharacters).ToList().SplitList(25))
            {
                pages.Add(new PaginatedMessage.Page
                              {
                                  Title = "Translate Rankings",
                                  Description = $"{string.Join("\n", group.Select(x => $"{Context.Client.GetGuild(x.Key)?.Name ?? x.Key.ToString()} => T: {x.Value.TotalCharacters} L: {x.Value.MaxCharacters()}"))}"
                              });
            }

            return PagedReplyAsync(new PaginatedMessage { Pages = pages }, new ReactionList { Forward = true, Backward = true, First = true, Last = true });
        }
        
        [Command("SetDBLVoteUrl")]
        [Summary("Set the discord bots list vote url")]
        public Task SetDBLVoteUrlAsync(string url = null)
        {
            // Here we can access the service provider via our custom context.
            var config = Context.Provider.GetRequiredService<ConfigModel>();
            config.DiscordBotListVoteUrl = url;
            Context.Provider.GetRequiredService<DatabaseHandler>().Execute<ConfigModel>(DatabaseHandler.Operation.SAVE, config, "Config");
            return SimpleEmbedAsync(
                $"Vote URL is now: {url}");
        }
        
        [Command("SetDBLToken")]
        [Summary("Set the discordBotsList api token")]
        public async Task SetDBLTokenAsync(string token = null)
        {
            // Here we can access the service provider via our custom context.
            var config = Context.Provider.GetRequiredService<ConfigModel>();
            config.DiscordBotsListApi = token;
            Context.Provider.GetRequiredService<DatabaseHandler>().Execute<ConfigModel>(DatabaseHandler.Operation.SAVE, config, "Config");
            await Context.Message.DeleteAsync();
            await SimpleEmbedAsync("Token is set!");
            await DblApi.InitializeAsync();
        }

        [Command("UpdateDBLGuildCount")]
        [Summary("Manually updates the discord bots list guild counter")]
        [RequireContext(ContextType.Guild)]
        public async Task DBLGuildCountAsync()
        {
            if (DblApi.Initialized)
            {
                await DblApi.DBLApi.UpdateStats(Context.Client.GetShardIdFor(Context.Guild), Context.Client.Shards.Count, Context.Client.Shards.Select(x => x.Guilds.Count).ToArray());
                await ReplyAsync("Complete");
                return;
            }

            await ReplyAsync("DBLApi is not initialized");
        }

        /// <summary>
        ///     The stats.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("ServerStats")] // The Main Command Name
        [Summary("Bot Statistics Command")] // A summary of what the command does
        [Remarks("Can only be run within a server")] // Extra notes on the command
        public Task StatsAsync()
        {
            var embed = new EmbedBuilder { Color = Color.Blue };
            embed.AddField("Server Name", Context.Guild.Name);
            embed.AddField("Server Owner", $"Name: {Context.Guild.Owner}\n" + $"ID: {Context.Guild.OwnerId}");
            embed.AddField("Users", $"user Count: {Context.Guild.MemberCount}\n" + $"Cached user Count: {Context.Guild.Users.Count}\n" + $"Cached Bots Count: {Context.Guild.Users.Count(x => x.IsBot)}");
            embed.AddField("Counts", $"Channels: {Context.Guild.TextChannels.Count + Context.Guild.VoiceChannels.Count}\n" + $"Text Channels: {Context.Guild.TextChannels.Count}\n" + $"Voice Channels: {Context.Guild.VoiceChannels.Count}\n" + $"Categories: {Context.Guild.CategoryChannels.Count}");
            return ReplyAsync(string.Empty, false, embed.Build());
        }

        /// <summary>
        ///     toggle command logging.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("ToggleCommandLog")]
        [Summary("Toggle the logging of all user messages to console")]
        public Task ToggleCommandLogAsync()
        {
            var config = Context.Provider.GetRequiredService<ConfigModel>();
            config.LogCommandUsages = !config.LogCommandUsages;
            Context.Provider.GetRequiredService<DatabaseHandler>().Execute<ConfigModel>(DatabaseHandler.Operation.SAVE, config, "Config");
            return SimpleEmbedAsync($"Log Command Usages: {config.LogCommandUsages}");
        }

        /// <summary>
        ///     toggle message logging.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("ToggleMessageLog")]
        [Summary("Toggle the logging of all user messages to console")]
        public Task ToggleMessageLogAsync()
        {
            var config = Context.Provider.GetRequiredService<ConfigModel>();
            config.LogUserMessages = !config.LogUserMessages;
            Context.Provider.GetRequiredService<DatabaseHandler>().Execute<ConfigModel>(DatabaseHandler.Operation.SAVE, config, "Config");
            return SimpleEmbedAsync($"Log user Messages: {config.LogUserMessages}");
        }

        [Command("BanPartner")]
        public Task BanPartnerAsync(ulong guildId)
        {
            PartnerService.PartnerInfo match = PartnerService.GetPartnerInfo(guildId, true);
            if (match == null)
            {
                return SimpleEmbedAsync("Partner not found");
            }

            match.Settings.Banned = true;
            match.Settings.Enabled = false;
            PartnerService.OverWrite(match);
            return SimpleEmbedAsync("Partner has been banned");
        }

        /// <summary>
        ///     toggle command logging.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("UnbanAllPartners", RunMode = RunMode.Async)]
        [Summary("Unbans all banned partner servers")]
        public async Task UnbanAllAsync()
        {
            var msg = await SimpleEmbedAsync("Unbanning servers");

            using (var session = docStore.OpenSession())
            {
                try
                {
                    var pInfos = session.Query<PartnerService.PartnerInfo>();
                    int i = 0;
                    foreach (var guild in pInfos)
                    {
                        i++;
                        if (guild.Settings.Banned)
                        {
                            guild.Settings.Banned = false;
                            guild.Settings.Enabled = true;
                        }

                        session.Store(guild);
                    }

                    session.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            await SimpleEmbedAsync("Success, servers have been unbanned");
        }

        [Group("Inspections")]
        public class Inspections : Base
        {
            private readonly PrefixService prefixService;

            /// <summary>
            ///     The timer service.
            /// </summary>
            private readonly TimerService timerService;
            
            private readonly TranslateLimitsNew Limits;

            /// <summary>
            /// Initializes a new instance of the <see cref="Inspections"/> class. 
            /// </summary>
            /// <param name="service">
            /// The service.
            /// </param>
            /// <param name="partnerService">
            /// The partner Service.
            /// </param>
            /// <param name="prefix">
            /// The prefix.
            /// </param>
            /// <param name="limits">
            /// The limits.
            /// </param>
            public Inspections(TimerService service, PartnerService partnerService, PrefixService prefix, TranslateLimitsNew limits)
            {
                timerService = service;
                prefixService = prefix;
                PartnerService = partnerService;
                Limits = limits;
            }

            private PartnerService PartnerService { get; }

            [RequireContext(ContextType.Guild)]
            [Command("TranslateLimitInfo")]
            [Summary("Gets translation limit info for the specified user")]
            public async Task GetTranslateInfoAsync(ulong guildId)
            {
                Limits.Guilds.TryGetValue(guildId, out var tGuild);
                if (tGuild != null)
                {
                    await SimpleEmbedAsync($"Max: {tGuild.MaxCharacters()}" + 
                                           $"Total: {tGuild.TotalCharacters}\n" + 
                                           $"Guild ID: {tGuild.GuildId}\n" + 
                                           $"Upgrades: \n{string.Join("\n", tGuild.Upgrades.Select(x => $"{x.Key} | {x.ValidFor}"))}");
                }
                else
                {
                    await SimpleEmbedAsync("Guild not found");
                }
            }

            public enum TranslateSelection
            {
                Guild,
                User
            }

            public enum TranslateRank
            {
                AllTime,
                Daily
            }
        }

        [Group("Tests")]
        public class Tests : Base
        {
            public Tests(PartnerService pService, PartnerHelper pHelper)
            {
                _Partner = pService;
                _PartnerHelp = pHelper;
            }

            private PartnerService _Partner { get; }

            private PartnerHelper _PartnerHelp { get; }

            /// <summary>
            ///     Trigger the goodbye event with the specified user
            /// </summary>
            /// <param name="user">
            ///     The user.
            /// </param>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            [Command("Goodbye_Event")]
            [Summary("Trigger a Goodbye event in the current server")]
            public Task GoodbyeEventAsync(SocketGuildUser user)
            {
                return Events.UserLeftAsync(Context.Server, user);
            }

            /// <summary>
            ///     Sends a custom message that performs a specific action upon reacting
            /// </summary>
            /// <param name="expires">True = Expires after first use</param>
            /// <param name="singleuse">True = Only one use per user</param>
            /// <param name="singleuser">True = Only the command invoker can use</param>
            /// <returns>Something or something</returns>
            [Command("embedreaction")]
            [Summary("Sends a custom message that performs a specific action upon reacting")]
            public Task Test_EmbedReactionReplyAsync(bool expires, bool singleuse, bool singleuser)
            {
                var one = new Emoji("1⃣");
                var two = new Emoji("2⃣");

                var embed = new EmbedBuilder().WithTitle("Choose one").AddField(one.Name, "Beer", true).AddField(two.Name, "Drink", true).Build();

                // This message does not expire after a single
                // it will not allow a user to react more than once
                // it allows more than one user to react
                return InlineReactionReplyAsync(
                    new ReactionCallbackData("text", embed, expires, singleuse).WithCallback(
                        one,
                        (c, r) =>
                            {
                                // You can do additional things with your reaction here, NOTE: c references this commands context whereas r references our added reaction.
                                // This is important to note because context.user can be a different user to reaction.user
                                var reactor = r.User.Value;
                                return c.Channel.SendMessageAsync($"{reactor.Mention} Here you go :beer:");
                            }).WithCallback(two, (c, r) => c.Channel.SendMessageAsync($"{r.User.Value.Mention} Here you go :tropical_drink:")),
                    singleuser);
            }

            /// <summary>
            ///     Trigger the Welcome event with the specified user
            /// </summary>
            /// <param name="user">
            ///     The user.
            /// </param>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            [Command("Welcome_Event")]
            [Summary("Trigger a welcome event in the current server")]
            public Task WelcomeEventAsync(SocketGuildUser user)
            {
                return Events.UserJoinedAsync(Context.Server, user);
            }

            [Command("GeneratePartner")]
            [Summary("Generate a partner message")]
            public Task GenerateAsync()
            {
                var msg = _PartnerHelp.GenerateMessage(_Partner.GetPartnerInfo(Context.Guild.Id), Context.Guild);
                return ReplyAsync(msg);
            }
        }

        /// <summary>
        ///     The home.
        /// </summary>
        [Group("Home")]
        public class Home : Base
        {
            private HomeService _Home { get; }

            public Home(HomeService home)
            {
                _Home = home;
            }

            /// <summary>
            ///     The blacklist user.
            /// </summary>
            /// <param name="guildId">
            ///     The guild Id.
            /// </param>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            [Command("BlacklistGuild")]
            public async Task BlacklistGuildAsync(ulong guildId)
            {
                var home = _Home.CurrentHomeModel;
                if (home.Blacklist.BlacklistedGuilds.Contains(guildId))
                {
                    home.Blacklist.BlacklistedGuilds.Remove(guildId);
                    await SimpleEmbedAsync("Guild has been removed from the blacklist");
                }
                else
                {
                    home.Blacklist.BlacklistedGuilds.Add(guildId);
                    await SimpleEmbedAsync("Guild has been added to the blacklist");
                }

                home.Save();
            }

            /// <summary>
            ///     The partner channel.
            /// </summary>
            /// <param name="user">
            ///     The user.
            /// </param>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            [Command("BlacklistUser")]
            public Task BlacklistUserAsync(SocketUser user)
            {
                return BlacklistUserAsync(user.Id);
            }

            /// <summary>
            ///     The blacklist user.
            /// </summary>
            /// <param name="userId">
            ///     The user id.
            /// </param>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            [Command("BlacklistUser")]
            public async Task BlacklistUserAsync(ulong userId)
            {
                var home = _Home.CurrentHomeModel;
                if (home.Blacklist.BlacklistedUsers.Contains(userId))
                {
                    home.Blacklist.BlacklistedUsers.Remove(userId);
                    await SimpleEmbedAsync("User has been removed from the blacklist");
                }
                else
                {
                    home.Blacklist.BlacklistedUsers.Add(userId);
                    await SimpleEmbedAsync("User has been added to the blacklist");
                }

                home.Save();
            }

            /// <summary>
            ///     The log partners toggle.
            /// </summary>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            [Command("LogPartners")]
            public Task LogPartnersToggleAsync()
            {
                var home = _Home.CurrentHomeModel;
                home.Logging.LogPartnerChanges = !home.Logging.LogPartnerChanges;
                home.Save();
                return SimpleEmbedAsync($"Log Partner Changes: {home.Logging.LogPartnerChanges}");
            }

            /// <summary>
            ///     The partner channel.
            /// </summary>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            [Command("PartnerLogChannel")]
            public Task PartnerChannelAsync()
            {
                var home = _Home.CurrentHomeModel;
                home.Logging.PartnerLogChannel = Context.Channel.Id;
                home.Save();
                return SimpleEmbedAsync("Partner changes will be sent to the current channel.");
            }

            /// <summary>
            ///     The set bot moderator.
            /// </summary>
            /// <param name="role">
            ///     The role.
            /// </param>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            [Command("BotModerator")]
            public Task SetBotModeratorAsync(SocketRole role = null)
            {
                var home = _Home.CurrentHomeModel;
                home.BotModerator = role?.Id ?? 0;
                home.Save();
                return SimpleEmbedAsync("Bot Mod Set/Reset.");
            }

            /// <summary>
            ///     The set bot moderator.
            /// </summary>
            /// <param name="invite">
            ///     The invite.
            /// </param>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            [Command("Invite")]
            public Task SetBotModeratorAsync(string invite)
            {
                var home = _Home.CurrentHomeModel;
                home.HomeInvite = invite;
                home.Save();
                return SimpleEmbedAsync($"Invite set to: {invite}");
            }

            /// <summary>
            ///     The set guild.
            /// </summary>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            [Command("Guild")]
            public Task SetGuildAsync()
            {
                var home = _Home.CurrentHomeModel;
                home.GuildId = Context.Guild.Id;
                home.Save();
                return SimpleEmbedAsync($"Home Guild Set to {Context.Guild.Id}");
            }
        }
    }
}