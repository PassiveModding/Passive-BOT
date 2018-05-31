using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using PassiveBOT.Discord.Context;
using PassiveBOT.Discord.Context.Interactive.Paginator;
using PassiveBOT.Discord.Extensions;
using PassiveBOT.Handlers;

namespace PassiveBOT.Modules.Info
{
    public class Info : Base
    {
        public static string CalculateYourAge(DateTime Dob)
        {
            var Now = DateTime.Now;
            var _Years = new DateTime(DateTime.Now.Subtract(Dob).Ticks).Year - 1;
            var _DOBDateNow = Dob.AddYears(_Years);
            var _Months = 0;
            for (var i = 1; i <= 12; i++)
            {
                if (_DOBDateNow.AddMonths(i) == Now)
                {
                    _Months = i;
                    break;
                }

                if (_DOBDateNow.AddMonths(i) < Now) continue;
                _Months = i - 1;
                break;
            }

            var Days = Now.Subtract(_DOBDateNow.AddMonths(_Months)).Days;

            return $"Age: {_Years} Years {_Months} Months {Days} Days";
        }

        [Command("invite")]
        [Summary("invite")]
        [Remarks("Returns the OAuth2 Invite URL of the bot")]
        public async Task InviteBot()
        {
            await ReplyAsync(
                $"A user with `MANAGE_SERVER` can invite me to your server here: <{TextManagement.GetInvite(Context.Client)}>");
        }

        [RequireContext(ContextType.Guild)]
        [Command("discrim")]
        [Summary("discrim")]
        [Remarks("Get all users with a particular discriminator")]
        public async Task Discrim(ushort disc = 0)
        {
            var usermatches = Context.Socket.Guild.Users.Where(x => x.DiscriminatorValue == disc)
                .Select(x => $"{x.Username}#{x.Discriminator}\n");
            var embed = new EmbedBuilder();
            var value = usermatches.ToList();
            if (!value.Any())
            {
                embed.AddField($"Users with Discriminator {disc}",
                    "N/A");
                await ReplyAsync("", false, embed.Build());
                return;
            }

            var pages = new List<string>();
            var desc = "";
            foreach (var user in value)
            {
                desc += user;
                if (desc.Split('\n').Length < 30) continue;
                pages.Add(desc);
                desc = "";
            }

            pages.Add(desc);

            var msg = new PaginatedMessage
            {
                Title = $"Users with Discriminator #{disc}",
                Pages = pages.Select(x => new PaginatedMessage.Page
                {
                    description = x
                }),

                Color = new Color(114, 137, 218)
            };

            await PagedReplyAsync(msg);
        }

