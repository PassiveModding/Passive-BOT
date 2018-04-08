using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Configuration;

namespace PassiveBOT.Commands.ServerSetup
{
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    public class GuildSetup : InteractiveBase
    {
        [Command("Setup", RunMode = RunMode.Async)]
        [Summary("Setup")]
        [Remarks("Initialises the servers configuration file")]
        public async Task Setup(int i = 0)
        {
            if (i == 0)
            {
                await ReplyAsync("```\n" +
                                 "Reply with the command you would like to perform\n" +
                                 "[1] Initialise the config file\n" +
                                 "[2] Read the config file\n" +
                                 "[3] Delete the config file\n" +
                                 "" +
                                 "```");
                return;
            }
            if (i == 1)
            {
                GuildConfig.Setup(Context.Guild);
                await ConfigInfo();
            }
            else if (i == 2)
            {
                await ConfigInfo();
            }
            else if (i== 3)
            {
                var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
                if (File.Exists(file))
                {
                    File.Delete(file);
                    await ReplyAsync("The config file has been deleted.");
                }
                else
                {
                    await ReplyAsync($"The config file does not exist, please use option 1 to initialise it");
                }
            }
            else
            {
                await ReplyAsync(
                    "Please only reply with the character of your option ie. if you picked 2, reply with just `2`");
            }
        }


        public async Task ConfigInfo()
        {
            var embed = new EmbedBuilder();
            var l = GuildConfig.GetServer(Context.Guild);
            string djstring;
            string guildstring;
            //string errorLogString;
            string subrolelist;
            string rss;
            string tags;
            string welcome;
            string goodbye;
            string nomention;
            string noinvite;
            string eventlogging;
            string admincommands;
            string blacklist;
            string modRole;


            try
            {
                subrolelist = "";
                foreach (var role in l.RoleList)
                    subrolelist += $"{Context.Guild.GetRole(role).Name}\n";
            }
            catch
            {
                subrolelist = "There are no joinable roles";
            }
            try
            {
                if (l.ModeratorRoleId == 0)
                {
                    modRole = "N/A";
                }
                else
                {
                    var mrole = Context.Guild.GetRole(l.ModeratorRoleId);
                    modRole = mrole.Name;
                }
            }
            catch
            {
                modRole = $"@deleted-role <{l.ModeratorRoleId}>";
            }
            try
            {
                djstring = Context.Guild.GetRole(l.DjRoleId).Name;
            }
            catch
            {
                djstring = "There is no DJ role configured";
            }
            try
            {
                guildstring = $"{l.GuildName}, {l.GuildId}";
            }
            catch
            {
                guildstring = $"{Context.Guild.Name}, {Context.Guild.Id}";
            }
            /*try
            {
                errorLogString = l.ErrorLog ? "Status: On" : "Status: Off";
            }
            catch
            {
                errorLogString = "Status: Off";
            }*/
            try
            {
                rss = $"{l.Rss}, {Context.Guild.GetChannel(l.RssChannel).Name}";
            }
            catch
            {
                rss = "Status: Disabled";
            }
            try
            {
                var dict = GuildConfig.GetServer(Context.Guild).Dict;
                var list = "";
                foreach (var tagging in dict)
                    list += $"{tagging.Tagname}, ";

                tags = list.Substring(0, list.Length - 2);
            }
            catch
            {
                tags = "there are none yet....";
            }
            try
            {
                var status = l.WelcomeEvent ? "On" : "Off";
                welcome = $"Status: {status}\n" +
                          $"Channel: {Context.Guild.GetChannel(l.WelcomeChannel).Name}\n" +
                          $"Message: {l.WelcomeMessage}";
            }
            catch
            {
                welcome = "Status: Disabled";
            }
            try
            {
                var status = l.GoodbyeEvent ? "On" : "Off";
                goodbye = $"Status: {status}\n" +
                          $"Channel: {Context.Guild.GetChannel(l.GoodByeChannel).Name}\n" +
                          $"Message: {l.GoodbyeMessage}";
            }
            catch
            {
                goodbye = "Status: Disabled";
            }
            try
            {
                nomention = l.MentionAll ? "Status: On" : "Status: Off";
            }
            catch
            {
                nomention = "Status: Off";
            }
            try
            {
                noinvite = l.Invite ? "Status: On" : "Status: Off";
            }
            catch
            {
                noinvite = "Status: Off";
            }
            try
            {
                if (l.EventLogging)
                    eventlogging = $"Status: On\n" +
                                   $"Channel: {Context.Guild.GetChannel(l.EventChannel).Name}";
                else
                    eventlogging = "Status: Off";
            }
            catch
            {
                eventlogging = "Disabled";
            }
            try
            {
                admincommands = $"Warnings: {l.Warnings.Count}\n" +
                                $"Kicks: {l.Kicking.Count}\n" +
                                $"Bans: {l.Banning.Count}";
            }
            catch
            {
                admincommands = "Warnings: 0\n" +
                                "Kicks: 0\n" +
                                "Bans: 0";
            }
            try
            {
                blacklist = $"{l.Blacklist.Count}";
            }
            catch
            {
                blacklist = $"0";
            }


            embed.AddField("DJ Role", $"Role: {djstring}");
            embed.AddField("Guild Name & ID", guildstring);
            //embed.AddField("Error logging", $"Status: {errorLogString}");
            embed.AddField("SubRoles", $"Role: {subrolelist}");
            embed.AddField("RSS URL/Channel", $"{rss}");
            embed.AddField("Tags", $"{tags}");
            embed.AddField("Welcome", $"{welcome}");
            embed.AddField("Goodbye", $"{goodbye}");
            embed.AddField("NoMention", $"{nomention}");
            embed.AddField("NoInvite", $"Status: {noinvite}");
            embed.AddField("EventLogging", eventlogging);
            embed.AddField("Admin Uses", admincommands);
            embed.AddField("Blacklisted Word Count", blacklist);
            embed.AddField("Moderator Role", modRole);


            await ReplyAsync("", false, embed.Build());
        }

        [Command("Welcome", RunMode = RunMode.Async)]
        [Remarks("Setup the Welcome Message for new users")]
        [Summary("Welcome")]
        public async Task InitialiseWelcome()
        {
            await ReplyAsync("```\n" +
                             "Reply with the command you would like to perform\n" +
                             "[1] Set the welcome message\n" +
                             "[2] Set the current channel for welcome events\n" +
                             "[3] Enable the welcome event\n" +
                             "[4] Disable the welcome event\n" +
                             "[5] View Welcome Info" +
                             "" +
                             "```");

            var next = await NextMessageAsync();
            if (next.Content == "1")
            {
                await ReplyAsync(
                    "Please reply with the welcome message you want to be sent when a user joins the server ie. `Welcome to Our Server!!`");
                var next2 = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
                GuildConfig.SetWMessage(Context.Guild, next2.Content);
                GuildConfig.SetWChannel(Context.Guild, Context.Channel.Id);
                await ReplyAsync("The Welcome Message for this server has been set to:\n" +
                                 $"**{next2.Content}**\n" +
                                 $"In the channel **{Context.Channel.Name}**");
            }
            else if (next.Content == "2")
            {
                GuildConfig.SetWChannel(Context.Guild, Context.Channel.Id);
                await ReplyAsync($"Welcome Events will be sent in the channel: **{Context.Channel.Name}**");
            }
            else if (next.Content == "3")
            {
                GuildConfig.SetWelcomeStatus(Context.Guild, true);
                await ReplyAsync("Welcome Messageing for this server has been set to: true");
            }
            else if (next.Content == "4")
            {
                GuildConfig.SetWelcomeStatus(Context.Guild, false);
                await ReplyAsync("Welcome Messageing for this server has been set to: false");
            }
            else if (next.Content == "5")
            {
                var l = GuildConfig.GetServer(Context.Guild);
                var embed = new EmbedBuilder();
                try
                {
                    embed.AddField("Message", l.WelcomeMessage);
                    embed.AddField("Channel", Context.Guild.GetChannel(l.WelcomeChannel).Name);
                    embed.AddField("Status", l.WelcomeEvent ? "On" : "Off");
                    await ReplyAsync("", false, embed.Build());
                }
                catch
                {
                    await ReplyAsync(
                        "Error, this guilds welcome config is not fully set up yet, please consider using options 1 thru 4 first");
                }
            }
            else
            {
                await ReplyAsync("ERROR: you did not supply an option. type only `1` etc.");
            }
        }

        [Command("GoodBye", RunMode = RunMode.Async)]
        [Remarks("Setup the goodbye message for new users")]
        [Summary("Goodbye")]
        public async Task InitialiseGoodbye()
        {
            await ReplyAsync("```\n" +
                             "Reply with the command you would like to perform\n" +
                             "[1] Set the GoodBye message\n" +
                             "[2] Set the current channel for GoodBye events\n" +
                             "[3] Enable the GoodBye event\n" +
                             "[4] Disable the GoodBye event\n" +
                             "[5] View GoodBye Info" +
                             "" +
                             "```");

            var next = await NextMessageAsync();

            var jsonObj = GuildConfig.GetServer(Context.Guild);

            if (next.Content == "1")
            {
                await ReplyAsync(
                    "Please reply with the Goodbye message you want to be sent when a user leaves the server ie. `Goodbye you noob!!`");
                var next2 = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
                jsonObj.GoodbyeMessage = next2.Content;
                jsonObj.GoodByeChannel = Context.Channel.Id;
                GuildConfig.SaveServer(jsonObj);

                await ReplyAsync("The Goodbye Message for this server has been set to:\n" +
                                 $"**{next2.Content}**\n" +
                                 $"In the channel **{Context.Channel.Name}**");
            }
            else if (next.Content == "2")
            {
                jsonObj.GoodByeChannel = Context.Channel.Id;
                GuildConfig.SaveServer(jsonObj);
                await ReplyAsync($"Goodbye Events will be sent in the channel: **{Context.Channel.Name}**");
            }
            else if (next.Content == "3")
            {
                jsonObj.GoodbyeEvent = true;
                GuildConfig.SaveServer(jsonObj);
                await ReplyAsync("GoodBye Messaging for this server has been set to: On");
            }
            else if (next.Content == "4")
            {
                jsonObj.GoodbyeEvent = false;
                GuildConfig.SaveServer(jsonObj);
                await ReplyAsync("GoodBye Messaging for this server has been set to: Off");
            }
            else if (next.Content == "5")
            {
                var embed = new EmbedBuilder();
                try
                {
                    embed.AddField("Message", jsonObj.GoodbyeMessage);
                    embed.AddField("Channel", Context.Guild.GetChannel(jsonObj.GoodByeChannel).Name);
                    embed.AddField("Status", jsonObj.GoodbyeEvent ? "On" : "Off");
                    await ReplyAsync("", false, embed.Build());
                }
                catch
                {
                    await ReplyAsync(
                        "Error, this guilds Goodbye config is not fully set up yet, please consider using options 1 thru 4 first");
                }
            }
            else
            {
                await ReplyAsync("ERROR: you did not supply an option. type only `1` etc.");
            }
        }

        [Command("EventLogging")]
        [Summary("EventLogging <True/False>")]
        [Remarks("Enables the ability for events to be logged in a specific channel")]
        public async Task EventToggle(bool status)
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.EventLogging = status;
            jsonObj.EventChannel = Context.Channel.Id;
            GuildConfig.SaveServer(jsonObj);

            if (status)
                await ReplyAsync($"Events will now be logged in {Context.Channel.Name}!");
            else
                await ReplyAsync("Events will no longer be logged");
        }

