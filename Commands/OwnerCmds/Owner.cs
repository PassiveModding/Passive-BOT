using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotsList.Api.Extensions.DiscordNet;
using PassiveBOT.Configuration;
using PassiveBOT.Configuration.Objects;
using PassiveBOT.Handlers;
using PassiveBOT.Handlers.Services;
using PassiveBOT.Handlers.Services.Interactive;
using PassiveBOT.Handlers.Services.Interactive.Paginator;
using PassiveBOT.preconditions;
using PassiveBOT.strings;
using EventHandler = System.EventHandler;

namespace PassiveBOT.Commands.OwnerCmds
{
    [RequireOwner]
    public class Owner : InteractiveBase
    {
        private readonly CommandService Service;
        //public DiscordSocketClient Client;


        public Owner(CommandService Cserv)
        {
            Service = Cserv;
        }

        [Command("Test+++", RunMode = RunMode.Async)]
        public async Task Tester()
        {
            var timer = new Stopwatch();
            timer.Start();
            var advertlist = new List<long>();
            var iplist = new List<long>();
            var mentionlist = new List<long>();
            var bllist = new List<long>();
            var toxlist = new List<long>();
            var vislist = new List<long>();
            for (int i = 0; i < 100; i++)
            {
                var context = Context;
                var guild = GuildConfig.GetServer(context.Guild);
                var exemptcheck = guild.Antispams.IgnoreRoles
                    .Where(x => ((IGuildUser)context.User).RoleIds.Contains(x.RoleID)).ToList();
                var message = context.Message;

                var timerInvite = new Stopwatch();
                timerInvite.Start();
                for (int ai = 0; ai < 10000; ai++)
                {
                    if (guild.Antispams.Advertising.Invite)
                    {
                        var BypassInvite = exemptcheck.Any(x => x.Advertising);
                        if (!BypassInvite)
                            if (Regex.Match(context.Message.Content,
                                    @"^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?(d+i+s+c+o+r+d+|a+p+p)+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$")
                                .Success)
                            {
                                //await message.DeleteAsync();
                                var emb = new EmbedBuilder
                                {
                                    Description =
                                        guild.Antispams.Advertising.NoInviteMessage ??
                                        $"{context.User.Mention} - Pls Daddy, no sending invite links... the admins might get angry"
                                };
                                //await context.Channel.SendMessageAsync("", false, emb.Build());
                                //if
                                // 1. The server Has Invite Deletions turned on
                                // 2. The user is not an admin
                                // 3. The user does not have one of the invite excempt roles
                                //return true;
                            }
                    }
                }

                timerInvite.Stop();
                advertlist.Add(timerInvite.ElapsedMilliseconds);

                var MentionT = new Stopwatch();
                MentionT.Start();
                for (int ai = 0; ai < 10000; ai++)
                {
                    if (guild.Antispams.Mention.RemoveMassMention || guild.Antispams.Mention.MentionAll)
                    {
                        var BypassMention = exemptcheck.Any(x => x.Mention);

                        if (!BypassMention)
                        {
                            if (guild.Antispams.Mention.RemoveMassMention)
                                if (message.MentionedRoles.Count + message.MentionedUsers.Count >= 5)
                                {
                                    //await message.DeleteAsync();
                                    var emb = new EmbedBuilder
                                    {
                                        Title =
                                            $"{context.User} - This server does not allow you to mention 5+ roles or uses at once"
                                    };
                                    //await context.Channel.SendMessageAsync("", false, emb.Build());
                                    //return true;
                                }

                            if (guild.Antispams.Mention.MentionAll)
                                if (message.Content.Contains("@everyone") || message.Content.Contains("@here"))
                                {
                                    //await message.DeleteAsync();

                                    var rnd = new Random();
                                    var res = rnd.Next(0, FunStr.Everyone.Length);
                                    var emb = new EmbedBuilder();
                                    if (guild.Antispams.Mention.MentionAllMessage != null)
                                    {
                                        emb.Description = guild.Antispams.Mention.MentionAllMessage;
                                    }
                                    else
                                    {
                                        emb.Title = $"{context.User} - the admins might get angry";
                                        emb.ImageUrl = FunStr.Everyone[res];
                                    }

                                    //await context.Channel.SendMessageAsync("", false, emb.Build());
                                    //return true;
                                    //if
                                    // 1. The server Has Mention Deletions turned on
                                    // 2. The user is not an admin
                                    // 3. The user does not have one of the mention excempt roles
                                }
                        }
                    }
                }

                MentionT.Stop();
                mentionlist.Add(MentionT.ElapsedMilliseconds);

                var IPT = new Stopwatch();
                IPT.Start();
                for (int ai = 0; ai < 10000; ai++)
                {
                    if (guild.Antispams.Privacy.RemoveIPs)
                    {
                        var BypassIP = exemptcheck.Any(x => x.Privacy);

                        if (!BypassIP)
                            if (Regex.IsMatch(message.Content,
                                @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$")
                            )
                            {
                                //await message.DeleteAsync();
                                var emb = new EmbedBuilder
                                {
                                    Title = $"{context.User} - This server does not allow you to post IP addresses"
                                };
                                //await context.Channel.SendMessageAsync("", false, emb.Build());
                                //return true;
                            }
                    }
                }

                IPT.Stop();
                iplist.Add(IPT.ElapsedMilliseconds);

                var BLT = new Stopwatch();
                BLT.Start();
                for (int ai = 0; ai < 10000; ai++)
                {
                    if (guild.Antispams.Blacklist.BlacklistWordSet.Any())
                    {
                        var BypassBlacklist = exemptcheck.Any(x => x.Blacklist);

                        if (!BypassBlacklist)
                        {
                            var blacklistdetected = false;
                            var blacklistmessage = guild.Antispams.Blacklist.DefaultBlacklistMessage;

                            if (guild.Antispams.Blacklist.BlacklistBetterFilter)
                            {
                                var filteredorig = ProfanityFilter.doreplacements(ProfanityFilter.RemoveDiacritics(context.Message.Content));
                                var detectedblacklistmodule = guild.Antispams.Blacklist.BlacklistWordSet.FirstOrDefault(blist => blist.WordList.Any(x => filteredorig.Contains(ProfanityFilter.doreplacements(ProfanityFilter.RemoveDiacritics(x)).ToLower())));
                                if (detectedblacklistmodule != null)
                                {
                                    blacklistdetected = true;
                                    blacklistmessage = detectedblacklistmodule.BlacklistResponse ??
                                                       guild.Antispams.Blacklist.DefaultBlacklistMessage;
                                }
                            }
                            else
                            {
                                var detectedblacklistmodule = guild.Antispams.Blacklist.BlacklistWordSet.FirstOrDefault(
                                    blist =>
                                        blist.WordList.Any(x =>
                                            context.Message.Content.ToLower().Contains(x.ToLower())));
                                if (detectedblacklistmodule != null)
                                {
                                    blacklistdetected = true;
                                    blacklistmessage = detectedblacklistmodule.BlacklistResponse ??
                                                       guild.Antispams.Blacklist.DefaultBlacklistMessage;
                                }
                            }

                            if (blacklistdetected)
                            {
                                //await message.DeleteAsync();

                                if (!string.IsNullOrEmpty(blacklistmessage))
                                {
                                    //var responsemessage = blacklistmessage.Replace("{user}", context.User.Username)
                                    //    .Replace("{user.mention}", context.User.Mention).Replace("{guild}", context.Guild.Name)
                                    //    .Replace("{channel}", context.Channel.Name).Replace("{channel.mention}",
                                    //        ((SocketTextChannel) context.Channel).Mention);

                                    var result = Regex.Replace(blacklistmessage, "{user}", context.User.Username,
                                        RegexOptions.IgnoreCase);
                                    result = Regex.Replace(result, "{user.mention}", context.User.Mention,
                                        RegexOptions.IgnoreCase);
                                    result = Regex.Replace(result, "{guild}", context.Guild.Name,
                                        RegexOptions.IgnoreCase);
                                    result = Regex.Replace(result, "{channel}", context.Channel.Name,
                                        RegexOptions.IgnoreCase);
                                    result = Regex.Replace(result, "{channel.mention}",
                                        ((SocketTextChannel) context.Channel).Mention, RegexOptions.IgnoreCase);
                                    //await context.Channel.SendMessageAsync(result);
                                    //return true;
                                }
                            }
                        }
                    }
                }

                BLT.Stop();
                bllist.Add(BLT.ElapsedMilliseconds);


                CommandInfo CMDCheck = null;
                var argPos = 0;
                if (message.HasMentionPrefix(context.Client.CurrentUser, ref argPos) && guild.chatwithmention ||
                    message.HasStringPrefix(Load.Pre, ref argPos) ||
                    message.HasStringPrefix(GuildConfig.GetServer(context.Guild).Prefix, ref argPos))
                {
                    var cmdSearch = Service.Search(context, argPos);
                    if (cmdSearch.IsSuccess)
                    {
                        CMDCheck = cmdSearch.Commands.FirstOrDefault().Command;
                    }
                }

                var tox = new Stopwatch();
                tox.Start();
                for (int ai = 0; ai < 10000; ai++)
                {
                    if (guild.Antispams.Toxicity.UsePerspective)
                    {
                        var BypassToxicity = exemptcheck.Any(x => x.Toxicity);

                        if (!BypassToxicity)
                        {
                            var CheckUsingToxicity = CMDCheck == null;

                            string token = null; //Tokens.Load().PerspectiveAPI;
                            if (token != null && CheckUsingToxicity && !string.IsNullOrWhiteSpace(message.Content))
                                try
                                {
                                    var res = new Perspective.Api(token).QueryToxicity(message.Content);
                                    if (res.attributeScores.TOXICITY.summaryScore.value * 100 >
                                        guild.Antispams.Toxicity.ToxicityThreshHold)
                                    {
                                        //await message.DeleteAsync();
                                        var emb = new EmbedBuilder
                                        {
                                            Title = "Toxicity Threshhold Breached",
                                            Description = $"{context.User.Mention}"
                                        };
                                        //await context.Channel.SendMessageAsync("", false, emb.Build());

                                        if (context.Client.GetChannel(guild.ModLogChannel) is IMessageChannel modchannel
                                        )
                                            try
                                            {
                                                emb.Description = "Message Auto-Removed.\n" +
                                                                  $"User: {context.User.Mention}\n" +
                                                                  $"Channel: {context.Channel.Name}\n" +
                                                                  $"Toxicity %: {res.attributeScores.TOXICITY.summaryScore.value * 100}\n" +
                                                                  "Message: \n" +
                                                                  $"{context.Message.Content}";
                                                //await modchannel.SendMessageAsync("", false, emb.Build());
                                            }
                                            catch
                                            {
                                                //
                                            }

                                        //return true;
                                    }
                                }
                                catch
                                {
                                    //
                                }
                        }
                    }
                }

                tox.Stop();
                toxlist.Add(tox.ElapsedMilliseconds);

                var visslist = new Stopwatch();
                visslist.Start();
                for (int ai = 0; ai < 10000; ai++)
                {
                    if (guild.Visibilityconfig.BlacklistedCommands.Any() ||
                        guild.Visibilityconfig.BlacklistedModules.Any())
                    {
                        if (CMDCheck != null)
                        {
                            var guser = (IGuildUser) context.User;
                            if (!guser.GuildPermissions.Administrator &&
                                !guild.RoleConfigurations.AdminRoleList.Any(x => guser.RoleIds.Contains(x)))
                            {
                                if (guild.Visibilityconfig.BlacklistedCommands.Any(x =>
                                        string.Equals(x, CMDCheck.Name, StringComparison.CurrentCultureIgnoreCase)) ||
                                    guild.Visibilityconfig.BlacklistedModules.Any(x =>
                                        string.Equals(x, CMDCheck.Module.Name,
                                            StringComparison.CurrentCultureIgnoreCase)))
                                {
                                    //return true;
                                }
                            }
                        }
                    }
                }

                visslist.Stop();
                vislist.Add(visslist.ElapsedMilliseconds);
            }
            timer.Stop();

            await ReplyAsync($"Total: {timer.ElapsedMilliseconds}ms\n" +
                             $"Invite: {advertlist.Sum()/advertlist.Count}ms\n" +
                             $"IP: {iplist.Sum() / iplist.Count}ms\n" +
                             $"Mention: {mentionlist.Sum() / mentionlist.Count}ms\n" +
                             $"Black: {bllist.Sum() / bllist.Count}ms\n" +
                             $"Tox: {toxlist.Sum() / toxlist.Count}ms\n" +
                             $"Vis: {vislist.Sum() / vislist.Count}ms\n" +
                             $"");
            //return false;
        }

        [Command("Toxicity+", RunMode = RunMode.Async)]
        [Summary("Toxicity+")]
        [Remarks("Evaluate Toxicity of a message")]
        public async Task Toxicity([Remainder] string message = null)
        {
            try
            {
                var token = Tokens.Load().PerspectiveAPI;
                if (token != null)
                {
                    var pages = new List<PaginatedMessage.Page>();
                    var requestlist = new List<string>
                    {
                        "ATTACK_ON_AUTHOR",
                        "ATTACK_ON_COMMENTER",
                        "INCOHERENT",
                        "INFLAMMATORY",
                        "LIKELY_TO_REJECT",
                        "OBSCENE",
                        "SEVERE_TOXICITY",
                        "SPAM",
                        "TOXICITY",
                        "UNSUBSTANTIAL"
                    };

                    foreach (var request in requestlist)
                        try
                        {
                            var timer = new Stopwatch();
                            timer.Start();
                            var requestedAttributeses =
                                new Dictionary<string, Perspective.RequestedAttributes>
                                {
                                    {request, new Perspective.RequestedAttributes()}
                                };
                            var req = new Perspective.AnalyzeCommentRequest(message, requestedAttributeses);
                            var res = new Perspective.Api(token).GetResponseString(req);
                            timer.Stop();
                            var t1 = timer.ElapsedMilliseconds;
                            pages.Add(new PaginatedMessage.Page
                            {
                                dynamictitle = request,
                                description = $"__Time__\n" +
                                              $"{t1}\n" +
                                              $"__Response__\n" +
                                              $"{res}"
                            });
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }

                    var pager = new PaginatedMessage
                    {
                        Pages = pages
                    };

                    await PagedReplyAsync(pager);
                }
            }
            catch (Exception e)
            {
                await ReplyAsync(e.ToString());
            }
        }

        [Command("UpdateStats+")]
        [Summary("UpdateStats+")]
        [Remarks("Update the Bots Stats on DiscordBots.org")]
        public async Task UpdateStats()
        {
            if (Tokens.Load().DiscordBotsListToken == null)
            {
                await ReplyAsync("Bot Not Configured for DiscordBots.org");
                return;
            }

            try
            {
                var DblApi = new DiscordNetDblApi(Context.Client, Tokens.Load().DiscordBotsListToken);
                var me = await DblApi.GetMeAsync();
                await me.UpdateStatsAsync(Context.Client.Guilds.Count);
            }
            catch
            {
                //
            }
        }

        [Command("ViewAntispamServers+")]
        [Summary("ViewAntispamServers+")]
        [Remarks("List all servers using antispam")]
        public async Task AntispamServers()
        {
            var pages = new List<PaginatedMessage.Page>();
            var s2 = new StringBuilder();
            foreach (var guild in Context.Client.Guilds.OrderByDescending(x => x.MemberCount))
            {
                var gobj = GuildConfig.GetServer(guild);
                if (gobj == null) continue;
                if (gobj.Antispams.Antispam.NoSpam || gobj.Antispams.Antispam.antiraid ||
                    gobj.Antispams.Advertising.Invite || gobj.Antispams.Blacklist.BlacklistWordSet.Any() ||
                    gobj.Antispams.Mention.MentionAll || gobj.Antispams.Mention.RemoveMassMention ||
                    gobj.Antispams.Privacy.RemoveIPs || gobj.Antispams.Toxicity.UsePerspective)
                {
                    var embdesc = $"Name: {guild.Name} || `{guild.Id}`\n" +
                                  $"Members: {guild.MemberCount}\n" +
                                  $"NoSpam: {gobj.Antispams.Antispam.NoSpam}\n" +
                                  $"AntiRaid: {gobj.Antispams.Antispam.antiraid}\n" +
                                  $"AntiAD: {gobj.Antispams.Advertising.Invite}\n" +
                                  $"Blacklist: {gobj.Antispams.Blacklist.BlacklistWordSet.Any()}\n" +
                                  $"MentionAll: {gobj.Antispams.Mention.MentionAll}\n" +
                                  $"MassMention: {gobj.Antispams.Mention.RemoveMassMention}\n" +
                                  $"AntiIP: {gobj.Antispams.Privacy.RemoveIPs}\n" +
                                  $"Toxicity: {gobj.Antispams.Toxicity.UsePerspective} // {gobj.Antispams.Toxicity.ToxicityThreshHold}\n";
                    s2.Append($"{embdesc}\n");
                }


                if (s2.ToString().Length <= 800) continue;
                pages.Add(new PaginatedMessage.Page
                {
                    description = s2.ToString()
                });
                s2.Clear();
            }

            pages.Add(new PaginatedMessage.Page
            {
                description = s2.ToString()
            });

            await PagedReplyAsync(new PaginatedMessage
            {
                Pages = pages
            });
        }

        [Command("PartnerUpdates+")]
        [Summary("PartnerUpdates+")]
        [Remarks("Set the PartnerUpdatesChannel")]
        public async Task PupDates()
        {
            var home = Homeserver.Load();
            home.PartnerUpdates = Context.Channel.Id;
            Homeserver.SaveHome(home);
            await ReplyAsync("PartnerUpdates will now be posted here!");
        }

        [Command("SetBotModerators+")]
        [Summary("SetBotModerators+")]
        [Remarks("Set a role of users which can access some Bot Owner Commands")]
        public async Task BModRole(SocketRole role = null)
        {
            var home = Homeserver.Load();
            home.BotModerator = role?.Id ?? 0;
            Homeserver.SaveHome(home);
            await ReplyAsync($"ModRole is set to {(role == null ? "N/A" : role.Name)}!");
        }

        [Command("GlobalBan+", RunMode = RunMode.Async)]
        [Summary("GlobalBan+")]
        [Remarks("For those who dont seem to go away")]
        public async Task GlobalBan(ulong ID)
        {
            foreach (var server in Context.Client.Guilds)
                if (server.Users.Any(x => x.Id == ID))
                    try
                    {
                        await server.AddBanAsync(ID);
                        await ReplyAsync($"Banned User in {server.Name}");
                    }
                    catch
                    {
                        await ReplyAsync($"Failed to Ban User in {server.Name}");
                    }

            await ReplyAsync("Complete");
        }

        [Command("PurgeServers+", RunMode = RunMode.Async)]
        [Summary("PurgeServers+")]
        [Remarks("Delete old server configs")]
        public async Task DeleteServerConfigs()
        {
            await ReplyAsync("Working....");
            var purged = 0;
            foreach (var config in Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "setup/server/")))
            {
                var p = Path.GetFileNameWithoutExtension(config);
                //Console.WriteLine(p);
                try
                {
                    var trythis = Context.Client.GetGuild(Convert.ToUInt64(p));
                    Console.WriteLine(trythis.Name);
                }
                catch
                {
                    File.Delete(config);
                    purged++;
                }
            }

