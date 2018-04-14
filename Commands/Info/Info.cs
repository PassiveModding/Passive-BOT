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
using PassiveBOT.Configuration;
using PassiveBOT.Discord.Addons.Interactive;
using PassiveBOT.Discord.Addons.Interactive.Paginator;

namespace PassiveBOT.Commands.Info
{
    public class Information : InteractiveBase
    {
        [Command("invite")]
        [Summary("invite")]
        [Remarks("Returns the OAuth2 Invite URL of the bot")]
        public async Task InviteBot()
        {
            await ReplyAsync(
                $"A user with `MANAGE_SERVER` can invite me to your server here: <{Load.GetInvite(Context.Client)}>");
        }

        [RequireContext(ContextType.Guild)]
        [Command("discrim")]
        [Summary("discrim")]
        [Remarks("Get all users with a particular discriminator")]
        public async Task Discrim(ushort disc = 0)
        {
            var usermatches = Context.Guild.Users.Where(x => x.DiscriminatorValue == disc).Select(x => $"{x.Username}#{x.Discriminator}\n");
            var embed = new EmbedBuilder();
            var value = usermatches.ToList();
            if (!value.Any())
            {
                embed.AddField($"Users with Discriminator {disc}",
                    $"N/A");
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
                .AddInlineField("Sign Up Date", user.CreatedAt.Date)
                .AddInlineField("User ID", user.Id)
                .AddInlineField("Username", user.Username)
                .AddInlineField("Discriminatior", user.Discriminator)
                .AddInlineField("Status", status)
                .AddField("Links",
                    $"[Site]({Load.Siteurl}) \n[Invite]({Load.GetInvite(Context.Client)})\n[Our Server]({Load.Server})")
                .WithFooter(x =>
                {
                    x.WithText("PassiveBOT");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, builder);
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

            await ReplyAsync("", false, builder);
        }

        [Command("Info")]
        [Summary("Info")]
        [Remarks("Bot Info and Stats")]
        public async Task Info()
        {
            var client = Context.Client;
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
                embed.AddField("Guilds", $"{client.Guilds.Count}\n[Support Guild]({Load.Server})",
                    true);
            }

            embed.AddField(":space_invader:",
                $"Commands Ran: {Load.Commands}\n" +
                $"Messages Received: {Load.Messages}", true);
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
                    x.Name = $"PassiveBOT {Load.Version}";
                    x.Url = "https://goo.gl/s3BZTw";
                })
                .WithFooter(x =>
                {
                    x.WithText("PassiveBOT");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            embed.AddInlineField("Author", Load.Owner);
            embed.AddInlineField("Uptime", GetUptime());
            embed.AddInlineField("Heap", $"{GetHeapSize()}MB");
            embed.AddInlineField("Guilds", Context.Client.Guilds.Count);
            embed.AddInlineField("Channels", Context.Client.Guilds.Sum(g => g.Channels.Count));
            embed.AddInlineField("Users", Context.Client.Guilds.Sum(g => g.MemberCount));

            embed.AddField("Links",
                $"[Site]({Load.Siteurl}) \n[Invite]({Context.Client})\n[Our Server]({Load.Server})");


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