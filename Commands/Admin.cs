using System;
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
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    public class Admin : ModuleBase
    {
        [Command("prune")]
        [Summary("prune <no. of messages>")]
        [Remarks("removes specified amount of messages")]
        public async Task Prune(int count = 100)
        {
            if (count < 1)
            {
                await ReplyAsync("**ERROR: **Please Specify the amount of messages you want to clear");
            }
            else if (count > 100)
            {
                await ReplyAsync("**Error: **I can only clear 100 Messages at a time!");
            }
            else
            {
                await Context.Message.DeleteAsync().ConfigureAwait(false);
                var limit = count < 100 ? count : 100;
                var enumerable = await Context.Channel.GetMessagesAsync(limit).Flatten().ConfigureAwait(false);
                await Context.Channel.DeleteMessagesAsync(enumerable).ConfigureAwait(false);
                await ReplyAsync($"Cleared **{count}** Messages");
            }
        }

        [Command("nopre")]
        [Summary("nopre")]
        [Remarks("toggles prefixless commands in the current server")]
        public async Task Nopre()
        {
            var lines = File.ReadAllLines(AppContext.BaseDirectory + "setup/moderation/nopre.txt");
            var result = lines.ToList();
            if (result.Contains(Context.Guild.Id.ToString()))
            {
                var oldLines = File.ReadAllLines($"{AppContext.BaseDirectory + "setup/moderation/nopre.txt"}");
                var newLines = oldLines.Where(line => !line.Contains(Context.Guild.Id.ToString()));
                File.WriteAllLines($"{AppContext.BaseDirectory + "setup/moderation/nopre.txt"}", newLines);
                await ReplyAsync(
                    $"{Context.Guild} has been removed from the noprefix list (secret commands and prefixless commands are now enabled)");
            }
            else
            {
                File.AppendAllText($"{AppContext.BaseDirectory + "setup/moderation/nopre.txt"}",
                    $"{Context.Guild.Id}" + Environment.NewLine);
                await ReplyAsync(
                    $"{Context.Guild} has been added to the noprefix list (secret commands and prefixless commands are now disabled)");
            }
        }

        [Command("errors")]
        [Summary("errors")]
        [Remarks("toggles error replies for this bot")]
        public async Task ErrorLog()
        {
            var lines = File.ReadAllLines(AppContext.BaseDirectory + "setup/moderation/errlogging.txt");
            var result = lines.ToList();
            if (result.Contains(Context.Guild.Id.ToString()))
            {
                var oldLines = File.ReadAllLines($"{AppContext.BaseDirectory + "setup/moderation/errlogging.txt"}");
                var newLines = oldLines.Where(line => !line.Contains(Context.Guild.Id.ToString()));
                File.WriteAllLines($"{AppContext.BaseDirectory + "setup/moderation/errlogging.txt"}", newLines);
                await ReplyAsync($"I will no longer reply if an error is thrown in {Context.Guild}");
            }
            else
            {
                File.AppendAllText($"{AppContext.BaseDirectory + "setup/moderation/errlogging.txt"}",
                    $"{Context.Guild.Id}" + Environment.NewLine);
                await ReplyAsync($"I will now reply if an error is thrown in {Context.Guild}");
            }
        }

        [Command("kick", RunMode = RunMode.Async)]
        [Summary("kick <@user> <reason>")]
        [Remarks("Kicks the specified user (requires Kick Permissions)")]
        public async Task Kickuser(SocketGuildUser user, [Remainder] [Optional] string reason)
        {
            if (!Directory.Exists(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/"))
                Directory.CreateDirectory(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/");

            var success = false;
            if (user.GuildPermissions.ManageRoles)
            {
                await ReplyAsync("**ERROR: **you cannot kick a a user with manage roles permission");
            }
            else if (reason == null)
            {
                await ReplyAsync("**ERROR: **Please specify a reason for Kicking the user");
            }
            else
            {
                try
                {
                    await user.KickAsync();
                    success = true;
                }
                catch
                {
                    await ReplyAsync(
                        "**ERROR: **I was unable to kick the specified user, please check I have sufficient permissions");
                }
                if (success)
                {
                    await Task.Delay(1000);
                    File.AppendAllText(
                        Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}/kick.txt"),
                        $"User: {user} || Moderator: {Context.User} || Reason: {reason}\n");
                    await ReplyAsync($"{user} has been kicked for `{reason}`:bangbang: ");
                    var dm = await user.CreateDMChannelAsync();
                    await dm.SendMessageAsync(
                        $"{user.Mention} you have been kicked from {Context.Guild} for `{reason}`");
                }
            }
        }

        [Command("warn", RunMode = RunMode.Async)]
        [Summary("warn <@user> <reason>")]
        [Remarks("warns the specified user")]
        public async Task NewWarnuser(SocketGuildUser user, [Remainder] string reason = null)
        {
            if (!Directory.Exists(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/"))
                Directory.CreateDirectory(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/");

            if (user.GuildPermissions.ManageRoles)
            {
                await ReplyAsync("**ERROR: **you cannot warn a a user with manage roles permission");
            }
            else if (reason == null)
            {
                await ReplyAsync("**ERROR: **Please Specify a reason for warning the user");
            }
            else
            {
                await ReplyAsync($"{user.Mention} has been warned for `{reason}`");
                var dm = await user.CreateDMChannelAsync();
                await dm.SendMessageAsync($"{user.Mention} you have been warned for `{reason}` in {Context.Guild}");

                await Task.Delay(1000);
                File.AppendAllText(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/warn.txt",
                    $"User: {user} || Moderator: {Context.User} || Reason: {reason}\n");
            }
        }

        [Command("ban", RunMode = RunMode.Async)]
        [Summary("ban <@user> <reason>-")]
        [Remarks("bans the specified user (requires Ban Permissions)")]
        public async Task Banuser(SocketGuildUser user, [Remainder] [Optional] string reason)
        {
            if (!Directory.Exists(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/"))
                Directory.CreateDirectory(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/");

            var success = false;
            if (user.GuildPermissions.ManageRoles)
            {
                await ReplyAsync("**ERROR: **you cannot ban a user with manage roles permission");
            }
            else if (reason == null)
            {
                await ReplyAsync("**ERROR: ** Please specify a reason for banning the user!");
            }
            else
            {
                try
                {
                    await Context.Guild.AddBanAsync(user);
                    success = true;
                }
                catch
                {
                    await ReplyAsync(
                        "**ERROR: **I was unable to ban the specified user, please check I have sufficient permissions");
                }
                if (success)
                {
                    await Task.Delay(1000);
                    File.AppendAllText(
                        Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}/ban.txt"),
                        $"User: {user} || Moderator: {Context.User} || Reason: {reason}\n");
                    await ReplyAsync($"{user} has been banned for `{reason}`:bangbang: ");
                    var dm = await user.CreateDMChannelAsync();
                    await dm.SendMessageAsync(
                        $"{user.Mention} you have been banned from {Context.Guild} for `{reason}`");
                }
            }
        }

        [Command("kicks")]
        [Summary("kicks")]
        [Remarks("Users kicked by passivebot")]
        public async Task Kicks()
        {
            if (!File.Exists(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/kick.txt"))
            {
                await ReplyAsync(
                    $"There are currently no kicks in this server, to kick someone type `{Load.Pre}kick @user 'reason'`");
            }
            else
            {
                var kicks = File.ReadAllText(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/kick.txt");
                await ReplyAsync("```\n" + kicks + "\n```");
            }
        }

        [Command("warns")]
        [Summary("warns")]
        [Remarks("Users warned by passivebot")]
        public async Task Warns()
        {
            if (!File.Exists(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/warn.txt"))
            {
                await ReplyAsync(
                    $"There are currently no warns in this server, to warn someone type `{Load.Pre}warn @user 'reason'`");
            }
            else
            {
                var warns = File.ReadAllText(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/warn.txt");
                await ReplyAsync("```\n" + warns + "\n```");
            }
        }

        [Command("bans")]
        [Summary("bans")]
        [Remarks("Users banned by passivebot")]
        public async Task Bans()
        {
            if (!File.Exists(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/ban.txt"))
            {
                await ReplyAsync(
                    $"There are currently no bans in this server, to ban someone type `{Load.Pre}ban @user 'reason'`");
            }
            else
            {
                var bans = File.ReadAllText(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/ban.txt");
                await ReplyAsync("```\n" + bans + "\n```");
            }
        }

        [Command("clear")]
        [Summary("clear <type>")]
        [Remarks("removes ban, kick or warn logs")]
        public async Task Clear([Optional] string type)
        {
            var success = true;
            if (type == null)
            {
                await ReplyAsync("Please specify a type of punishment to clear:\n" +
                    "`warn`, `kick`, `ban`");
                return;
            }
            else if (type == "warn" || File.Exists(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/warn.txt"))
                File.Delete(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/warn.txt");
            else if (type == "kick" || File.Exists(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/kick.txt"))
                File.Delete(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/kick.txt");
            else if (type == "ban" || File.Exists(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/ban.txt"))
                File.Delete(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/ban.txt");
            else
            {
                success = false;
            }
            if (success)
            {
                await ReplyAsync($"All {type}'s have been cleared for this server");
            }
            else
            {
                await ReplyAsync("Invalid type or there are none of the specified type in the server");
            }
        }
    }
}