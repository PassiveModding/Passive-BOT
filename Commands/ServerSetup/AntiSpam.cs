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
using PassiveBOT.Handlers.Services.Interactive;
using PassiveBOT.Handlers.Services.Interactive.Paginator;

namespace PassiveBOT.Commands.ServerSetup
{
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    public class AntiSpam : InteractiveBase
    {
        [Command("SetMuted")]
        [Summary("SetMuted <@role>")]
        [Remarks("Set the Mute Role For your server NOTE: Will try to reset all permissions for that role!")]
        public async Task SetMute(SocketRole muteRole)
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);

            jsonObj.MutedRole = muteRole.Id;
            string perms;
            var channels = "";
            try
            {
                var unverifiedPerms =
                    new OverwritePermissions(sendMessages: PermValue.Deny, addReactions: PermValue.Deny);
                foreach (var channel in Context.Guild.TextChannels)
                    try
                    {
                        await channel.AddPermissionOverwriteAsync(muteRole, unverifiedPerms);
                        channels += $"`#{channel.Name}` Perms Modified\n";
                    }
                    catch
                    {
                        channels += $"`#{channel.Name}` Perms Not Modified\n";
                    }

                perms = "Role Can No longer Send Messages, or Add Reactions";
            }
            catch
            {
                perms = "Role Unable to be modified, ask an administrator to do this manually.";
            }


            GuildConfig.SaveServer(jsonObj);