            await ReplyAsync("Guilds Purged.\n" +
                             $"Purged: {purged}");
        }


        [Command("sethome+", RunMode = RunMode.Async)]
        [Summary("sethome+")]
        [Remarks("set the owner server")]
        [Ratelimit(1, 15, Measure.Seconds)]
        public async Task HomeAsync()
        {
            var homes = new Homeserver
            {
                GuildId = Context.Guild.Id,
                GuildName = Context.Guild.Name
            };
            Homeserver.SaveHome(homes);
            await ReplyAsync("Done");
        }

        [Command("setsuggest+")]
        [Summary("setsuggest+")]
        [Remarks("set the suggestion channel")]
        public async Task Suggest()
        {
            var home = Homeserver.Load();
            home.Suggestion = Context.Channel.Id;
            Homeserver.SaveHome(home);
            await ReplyAsync("Done");
        }

        [Command("seterror+")]
        [Summary("seterror+")]
        [Remarks("set the suggestion channel")]
        public async Task Error()
        {
            var home = Homeserver.Load();
            home.Error = Context.Channel.Id;
            Homeserver.SaveHome(home);
            await ReplyAsync("Done");
        }

        [Command("help+", RunMode = RunMode.Async)]
        [Summary("help+")]
        [Remarks("Owner Commands")]
        [Ratelimit(1, 15, Measure.Seconds)]
        public async Task Help2Async()
        {
            var description = "";
            foreach (var module in Service.Modules)
                if (module.Name == "Owner")
                    description = module.Commands.Aggregate(description,
                        (current, cmd) => current + $"{Load.Pre}{cmd.Aliases.First()} - {cmd.Remarks}\n");

            var embed = new EmbedBuilder()
                .WithTitle("Owner Commands")
                .WithDescription(description);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("die+")]
        [Summary("die+")]
        [Remarks("Kills the bot")]
        public async Task Die()
        {
            await ReplyAsync("Bye Bye :heart:");
            Environment.Exit(0);
        }

