using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotsList.Api.Extensions.DiscordNet;
using PassiveBOT.Configuration;
using PassiveBOT.Configuration.Objects;
using PassiveBOT.Handlers.Services.Interactive;
using PassiveBOT.Handlers.Services.Interactive.Paginator;
using PassiveBOT.preconditions;

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


        [Command("NoToxicityDisable+")]
        [Summary("NoToxicityDisable+")]
        [Remarks("Toggle Toxicity")]
        public async Task NTDis()
        {
            var hs = Homeserver.Load();
            hs.NoToxicityDisabled = !hs.NoToxicityDisabled;
            Homeserver.SaveHome(hs);
            await ReplyAsync($"Toxicity Disabled: {hs.NoToxicityDisabled}");
        }

        [Command("CheckMsgDisable+")]
        [Summary("CheckMsgDisable+")]
        [Remarks("Toggle AntiSpam & Levelling features")]
        public async Task ASTog()
        {
            var hs = Homeserver.Load();
            hs.DisableCheckMsg = !hs.DisableCheckMsg;
            Homeserver.SaveHome(hs);
            await ReplyAsync($"Antispam Disabled: {hs.NoToxicityDisabled}");
        }

        [Command("ViewMSGSpam+")]
        [Summary("ViewMSGSpam+")]
        [Remarks("View spam by server activity.")]
        public async Task VSpam()
        {
            var pages = new List<PaginatedMessage.Page>();
            var s2 = new StringBuilder();
            foreach (var guild in Load.GuildMsgCounts.OrderByDescending(x => x.msgs)) //Context.Client.Guilds.OrderByDescending(x => x.MemberCount))
            {
                var firststring = Context.Client.GetGuild(guild.GuildID);
                s2.Append($"S: {firststring?.Name ?? "Unknown"} || ID:{guild.GuildID} || M:{guild.msgs}\n");

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