            await ReplyAsync($"Muted Role has been set as {muteRole.Mention}\n" +
                             $"{perms}\n" +
                             $"{channels}");
        }

        [Command("NoIPs")]
        [Summary("NoIps")]
        [Remarks("Toggle the auto-removal of IP addresses")]
        public async Task NoIP()
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.RemoveIPs = !jsonObj.RemoveIPs;
            GuildConfig.SaveServer(jsonObj);

            await ReplyAsync($"IPs Removal: {jsonObj.RemoveIPs}");
        }

        [Command("NoInvite")]
        [Summary("NoInvite <true/false>")]
        [Remarks("disables/enables the sending of invites in a server from regular members")]
        public async Task NoInvite()
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.Invite = !jsonObj.Invite;
            GuildConfig.SaveServer(jsonObj);

            if (jsonObj.Invite)
                await ReplyAsync("Invite links will now be deleted!");
            else
                await ReplyAsync("Invite links are now allowed to be sent");
        }


        [Command("NoInviteMessage")]
        [Summary("NoInviteMessage <message>")]
        [Remarks("set the no invites message")]
        public async Task NoinviteMSG([Remainder] string noinvmessage = null)
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.NoInviteMessage = noinvmessage;
            GuildConfig.SaveServer(jsonObj);

            await ReplyAsync("The blacklist message is now:\n" +
                             $"{jsonObj.NoInviteMessage ?? "Default"}");
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

        [Command("NoSpam")]
        [Summary("NoSpam")]
        [Remarks("Toggle wether or not to disable spam in the server")]
        public async Task SpamToggle()
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.NoSpam = !jsonObj.NoSpam;
            GuildConfig.SaveServer(jsonObj);

            if (jsonObj.NoSpam)
                await ReplyAsync($"NoSpam: {jsonObj.NoSpam}");
            else
                await ReplyAsync($"NoSpam: {jsonObj.NoSpam}");
        }

        [Command("SkipAntiSpam")]
        [Summary("SkipAntiSpam <message>")]
        [Remarks("Skip antispam on messages starting with the given message (useful for gambling commands)")]
        public async Task SkipAntiSpam([Remainder] string message = null)
        {
            if (message == null)
            {
                await ReplyAsync("Please provide a message that will be skipped.");
                return;
            }

            var guild = GuildConfig.GetServer(Context.Guild);

            if (guild.AntiSpamSkip.Any(x => string.Equals(x, message, StringComparison.CurrentCultureIgnoreCase)))
            {
                await ReplyAsync($"`{message}` is already included in the SkipAntiSpam list");
                return;
            }
            guild.AntiSpamSkip.Add(message);

            GuildConfig.SaveServer(guild);
            await ReplyAsync("Complete.");
        }

        [Command("RemSkipAntiSpam")]
        [Summary("RemSkipAntiSpam <message>")]
        [Remarks("Remove a message from anti spam skipper")]
        public async Task RemSkipAntiSpam([Remainder] string message = null)
        {
            if (message == null)
            {
                await ReplyAsync("Please provide a message that will be removed.");
                return;
            }

            var guild = GuildConfig.GetServer(Context.Guild);

            if (!guild.AntiSpamSkip.Any(x => string.Equals(x, message, StringComparison.CurrentCultureIgnoreCase)))
            {
                await ReplyAsync($"`{message}` is already not included in the SkipAntiSpam list");
                return;
            }
            guild.AntiSpamSkip.Remove(message);

            GuildConfig.SaveServer(guild);
            await ReplyAsync("Complete.");
        }

        [Command("ClearSkipAntiSpam")]
        [Summary("ClearSkipAntiSpam")]
        [Remarks("Clear the SkipAntiSpam List")]
        public async Task ClearAntiSpam()
        {

            var guild = GuildConfig.GetServer(Context.Guild);
            guild.AntiSpamSkip = new List<string>();

            GuildConfig.SaveServer(guild);
            await ReplyAsync("Complete.");
        }

        [Command("SkipAntiSpamList")]
        [Summary("SkipAntiSpamList")]
        [Remarks("List of messages antispam will skip")]
        public async Task SkipAntiSpam()
        {
            var guild = GuildConfig.GetServer(Context.Guild);
            var embed = new EmbedBuilder {Description = string.Join("\n", guild.AntiSpamSkip)};
            await ReplyAsync("", false, embed.Build());
        }

        [Command("NoMassMention")]
        [Summary("NoMassMention")]
        [Remarks("Stops users from tagging more than 5 users or roles in a single message")]
        public async Task NoMassMention()
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.RemoveMassMention = !jsonObj.RemoveMassMention;
            GuildConfig.SaveServer(jsonObj);

            if (jsonObj.RemoveMassMention)
                await ReplyAsync("Mass Mentions will now be deleted!");
            else
                await ReplyAsync("Mass Mentions are now allowed to be sent");
        }

        [Command("NoMention")]
        [Summary("NoMention")]
        [Remarks("disables/enables the use of @ everyone and @ here in a server from regular members")]
        public async Task NoMention()
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.MentionAll = !jsonObj.MentionAll;
            GuildConfig.SaveServer(jsonObj);

            if (jsonObj.MentionAll)
                await ReplyAsync("Everyone and Here mentions will be deleted");
            else
                await ReplyAsync("Everyone and Here mentions will no longer be deleted");
        }

        [Command("NoMentionMessage")]
        [Summary("NoMentionMessage <meggage>")]
        [Remarks("set the no mention message")]
        public async Task NoMentionMSG([Remainder] string noMentionmsg = null)
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.MentionAllMessage = noMentionmsg;
            GuildConfig.SaveServer(jsonObj);

            await ReplyAsync("The blacklist message is now:\n" +
                             $"{jsonObj.MentionAllMessage ?? "Default"}");
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

        [Group("Blacklist")]
        public class Blacklist : InteractiveBase
        {
            [Command("")]
            [Summary("blacklist")]
            [Remarks("displays the blacklist")]
            public async Task B()
            {
                var jsonObj = GuildConfig.GetServer(Context.Guild);
                var pages = new List<PaginatedMessage.Page>();
                var sb = new StringBuilder();
                foreach (var blacklistw in jsonObj.BlacklistWordSet)
                {
                    if (sb.ToString().Length >= 800)
                    {
                        pages.Add(new PaginatedMessage.Page
                        {
                            description = sb.ToString()
                        });
                        sb.Clear();
                    }

                    sb.Append($"**Word(s)**\n" +
                              $"{string.Join("\n", blacklistw.WordList)}\n" +
                              $"**Response**\n" +
                              $"{blacklistw?.BlacklistResponse ?? jsonObj.DefaultBlacklistMessage}\n\n");
                }
                pages.Add(new PaginatedMessage.Page
                {
                    description = sb.ToString()
                });
                var pager = new PaginatedMessage
                {
                    Title = "Blacklisted Messages :stop_button: to remove",
                    Pages = pages
                };

                await PagedReplyAsync(pager, true, true, true);
            }

            [Command("add")]
            [Summary("blacklist add <word> <response>")]
            [Remarks("adds a word to the blacklist, leave response blank to use the default message, use the same response for different blacklisted words to be grouped. Also separate sentences like so: hi_there_person for the keyword")]
            public async Task Ab(string keyword, [Remainder]string response = null)
            {
                var jsonObj = GuildConfig.GetServer(Context.Guild);

                keyword = keyword.Replace("_", " ");

                if (!jsonObj.BlacklistWordSet.Any(x => x.WordList.Contains(keyword)))
                {
                    var blacklistunit = jsonObj.BlacklistWordSet.FirstOrDefault(x => x.BlacklistResponse == response);
                    if (blacklistunit != null)
                    {
                        blacklistunit.WordList.Add(keyword.ToLower());
                        await Context.Message.DeleteAsync();
                        await ReplyAsync("Added to the Blacklist");
                    }
                    else
                    {
                        blacklistunit = new GuildConfig.BlacklistWords
                        {
                            WordList = new List<string>
                            {
                                keyword.ToLower()
                            },
                            BlacklistResponse = response
                        };
                        jsonObj.BlacklistWordSet.Add(blacklistunit);
                        await Context.Message.DeleteAsync();
                        await ReplyAsync("Added to the Blacklist");
                    }

                }
                else
                {
                    await Context.Message.DeleteAsync();
                    await ReplyAsync("Keyword is already in the blacklist");
                    return;
                }

                GuildConfig.SaveServer(jsonObj);
            }

            [Command("del")]
            [Summary("blacklist del <word>")]
            [Remarks("removes a word from the blacklist")]
            public async Task Db(string keyword)
            {
                var jsonObj = GuildConfig.GetServer(Context.Guild);
                keyword = keyword.Replace("_", " ");
                var blacklistunit = jsonObj.BlacklistWordSet.FirstOrDefault(x => x.WordList.Contains(keyword.ToLower()));
                if (blacklistunit != null)
                {

                    blacklistunit.WordList.Remove(keyword.ToLower());
                    if (blacklistunit.WordList.Count == 0)
                    {
                        jsonObj.BlacklistWordSet.Remove(blacklistunit);
                    }
                    await ReplyAsync($"{keyword} is has been removed from the blacklist");
                }
                else
                {
                    await ReplyAsync($"{keyword} is not in the blacklist");
                    return;
                }

                GuildConfig.SaveServer(jsonObj);
            }

            [Command("clear")]
            [Summary("blacklist clear")]
            [Remarks("clears the blacklist")]
            public async Task Clear()
            {
                var jsonObj = GuildConfig.GetServer(Context.Guild);
                jsonObj.BlacklistWordSet = new List<GuildConfig.BlacklistWords>();
                GuildConfig.SaveServer(jsonObj);

                await ReplyAsync("The blacklist has been cleared.");
            }

            [Command("BFToggle")]
            [Summary("blacklist BFToggle")]
            [Remarks("Toggles whether or not to filter special characters for spam")]
            public async Task BFToggle()
            {
                var jsonObj = GuildConfig.GetServer(Context.Guild);
                jsonObj.BlacklistBetterFilter = !jsonObj.BlacklistBetterFilter;
                GuildConfig.SaveServer(jsonObj);

                await ReplyAsync(
                    $"Blacklist BetterFilter status set to {(jsonObj.BlacklistBetterFilter ? "ON" : "OFF")}");
            }

            [Command("defaultmessage")]
            [Summary("blacklist defaultmessage <message>")]
            [Remarks("set the default blacklist message")]
            public async Task BlMessage([Remainder] string blmess = "")
            {
                var jsonObj = GuildConfig.GetServer(Context.Guild);
                jsonObj.DefaultBlacklistMessage = blmess;
                GuildConfig.SaveServer(jsonObj);

                await ReplyAsync("The default blacklist message is now:\n" +
                                 $"{jsonObj.DefaultBlacklistMessage}");
            }
        }
    }
}