        [Command("LeaveServer+", RunMode = RunMode.Async)]
        [Summary("LeaveServer+ <guild ID> [Optional]<reason>")]
        [Remarks("Makes the bot leave the specified guild")]
        public async Task LeaveAsync(ulong id, [Remainder] string reason = null)
        {
            await LeaveGuild(id, true, reason);
        }

        public async Task<bool> LeaveGuild(ulong id, bool respond_on_leave, string reason = null)
        {
            var gld = Context.Client.GetGuild(id);
            if (gld == null) return false;

            foreach (var channel in gld.TextChannels.OrderByDescending(x => x.Users.Count))
                try
                {
                    await channel.SendMessageAsync($"Goodbye. I am leaving this Server!\n" +
                                                   $"__**Reason**__\n" +
                                                   $"{reason ?? "No reason Provided."}");
                    break;
                }
                catch
                {
                    //
                }

            await gld.LeaveAsync();

            if (respond_on_leave) await ReplyAsync("Message has been sent and I've left the guild!");

            return true;
        }

        [Command("ReduceServers+", RunMode = RunMode.Async)]
        [Summary("ReduceServers+")]
        [Remarks("Reduce the amount of servers the bot is in.")]
        public async Task ReduceAsync()
        {
            var i = 0;
            var guildstoleave = Context.Client.Guilds.Where(x => x.MemberCount < 15).ToList();
            await ReplyAsync("Leaving all servers with less than 15 members.\n" +
                             $"Attepting to leave {guildstoleave.Count} guilds.");
            foreach (var guild in guildstoleave)
                if (await LeaveGuild(guild.Id, false,
                    "PassiveBOT is leaving this server due to low usercount. Please feel free to invite it back by going to our dev server and using the invite command:\n" +
                    $"{Tokens.Load().SupportServer}"))
                    i++;
            await ReplyAsync($"{i} servers left.");
        }

