using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Configuration;
using Color = ImageSharp.Color;

namespace PassiveBOT.Commands
{
    public class Invite : ModuleBase
    {
        [Command("invite")]
        [Summary("invite")]
        [Remarks("Returns the OAuth2 Invite URL of the bot")]
        public async Task InviteBot()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            await ReplyAsync(
                $"A user with `MANAGE_SERVER` can invite me to your server here: <https://discordapp.com/oauth2/authorize?client_id={application.Id}&scope=bot&permissions=2146958591>");
        }
    }

    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    public class Admin : ModuleBase
    {
        //modified from Nadeko
        private static readonly ConcurrentDictionary<ulong, Timer> RotatingRoleColors =
            new ConcurrentDictionary<ulong, Timer>();

        [Command("qc")]
        [Summary("qc '@role' '60'")]
        [Remarks("quickcolour role")]
        public async Task Quickcolour([Remainder, Optional]string empty)
        {
            await ReplyAsync(
                "this command has been removed from PassiveBOT, however it has been added to our new bot RainbowBOT, you can get an invite link and some help here: http://passivenation.com/showthread.php?tid=64");
        }

        [Command("prune")]
        [Summary("prune")]
        [Remarks("removes all the bots recent messages")]
        public async Task Prune()
        {
            var user = await Context.Guild.GetCurrentUserAsync().ConfigureAwait(false);

            var enumerable = (await Context.Channel.GetMessagesAsync().Flatten()).AsEnumerable();
            enumerable = enumerable.Where(x => x.Author.Id == user.Id);
            await Context.Channel.DeleteMessagesAsync(enumerable).ConfigureAwait(false);
        }

        [Command("clear")]
        [Summary("clear 26")]
        [Remarks("removes the specified amount of messages")]
        public async Task Clear([Optional] int count)
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
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "moderation/prefix/")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "moderation/prefix/"));

            var lines = File.ReadAllLines(AppContext.BaseDirectory + "moderation/prefix/nopre.txt");
            var result = lines.ToList();
            if (result.Contains(Context.Guild.Id.ToString()))
            {
                var oldLines = File.ReadAllLines($"{AppContext.BaseDirectory + "moderation/prefix/nopre.txt"}");
                var newLines = oldLines.Where(line => !line.Contains(Context.Guild.Id.ToString()));
                File.WriteAllLines($"{AppContext.BaseDirectory + "moderation/prefix/nopre.txt"}", newLines);
                await ReplyAsync(
                    $"{Context.Guild} has been removed from the noprefix list (secret commands and prefixless commands are now enabled)");
            }
            else
            {
                File.AppendAllText($"{AppContext.BaseDirectory + "moderation/prefix/nopre.txt"}",
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
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "moderation/error/")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "moderation/error/"));

            var lines = File.ReadAllLines(AppContext.BaseDirectory + "moderation/error/logging.txt");
            var result = lines.ToList();
            if (result.Contains(Context.Guild.Id.ToString()))
            {
                var oldLines = File.ReadAllLines($"{AppContext.BaseDirectory + "moderation/error/logging.txt"}");
                var newLines = oldLines.Where(line => !line.Contains(Context.Guild.Id.ToString()));
                File.WriteAllLines($"{AppContext.BaseDirectory + "moderation/error/logging.txt"}", newLines);
                await ReplyAsync($"I will no longer reply if an error is thrown in {Context.Guild}");
            }
            else
            {
                File.AppendAllText($"{AppContext.BaseDirectory + "moderation/error/logging.txt"}",
                    $"{Context.Guild.Id}" + Environment.NewLine);
                await ReplyAsync($"I will now reply if an error is thrown in {Context.Guild}");
            }
        }

        [Command("kick")]
        [Summary("kick '@badperson' 'for not being cool'")]
        [Remarks("Kicks the specified user (requires Kick Permissions)")]
        public async Task Kickuser(SocketGuildUser user, [Remainder] [Optional] string reason)
        {
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

                    File.AppendAllText(AppContext.BaseDirectory + $"moderation/kick/{Context.Guild.Id}.txt",
                        $"User: {user} || Moderator: {Context.User} || Reason: {reason}" + Environment.NewLine);
                    success = true;
                }
                catch
                {
                    await ReplyAsync(
                        "**ERROR: **I was unable to kick the specified user, please check I have sufficient permissions");
                }
                if (success)
                {
                    await ReplyAsync($"{user} has been kicked for `{reason}`:bangbang: ");
                    var dm = await user.CreateDMChannelAsync();
                    await dm.SendMessageAsync(
                        $"{user.Mention} you have been kicked from {Context.Guild} for `{reason}`");
                }
            }
        }

        [Command("warn")]
        [Summary("warn '@naughtykiddo' 'for being a noob'")]
        [Remarks("warns the specified user")]
        public async Task NewWarnuser(SocketGuildUser user, [Remainder] [Optional] string reason)
        {
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

                File.AppendAllText(AppContext.BaseDirectory + $"moderation/warn/{Context.Guild.Id}.txt",
                    $"User: {user} || Moderator: {Context.User} || Reason: {reason}" + Environment.NewLine);
            }
        }

        [Command("ban")]
        [Summary("ban 'badfag' 'for sucking'")]
        [Remarks("bans the specified user (requires Ban Permissions)")]
        public async Task Banuser(SocketGuildUser user, [Remainder] [Optional] string reason)
        {
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

                    File.AppendAllText(AppContext.BaseDirectory + $"moderation/ban/{Context.Guild.Id}.txt",
                        $"User: {user} || Moderator: {Context.User} || Reason: {reason}" + Environment.NewLine);
                    success = true;
                }
                catch
                {
                    await ReplyAsync(
                        "**ERROR: **I was unable to ban the specified user, please check I have sufficient permissions");
                }
                if (success)
                {
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
            if (!File.Exists(AppContext.BaseDirectory + $"moderation/kick/{Context.Guild.Id}.txt"))
                await ReplyAsync(
                    $"There are currently no kicks in this server, to kick someone type `{Load.Pre}kick @user 'reason'`");
            var kicks = File.ReadAllText(AppContext.BaseDirectory + $"moderation/kick/{Context.Guild.Id}.txt");
            await ReplyAsync("```\n" + kicks + "\n```");
        }

        [Command("warns")]
        [Summary("warns")]
        [Remarks("Users warned by passivebot")]
        public async Task Warns()
        {
            if (!File.Exists(AppContext.BaseDirectory + $"moderation/warn/{Context.Guild.Id}.txt"))
                await ReplyAsync(
                    $"There are currently no warns in this server, to warn someone type `{Load.Pre}warn @user 'reason'`");
            var warns = File.ReadAllText(AppContext.BaseDirectory + $"moderation/warn/{Context.Guild.Id}.txt");
            await ReplyAsync("```\n" + warns + "\n```");
        }

        [Command("bans")]
        [Summary("bans")]
        [Remarks("Users banned by passivebot")]
        public async Task Bans()
        {
            if (!File.Exists(AppContext.BaseDirectory + $"moderation/ban/{Context.Guild.Id}.txt"))
                await ReplyAsync(
                    $"There are currently no bans in this server, to ban someone type `{Load.Pre}ban @user 'reason'`");
            var bans = File.ReadAllText(AppContext.BaseDirectory + $"moderation/ban/{Context.Guild.Id}.txt");
            await ReplyAsync("```\n" + bans + "\n```");
        }
    }
}