        [Command("AutoMessage", RunMode = RunMode.Async)]
        [Summary("AutoMessage")]
        [Remarks("Enable to use of automatic bot messages in channels")]
        public async Task AutoMessage()
        {
            await ReplyAsync("```\n" +
                             "Reply with the command you would like to perform\n" +
                             "[1] Set the auto message for this channel\n" +
                             "[2] Set amount of messages before automessaging\n" +
                             "[3] Enable Automessaging in this channel\n" +
                             "[4] Disable AutoMessaging\n" +
                             "[5] View AutoMessage Info" +
                             "" +
                             "```");
            var serverobj = GuildConfig.GetServer(Context.Guild);
            if (serverobj.AutoMessage.All(x => x.channelID != Context.Channel.Id))
            {
                serverobj.AutoMessage.Add(new GuildConfig.autochannels
                {
                    automessage = "AutoMessage",
                    channelID = Context.Channel.Id,
                    enabled = false,
                    messages = 0,
                    sendlimit = 50
                });
            }
            var chan = serverobj.AutoMessage.First(x => x.channelID == Context.Channel.Id);
            var next = await NextMessageAsync();
            if (next.Content == "1")
            {
                await ReplyAsync(
                    "Please reply with the message you would like to auto-send in this channel");
                var next2 = await NextMessageAsync();
                chan.automessage = next2.Content;
                await ReplyAsync(
                    "The auto-message for this channel will now be:\n" +
                    "`-----`\n" +
                    $"{chan.automessage}\n" +
                    $"`-----`");
            }
            else if (next.Content == "2")
            {
                await ReplyAsync($"Please reply with the amount of messages you would like in between automessages");
                var next2 = await NextMessageAsync();
                if (Int32.TryParse(next2.Content, out var result))
                {
                    chan.sendlimit = result;
                    await ReplyAsync($"Automessages will now be sent every {result} messages in this channel");
                }
                else
                {
                    await ReplyAsync("Error: Not an integer");
                }
            }
            else if (next.Content == "3")
            {
                chan.enabled = true;
                await ReplyAsync($"Automessages will now be sent in this channel");
            }
            else if (next.Content == "4")
            {
                chan.enabled = false;
                await ReplyAsync($"Automessages will no longer be sent  in this channel");
            }
            else if (next.Content == "5")
            {
                var embed = new EmbedBuilder();
                foreach (var channel in serverobj.AutoMessage)
                {
                    if (Context.Guild.TextChannels.Any(x => x.Id == channel.channelID))
                    {
                        embed.AddField(Context.Guild.TextChannels.First(x => x.Id == channel.channelID).Name, $"Message: {channel.automessage}\n" +
                                                     $"Message Count: {channel.messages}\n" +
                                                     $"Enabled: {channel.enabled}\n" +
                                                     $"Msg/AutoMessage: {channel.sendlimit}");
                    }

                }

                await Context.Channel.SendMessageAsync("", false, embed.Build());
                await ReplyAsync("Done");
            }
            else
            {
                await ReplyAsync("ERROR: you did not supply an option. type only `1` etc.");
            }
            GuildConfig.SaveServer(serverobj);
        }