        [Command("GetServer+")]
        [Summary("Getserver+ <string>")]
        [Remarks("Get servers containing the provided string")]
        public async Task GetAsync([Remainder] string s = "")
        {
            var pages = new List<PaginatedMessage.Page>();
            var s2 = new StringBuilder();
            foreach (var guild in Context.Client.Guilds.OrderByDescending(x => x.MemberCount))
            {
                if (s != "")
                {
                    if (guild.Name.ToLower().Contains(s.ToLower()))
                        s2.Append($"{guild.Name} : `{guild.Id}` : U- {guild.MemberCount}\n");
                }
                else
                {
                    s2.Append($"{guild.Name} : `{guild.Id}` : U- {guild.MemberCount}\n");
                }

                if (s2.ToString().Length <= 800) continue;
                pages.Add(new PaginatedMessage.Page
                {
                    description = s2.ToString()
                });
                s2.Clear();
            }

            pages.Add(new PaginatedMessage.Page
            {
                description = s2.ToString()
            });

            await PagedReplyAsync(new PaginatedMessage
            {
                Pages = pages
            });
        }

        [Command("Username+")]
        [Summary("username+ <name>")]
        [Remarks("Sets the bots username")]
        public async Task UsernameAsync([Remainder] string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                await ReplyAsync("Value cannot be empty");
            await Context.Client.CurrentUser.ModifyAsync(x => x.Username = value).ConfigureAwait(false);
            await ReplyAsync("Bot Username updated").ConfigureAwait(false);
        }

