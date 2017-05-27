using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using PassiveBOT.Configuration;
using PassiveBOT.Handlers;

namespace PassiveBOT.Commands
{
    public class Information : ModuleBase
    {
        [Command("user"), Summary("user '@user'"), Alias("whois", "userinfo"), Remarks("Returns info about the current user, or the given user")]
        public async Task UserInformation([Remainder, Optional] IUser user)
        {
            if (user == null)
                user = Context.User;
            string status = user.Status.ToString();
            if (status == null)
                status = "Null";

            var builder = new EmbedBuilder()
            .WithTitle($"WhoIS: {user}")
            .AddInlineField("Sign Up Date", user.CreatedAt.Date)
            .AddInlineField("User ID", user.Id)
            .AddInlineField("Username", user.Username)
            .AddInlineField("Discriminatior", user.Discriminator)
            .AddInlineField("Status", status)
            .WithFooter(x =>
            {
                x.WithText("PassiveBOT");
                x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
            });

            await ReplyAsync("", false, builder);
        }

        [Command("info"), Alias("botinfo"), Summary("info"), Remarks("Display's the bots information and statistics.")]
        public async Task Info()
        {
            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x.Name = "PassiveBOT";
                    x.Url = "https://goo.gl/s3BZTw";
                })
                .AddInlineField("Author", Load.owner)
                .AddInlineField("Uptime", GetUptime())
                .AddInlineField("Heap", $"{GetHeapSize()}MB")
                .AddInlineField("Servers", (Context.Client as DiscordSocketClient).Guilds.Count)
                .AddInlineField("Channels", (Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count))
                .AddInlineField("Users", (Context.Client as DiscordSocketClient).Guilds.Sum(g => g.MemberCount))
                .AddField("Links", "[Forums](https://goo.gl/s3BZTw) \n[Invite](https://discordapp.com/oauth2/authorize?client_id={application.Id}&scope=bot&permissions=2146958591)\n[Main Server](https://discord.gg/ZKXqt2a) \n[Testing Server](https://discord.gg/bmXfBQM)")
                .WithFooter(x =>
                {
                    x.WithText("PassiveBOT");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });


            await ReplyAsync("", false, embed.Build());
        }

        //From Rick
        [Command("Roleinfo"), Summary("roleinfo '@role'"), Remarks("Displays information about given Role"), Alias("RI")]
        [RequireContext(ContextType.Guild)]
        public async Task RoleInfoAsync(IRole role)
        {
            var gld = Context.Guild;
            var chn = Context.Channel;
            var msg = Context.Message;
            var grp = role;
            if (grp == null)
                await ReplyAsync("You must supply a role.");
            var grl = grp as SocketRole;
            var gls = gld as SocketGuild;
            var perms = new List<string>(23);
            if (grl.Permissions.Administrator)
                perms.Add("Administrator");
            if (grl.Permissions.AttachFiles)
                perms.Add("Can attach files");
            if (grl.Permissions.BanMembers)
                perms.Add("Can ban members");
            if (grl.Permissions.ChangeNickname)
                perms.Add("Can change nickname");
            if (grl.Permissions.Connect)
                perms.Add("Can use voice chat");
            if (grl.Permissions.CreateInstantInvite)
                perms.Add("Can create instant invites");
            if (grl.Permissions.DeafenMembers)
                perms.Add("Can deafen members");
            if (grl.Permissions.EmbedLinks)
                perms.Add("Can embed links");
            if (grl.Permissions.KickMembers)
                perms.Add("Can kick members");
            if (grl.Permissions.ManageChannels)
                perms.Add("Can manage channels");
            if (grl.Permissions.ManageMessages)
                perms.Add("Can manage messages");
            if (grl.Permissions.ManageNicknames)
                perms.Add("Can manage nicknames");
            if (grl.Permissions.ManageRoles)
                perms.Add("Can manage roles");
            if (grl.Permissions.ManageGuild)
                perms.Add("Can manage guild");
            if (grl.Permissions.MentionEveryone)
                perms.Add("Can mention everyone group");
            if (grl.Permissions.MoveMembers)
                perms.Add("Can move members between voice channels");
            if (grl.Permissions.MuteMembers)
                perms.Add("Can mute members");
            if (grl.Permissions.ReadMessageHistory)
                perms.Add("Can read message history");
            if (grl.Permissions.ReadMessages)
                perms.Add("Can read messages");
            if (grl.Permissions.SendMessages)
                perms.Add("Can send messages");
            if (grl.Permissions.SendTTSMessages)
                perms.Add("Can send TTS messages");
            if (grl.Permissions.Speak)
                perms.Add("Can speak");
            if (grl.Permissions.UseVAD)
                perms.Add("Can use voice activation");

            var embed = new EmbedBuilder()
            .WithTitle($"Info For: {role}")
            .AddInlineField("Name", grl.Name)
            .AddInlineField("ID", grl.Id.ToString())
            .AddInlineField("Color", grl.Color.RawValue.ToString("X6"))
            .AddInlineField("Displayed Seperately?", grl.IsHoisted ? "Yes" : "No")
            .AddInlineField("Mentionable?", grl.IsMentionable ? "Yes" : "No")
            .AddInlineField("Permissions", string.Join("\n", perms))
            .WithFooter(x =>
            {
                x.WithText("PassiveBOT");
                x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
            });

            await chn.SendMessageAsync("", false, embed);
        }

        [Command("UserCount"), Alias("UC"), Summary("usercount"), Remarks("User Count for the current server")]
        [RequireContext(ContextType.Guild)]
        public async Task Ucount()
        {
            var botlist = (Context.Guild as SocketGuild).Users.Count(x => x.IsBot);
            var mem = (Context.Guild as SocketGuild).MemberCount;
            var guildusers = mem - botlist;
            var Channels = (Context.Guild as SocketGuild).TextChannels;

            var embed = new EmbedBuilder()
                .WithTitle($"User Count for {Context.Guild.Name}")
                .AddInlineField(":busts_in_silhouette: Total Members", mem)
                .AddInlineField(":robot: Total Bots", botlist)
                .AddInlineField(":man_in_tuxedo: Total Users", guildusers)
                .AddInlineField(":newspaper2: Total Channels", (Context.Guild as SocketGuild).Channels.Count)
                .AddInlineField(":microphone: Text/Voice Channels", $"{(Context.Guild as SocketGuild).TextChannels.Count}/{(Context.Guild as SocketGuild).VoiceChannels.Count}")
                .AddInlineField(":spy: Role Count", (Context.Guild as SocketGuild).Roles.Count)
                .AddField("Links", "[Forums](https://goo.gl/s3BZTw) \n[Invite](https://discordapp.com/oauth2/authorize?client_id={application.Id}&scope=bot&permissions=2146958591)\n[Main Server](https://discord.gg/ZKXqt2a) \n[Testing Server](https://discord.gg/bmXfBQM)")
                .WithFooter(x =>
                {
                    x.WithText("PassiveBOT");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, embed);
        }

        [Command("RoleList"), Summary("rolelist"), Remarks("Displays roles for the current server"), Alias("RL")]
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

        [Command("RoleMembers"), Summary("rolemembers '@role' 'nick or username'"), Remarks("Displays a list of members with the given role"), Alias("RM")]
        [RequireContext(ContextType.Guild)]
        public async Task Rmem(IRole role, [Remainder, Optional] string type)
        {
            var id = role.Id;
            var guild = Context.Guild as SocketGuild;
            var members = new List<string>();
            foreach (var user in guild.Users)
            {
                if (user.Roles.Contains(Context.Guild.GetRole(id)))
                {
                    if (type == "username")
                        members.Add(user.Username);
                    else
                    {
                        if (user.Nickname == null)
                            members.Add(user.Username);
                        else
                            members.Add(user.Nickname);
                    }
                }
            }
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

        [Command("Uptime"), Summary("uptime"), Remarks("Current uptime for passivebot since last restart")]
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

        [Command("Heap"), Summary("heap"), Remarks("Current Heap Size")]
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
    => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();
    }
}