        [Command("user")]
        [Summary("user [Optional]<@user>")]
        [Alias("whois", "userinfo")]
        [Remarks("Returns info about the current user, or the given user")]
        public async Task UserInformation([Remainder] [Optional] IUser user)
        {
            if (user == null)
                user = Context.User;
            var status = user.Status.ToString();
            if (status == "")
                status = "Null";

            var builder = new EmbedBuilder()
                .WithTitle($"Who Is: {user}")
                .AddField("Sign Up Date", user.CreatedAt.Date, true)
                .AddField("User ID", user.Id, true)
                .AddField("Username", user.Username, true)
                .AddField("Discriminatior", user.Discriminator, true)
                .AddField("Status", status, true)
                .AddField("Links",
                    $"[Invite]({TextManagement.GetInvite(Context.Client)})\n[Support Server]({CommandHandler.Config.SupportServer})")
                .WithFooter(x =>
                {
                    x.WithText("PassiveBOT");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync(builder);
        }

        [Command("avatar")]
        [Summary("avatar [Optional]<@user>")]
        [Remarks("Returns the users avatar")]
        public async Task Avatar([Remainder] IUser user = null)
        {
            if (user == null)
                user = Context.User;

            var builder = new EmbedBuilder()
                .WithTitle($"{user}'s Avatar")
                .WithImageUrl(user.GetAvatarUrl())
                .WithFooter(x =>
                {
                    x.WithText("PassiveBOT");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync(builder);
        }

        [Command("Info")]
        [Summary("Info")]
        [Remarks("Bot Info and Stats")]
        public async Task InfoCMD()
        {
            var client = Context.Socket.Client;
            var hClient = new HttpClient();
            string changes;
            hClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            using (var response =
                await hClient.GetAsync("https://api.github.com/repos/PassiveModding/Passive-BOT/commits"))
            {
                if (!response.IsSuccessStatusCode)
                {
                    changes = "There was an error fetching the latest changes.";
                }
                else
                {
                    dynamic result = JArray.Parse(await response.Content.ReadAsStringAsync());
                    changes =
                        $"[{((string) result[0].sha).Substring(0, 7)}]({result[0].html_url}) {result[0].commit.message}\n" +
                        $"[{((string) result[1].sha).Substring(0, 7)}]({result[1].html_url}) {result[1].commit.message}\n" +
                        $"[{((string) result[2].sha).Substring(0, 7)}]({result[2].html_url}) {result[2].commit.message}";
                }

                response.Dispose();
            }

            var embed = new EmbedBuilder();

            if (changes.Length > 1000)
            {
                changes = changes.Substring(0, 1000);
                changes = $"{changes}...";
            }

            embed.WithAuthor(x =>
            {
                x.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
                x.Name = $"{client?.CurrentUser.Username}'s Official Invite";
                if (client != null)
                    x.Url =
                        $"https://discordapp.com/oauth2/authorize?client_id={client.CurrentUser.Id}&scope=bot&permissions=2146958591";
            });
            embed.AddField("Changes", changes);
            if (client != null)
            {
                embed.AddField("Members",
                    $"Bot: {client.Guilds.Sum(x => x.Users.Count(z => z.IsBot))}\n" +
                    $"Human: {client.Guilds.Sum(x => x.Users.Count(z => !z.IsBot))}\n" +
                    $"Total: {client.Guilds.Sum(x => x.Users.Count)}", true);
                embed.AddField("Channels",
                    $"Text: {client.Guilds.Sum(x => x.TextChannels.Count)}\n" +
                    $"Voice: {client.Guilds.Sum(x => x.VoiceChannels.Count)}\n" +
                    $"Total: {client.Guilds.Sum(x => x.Channels.Count)}", true);
                embed.AddField("Guilds", $"{client.Guilds.Count}\n[Support Server]({CommandHandler.Config.SupportServer})",
                    true);
            }

            embed.AddField(":space_invader:",
                $"Commands Ran: {CommandHandler.Stats.CommandsRan}\n" +
                $"Messages Received: {CommandHandler.Stats.MessagesReceived}", true);
            embed.AddField(":hammer_pick:",
                $"Heap: {Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)} MB\n" +
                $"Up: {GetUptime()}", true);
            embed.AddField(":beginner:", "Written by: [PassiveModding](https://github.com/PassiveModding)\n" +
                                         $"Discord.Net {DiscordConfig.Version}", true);

            await ReplyAsync("", embed: embed.Build());
        }

        [Command("stats")]
        [Summary("stats")]
        [Remarks("Statistics about passivebot")]
        public async Task Stats()
        {
            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x.Name = $"{Context.Client.CurrentUser.Username}";
                    x.Url = TextManagement.GetInvite(Context.Client);
                })
                .WithFooter(x =>
                {
                    x.WithText("PassiveBOT");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            embed.AddField("Author", Context.Client.GetApplicationInfoAsync().Result.Owner, true);
            embed.AddField("Uptime", GetUptime(), true);
            embed.AddField("Heap", $"{GetHeapSize()}MB", true);
            embed.AddField("Guilds", Context.Socket.Client.Guilds.Count, true);
            embed.AddField("Channels", Context.Socket.Client.Guilds.Sum(g => g.Channels.Count), true);
            embed.AddField("Users", Context.Socket.Client.Guilds.Sum(g => g.MemberCount), true);

            embed.AddField("Links",
                $"[Invite]({TextManagement.GetInvite(Context.Client)})\n[Support Server]({CommandHandler.Config.SupportServer})");


            await ReplyAsync("", false, embed.Build());
        }

        [Command("Uptime")]
        [Summary("uptime")]
        [Remarks("Current uptime for passivebot since last restart")]
        public async Task Uptime()
        {
            var embed = new EmbedBuilder
            {
                Title = "PassiveBOT Uptime:",
                Description = GetUptime(),
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
            };

            await ReplyAsync("", false, embed.Build());
        }

        [Command("Heap")]
        [Summary("heap")]
        [Remarks("Current Heap Size")]
        public async Task Heap()
        {
            var embed = new EmbedBuilder
            {
                Title = "PassiveBOT Heap Size:",
                Description = GetHeapSize() + "MB",
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
            };

            await ReplyAsync("", false, embed.Build());
        }

        private static string GetUptime()
        {
            return (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\D\ hh\H\ mm\M\ ss\S");
        }

        private static string GetHeapSize()
        {
            return Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString(CultureInfo.InvariantCulture);
        }
    }
}