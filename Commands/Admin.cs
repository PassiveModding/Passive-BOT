using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using PassiveBOT.Configuration;

namespace PassiveBOT.Commands
{
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    public class Admin : InteractiveBase
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



        [Command("EmbedBuilder", RunMode = RunMode.Async)]
        [Summary("EmbedBuilder")]
        [Remarks("Create an embedded message")]
        public async Task BuildEmbed()
        {
            var embed = new EmbedBuilder();
            var embedbuilt = await ReplyAsync("", false, embed.Build());

            await ReplyAndDeleteAsync("```\n" +
                             "Reply addon you would like to perform\n" +
                             "[1] Set the Title\n" +
                             "[2] Set the Description\n" +
                             "[3] Add a Field\n" +
                             "[4] Finish\n" +
                             "```");
            var n1 = await NextMessageAsync();
            if (n1.Content.StartsWith("1"))
            {
                await ReplyAndDeleteAsync("In the next message you send please specify the embed title");
                var ntitle = await NextMessageAsync();
                embed.WithTitle(ntitle.Content);
                await embedbuilt.ModifyAsync(x =>
                {
                    x.Embed = embed.Build();
                });
                await ntitle.DeleteAsync();
            }
            else if (n1.Content.StartsWith("2"))
            {
                await ReplyAndDeleteAsync("In the next message you send please specify the embed description");
                var ndesc = await NextMessageAsync();
                embed.WithDescription(ndesc.Content);
                await embedbuilt.ModifyAsync(x =>
                {
                    x.Embed = embed.Build();
                });
                await ndesc.DeleteAsync();
            }
            else if (n1.Content.StartsWith("3"))
            {
                await ReplyAndDeleteAsync("In the next message, specify the field title");
                var nfieldtitle = await NextMessageAsync();
                await ReplyAndDeleteAsync("In the next message, specify the field description");
                var nfielddesc = await NextMessageAsync();
                embed.AddField(nfieldtitle.Content, nfielddesc.Content);
                await embedbuilt.ModifyAsync(x =>
                {
                    x.Embed = embed.Build();
                });
                await nfielddesc.DeleteAsync();
                await nfieldtitle.DeleteAsync();
            }
            else if (n1.Content.StartsWith("4"))
            {
                return;
            }
            else
            {
                await ReplyAsync("Invalid number... exiting");
                return;
            }


            await ReplyAsync("```\n" +
                             "Reply addon you would like to perform\n" +
                             "[1] Set the Title\n" +
                             "[2] Set the Description\n" +
                             "[3] Add a Field\n" +
                             "[4] Finish\n" +
                             "```");
            var n2 = await NextMessageAsync();

            if (n2.Content.StartsWith("1"))
            {
                await ReplyAndDeleteAsync("In the next message you send please specify the embed title");
                var ntitle = await NextMessageAsync();
                embed.WithTitle(ntitle.Content);
                await embedbuilt.ModifyAsync(x =>
                {
                    x.Embed = embed.Build();
                });
                await ntitle.DeleteAsync();
            }
            else if (n2.Content.StartsWith("2"))
            {
                await ReplyAndDeleteAsync("In the next message you send please specify the embed description");
                var ndesc = await NextMessageAsync();
                embed.WithDescription(ndesc.Content);
                await embedbuilt.ModifyAsync(x =>
                {
                    x.Embed = embed.Build();
                });
                await ndesc.DeleteAsync();
            }
            else if (n2.Content.StartsWith("3"))
            {
                await ReplyAndDeleteAsync("In the next message, specify the field title");
                var nfieldtitle = await NextMessageAsync();
                await ReplyAndDeleteAsync("In the next message, specify the field description");
                var nfielddesc = await NextMessageAsync();
                embed.AddField(nfieldtitle.Content, nfielddesc.Content);
                await embedbuilt.ModifyAsync(x =>
                {
                    x.Embed = embed.Build();
                });
                await nfieldtitle.DeleteAsync();
                await nfielddesc.DeleteAsync();
            }
            else if (n2.Content.StartsWith("4"))
            {
                return;
            }
            else
            {
                await ReplyAsync("Invalid number... exiting");
                return;
            }

            await ReplyAsync("```\n" +
                             "Reply addon you would like to perform\n" +
                             "[1] Set the Title\n" +
                             "[2] Set the Description\n" +
                             "[3] Add a Field\n" +
                             "[4] Finish\n" +
                             "```");
            var n3 = await NextMessageAsync();

            if (n3.Content.StartsWith("1"))
            {
                await ReplyAsync("In the next message you send please specify the embed title");
                var ntitle = await NextMessageAsync();
                embed.WithTitle(ntitle.Content);
                await embedbuilt.ModifyAsync(x =>
                {
                    x.Embed = embed.Build();
                });
                await ntitle.DeleteAsync();
            }
            else if (n3.Content.StartsWith("2"))
            {
                await ReplyAsync("In the next message you send please specify the embed description");
                var ndesc = await NextMessageAsync();
                embed.WithDescription(ndesc.Content);
                await embedbuilt.ModifyAsync(x =>
                {
                    x.Embed = embed.Build();
                });
                await ndesc.DeleteAsync();
            }
            else if (n3.Content.StartsWith("3"))
            {
                await ReplyAndDeleteAsync("In the next message, specify the field title");
                var nfieldtitle = await NextMessageAsync();
                await ReplyAndDeleteAsync("In the next message, specify the field description");
                var nfielddesc = await NextMessageAsync();
                embed.AddField(nfieldtitle.Content, nfielddesc.Content);
                await embedbuilt.ModifyAsync(x =>
                {
                    x.Embed = embed.Build();
                });
                await nfielddesc.DeleteAsync();
                await nfieldtitle.DeleteAsync();
            }
            else if (n3.Content.StartsWith("4"))
            {
            }
            else
            {
                await ReplyAsync("Invalid number... exiting");
            }
        }

