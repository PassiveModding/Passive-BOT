using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Configuration;
using PassiveBOT.Preconditions;

namespace PassiveBOT.Commands
{
    [RequireContext(ContextType.Guild)]
    [CheckModerator]
    public class Moderator : ModuleBase
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

        [Command("prune")]
        [Summary("prune <user>")]
        [Remarks("removes most recent messages from a user")]
        public async Task Prune(IUser user)
        {
            await Context.Message.DeleteAsync().ConfigureAwait(false);
            var enumerable = await Context.Channel.GetMessagesAsync().Flatten().ConfigureAwait(false);
            var newlist = enumerable.Where(x => x.Author == user).ToList();
            await Context.Channel.DeleteMessagesAsync(newlist).ConfigureAwait(false);
            await ReplyAsync($"Cleared **{user.Username}'s** Messages (Count = {newlist.Count})");
        }


        [Command("Kick")]
        [Summary("kick <@user> <reason>")]
        [Remarks("Kicks the specified user (requires Admin Permissions)")]
        public async Task Kickuser(SocketGuildUser user, [Remainder] string reason = null)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);

            var embed = new EmbedBuilder();

            if (user.GuildPermissions.Administrator || user.GuildPermissions.KickMembers)
            {
                embed.AddField("User Kick Failed",
                    $"This user is an administrator or has the ability to kick other users");
                await ReplyAsync("", false, embed.Build());
                return;
            }

            if (reason == null)
            {
                embed.AddField("Error", "Please Specify a reason for kicking the user, ie\n" +
                                        "`.kick @noobnoob being a noob");
                await ReplyAsync("", false, embed.Build());
                return;
            }

            var config = GuildConfig.GetServer(Context.Guild);

            var add = new GuildConfig.Kicks
            {
                Moderator = Context.User.Username,
                Reason = reason,
                User = user.Username,
                UserId = user.Id
            };


            try
            {
                await user.KickAsync();
            }
            catch
            {
                embed.AddField("User Kick Failed", $"This user was unable to be kicked");
                await ReplyAsync("", false, embed.Build());
                return;
            }

            config.Kicking.Add(add);
            GuildConfig.SaveServer(config, Context.Guild);

            embed.AddField("User Kicked", $"User: {user.Username}\n" +
                                          $"UserID: {user.Id}\n" +
                                          $"Moderator: {Context.User.Username}\n" +
                                          $"Reason: {reason}");
            await ReplyAsync("", false, embed.Build());
        }

        [Command("Kicks")]
        [Summary("kicks")]
        [Remarks("view all kicks for the current server")]
        public async Task Kicks()
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var embed = new EmbedBuilder();
            embed.WithTitle("Kicks");
            var config = GuildConfig.GetServer(Context.Guild);

            var groupedlist = config.Kicking.GroupBy(x => x.UserId)
                .Select(group => new
                {
                    UserId = group.Key,
                    List = group.ToList()
                })
                .ToList();

            foreach (var group in groupedlist)
            {
                string username;
                try
                {
                    var user = await ((IGuild)Context.Guild).GetUserAsync(group.UserId);
                    username = user.Username;
                }
                catch
                {
                    username = group.List.First().User;
                }
                var list = "";
                foreach (var x in group.List)
                {
                    var moderator =
                        $"{x.Moderator}                                             ".Substring(0, 20);
                    list += $"Mod: {moderator} || Reason: {x.Reason}\n";
                }
                embed.AddField(username, list);
            }

            if (embed.Fields.Count > 0)
                await ReplyAsync("", false, embed.Build());
            else
                await ReplyAsync("There are no kicks in the server...");
        }

        [Command("Warns")]
        [Summary("Warns")]
        [Remarks("view all Warns for the current server")]
        public async Task Warns()
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var embed = new EmbedBuilder();
            embed.WithTitle("Warns");
            var config = GuildConfig.GetServer(Context.Guild);

            var groupedlist = config.Warnings.GroupBy(x => x.UserId)
                .Select(group => new
                {
                    UserId = group.Key,
                    List = group.ToList()
                })
                .ToList();

            foreach (var group in groupedlist)
            {
                string username;
                try
                {
                    var user = await ((IGuild)Context.Guild).GetUserAsync(group.UserId);
                    username = user.Username;
                }
                catch
                {
                    username = group.List.First().User;
                }
                var list = "";
                foreach (var x in group.List)
                {
                    var moderator =
                        $"{x.Moderator}                                             ".Substring(0, 20);
                    list += $"Mod: {moderator} || Reason: {x.Reason}\n";
                }
                embed.AddField(username, list);
            }

            if (embed.Fields.Count > 0)
                await ReplyAsync("", false, embed.Build());
            else
                await ReplyAsync("There are no warns in the server...");
        }
    }
}