        [Command("NoInvite")]
        [Summary("NoInvite <true/false>")]
        [Remarks("disables/enables the sending of invites in a server from regular members")]
        public async Task NoInvite(bool status)
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.Invite = status;
            GuildConfig.SaveServer(jsonObj);

            if (status)
                await ReplyAsync("Invite links will now be deleted!");
            else
                await ReplyAsync("Invite links are now allowed to be sent");
        }

        [Command("NoMention")]
        [Summary("NoMention <true/false>")]
        [Remarks("disables/enables the use of @ everyone and @ here in a server from regular members")]
        public async Task NoMention(bool status)
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.MentionAll = status;
            GuildConfig.SaveServer(jsonObj);

            if (status)
                await ReplyAsync("Mass Mentions will now be deleted!");
            else
                await ReplyAsync("Mass Mentions are now allowed to be sent");
        }

        [Command("SetDj")]
        [Summary("SetDj <@role>")]
        [Remarks("Sets the DJ role")]
        public async Task Dj([Remainder] IRole role)
        {
            GuildConfig.SetDj(Context.Guild, role.Id);
            await ReplyAsync($"The DJ Role has been set to: {role.Name}");
        }

        /*[Command("Errors")]
        [Summary("Errors <true/false>")]
        [Remarks("Toggles Error Status")]
        public async Task Errors(bool status)
        {
            GuildConfig.SetError(Context.Guild, status);
            if (status)
                await ReplyAsync("Errors will now be Logged");
            else
                await ReplyAsync("Errors will no longer be logged");
        }*/

        [Command("addrole")]
        [Summary("addrole @role")]
        [Remarks("adds a subscribable role")]
        public async Task Arole(IRole role)
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            if (jsonObj.RoleList == null)
                jsonObj.RoleList = new List<ulong>();
            if (!jsonObj.RoleList.Contains(role.Id))
            {
                jsonObj.RoleList.Add(role.Id);
                await ReplyAsync($"{role.Name} is now joinable");
            }
            else
            {
                await ReplyAsync($"{role.Name} is already joinable");
                return;
            }

            GuildConfig.SaveServer(jsonObj);
        }

        [Command("delrole")]
        [Summary("delrole @role")]
        [Remarks("removes the subscribable role")]
        public async Task Drole(IRole role)
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);

            if (jsonObj.RoleList.Contains(role.Id))
            {
                jsonObj.RoleList.Remove(role.Id);
                await ReplyAsync($"{role.Name} is has been removed from the subscribable roles list");
            }
            else
            {
                await ReplyAsync($"{role.Name} is not a subscribable role");
                return;
            }

            GuildConfig.SaveServer(jsonObj);
        }

        /*[Command("rss", RunMode = RunMode.Async)]
        [Summary("rss <feed url>")]
        [Remarks("adds an rss feed")]
        public async Task Rss(string url1 = null)
        {
            if (url1 != null)
            {
                GuildConfig.GetServer(Context.Guild);
                GuildConfig.RssSet(Context.Guild, Context.Channel.Id, url1, true);
                await ReplyAsync("Rss Config has been updated!\n" +
                                 $"Updates will be posted in: {Context.Channel.Name}\n" +
                                 $"Url: {url1}");

                await _rss.Rss(url1, Context.Channel as IGuildChannel);
            }
            else
            {
                await ReplyAsync("The RSS Feed has been removed (null input)");
                await _rss.Rss(null, Context.Channel as IGuildChannel);
            }
        }*/

        [Command("prefix")]
        [Summary("prefix <newprefix>")]
        [Remarks("change the bots prefix")]
        public async Task Prefix([Remainder] string newpre = null)
        {
            if (newpre == null)
            {
                await ReplyAsync("Please supply a prefix to use.");
                return;
            }

            if (newpre.StartsWith("(") && newpre.EndsWith(")"))
            {
                newpre = newpre.TrimStart('(');
                newpre = newpre.TrimEnd(')');
            }

            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.Prefix = newpre;

            GuildConfig.SaveServer(jsonObj);

            await ReplyAsync($"the prefix has been updated to `{newpre}`\n" +
                             $"NOTE: the Default prefix `{Load.Pre}` and @mentions will still work\n" +
                             $"NOTE: if you want to have any spaces in the prefix enclose your new prefix in brackets, ie.\n" +
                             $"`(newprefix)`");
        }

        [Command("blacklist")]
        [Summary("blacklist")]
        [Remarks("displays the blacklist for 5 seconds")]
        public async Task B()
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            if (jsonObj.Blacklist == null)
                jsonObj.Blacklist = new List<string>();
            var embed = new EmbedBuilder();
            var blackl = "";
            foreach (var word in jsonObj.Blacklist)
                blackl += $"{word} \n";
            try
            {
                embed.AddField("Blacklisted Words", blackl);
            }
            catch
            {
                //
            }
            embed.AddField("Timeout", "This message self destructs after 5 seconds.");

            await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(5));
        }

        [Command("blacklist add")]
        [Summary("blacklist add <word>")]
        [Remarks("adds a word to the blacklist")]
        public async Task Ab(string keyword)
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            if (jsonObj.Blacklist == null)
                jsonObj.Blacklist = new List<string>();
            if (!jsonObj.Blacklist.Contains(keyword))
            {
                jsonObj.Blacklist.Add(keyword);
                await Context.Message.DeleteAsync();
                await ReplyAsync("Added to the Blacklist");
            }
            else
            {
                await Context.Message.DeleteAsync();
                await ReplyAsync("Keyword is already in the blacklist");
                return;
            }
            GuildConfig.SaveServer(jsonObj);
        }

        [Command("blacklist del")]
        [Summary("blacklist del <word>")]
        [Remarks("removes a word from the blacklist")]
        public async Task Db(string keyword)
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);

            if (jsonObj.Blacklist == null)
                jsonObj.Blacklist = new List<string>();

            if (jsonObj.Blacklist.Contains(keyword))
            {
                jsonObj.Blacklist.Remove(keyword);
                await ReplyAsync($"{keyword} is has been removed from the blacklist");
            }
            else
            {
                await ReplyAsync($"{keyword} is not in the blacklist");
                return;
            }

            GuildConfig.SaveServer(jsonObj);
        }

        [Command("blacklist clear")]
        [Summary("blacklist clear")]
        [Remarks("clears the blacklist")]
        public async Task Clear()
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.Blacklist = new List<string>();

            GuildConfig.SaveServer(jsonObj);

            await ReplyAsync("The blacklist has been cleared.");
        }

        [Command("blacklistBFToggle")]
        [Summary("blacklistBFToggle")]
        [Remarks("Toggles whether or not to filter special characters for spam")]
        public async Task BFToggle()
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.BlacklistBetterFilter = !jsonObj.BlacklistBetterFilter;
            GuildConfig.SaveServer(jsonObj);

            await ReplyAsync($"Blacklist BetterFilter status set to {(jsonObj.BlacklistBetterFilter ? "ON" : "OFF")}");
        }

        [Command("blacklist message")]
        [Summary("blacklist message <message>")]
        [Remarks("set the blaklist message")]
        public async Task BlMessage([Remainder] string blmess = null)
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.BlacklistMessage = blmess ?? "";
            GuildConfig.SaveServer(jsonObj);

            await ReplyAsync("The blacklist message is now:\n" +
                             $"{jsonObj.BlacklistMessage}");
        }


        [Command("SetMod")]
        [Summary("SetMod <@role>")]
        [Remarks("Set the Moderator Role For your server")]
        public async Task SetMod(IRole modRole)
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);

            jsonObj.ModeratorRoleId = modRole.Id;

            GuildConfig.SaveServer(jsonObj);

            await ReplyAsync($"ModRole has been set as {modRole.Mention}");
        }

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


            await ReplyAsync($"ModRole has been set as {muteRole.Mention}\n" +
                             $"{perms}\n" +
                             $"{channels}");
        }
    }
}