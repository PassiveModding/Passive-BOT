namespace PassiveBOT.Modules.GlobalCommands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Addons.Interactive;
    using Discord.Commands;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json.Linq;

    using PassiveBOT.Context;
    using PassiveBOT.Extensions;
    using PassiveBOT.Models;
    using PassiveBOT.Services;

    /// <summary>
    ///     The info.
    /// </summary>
    public class Info : Base
    {
        /// <summary>
        ///     The timer service.
        /// </summary>
        private readonly TimerService timerService;

        private HomeService _Home { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Info"/> class.
        /// </summary>
        /// <param name="service">
        /// The timer service.
        /// </param>
        /// <param name="home">
        /// The home.
        /// </param>
        public Info(TimerService service, HomeService home)
        {
            timerService = service;
            _Home = home;
        }

        /// <summary>
        ///     Gets users with a particular discriminator
        /// </summary>
        /// <param name="disc">
        ///     The disc.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [RequireContext(ContextType.Guild)]
        [Command("discrim")]
        [Summary("Get all users with a particular discriminator")]
        public async Task DiscrimAsync(ushort disc = 0)
        {
            var userMatches = Context.Guild.Users.Where(x => x.DiscriminatorValue == disc).Select(x => $"{x.Username}#{x.Discriminator}\n");
            var embed = new EmbedBuilder();
            var value = userMatches.ToList();
            if (!value.Any())
            {
                embed.AddField($"Users with Discriminator {disc}", "N/A");
                await ReplyAsync(embed.Build());
                return;
            }

            var pages = new List<string>();
            var desc = string.Empty;
            foreach (var user in value)
            {
                desc += user;
                if (desc.Split('\n').Length < 30)
                {
                    continue;
                }

                pages.Add(desc);
                desc = string.Empty;
            }

            pages.Add(desc);

            var msg = new PaginatedMessage { Title = $"Users with Discriminator #{disc}", Pages = pages.Select(x => new PaginatedMessage.Page { Description = x }), Color = new Color(114, 137, 218) };

            await PagedReplyAsync(msg, new ReactionList { Forward = true, Backward = true, Trash = true });
        }

        /// <summary>
        ///     Bot info and stats
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("Info")]
        [Summary("Bot Info and Stats")]
        public async Task InformationAsync()
        {
            var httpClient = Context.Provider.GetRequiredService<HttpClient>();
            string changes;
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            using (var response = await httpClient.GetAsync("https://api.github.com/repos/PassiveModding/Passive-BOT/commits"))
            {
                if (!response.IsSuccessStatusCode)
                {
                    changes = "There was an error fetching the latest changes.";
                }
                else
                {
                    dynamic result = JArray.Parse(await response.Content.ReadAsStringAsync());
                    changes = $"[{((string)result[0].sha).Substring(0, 7)}]({result[0].html_url}) {result[0].commit.message}\n" + $"[{((string)result[1].sha).Substring(0, 7)}]({result[1].html_url}) {result[1].commit.message}\n" + $"[{((string)result[2].sha).Substring(0, 7)}]({result[2].html_url}) {result[2].commit.message}";
                }

                response.Dispose();
            }

            var embed = new EmbedBuilder();

            if (changes.Length > 1000)
            {
                changes = changes.Substring(0, 1000);
                changes = $"{changes}...";
            }

            embed.WithAuthor(
                x =>
                    {
                        x.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
                        x.Name = $"{Context.Client.CurrentUser.Username}'s Official Invite";
                        x.Url = InviteHelper.GetInvite(Context.Client);
                    });
            embed.AddField("Changes", changes);

            embed.AddField("Members", $"Bot: {Context.Client.Guilds.Sum(x => x.Users.Count(z => z.IsBot))}\n" + $"Human: {Context.Client.Guilds.Sum(x => x.Users.Count(z => !z.IsBot))}\n" + $"Total: {Context.Client.Guilds.Sum(x => x.Users.Count)}", true);
            embed.AddField("Channels", $"Text: {Context.Client.Guilds.Sum(x => x.TextChannels.Count)}\n" + $"Voice: {Context.Client.Guilds.Sum(x => x.VoiceChannels.Count)}\n" + $"Total: {Context.Client.Guilds.Sum(x => x.Channels.Count)}", true);
            embed.AddField("Guilds", $"{Context.Client.Guilds.Count}\n[Support Server]({_Home.CurrentHomeModel.HomeInvite})", true);
            var orderedShards = Context.Client.Shards.OrderByDescending(x => x.Guilds.Count).ToList();
            embed.AddField("Stats", $"**Guilds:** {Context.Client.Guilds.Count}\n" + $"**Users:** {Context.Client.Guilds.Sum(x => x.MemberCount)}\n" + $"**Shards:** {Context.Client.Shards.Count}\n" + $"**Max Shard:** G:{orderedShards.First().Guilds.Count} ID:{orderedShards.First().ShardId}\n" + $"**Min Shard:** G:{orderedShards.Last().Guilds.Count} ID:{orderedShards.Last().ShardId}");

            embed.AddField(":hammer_pick:", $"Heap: {Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)} MB\n" + $"Up: {GetUptime()}", true);
            embed.AddField(":beginner:", "Written by: [PassiveModding](https://github.com/PassiveModding)\n" + $"Discord.Net {DiscordConfig.Version}", true);

            await ReplyAsync(embed.Build());
        }

        /// <summary>
        ///     Returns an invite link for the bot.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("Invite")]
        public Task InviteAsync()
        {
            return SimpleEmbedAsync("You may invite this bot to your own server using the following link:\n" + $"{InviteHelper.GetInvite(Context.Client)}");
        }

        /// <summary>
        ///     Displays Bot Statistics
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("Stats")]
        [Summary("Bot Statistics")]
        public Task StatsAsync()
        {
            var embed = new EmbedBuilder { Title = "Bot Statistics", Color = Color.DarkRed, Timestamp = DateTimeOffset.UtcNow };
            var stats = StatModel.Load();

            var errorFrequent = stats.CommandStats.OrderByDescending(x => x.ErrorCount).FirstOrDefault();
            var mostPopular = stats.CommandStats.OrderByDescending(x => x.CommandUses).FirstOrDefault();
            var diverseByGuild = stats.CommandStats.OrderByDescending(x => x.CommandGuilds.Count).FirstOrDefault();
            var diverseByUsers = stats.CommandStats.OrderByDescending(x => x.CommandUsers.Count).FirstOrDefault();

            embed.AddField("Message Stats", $"**Messages Received:** {stats.MessageCount}\n" + $"**Commands Run:** {stats.CommandStats.Sum(x => x.CommandUses)}\n" + $"**Command Errors:** {stats.CommandStats.Sum(x => x.ErrorCount)}\n" + $"**Most Popular Command:** {mostPopular?.CommandName} => {mostPopular?.CommandUses} Uses\n" + $"**Most Error Frequent Command:** {errorFrequent?.CommandName} => {errorFrequent?.ErrorCount} Errors\n" + $"**Most Diverse Command by Guild:** {diverseByGuild?.CommandName} => {diverseByGuild?.CommandGuilds.Count} Unique Guilds\n" + $"**Most Diverse Command by Users:** {diverseByUsers?.CommandName} => {diverseByUsers?.CommandUsers.Count} Unique Users");

            embed.AddField("Loaded Users", $":robot: **Bot:** {Context.Client.Guilds.Sum(x => x.Users.Count(z => z.IsBot))}\n" + $":man_in_tuxedo: **Human:** {Context.Client.Guilds.Sum(x => x.Users.Count(z => !z.IsBot))}\n" + $"**Total:** {Context.Client.Guilds.Sum(x => x.Users.Count)}", true);

            embed.AddField("Channels", $":abc: **Text:** {Context.Client.Guilds.Sum(x => x.TextChannels.Count)}\n" + $":microphone: **Voice:** {Context.Client.Guilds.Sum(x => x.VoiceChannels.Count)}\n" + $"**Total:** {Context.Client.Guilds.Sum(x => x.Channels.Count)}", true);

            var orderedShards = Context.Client.Shards.OrderByDescending(x => x.Guilds.Count).ToList();
            embed.AddField("Stats", $":small_orange_diamond: **Guilds:** {Context.Client.Guilds.Count}\n" + $":small_blue_diamond: **Users:** {Context.Client.Guilds.Sum(x => x.MemberCount)}\n" + $":boom: **Shards:** {Context.Client.Shards.Count}\n" + $"**Max Shard:** G:{orderedShards.First().Guilds.Count} ID:{orderedShards.First().ShardId}\n" + $"**Min Shard:** G:{orderedShards.Last().Guilds.Count} ID:{orderedShards.Last().ShardId}");
            
            embed.AddField(":hammer_pick:", $"Heap: {Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)} MB\n" + $"Up: {GetUptime()}", true);
            embed.AddField(":beginner:", "Written by: [PassiveModding](https://github.com/PassiveModding)\n" + $"Discord.Net {DiscordConfig.Version}", true);

            return ReplyAsync(embed);
        }

        [Command("Shards")]
        [Summary("Displays information about all shards")]
        public Task ShardInfoAsync()
        {
            var info = Context.Client.Shards.Select(x => $"[{x.ShardId}] {x.Status} - Guilds: {x.Guilds} Users: {x.Guilds.Sum(g => g.MemberCount)}");
            return SimpleEmbedAsync($"```\n" + $"{string.Join("\n", info)}\n" + $"```");
        }

        /// <summary>
        ///     The user information.
        /// </summary>
        /// <param name="user">
        ///     The user.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("user")]
        [Alias("whois", "userinfo")]
        [Summary("Returns info about the current user, or the given user")]
        public Task UserInformationAsync([Remainder] IUser user = null)
        {
            if (user == null)
            {
                user = Context.User;
            }

            var status = user.Status.ToString();
            if (status == string.Empty)
            {
                status = "Null";
            }

            var builder = new EmbedBuilder().WithTitle($"Who Is: {user}").WithThumbnailUrl(user.GetAvatarUrl()).AddField("Sign Up Date", user.CreatedAt.Date).AddField("User ID", user.Id).AddField("Username", user.Username).AddField("Discriminator", user.Discriminator).AddField("Status", status).AddField("Links", $"[Invite]({InviteHelper.GetInvite(Context.Client)})\n[Support Server]({_Home.CurrentHomeModel.HomeInvite})");

            return ReplyAsync(builder);
        }

        /// <summary>
        ///     Gets the process uptime.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        private static string GetUptime()
        {
            return (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\D\ hh\H\ mm\M\ ss\S");
        }
    }
}