using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Configuration;

namespace PassiveBOT.Commands
{
    [RequireContext(ContextType.Guild)]
    public class Server : ModuleBase
    {
        [Command("serverinfo")]
        [Summary("serverinfo")]
        [Remarks("Displays information about the current server")]
        public async Task ServerInfo()
        {
            var embed = new EmbedBuilder();
            var botlist = ((SocketGuild) Context.Guild).Users.Count(x => x.IsBot);
            var mem = ((SocketGuild) Context.Guild).MemberCount;
            var guildusers = mem - botlist;
            var s = (SocketGuild) Context.Guild;
            var g = Context.Guild;

            try
            {
                embed.AddInlineField("Server Name", g.Name);
            }
            catch
            {
                //
            }
            try
            {
                embed.AddInlineField("Owner", s.Owner);
            }
            catch
            {
                //
            }
            try
            {
                embed.AddInlineField("Owner ID", s.OwnerId);
            }
            catch
            {
                //
            }
            try
            {
                embed.AddInlineField("Voice Region", s.VoiceRegionId);
            }
            catch
            {
                //
            }
            try
            {
                embed.AddInlineField("Verification Level", s.VerificationLevel);
            }
            catch
            {
                //
            }
            try
            {
                if (s.SplashUrl == null)
                    embed.AddInlineField("Splash Url", "null");
                else
                    embed.AddInlineField("Splash Url", s.SplashUrl);
            }
            catch
            {
                //
            }
            try
            {
                embed.AddField("Creation Date", s.CreatedAt);
            }
            catch
            {
                //
            }
            try
            {
                if (s.Features.Count > 0)
                {
                    embed.AddField("--------------", "**Features**");
                    var i = 0;
                    foreach (var feature in s.Features.ToList())
                    {
                        i++;
                        embed.AddField($"{i}", feature);
                    }
                }
            }
            catch
            {
                //
            }
            try
            {
                embed.AddInlineField("Default Notifications", s.DefaultMessageNotifications);
            }
            catch
            {
                //
            }
            try
            {
                embed.AddField("--------------", "**Defaults**");
            }
            catch
            {
                //
            }
            try
            {
                embed.AddInlineField("Default Channel", s.DefaultChannel.Name);
            }
            catch
            {
                //
            }
            try
            {
                embed.AddInlineField("AFK Channel", s.AFKChannel);
            }
            catch
            {
                //
            }
            try
            {
                embed.AddInlineField("AFK Timeout", s.AFKTimeout);
            }
            catch
            {
                //
            }
            try
            {
                embed.AddInlineField("Available", g.Available);
            }
            catch
            {
                //
            }
            try
            {
                embed.AddInlineField("MFA Status", g.MfaLevel);
            }
            catch
            {
                //
            }
            try
            {
                embed.AddField("--------------", "**User Counts**");
            }
            catch
            {
                //
            }
            try
            {
                embed.AddInlineField(":busts_in_silhouette: Total Members", mem);
            }
            catch
            {
                //
            }
            try
            {
                embed.AddInlineField(":robot: Total Bots", botlist);
            }
            catch
            {
                //
            }
            try
            {
                embed.AddInlineField(":man_in_tuxedo: Total Users", guildusers);
            }
            catch
            {
                //
            }
            try
            {
                embed.AddInlineField(":newspaper2: Total Channels", s.Channels.Count);
            }
            catch
            {
                //
            }
            try
            {
                embed.AddInlineField(":microphone: Text/Voice Channels",
                    $"{s.TextChannels.Count}/{s.VoiceChannels.Count}");
            }
            catch
            {
                //
            }
            try
            {
                embed.AddInlineField(":spy: Role Count", ((SocketGuild) Context.Guild).Roles.Count);
            }
            catch
            {
                //
            }
            try
            {
                embed.AddField("Links",
                    $"[Site]({Load.Siteurl}) \n[Invite]({Load.Invite})\n[Our Server]({Load.Server})");
            }
            catch
            {
                //
            }
            try
            {
                embed.WithFooter(x =>
                {
                    x.WithText("PassiveBOT");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });
            }
            catch
            {
                //
            }

            await ReplyAsync("", false, embed.Build());
        }

        [Command("Emotes")]
        [Summary("Emoted")]
        [Remarks("Displays a list of the servers emotes")]
        public async Task Emotes()
        {
            var embed = new EmbedBuilder();

            var emotelist = "";
            var emotelist2 = "";
            foreach (var emote in Context.Guild.Emotes)
                if (emotelist.Length > 1000)
                    emotelist2 += $"<:{emote.Name}:{emote.Id}>";
                else
                    emotelist += $"<:{emote.Name}:{emote.Id}>";
            if (emotelist != "")
            {
                embed.AddField("Emotes", emotelist);
                if (emotelist2 != "")
                    embed.AddField("Emotes 2", emotelist2);

                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("subrole")]
        [Alias("setrole", "joinrole")]
        [Summary("subrole @role")]
        [Remarks("Joins/Leaves the specified(subscribable) role")]
        public async Task JoinRole(IRole role = null)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (role == null)
            {
                if (File.Exists(file))
                {
                    var jsonObj = GuildConfig.GetServer(Context.Guild);
                    var embed = new EmbedBuilder();
                    var roles = "";
                    foreach (var roleid in jsonObj.RoleList)
                        try
                        {
                            roles += $"{Context.Guild.GetRole(roleid).Name}\n";
                        }
                        catch
                        {
                            //
                        }
                    embed.AddField("Subscribable Roles", $"{roles}\n" +
                                                         "NOTE: If this list is empty, there are no roles setup in this server");
                    await ReplyAsync("", false, embed.Build());
                }
            }
            else
            {
                var jsonObj = GuildConfig.GetServer(Context.Guild);
                var embed = new EmbedBuilder();
                if (jsonObj.RoleList.Contains(role.Id))
                {
                    var u = (IGuildUser)Context.User;
                    if (u.RoleIds.Contains(role.Id))
                    {
                        await u.RemoveRoleAsync(role);
                        embed.AddField("Success", $"You have been removed from the role {role.Name}");
                    }
                    else
                    {
                        await u.AddRoleAsync(role);
                        embed.AddField("Success", $"You have been added to the role {role.Name}");
                    }
                }
                else
                {
                    embed.AddField("Failed", $"{role.Name} is not a subscribable role");
                }
                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("Roleinfo")]
        [Summary("roleinfo <@role>")]
        [Remarks("Displays information about given Role")]
        [Alias("RI")]
        [RequireContext(ContextType.Guild)]
        public async Task RoleInfoAsync(IRole role)
        {
            var srole = ((SocketRole) role).Permissions;
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
                .WithDescription(string.Join("\n", rol.OrderByDescending(x => x.Position)))
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
            if (guild != null)
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
    }
}