        [Group("Token")]
        [RequireOwner]
        public class TokenSetup : ModuleBase
        {
            [Command("SetFortniteToken+")]
            [Summary("Token SetFortniteToken+ <token>")]
            [Remarks("set the fortnite api token")]
            public async Task FNToken([Remainder] string token = null)
            {
                if (token == null)
                {
                    await ReplyAsync("Please input a token");
                    return;
                }

                var TokenConfig = Tokens.Load();
                TokenConfig.FortniteToken = token;
                Tokens.SaveTokens(TokenConfig);
                await ReplyAsync("Done");
            }

            [Command("SetDialogFlowToken+")]
            [Summary("Token SetDialogFlowToken+")]
            [Remarks("set the DialogFlow api token")]
            public async Task DFToken([Remainder] string token = null)
            {
                if (token == null)
                {
                    await ReplyAsync("Please input a token");
                    return;
                }

                var TokenConfig = Tokens.Load();
                TokenConfig.DialogFlowToken = token;
                Tokens.SaveTokens(TokenConfig);
                await ReplyAsync("Done");
            }

            [Command("SetDiscordBotsListToken+")]
            [Summary("Token SetDiscordBotsListToken+")]
            [Remarks("set the DBL api token")]
            public async Task DBLToken([Remainder] string token = null)
            {
                if (token == null)
                {
                    await ReplyAsync("Please input a token");
                    return;
                }

                var TokenConfig = Tokens.Load();
                TokenConfig.DiscordBotsListToken = token;
                Tokens.SaveTokens(TokenConfig);
                await ReplyAsync("Done");
            }

