using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Configuration;
using PassiveBOT.Preconditions;

namespace PassiveBOT.Commands.ServerModeration
{
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    public class Admin : InteractiveBase
    {
        [Command("ClearWarn")]
        [Summary("ClearWarn <@user>")]
        [Remarks("Clears warnings for the specified user")]
        public async Task ClearWarn(SocketGuildUser removeuser)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var embed = new EmbedBuilder();
            embed.WithTitle("Warns Removed");
            var config = GuildConfig.GetServer(Context.Guild);
            var list = "";
            var newconfig = new List<GuildConfig.Warns>();
            foreach (var group in config.Warnings)
                if (group.UserId == removeuser.Id)
                {
                    var moderator =
                        $"{group.Moderator}                                             ".Substring(0, 20);
                    list += $"Mod: {moderator} || Reason: {group.Reason}\n";
                    //config.Warnings.Remove(group);
                }
                else
                {
                    newconfig.Add(group);
                }
            config.Warnings = newconfig;
            embed.WithDescription(list);
            GuildConfig.SaveServer(config);
            await ReplyAsync($"Warnings for the user {removeuser} have been cleared");
            try
            {
                await ReplyAsync("", false, embed.Build());
            }
            catch
            {
                //
            }
        }

        [Command("ClearKick")]
        [Summary("ClearKick <@user>")]
        [Remarks("Clears Kicks for the specified user")]
        public async Task ClearKick(SocketGuildUser removeuser)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var embed = new EmbedBuilder();
            embed.WithTitle("Kicks Removed");
            var config = GuildConfig.GetServer(Context.Guild);
            var list = "";
            var newconfig = new List<GuildConfig.Kicks>();
            foreach (var group in config.Kicking)
                if (group.UserId == removeuser.Id)
                {
                    var moderator =
                        $"{group.Moderator}                                             ".Substring(0, 20);
                    list += $"Mod: {moderator} || Reason: {group.Reason}\n";
                    //config.Kicking.Remove(group);
                }
                else
                {
                    newconfig.Add(group);
                }
            embed.WithDescription(list);
            config.Kicking = newconfig;
            GuildConfig.SaveServer(config);
            await ReplyAsync($"Kicks for the user {removeuser} have been cleared");
            try
            {
                await ReplyAsync("", false, embed.Build());
            }
            catch
            {
                //
            }
        }

        [Command("ClearBan")]
        [Summary("ClearBan <@user>")]
        [Remarks("Clears Bans for the specified user")]
        public async Task ClearBan(SocketGuildUser removeuser)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var embed = new EmbedBuilder();
            embed.WithTitle("Bans Removed");
            var config = GuildConfig.GetServer(Context.Guild);
            var list = "";
            var newconfig = new List<GuildConfig.Bans>();
            foreach (var group in config.Banning)
                if (group.UserId == removeuser.Id)
                {
                    var moderator =
                        $"{group.Moderator}                                             ".Substring(0, 20);
                    list += $"Mod: {moderator} || Reason: {group.Reason}\n";
                    //config.Banning.Remove(group);
                }
                else
                {
                    newconfig.Add(group);
                }
            config.Banning = newconfig;
            embed.WithDescription(list);
            GuildConfig.SaveServer(config);
            await ReplyAsync($"Warnings for the user {removeuser} have been cleared");
            try
            {
                await ReplyAsync("", false, embed.Build());
            }
            catch
            {
                //
            }
        }

        [Command("ClearWarn")]
        [Summary("ClearWarn <userID>")]
        [Remarks("Clears warnings for the specified user")]
        public async Task ClearWarn(ulong userId)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var embed = new EmbedBuilder();
            embed.WithTitle("Warns Removed");
            var config = GuildConfig.GetServer(Context.Guild);
            var list = "";
            var newconfig = new List<GuildConfig.Warns>();
            foreach (var group in config.Warnings)
                if (group.UserId == userId)
                {
                    var moderator =
                        $"{group.Moderator}                                             ".Substring(0, 20);
                    list += $"Mod: {moderator} || Reason: {group.Reason}\n";
                    //config.Warnings.Remove(group);
                }
                else
                {
                    newconfig.Add(group);
                }
            config.Warnings = newconfig;
            embed.WithDescription(list);
            GuildConfig.SaveServer(config);
            await ReplyAsync($"Warnings for the user with the id {userId} have been cleared");
            try
            {
                await ReplyAsync("", false, embed.Build());
            }
            catch
            {
                //
            }
        }

        [Command("ClearKick")]
        [Summary("ClearKick <UserID>")]
        [Remarks("Clears Kicks for the specified user")]
        public async Task ClearKick(ulong userId)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var embed = new EmbedBuilder();
            embed.WithTitle("Kicks Removed");
            var config = GuildConfig.GetServer(Context.Guild);
            var list = "";
            var newconfig = new List<GuildConfig.Kicks>();
            foreach (var group in config.Kicking)
                if (group.UserId == userId)
                {
                    var moderator =
                        $"{group.Moderator}                                             ".Substring(0, 20);
                    list += $"Mod: {moderator} || Reason: {group.Reason}\n";
                    //config.Kicking.Remove(group);
                }
                else
                {
                    newconfig.Add(group);
                }
            embed.WithDescription(list);
            config.Kicking = newconfig;
            GuildConfig.SaveServer(config);
            await ReplyAsync($"Kicks for the user with the id {userId} have been cleared");
            try
            {
                await ReplyAsync("", false, embed.Build());
            }
            catch
            {
                //
            }
        }

        [Command("ClearBan")]
        [Summary("ClearBan <UserID>")]
        [Remarks("Clears Bans for the specified user")]
        public async Task ClearBan(ulong userId)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var embed = new EmbedBuilder();
            embed.WithTitle("Bans Removed");
            var config = GuildConfig.GetServer(Context.Guild);
            var list = "";
            var newconfig = new List<GuildConfig.Bans>();
            foreach (var group in config.Banning)
                if (group.UserId == userId)
                {
                    var moderator =
                        $"{group.Moderator}                                             ".Substring(0, 20);
                    list += $"Mod: {moderator} || Reason: {group.Reason}\n";
                    //config.Banning.Remove(group);
                }
                else
                {
                    newconfig.Add(group);
                }
            config.Banning = newconfig;
            embed.WithDescription(list);
            GuildConfig.SaveServer(config);
            await ReplyAsync($"Warnings for the user with the id {userId} have been cleared");
            try
            {
                await ReplyAsync("", false, embed.Build());
            }
            catch
            {
                //
            }
        }

        [Command("ResetBans")]
        [Summary("ResetBans")]
        [Remarks("Clears all bans logged in the server")]
        [ServerOwner]
        public async Task ResetBan()
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var config = GuildConfig.GetServer(Context.Guild);
            config.Banning = new List<GuildConfig.Bans>();
            GuildConfig.SaveServer(config);
            await ReplyAsync($"All server bans have been cleared.");
        }

        [Command("ResetWarns")]
        [Summary("ResetWarns")]
        [Remarks("Clears all warnings logged in the server")]
        [ServerOwner]
        public async Task ResetWarn()
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var config = GuildConfig.GetServer(Context.Guild);
            config.Warnings = new List<GuildConfig.Warns>();
            GuildConfig.SaveServer(config);
            await ReplyAsync($"All server warnings have been cleared.");
        }

        [Command("ResetKicks")]
        [Summary("ResetKicks")]
        [Remarks("Clears all kicks logged in the server")]
        [ServerOwner]
        public async Task ResetKick()
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var config = GuildConfig.GetServer(Context.Guild);
            config.Kicking = new List<GuildConfig.Kicks>();
            GuildConfig.SaveServer(config);
            await ReplyAsync($"All server kicks have been cleared.");
        }

        [Command("InviteExcempt")]
        [Summary("InviteExcempt <@role>")]
        [Remarks("Set roles that are excempt from the Invite Block command")]
        public async Task InvExcempt(IRole role = null)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var config = GuildConfig.GetServer(Context.Guild);
            if (role == null)
            {
                var embed = new EmbedBuilder();
                foreach (var r in config.InviteExcempt)
                    try
                    {
                        var rol = Context.Guild.GetRole(r);
                        embed.Description += $"{rol.Name}\n";
                    }
                    catch
                    {
                        //
                    }
                embed.Title = "Roles Excempt from Invite Block";
                await ReplyAsync("", false, embed.Build());
                return;
            }

            config.InviteExcempt.Add(role.Id);

            GuildConfig.SaveServer(config);
            await ReplyAsync($"{role.Mention} has been added to those excempt from the Invite Blocker");
        }

        [Command("RemoveInviteExcempt")]
        [Summary("RemoveInviteExcempt <@role>")]
        [Remarks("Remove roles that are excempt from the Invite Block command")]
        public async Task UndoInvExcempt(IRole role = null)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var config = GuildConfig.GetServer(Context.Guild);
            if (role == null)
            {
                var embed = new EmbedBuilder();
                foreach (var r in config.InviteExcempt)
                    try
                    {
                        var rol = Context.Guild.GetRole(r);
                        embed.Description += $"{rol.Name}\n";
                    }
                    catch
                    {
                        //
                    }
                embed.Title = "Roles Excempt from Invite Block";
                await ReplyAsync("", false, embed.Build());
                return;
            }

            config.InviteExcempt.Remove(role.Id);

            GuildConfig.SaveServer(config);
            await ReplyAsync($"{role.Mention} has been removed from those excempt from the Invite Blocker");
        }

        [Command("MentionExcempt")]
        [Summary("MentionExcempt <@role>")]
        [Remarks("Set roles that are excempt from the Mention Block command")]
        public async Task MentionExcempt(IRole role = null)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var config = GuildConfig.GetServer(Context.Guild);
            if (role == null)
            {
                var embed = new EmbedBuilder();
                foreach (var r in config.MentionallExcempt)
                    try
                    {
                        var rol = Context.Guild.GetRole(r);
                        embed.Description += $"{rol.Name}\n";
                    }
                    catch
                    {
                        //
                    }
                embed.Title = "Roles Excempt from Mention Blocker";
                await ReplyAsync("", false, embed.Build());
                return;
            }

            config.MentionallExcempt.Add(role.Id);

            GuildConfig.SaveServer(config);
            await ReplyAsync($"{role.Mention} has been added to those excempt from the Mention Blocker");
        }

        [Command("RemoveMentionExcempt")]
        [Summary("RemoveMentionExcempt <@role>")]
        [Remarks("Remove roles that are excempt from the Mention Blocker command")]
        public async Task UndoMentionExcempt(IRole role = null)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var config = GuildConfig.GetServer(Context.Guild);
            if (role == null)
            {
                var embed = new EmbedBuilder();
                foreach (var r in config.MentionallExcempt)
                    try
                    {
                        var rol = Context.Guild.GetRole(r);
                        embed.Description += $"{rol.Name}\n";
                    }
                    catch
                    {
                        //
                    }
                embed.Title = "Roles Excempt from Mention Blocker";
                await ReplyAsync("", false, embed.Build());
                return;
            }

            config.MentionallExcempt.Remove(role.Id);

            GuildConfig.SaveServer(config);
            await ReplyAsync($"{role.Mention} has been removed from those excempt from the Mention Blocker");
        }
    }
}