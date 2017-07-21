using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using PassiveBOT.Configuration;

namespace PassiveBOT.Commands
{
    public class Information : ModuleBase
    {
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
                    $"[Site]({Load.Siteurl}) \n[Invite]({Load.Invite})\n[Our Server]({Load.Server})")
                .WithFooter(x =>
                {
                    x.WithText("PassiveBOT");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, builder);
        }


        [Command("subrole")]
        [Summary("subrole @role")]
        [Remarks("Joins/Leaves the specified(subscribable) role")]
        public async Task JoinRole(IRole role = null)
        {
            if (role == null)
            {
                var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}/config.json");
                if (File.Exists(file))
                {
                    dynamic jsonObj = JsonConvert.DeserializeObject(File.ReadAllText(file));
                    var list = new List<ulong>();

                    ulong val = jsonObj.Roles;
                    if (val != 0)
                        list.Add(val);

                    if (list.Count == 0)
                    {
                        await ReplyAsync("This server has no roles configured for this command.");
                    }
                    else if (list.Count == 1)
                    {
                        ulong value = jsonObj.Roles;
                        var rolename = Context.Guild.GetRole(value);
                        await ReplyAsync($"There is one available role to join in this server => {rolename}");
                    }
                }
            }
            else
            {
                var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}/config.json");
                if (File.Exists(file))
                {
                    dynamic jsonObj = JsonConvert.DeserializeObject(File.ReadAllText(file));

                    ulong val = jsonObj.Roles;
                    if ((Context.User as SocketGuildUser).Roles.ToArray().Contains(role))
                    {
                        await (Context.User as SocketGuildUser).RemoveRoleAsync(role);
                        await ReplyAsync($"{Context.User.Mention} has been removed from the role {role.Name}");
                    }
                    else
                    {
                        if (role.Id == val)
                        {
                            await (Context.User as SocketGuildUser).AddRoleAsync(role);
                            await ReplyAsync($"{Context.User.Mention} has been added to the role {role.Name}");
                        }

                        else
                        {
                            await ReplyAsync("This role is not joinable!");
                        }
                    }
                }
            }
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

        [Command("info")]
        [Alias("botinfo")]
        [Summary("info")]
        [Remarks("Display's the bots information and statistics.")]
        public async Task Info()
        {
            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x.Name = "PassiveBOT";
                    x.Url = "https://goo.gl/s3BZTw";
                })
                .AddInlineField("Author", Load.Owner)
                .AddInlineField("Uptime", GetUptime())
                .AddInlineField("Heap", $"{GetHeapSize()}MB")
                .AddInlineField("Servers", (Context.Client as DiscordSocketClient).Guilds.Count)
                .AddInlineField("Channels", (Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count))
                .AddInlineField("Users", (Context.Client as DiscordSocketClient).Guilds.Sum(g => g.MemberCount))
                .AddField("Links",
                    $"[Site]({Load.Siteurl}) \n[Invite]({Load.Invite})\n[Our Server]({Load.Server})")
                .WithFooter(x =>
                {
                    x.WithText("PassiveBOT");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, embed.Build());
        }

        [Command("Roleinfo")]
        [Summary("roleinfo <@role>")]
        [Remarks("Displays information about given Role")]
        [Alias("RI")]
        [RequireContext(ContextType.Guild)]
        public async Task RoleInfoAsync(IRole role)
        {
            var srole = (role as SocketRole).Permissions;
            var l = new List<string>();
            if (srole.AddReactions)
                l.Add("Can Add Reactions");
            if (srole.Administrator)
                l.Add("Is Administrator");
            if (srole.AttachFiles)
                l.Add("Can Attach Files");
            if (srole.BanMembers)
                l.Add("Can Ban Members");
            if (srole.ChangeNickname)
                l.Add("Can Change Nickname");
            if (srole.Connect)
                l.Add("Can Connect");
            if (srole.CreateInstantInvite)
                l.Add("Can Create Invite");
            if (srole.DeafenMembers)
                l.Add("Can Deafen Members");
            if (srole.EmbedLinks)
                l.Add("Can Embed Links");
            if (srole.KickMembers)
                l.Add("Can Kick Members");
            if (srole.ManageChannels)
                l.Add("Can Manage Channels");
            if (srole.ManageEmojis)
                l.Add("Can Manage Emojis");
            if (srole.ManageGuild)
                l.Add("Can Manage Guild");
            if (srole.ManageMessages)
                l.Add("Can Manage Messages");
            if (srole.ManageNicknames)
                l.Add("Can Manage Nicknames");
            if (srole.ManageRoles)
                l.Add("Can Manage Roles");
            if (srole.ManageWebhooks)
                l.Add("Can Manage Webhooks");
            if (srole.MentionEveryone)
                l.Add("Can Mention Everyone");
            if (srole.MoveMembers)
                l.Add("Can Move Members");
            if (srole.MuteMembers)
                l.Add("Can Mute Members");
            if (srole.ReadMessageHistory)
                l.Add("Can Read Message History");
            if (srole.ReadMessages)
                l.Add("Can Read Messages");
            if (srole.SendMessages)
                l.Add("Can Send Messages");
            if (srole.SendTTSMessages)
                l.Add("Can Send TTS Messages");
            if (srole.Speak)
                l.Add("Can Speak");
            if (srole.UseExternalEmojis)
                l.Add("Can Use Externam Emojis");
            if (srole.UseVAD)
                l.Add("Can Use VAD");

            var list = string.Join("\n", l.ToArray());
            if (list == "")
                list = "None :(";
            var embed = new EmbedBuilder()
                .WithTitle($"RoleInfo for {role.Name}")
                .AddInlineField("Colour", $"{role.Color}")
                .AddInlineField("ID", $"{role.Id}")
                .AddInlineField("Creation Date", $"{role.CreatedAt}")
                .AddInlineField("Displayed Separately?", $"{role.IsHoisted}")
                .AddInlineField("Mentionable?", $"{role.IsMentionable}")
                .AddInlineField("Discord Generated?", $"{role.IsManaged}")
                .AddField("Permissions", list)
                .WithFooter(x =>
                {
                    x.WithText("PassiveBOT");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, embed.Build());
        }

        [Command("UserCount")]
        [Alias("UC")]
        [Summary("usercount")]
        [Remarks("User Count for the current server")]
        [RequireContext(ContextType.Guild)]
        public async Task Ucount()
        {
            var botlist = ((SocketGuild) Context.Guild).Users.Count(x => x.IsBot);
            var mem = ((SocketGuild) Context.Guild).MemberCount;
            var guildusers = mem - botlist;

            var embed = new EmbedBuilder()
                .WithTitle($"User Count for {Context.Guild.Name}")
                .AddInlineField(":busts_in_silhouette: Total Members", mem)
                .AddInlineField(":robot: Total Bots", botlist)
                .AddInlineField(":man_in_tuxedo: Total Users", guildusers)
                .AddInlineField(":newspaper2: Total Channels", ((SocketGuild) Context.Guild).Channels.Count)
                .AddInlineField(":microphone: Text/Voice Channels",
                    $"{((SocketGuild) Context.Guild).TextChannels.Count}/{((SocketGuild) Context.Guild).VoiceChannels.Count}")
                .AddInlineField(":spy: Role Count", ((SocketGuild) Context.Guild).Roles.Count)
                .AddField("Links",
                    $"[Site]({Load.Siteurl}) \n[Invite]({Load.Invite})\n[Our Server]({Load.Server})")
                .WithFooter(x =>
                {
                    x.WithText("PassiveBOT");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, embed);
        }

        [Command("RoleList")]
        [Summary("rolelist")]
        [Remarks("Displays roles for the current server")]
        [Alias("RL")]
        [RequireContext(ContextType.Guild)]
        public async Task RoleList()
        {
            var rol = Context.Guild.Roles;

            var embed = new EmbedBuilder()
                .WithTitle($"Roles for {Context.Guild.Name}")
                .WithDescription(string.Join("\n", rol))
                .WithFooter(x =>
                {
                    x.WithText("PassiveBOT");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, embed);
        }

        [Command("RoleMembers")]
        [Summary("rolemembers <@role> <nick/username>")]
        [Remarks("Displays a list of members with the given role")]
        [Alias("RM")]
        [RequireContext(ContextType.Guild)]
        public async Task Rmem(IRole role, [Remainder] [Optional] string type)
        {
            var id = role.Id;
            var guild = Context.Guild as SocketGuild;
            var members = new List<string>();
            foreach (var user in guild.Users)
                if (user.Roles.Contains(Context.Guild.GetRole(id)))
                    if (type == "username")
                        members.Add(user.Username);
                    else
                        members.Add(user.Nickname ?? user.Username);
            var embed = new EmbedBuilder()
                .WithTitle($"Here is a list of Members with the role {role}")
                .WithDescription(string.Join(" \n", members))
                .WithFooter(x =>
                {
                    x.WithText("PassiveBOT");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

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
                Description = GetUptime() + "\nDD.HH:MM:SS",
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
            return (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        }

        private static string GetHeapSize()
        {
            return Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString(CultureInfo.InvariantCulture);
        }
    }
}