            [Command("SetTwitchToken+")]
            [Summary("Token SetTwitchToken+")]
            [Remarks("set the Twitch api token")]
            public async Task TwitchToken([Remainder] string token = null)
            {
                if (token == null)
                {
                    await ReplyAsync("Please input a token");
                    return;
                }

                var TokenConfig = Tokens.Load();
                TokenConfig.TwitchToken = token;
                Tokens.SaveTokens(TokenConfig);
                await ReplyAsync("Done");
            }

            [Command("SetDiscordBotsListURL+")]
            [Summary("Token SetDiscordBotsListURL+")]
            [Remarks("set the DBL bot URL")]
            public async Task DBLUrl([Remainder] string URL = null)
            {
                if (URL == null)
                {
                    await ReplyAsync("Please input a token");
                    return;
                }

                var TokenConfig = Tokens.Load();
                TokenConfig.DiscordBotsListUrl = URL;
                Tokens.SaveTokens(TokenConfig);
                await ReplyAsync("Done");
            }

            [Command("SetSupportServerURL+")]
            [Summary("Token SetSupportServerURL+")]
            [Remarks("set the DBL bot URL")]
            public async Task SupportURL([Remainder] string URL = null)
            {
                if (URL == null)
                {
                    await ReplyAsync("Please input a token");
                    return;
                }

                var TokenConfig = Tokens.Load();
                TokenConfig.SupportServer = URL;
                Tokens.SaveTokens(TokenConfig);
                await ReplyAsync("Done");
            }

            [Command("SetPerspectiveToken+")]
            [Summary("Token SetPerspectiveToken+")]
            [Remarks("set the google perspective api toxicity token")]
            public async Task Perspective([Remainder] string token = null)
            {
                if (token == null)
                {
                    await ReplyAsync("Please input a token");
                    return;
                }

                var TokenConfig = Tokens.Load();
                TokenConfig.PerspectiveAPI = token;
                Tokens.SaveTokens(TokenConfig);
                await ReplyAsync("Done");
            }
        }
    }
}