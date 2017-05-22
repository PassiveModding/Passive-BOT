using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using PassiveBOT.Services;

namespace PassiveBOT.Commands
{
    public class Information : ModuleBase
    {
        [Command("user"), Summary("user '@user'"), Alias("whois"), Remarks("Returns info about the current user, or the given user")]
        public async Task UserInformation([Remainder, Optional] IUser user)
        {
            if (user == null)
            {
                user = Context.User;
            }

            var builder = new EmbedBuilder()
            {
                Color = new Discord.Color(255, 64, 197),
                Title = $"{user.Username}",
                Description = $"User Info for {user.Username} by PassiveBOT",
                ThumbnailUrl = user.GetAvatarUrl()
            };
            builder.AddField(x => {
                x.Name = "Account Creation Date";
                x.Value = user.CreatedAt.Date;
                x.IsInline = true;
            });
            builder.AddField(x => {
                x.Name = "Is Bot";
                x.Value = user.IsBot;
                //x.IsInline = true;
            });
            builder.AddField(x => {
                x.Name = "User ID";
                x.Value = user.Id;
                //x.IsInline = true;
            });
            builder.AddField(x => {
                x.Name = "Username";
                x.Value = user.Username;
                //x.IsInline = true;
            });
            builder.AddField(x => {
                x.Name = "Status";
                x.Value = user.Status;
                //x.IsInline = true;
            });
            builder.AddField(x => {
                x.Name = "PassiveBOT";
                x.Value = $"{Linkcfg.siteurl}";
                //x.IsInline = true;
            });

            await ReplyAsync("", false, builder.Build());
        }

        [Command("info"), Summary("info"), Remarks("Display's the bots information and statistics.")]
        public async Task Info()
        {
            var embed = new EmbedBuilder()
            {
                Color = new Discord.Color(255, 64, 197),
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
            };
            embed.AddField(x => {
                x.Name = "Author:";
                x.Value = $"{Linkcfg.owner}";
                x.IsInline = false;
            });
            embed.AddField(x => {
                x.Name = "Statistics:";
                x.Value = $"Uptime: {GetUptime()}\nHeap Size: {GetHeapSize()}MB";
                x.IsInline = false;
            });
            embed.AddField(x => {
                x.Name = "Guilds/Channels/Users:";
                x.Value = $"{(Context.Client as DiscordSocketClient).Guilds.Count} / {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count)} / {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.MemberCount)}";
                x.IsInline = false;
            });
            embed.AddField(x => {
                x.Name = "PassiveBOT";
                x.Value = $"{Linkcfg.siteurl}";
                x.IsInline = false;
            });

            await ReplyAsync("", false, embed.Build());
        }

        [Command("Roleinfo"), Summary("roleinfo '@role'"), Remarks("Displays information about given Role"), Alias("RI")]
        public async Task RoleInfoAsync(IRole role)
        {
            var gld = Context.Guild;
            var chn = Context.Channel;
            var msg = Context.Message;
            var grp = role;
            if (grp == null)
                throw new ArgumentException("You must supply a role.");
            var grl = grp as SocketRole;
            var gls = gld as SocketGuild;

            var embed = new EmbedBuilder()
            {
                Title = "Role"
            };
            embed.AddField(x => {
                x.IsInline = true;
                x.Name = "Name";
                x.Value = grl.Name;
            });

            embed.AddField(x => {
                x.IsInline = true;
                x.Name = "ID";
                x.Value = grl.Id.ToString();
            });

            embed.AddField(x => {
                x.IsInline = true;
                x.Name = "Color";
                x.Value = grl.Color.RawValue.ToString("X6");
            });

            embed.AddField(x => {
                x.IsInline = true;
                x.Name = "Displayed Seperately?";
                x.Value = grl.IsHoisted ? "Yes" : "No";
            });

            embed.AddField(x => {
                x.IsInline = true;
                x.Name = "Mentionable?";
                x.Value = grl.IsMentionable ? "Yes" : "No";
            });

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
            embed.AddField(x => {
                x.IsInline = false;
                x.Name = "Permissions";
                x.Value = string.Join("\n", perms);
            });

            await chn.SendMessageAsync("", false, embed);
        }

        [Command("GuildCount"), Alias("GC"), Summary("guildcount"), Remarks("User Count for the current server")]
        [RequireContext(ContextType.Guild)]
        public async Task Ucount()
        {
            var embed = new EmbedBuilder()
            {
                Color = new Discord.Color(255, 64, 197),
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                Title = Context.Guild.Name
            };
            embed.AddField(x => {
                x.Name = $"User Count:";
                x.Value = $"{(Context.Guild as SocketGuild).MemberCount}";
                x.IsInline = false;
            });
            embed.AddField(x => {
                x.Name = $"Channel Count: (Text + Voice)";
                x.Value = $"{(Context.Guild as SocketGuild).Channels.Count}";
                x.IsInline = false;
            });
            embed.AddField(x => {
                x.Name = $"Role Count:";
                x.Value = $"{(Context.Guild as SocketGuild).Roles.Count}";
                x.IsInline = false;
            });
            embed.AddField(x => {
                x.Name = "PassiveBOT";
                x.Value = $"{Linkcfg.siteurl}";
                x.IsInline = false;
            });           

            await ReplyAsync("", false, embed.Build());
        }

        [Command("RoleList"), Summary("rolelist"), Remarks("Displays roles for the current server"), Alias("RL")]
        public async Task RoleList()
        {
            var rol = Context.Guild.Roles;

            var embed = new EmbedBuilder()
            {
                Title = $"Roles for {Context.Guild.Name}",
                Description = string.Join("\n", rol),
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                Url = $"{Linkcfg.siteurl}"
            };

            await ReplyAsync("", false, embed.Build());
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
            {
                Title = $"Here is a list of Members with the role {role}",
                Description = string.Join(" \n", members)
            };

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