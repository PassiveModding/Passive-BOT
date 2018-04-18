using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using PassiveBOT.Configuration;
using PassiveBOT.Preconditions;

namespace PassiveBOT.Commands.ServerModeration
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
                try
                {
                    await Context.Channel.DeleteMessagesAsync(enumerable).ConfigureAwait(false);
                }
                catch
                {
                    //
                }

                await ReplyAsync($"Cleared **{count}** Messages");
            }
        }

        [Command("pruneUser")]
        [Summary("pruneUser <user>")]
        [Remarks("removes messages from a user in the last 100 messages")]
        public async Task Prune(IUser user)
        {
            await Context.Message.DeleteAsync().ConfigureAwait(false);
            var enumerable = await Context.Channel.GetMessagesAsync().Flatten().ConfigureAwait(false);
            var messages = enumerable as IMessage[] ?? enumerable.ToArray();
            var newlist = messages.Where(x => x.Author == user).ToList();
            try
            {
                await Context.Channel.DeleteMessagesAsync(newlist).ConfigureAwait(false);
            }
            catch
            {
                //
            }

            await ReplyAsync($"Cleared **{user.Username}'s** Messages (Count = {newlist.Count})");
        }


        [Command("pruneID")]
        [Summary("pruneID <userID>")]
        [Remarks("removes messages from a user ID in the last 100 messages")]
        public async Task Prune(ulong userID)
        {
            await Context.Message.DeleteAsync().ConfigureAwait(false);
            var enumerable = await Context.Channel.GetMessagesAsync().Flatten().ConfigureAwait(false);
            var messages = enumerable as IMessage[] ?? enumerable.ToArray();
            var newlist = messages.Where(x => x.Author.Id == userID).ToList();
            try
            {
                await Context.Channel.DeleteMessagesAsync(newlist).ConfigureAwait(false);
            }
            catch
            {
                //
            }

            await ReplyAsync($"Cleared Messages (Count = {newlist.Count})");
        }

        
        [Command("pruneRole")]
        [Summary("pruneRole <@role>")]
        [Remarks("removes messages from a role in the last 100 messages")]
        public async Task Prune(IRole role)
        {
            await Context.Message.DeleteAsync().ConfigureAwait(false);
            var enumerable = await Context.Channel.GetMessagesAsync().Flatten().ConfigureAwait(false);
            var messages = enumerable as IMessage[] ?? enumerable.ToArray();
            var newerlist = messages.ToList().Where(x => Context.Guild.GetUserAsync(x.Author.Id).Result != null && Context.Guild.GetUserAsync(x.Author.Id).Result.RoleIds.Contains(role.Id)).ToList();

            try
            {
                await Context.Channel.DeleteMessagesAsync(newerlist).ConfigureAwait(false);
            }
            catch
            {
                //
            }

            await ReplyAsync($"Cleared Messages (Count = {newerlist.Count()})");
        }
        

        [Command("Kick")]
        [Summary("kick <@user> <reason>")]
        [Remarks("Kicks the specified user (Admin)")]
        public async Task Kickuser(SocketGuildUser user, [Remainder] string reason = null)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);

            var embed = new EmbedBuilder();

            if (user.GuildPermissions.Administrator || user.GuildPermissions.KickMembers)
            {
                embed.AddField("User Kick Failed",
                    "This user is an administrator or has the ability to kick other users");
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
                embed.AddField("User Kick Failed", "This user was unable to be kicked");
                await ReplyAsync("", false, embed.Build());
                return;
            }

            config.Kicking.Add(add);
            GuildConfig.SaveServer(config);

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
                    var user = await Context.Guild.GetUserAsync(group.UserId);
                    username = user.Username;
                }
                catch
                {
                    username = group.List.First().User;
                }

                var list = "";
                foreach (var x in group.List)
                {
                    if (list.Length >= 800)
                    {
                        embed.AddField($"{username} [{group.UserId}]", list);
                        list = "";
                    }

                    var moderator =
                        $"{x.Moderator}                                             ".Substring(0, 20);
                    list += $"Mod: {moderator} || Reason: {x.Reason}\n";
                }

                embed.AddField($"{username} [{group.UserId}]", list);
            }

            if (embed.Fields.Count > 0)
                await ReplyAsync("", false, embed.Build());
            else
                await ReplyAsync("There are no kicks in the server...");
        }

        [Command("Warn")]
        [Summary("Warn <@user> <reason>")]
        [Remarks("Warns the specified user")]
        public async Task WarnUser(SocketGuildUser user, [Remainder] string reason = null)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var embed = new EmbedBuilder();
            if (reason == null)
            {
                embed.AddField("Error", "Please Specify a reason for warning the user, ie\n" +
                                        "`.warn @noobnoob being a noob");
                await ReplyAsync("", false, embed.Build());
                return;
            }

            var config = GuildConfig.GetServer(Context.Guild);

            var add = new GuildConfig.Warns
            {
                Moderator = Context.User.Username,
                Reason = reason,
                User = user.Username,
                UserId = user.Id
            };

            try
            {
                await user.SendMessageAsync($"You have been warned in {Context.Guild.Name} for:\n" +
                                            $"`{reason}`");
            }
            catch
            {
                //
            }

            config.Warnings.Add(add);
            GuildConfig.SaveServer(config);

            embed.AddField("User Warned", $"User: {user.Username}\n" +
                                          $"UserID: {user.Id}\n" +
                                          $"Moderator: {Context.User.Username}\n" +
                                          $"Reason: {reason}");
            await ReplyAsync("", false, embed.Build());
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
                    var user = await Context.Guild.GetUserAsync(group.UserId);
                    username = user.Username;
                }
                catch
                {
                    username = group.List.First().User;
                }

                var list = "";
                foreach (var x in group.List)
                {
                    if (list.Length >= 800)
                    {
                        embed.AddField($"{username} [{group.UserId}]", list);
                        list = "";
                    }

                    var moderator =
                        $"{x.Moderator}                                             ".Substring(0, 20);
                    list += $"Mod: {moderator} || Reason: {x.Reason}\n";
                }

                embed.AddField($"{username} [{group.UserId}]", list);
            }

            if (embed.Fields.Count > 0)
                await ReplyAsync("", false, embed.Build());
            else
                await ReplyAsync("There are no warns in the server...");
        }

        [Command("HackBan")]
        [Summary("HackBan <User ID>")]
        [Remarks("Bans the specified user from a server they are not in (Admin)")]
        public async Task BanUser(ulong UserID)
        {
            if (!((SocketGuildUser) Context.User).GuildPermissions.Administrator)
            {
                await ReplyAsync("Admin Only!");
                return;
            }

            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var config = GuildConfig.GetServer(Context.Guild);

            var add = new GuildConfig.Bans
            {
                Moderator = Context.User.Username,
                Reason = "HackBan",
                User = $"{UserID}",
                UserId = UserID
            };

            try
            {
                await Context.Guild.AddBanAsync(UserID);
            }
            catch
            {
                await ReplyAsync("This user was unable to be banned");
                return;
            }

            config.Banning.Add(add);
            GuildConfig.SaveServer(config);
            await ReplyAsync("Success!! User Banned");
        }

        [Command("Ban")]
        [Summary("Ban <@user> <reason>")]
        [Remarks("Bans the specified user (requires Admin Permissions)")]
        public async Task BanUser(SocketGuildUser user, [Remainder] string reason = null)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var embed = new EmbedBuilder();
            if (reason == null)
            {
                embed.AddField("Error", "Please Specify a reason for banning the user, ie\n" +
                                        "`.ban @noobnoob being a noob");
                await ReplyAsync("", false, embed.Build());
                return;
            }

            var config = GuildConfig.GetServer(Context.Guild);

            var add = new GuildConfig.Bans
            {
                Moderator = Context.User.Username,
                Reason = reason,
                User = user.Username,
                UserId = user.Id
            };

            if (user.GuildPermissions.Administrator || user.GuildPermissions.BanMembers)
            {
                embed.AddField("User Ban Failed",
                    "This user is an administrator or has the ability to ban other users");
                await ReplyAsync("", false, embed.Build());
                return;
            }

            try
            {
                await user.SendMessageAsync($"You have been banned from {Context.Guild.Name} for:\n" +
                                            $"`{reason}`");
                await Context.Guild.AddBanAsync(user);
            }
            catch
            {
                embed.AddField("User Ban Failed", "This user was unable to be banned");
                await ReplyAsync("", false, embed.Build());
                return;
            }

            config.Banning.Add(add);
            GuildConfig.SaveServer(config);

            embed.AddField("User Banned", $"User: {user.Username}\n" +
                                          $"UserID: {user.Id}\n" +
                                          $"Moderator: {Context.User.Username}\n" +
                                          $"Reason: {reason}");
            await ReplyAsync("", false, embed.Build());
        }

        [Command("Bans")]
        [Summary("Bans")]
        [Remarks("view all Bans for the current server")]
        public async Task Bans()
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);

            var embed = new EmbedBuilder();
            embed.WithTitle("Bans");
            var config = GuildConfig.GetServer(Context.Guild);

            var groupedlist = config.Banning.GroupBy(x => x.UserId)
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
                    var user = await Context.Guild.GetUserAsync(group.UserId);
                    username = user.Username;
                }
                catch
                {
                    username = group.List.First().User;
                }

                var list = "";
                foreach (var x in group.List)
                {
                    if (list.Length >= 800)
                    {
                        embed.AddField($"{username} [{group.UserId}]", list);
                        list = "";
                    }

                    var moderator =
                        $"{x.Moderator}                                             ".Substring(0, 20);
                    list += $"Mod: {moderator} || Reason: {x.Reason}\n";
                }

                embed.AddField($"{username} [{group.UserId}]", list);
            }

            if (embed.Fields.Count > 0)
                await ReplyAsync("", false, embed.Build());
            else
                await ReplyAsync("There are no bans in the server...");
        }

        [Command("Mute")]
        [Summary("Mute <@user>")]
        [Remarks("Mute a user from all chats")]
        public async Task Mute(IGuildUser user)
        {
            // 1. Check the user isnt an admin or moderator
            // 2. Try to get the muted role
            // 3. If the muted role is not setup, unavailable or does not exist, return.
            // 4. Try to mute the user.

            if (user.RoleIds.Contains(GuildConfig.GetServer(Context.Guild).ModeratorRoleId) ||
                user.GuildPermissions.Administrator)
            {
                await ReplyAsync("ERROR: User is a moderator or administrator");
                return;
            }

            IRole mutedrole;
            try
            {
                var mrole = GuildConfig.GetServer(Context.Guild).MutedRole;
                if (mrole == 0)
                {
                    await ReplyAsync("The servers muted role is not setup");
                    return;
                }

                mutedrole = Context.Guild.GetRole(mrole);
            }
            catch
            {
                await ReplyAsync("Muted Role has been deleted or could not be found. ERROR");
                return;
            }


            if (mutedrole == null)
            {
                await ReplyAsync("Muted Role has been deleted or could not be found. ERROR");
                return;
            }

            try
            {
                await user.AddRoleAsync(mutedrole);
                await ReplyAsync($"SUCCESS. The user has been muted and added to the muted role: {mutedrole.Mention}");
            }
            catch (Exception e)
            {
                await ReplyAsync("Use role unable to be modified. ERROR\n" +
                                 $"{e}");
            }
        }

        [Command("Unmute")]
        [Summary("Unmute <@user>")]
        [Remarks("Unmute a user")]
        public async Task Unmute(IGuildUser user)
        {
            var muteId = GuildConfig.GetServer(Context.Guild).MutedRole;
            var muterole = Context.Guild.GetRole(muteId);
            if (user.RoleIds.Contains(muteId))
            {
                await user.RemoveRoleAsync(muterole);
                await ReplyAsync("SUCCESS! User unmuted.");
                return;
            }

            await ReplyAsync("User was not muted to begin with.");
        }
    }
}