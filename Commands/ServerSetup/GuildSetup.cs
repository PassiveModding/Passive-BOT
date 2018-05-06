using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Configuration;
using PassiveBOT.Handlers.Services.Interactive;
using PassiveBOT.Handlers.Services.Interactive.Paginator;

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
                await ServerSetup();
            }
            else if (i == 2)
            {
                await ServerSetup();
            }
            else if (i == 3)
            {
                var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
                if (File.Exists(file))
                {
                    File.Delete(file);
                    await ReplyAsync("The config file has been deleted.");
                }
                else
                {
                    await ReplyAsync("The config file does not exist, please use option 1 to initialise it");
                }
            }
            else
            {
                await ReplyAsync(
                    "Please only reply with the character of your option ie. if you picked 2, reply with just `2`");
            }
        }

        /*
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
                foreach (var role in l.RoleConfigurations.SubRoleList)
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

            try
            {
                errorLogString = l.ErrorLog ? "Status: On" : "Status: Off";
            }
            catch
            {
                errorLogString = "Status: Off";
            }
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
                    eventlogging = "Status: On\n" +
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
                blacklist = "0";
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
        }*/

        [Command("Welcome", RunMode = RunMode.Async)]
        [Remarks("Setup the Welcome Message for new users")]
        [Summary("Welcome")]
        public async Task InitialiseWelcome(int option = 0)
        {
            int input;
            if (option == 0)
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
                var next = await NextMessageAsync(timeout: TimeSpan.FromMinutes(1));
                input = int.Parse(next.Content);
            }
            else
            {
                input = option;
            }

            var Guild = GuildConfig.GetServer(Context.Guild);

            if (input == 1)
            {
                await ReplyAsync(
                    "Please reply with the welcome message you want to be sent when a user joins the server ie. `Welcome to Our Server!!`");
                var next2 = await NextMessageAsync(timeout: TimeSpan.FromMinutes(1));
                Guild.WelcomeMessage = next2.Content;
                Guild.WelcomeChannel = Context.Channel.Id;
                await ReplyAsync("The Welcome Message for this server has been set to:\n" +
                                 $"**{next2.Content}**\n" +
                                 $"In the channel **{Context.Channel.Name}**");
            }
            else if (input == 2)
            {
                Guild.WelcomeChannel = Context.Channel.Id;
                await ReplyAsync($"Welcome Events will be sent in the channel: **{Context.Channel.Name}**");
            }
            else if (input == 3)
            {
                Guild.WelcomeEvent = true;
                await ReplyAsync("Welcome Messageing for this server has been set to: true");
            }
            else if (input == 4)
            {
                Guild.WelcomeEvent = false;
                await ReplyAsync("Welcome Messageing for this server has been set to: false");
            }
            else if (input == 5)
            {
                var embed = new EmbedBuilder();
                try
                {
                    embed.AddField("Message", Guild.WelcomeMessage);
                    embed.AddField("Channel", Context.Guild.GetChannel(Guild.WelcomeChannel).Name);
                    embed.AddField("Status", Guild.WelcomeEvent ? "On" : "Off");
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
                await ReplyAsync("ERROR: you did not supply a valid option. type only `1` etc.");
            }

            GuildConfig.SaveServer(Guild);
        }

        [Command("GoodBye", RunMode = RunMode.Async)]
        [Remarks("Setup the goodbye message for new users")]
        [Summary("Goodbye")]
        public async Task InitialiseGoodbye(int option = 0)
        {
            int input;
            if (option == 0)
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
                var next = await NextMessageAsync(timeout: TimeSpan.FromMinutes(1));
                input = int.Parse(next.Content);
            }
            else
            {
                input = option;
            }

            var jsonObj = GuildConfig.GetServer(Context.Guild);

            if (input == 1)
            {
                await ReplyAsync(
                    "Please reply with the Goodbye message you want to be sent when a user leaves the server ie. `Goodbye you noob!!`");
                var next2 = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
                jsonObj.GoodbyeMessage = next2.Content;
                jsonObj.GoodByeChannel = Context.Channel.Id;

                await ReplyAsync("The Goodbye Message for this server has been set to:\n" +
                                 $"**{next2.Content}**\n" +
                                 $"In the channel **{Context.Channel.Name}**");
            }
            else if (input == 2)
            {
                jsonObj.GoodByeChannel = Context.Channel.Id;
                await ReplyAsync($"Goodbye Events will be sent in the channel: **{Context.Channel.Name}**");
            }
            else if (input == 3)
            {
                jsonObj.GoodbyeEvent = true;
                await ReplyAsync("GoodBye Messaging for this server has been set to: On");
            }
            else if (input == 4)
            {
                jsonObj.GoodbyeEvent = false;
                await ReplyAsync("GoodBye Messaging for this server has been set to: Off");
            }
            else if (input == 5)
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
                await ReplyAsync("ERROR: you did not supply a valid option. type only `1` etc.");
            }

            GuildConfig.SaveServer(jsonObj);
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

        [Command("ApiAI")]
        [Summary("ApiAI")]
        [Remarks("Toggles the use of chatting when you @ the bot")]
        public async Task ApiAIToggle()
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.chatwithmention = !jsonObj.chatwithmention;
            GuildConfig.SaveServer(jsonObj);
            await ReplyAsync(
                $"Using {Context.Client.CurrentUser.Mention} at the start of a message will allow the use or random chat messages is set to: {jsonObj.chatwithmention}");
        }

        [Command("AutoMessage", RunMode = RunMode.Async)]
        [Summary("AutoMessage")]
        [Remarks("Enable to use of automatic bot messages in channels")]
        public async Task AutoMessage(int option = 0)
        {
            int input;
            if (option == 0)
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
                var next = await NextMessageAsync(timeout: TimeSpan.FromMinutes(1));
                input = int.Parse(next.Content);
            }
            else
            {
                input = option;
            }

            var serverobj = GuildConfig.GetServer(Context.Guild);
            if (serverobj.AutoMessage.All(x => x.channelID != Context.Channel.Id))
                serverobj.AutoMessage.Add(new GuildConfig.autochannels
                {
                    automessage = "AutoMessage",
                    channelID = Context.Channel.Id,
                    enabled = false,
                    messages = 0,
                    sendlimit = 50
                });
            var chan = serverobj.AutoMessage.First(x => x.channelID == Context.Channel.Id);
            if (input == 1)
            {
                await ReplyAsync(
                    "Please reply with the message you would like to auto-send in this channel");
                var next2 = await NextMessageAsync();
                chan.automessage = next2.Content;
                await ReplyAsync(
                    "The auto-message for this channel will now be:\n" +
                    "`-----`\n" +
                    $"{chan.automessage}\n" +
                    "`-----`");
            }
            else if (input == 2)
            {
                await ReplyAsync("Please reply with the amount of messages you would like in between automessages");
                var next2 = await NextMessageAsync();
                if (int.TryParse(next2.Content, out var result))
                {
                    chan.sendlimit = result;
                    await ReplyAsync($"Automessages will now be sent every {result} messages in this channel");
                }
                else
                {
                    await ReplyAsync("Error: Not an integer");
                }
            }
            else if (input == 3)
            {
                chan.enabled = true;
                await ReplyAsync("Automessages will now be sent in this channel");
            }
            else if (input == 4)
            {
                chan.enabled = false;
                await ReplyAsync("Automessages will no longer be sent  in this channel");
            }
            else if (input == 5)
            {
                var embed = new EmbedBuilder();
                foreach (var channel in serverobj.AutoMessage)
                    if (Context.Guild.TextChannels.Any(x => x.Id == channel.channelID))
                        embed.AddField(Context.Guild.TextChannels.First(x => x.Id == channel.channelID).Name,
                            $"Message: {channel.automessage}\n" +
                            $"Message Count: {channel.messages}\n" +
                            $"Enabled: {channel.enabled}\n" +
                            $"Msg/AutoMessage: {channel.sendlimit}");

                await Context.Channel.SendMessageAsync("", false, embed.Build());
                await ReplyAsync("Done");
            }
            else
            {
                await ReplyAsync("ERROR: you did not supply an option. type only `1` etc.");
            }

            GuildConfig.SaveServer(serverobj);
        }

        /* [Command("SetDj")]
         [Summary("SetDj <@role>")]
         [Remarks("Sets the DJ role")]
         public async Task Dj([Remainder] IRole role)
         {
             GuildConfig.SetDj(Context.Guild, role.Id);
             await ReplyAsync($"The DJ Role has been set to: {role.Name}");
         }*/

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
            if (jsonObj.RoleConfigurations.SubRoleList == null)
                jsonObj.RoleConfigurations.SubRoleList = new List<ulong>();
            if (!jsonObj.RoleConfigurations.SubRoleList.Contains(role.Id))
            {
                jsonObj.RoleConfigurations.SubRoleList.Add(role.Id);
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

            if (jsonObj.RoleConfigurations.SubRoleList.Contains(role.Id))
            {
                jsonObj.RoleConfigurations.SubRoleList.Remove(role.Id);
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
                             "NOTE: if you want to have any spaces in the prefix enclose your new prefix in brackets, ie.\n" +
                             "`(newprefix)`");
        }

        [Command("SetAdmin")]
        [Summary("SetAdmin <@role>")]
        [Remarks("Set the Admin Role For your server")]
        public async Task SetAdmin(IRole modRole)
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            if (!jsonObj.RoleConfigurations.AdminRoleList.Contains(modRole.Id))
                jsonObj.RoleConfigurations.AdminRoleList.Add(modRole.Id);

            GuildConfig.SaveServer(jsonObj);

            await ReplyAsync($"{modRole.Mention} has been added to the Administrator roles");
        }

        [Command("SetMod")]
        [Summary("SetMod <@role>")]
        [Remarks("Set the Moderator Role For your server")]
        public async Task SetMod(IRole modRole)
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            if (!jsonObj.RoleConfigurations.ModeratorRoleList.Contains(modRole.Id))
                jsonObj.RoleConfigurations.ModeratorRoleList.Add(modRole.Id);

            GuildConfig.SaveServer(jsonObj);

            await ReplyAsync($"{modRole.Mention} has been added to the moderator roles");
        }

        [Command("ToggleModLog")]
        [Summary("ToggleModLog")]
        [Remarks("Toggle logging of kick, warn and ban commands")]
        public async Task ToggleModLog()
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);

            jsonObj.LogModCommands = !jsonObj.LogModCommands;

            GuildConfig.SaveServer(jsonObj);

            await ReplyAsync($"Log Mod Commands: {jsonObj.LogModCommands}\n" +
                             $"Use the SetModLogChannel Command so set the channel where these will be sent!");
        }

        [Command("SetModLogChannel")]
        [Summary("SetModLogChannel")]
        [Remarks("Set Mod Logging Channel")]
        public async Task SetModLogChannel()
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);

            jsonObj.ModLogChannel = Context.Channel.Id;

            GuildConfig.SaveServer(jsonObj);

            await ReplyAsync($"Mod Log will now be sent in:\n" +
                             $"{Context.Channel.Name}");
        }


        public EmbedBuilder SafeEmbed(EmbedBuilder input, string addition, string additiontitle, bool inline = false)
        {
            if (addition == null)
            {
                input.AddField(additiontitle, "NULL", inline);
                return input;
            }

            input.AddField(additiontitle, addition, inline);
            return input;
        }

        [Command("ServerSetup")]
        [Summary("ServerSetup")]
        [Remarks("View the PassiveBOT Server Config")]
        public async Task ServerSetup()
        {
            var Guild = GuildConfig.GetServer(Context.Guild);
            var pages = new List<PaginatedMessage.Page>
            {
                new PaginatedMessage.Page
                {
                    dynamictitle = $"Guild Info",
                    description = $"ID: {Guild.GuildId}\n" +
                                  $"Origin Name: {Guild.GuildName}\n" +
                                  $"Prefix: {Guild.Prefix ?? Config.Load().Prefix}\n"
                },
                new PaginatedMessage.Page
                {
                    dynamictitle = "Welcome and Goodbye",
                    description = $"__**Welcome**__\n" +
                                  $"Event: {Guild.WelcomeEvent}\n" +
                                  $"Message: {Guild.WelcomeMessage ?? "N/A"}\n" +
                                  $"Channel: {Context.Guild.GetChannel(Guild.WelcomeChannel)?.Name ?? "N/A"}\n\n" +
                                  $"__**Goodbye**__\n" +
                                  $"Event: {Guild.GoodbyeEvent}\n" +
                                  $"Message: {Guild.GoodbyeMessage ?? "N/A"}\n" +
                                  $"Channel: {Context.Guild.GetChannel(Guild.GoodByeChannel)?.Name ?? "N/A"}\n"
                },
                new PaginatedMessage.Page
                {
                    dynamictitle = $"Partner Program",
                    description = $"Signed Up: {Guild.PartnerSetup.IsPartner}\n" +
                                  $"Banned: {Guild.PartnerSetup.banned}\n" +
                                  $"Channel: {Context.Guild.GetChannel(Guild.PartnerSetup.PartherChannel)?.Name ?? "N/A"}\n" +
                                  $"Image URL: {Guild.PartnerSetup.ImageUrl ?? "N/A"}\n" +
                                  $"Show User Count: {Guild.PartnerSetup.showusercount}\n" +
                                  $"Message: \n" +
                                  $"{Guild.PartnerSetup.Message ?? "N/A"}"
                },
                new PaginatedMessage.Page
                {
                    dynamictitle = $"AntiSpam 1.Prevention",
                    description = $"NoSpam: {Guild.Antispams.Antispam.NoSpam}\n" +
                                  $"Remove IPs: {Guild.Antispams.Privacy.RemoveIPs}\n" +
                                  $"Remove Invites: {Guild.Antispams.Advertising.Invite}\n" +
                                  $"Remove Invites Message:\n" +
                                  $"{Guild.Antispams.Advertising.NoInviteMessage ?? "N/A"}\n\n" +
                                  $"Remove @Everyone and @Here: {Guild.Antispams.Mention.MentionAll}\n" +
                                  $"Remove @Everyone and @Here Message:\n" +
                                  $"{Guild.Antispams.Mention.MentionAllMessage}\n\n" +
                                  $"Remove @everyone and @here excempt:\n" +
                                  $"Remove Messages with 5+ Mentions: {Guild.Antispams.Mention.RemoveMassMention}\n"
                },
                new PaginatedMessage.Page
                {
                    dynamictitle = $"AntiSpam 2.Blacklist",
                    description = $"Using Blacklist: {Guild.Antispams.Blacklist.BlacklistWordSet.Any()}\n" +
                                  $"Default Blacklist Message: {Guild.Antispams.Blacklist.DefaultBlacklistMessage ?? "N/A"}\n" +
                                  $"Filter Special Characters and numbers: {Guild.Antispams.Blacklist.BlacklistBetterFilter}\n" +
                                  $"Blacklisted Words:\n" +
                                  $"Use the `{Config.Load().Prefix}blacklist` message to show this\n"
                },
                new PaginatedMessage.Page
                {
                    dynamictitle = $"AntiSpam 3.Excempt",
                    description =
                        $"{(Guild.Antispams.IngoreRoles.Any() ? string.Join("\n", Guild.Antispams.IngoreRoles.Where(x => Context.Guild.GetRole(x.RoleID) != null).Select(x => $"__{Context.Guild.GetRole(x.RoleID).Name}__\nBypass Antispam: {x.AntiSpam}\nBypass Blacklist: {x.Blacklist}\nBypass Mention: {x.Mention}\nBypass Invite: {x.Advertising}\nBypass Filtering: {x.Privacy}\n")) : "N/A")}"
                },
                new PaginatedMessage.Page
                {
                    dynamictitle = "Kicks Warns and Bans",
                    description = $"Kicks: {(Guild.Kicking.Any() ? Guild.Kicking.Count.ToString() : "N/A")}\n" +
                                  $"Warns: {(Guild.Warnings.Any() ? Guild.Warnings.Count.ToString() : "N/A")}\n" +
                                  $"Bans: {(Guild.Banning.Any() ? Guild.Banning.Count.ToString() : "N/A")}\n"
                },
                new PaginatedMessage.Page
                {
                    dynamictitle = "Event & Error Logging",
                    description = $"Event Logging: {Guild.EventLogging}\n" +
                                  $"Event Channel: {Context.Guild.GetChannel(Guild.EventChannel)?.Name ?? "N/A"}\n" +
                                  $"Error Logging (dep): {Guild.ErrorLog}\n"
                },
                new PaginatedMessage.Page
                {
                    dynamictitle = "Tagging",
                    description = $"Using Tags: {Guild.Dict.Any()}\n" +
                                  $"Tag Names: \n{(Guild.Dict.Any() ? string.Join("\n", Guild.Dict.Select(x => x.Tagname)) : "N/A")}\n"
                },
                new PaginatedMessage.Page
                {
                    dynamictitle = "AutoMessage",
                    description = $"Using Automessage: {Guild.AutoMessage.Any()}\n" +
                                  $"Auto Message Channels:\n" +
                                  $"{(Guild.AutoMessage.Any() ? string.Join("\n", Guild.AutoMessage.Select(x => Context.Guild.GetChannel(x.channelID)?.Name).Where(x => x != null)) : "N/A")}"
                },
                new PaginatedMessage.Page
                {
                    dynamictitle = "Levelling",
                    description = $"Enabled: {Guild.Levels.LevellingEnabled}\n" +
                                  $"Level Messages: {Guild.Levels.UseLevelMessages}\n" +
                                  $"Use Level Log Channel: {Guild.Levels.UseLevelChannel}\n" +
                                  $"Level Log Channel: {Context.Guild.GetChannel(Guild.Levels.LevellingChannel)?.Name ?? "N/A"}\n" +
                                  $"Increment Rewards: {Guild.Levels.IncrementLevelRewards}\n" +
                                  $"Levels: \n" +
                                  $"{(Guild.Levels.LevelRoles.Any() ? string.Join("\n", Guild.Levels.LevelRoles.Select(x => $"{Context.Guild.GetRole(x.RoleID)?.Mention ?? "Removed Role"} Level Requirement: {x.LevelToEnter}")) : "N/A")}"
                },
                new PaginatedMessage.Page
                {
                    dynamictitle = "Moderators",
                    description =
                        $"Mod Roles: {(Guild.RoleConfigurations.ModeratorRoleList.Any() ? string.Join("\n", Guild.RoleConfigurations.ModeratorRoleList.Where(mr => Context.Guild.Roles.Any(x => x.Id == mr)).Select(mr => Context.Guild.GetRole(mr)?.Name)) : "N/A")}\n" +
                        $"Moderator Logging: {Guild.LogModCommands}\n" +
                        $"Moderator Log Channel: {Context.Guild.GetChannel(Guild.ModLogChannel)?.Name ?? "N/A"}"
                },
                new PaginatedMessage.Page
                {
                    dynamictitle = "Administrators",
                    description =
                        $"Admin Roles: {(Guild.RoleConfigurations.AdminRoleList.Any() ? string.Join("\n", Guild.RoleConfigurations.AdminRoleList.Where(ar => Context.Guild.Roles.Any(x => x.Id == ar)).Select(ar => Context.Guild.GetRole(ar)?.Name)) : "N/A")}"
                },
                new PaginatedMessage.Page
                {
                    dynamictitle = "Current Giveaway",
                    description = $"Message: {Guild.Comp.Message ?? "N/A"}\n" +
                                  $"Creator: {Context.Guild.GetUser(Guild.Comp.Creator)?.Username ?? "N/A"}\n"
                },
                new PaginatedMessage.Page
                {
                    dynamictitle = "Starboard (dep)",
                    description = $"Starboard Channel: {Context.Guild.GetChannel(Guild.Starboard)?.Name ?? "N/A"}"
                },
                new PaginatedMessage.Page
                {
                    dynamictitle = "Twitch (dep)",
                    description = $"N/A"
                }
            };

            await PagedReplyAsync(new PaginatedMessage
            {
                Pages = pages
            });
        }
    }
}