        [Command("Kick")]
        [Summary("kick <@user> <reason>")]
        [Remarks("Kicks the specified user (requires Admin Permissions)")]
        public async Task Kickuser(SocketGuildUser user, [Remainder] string reason = null)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
            {
                GuildConfig.Setup(Context.Guild);
            }

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

            var config = JsonConvert.DeserializeObject<GuildConfig>(File.ReadAllText(file));

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
            {
                GuildConfig.Setup(Context.Guild);
            }
            var embed = new EmbedBuilder();
            embed.WithTitle("Kicks");
            var config = JsonConvert.DeserializeObject<GuildConfig>(File.ReadAllText(file));

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

        [Command("Ban")]
        [Summary("Ban <@user> <reason>")]
        [Remarks("Bans the specified user (requires Admin Permissions)")]
        public async Task BanUser(SocketGuildUser user, [Remainder] string reason = null)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
            {
                GuildConfig.Setup(Context.Guild);
            }
            var embed = new EmbedBuilder();
            if (reason == null)
            {
                embed.AddField("Error", "Please Specify a reason for banning the user, ie\n" +
                                        "`.ban @noobnoob being a noob");
                await ReplyAsync("", false, embed.Build());
                return;
            }
            var config = JsonConvert.DeserializeObject<GuildConfig>(File.ReadAllText(file));

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
                    $"This user is an administrator or has the ability to ban other users");
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
                embed.AddField("User Ban Failed", $"This user was unable to be banned");
                await ReplyAsync("", false, embed.Build());
                return;
            }

            config.Banning.Add(add);
            GuildConfig.SaveServer(config, Context.Guild);

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
            {
                GuildConfig.Setup(Context.Guild);
            }

            var embed = new EmbedBuilder();
            embed.WithTitle("Bans");
            var config = JsonConvert.DeserializeObject<GuildConfig>(File.ReadAllText(file));

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
                await ReplyAsync("There are no bans in the server...");
        }

        [Command("Warn")]
        [Summary("Warn <@user> <reason>")]
        [Remarks("Warns the specified user (requires Admin Permissions)")]
        public async Task WarnUser(SocketGuildUser user, [Remainder] string reason = null)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
            {
                GuildConfig.Setup(Context.Guild);
            }
            var embed = new EmbedBuilder();
            if (reason == null)
            {
                embed.AddField("Error", "Please Specify a reason for warning the user, ie\n" +
                                        "`.warn @noobnoob being a noob");
                await ReplyAsync("", false, embed.Build());
                return;
            }
            var config = JsonConvert.DeserializeObject<GuildConfig>(File.ReadAllText(file));

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
            GuildConfig.SaveServer(config, Context.Guild);

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
            {
                GuildConfig.Setup(Context.Guild);
            }
            var embed = new EmbedBuilder();
            embed.WithTitle("Warns");
            var config = JsonConvert.DeserializeObject<GuildConfig>(File.ReadAllText(file));

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

        [Command("ClearWarn")]
        [Summary("ClearWarn")]
        [Remarks("Clears warnings for the specified user")]
        public async Task ClearWarn(SocketGuildUser removeuser)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
            {
                GuildConfig.Setup(Context.Guild);
            }
            var embed = new EmbedBuilder();
            embed.WithTitle("Warns Removed");
            var config = JsonConvert.DeserializeObject<GuildConfig>(File.ReadAllText(file));
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
            GuildConfig.SaveServer(config, Context.Guild);
            await ReplyAsync($"Warnings for the user {removeuser} have been cleared", false, embed.Build());
        }

        [Command("ClearKick")]
        [Summary("ClearKick")]
        [Remarks("Clears Kicks for the specified user")]
        public async Task ClearKick(SocketGuildUser removeuser)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
            {
                GuildConfig.Setup(Context.Guild);
            }
            var embed = new EmbedBuilder();
            embed.WithTitle("Kicks Removed");
            var config = JsonConvert.DeserializeObject<GuildConfig>(File.ReadAllText(file));
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
            GuildConfig.SaveServer(config, Context.Guild);
            await ReplyAsync($"Kicks for the user {removeuser} have been cleared", false, embed.Build());
        }

        [Command("ClearBan")]
        [Summary("ClearBan")]
        [Remarks("Clears Bans for the specified user")]
        public async Task ClearBan(SocketGuildUser removeuser)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
            {
                GuildConfig.Setup(Context.Guild);
            }
            var embed = new EmbedBuilder();
            embed.WithTitle("Bans Removed");
            var config = JsonConvert.DeserializeObject<GuildConfig>(File.ReadAllText(file));
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
            GuildConfig.SaveServer(config, Context.Guild);
            await ReplyAsync($"Warnings for the user {removeuser} have been cleared", false, embed.Build());
        }
    }
}