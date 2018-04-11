using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Configuration;
using PassiveBOT.Discord.Addons.Interactive;
using PassiveBOT.Discord.Addons.Interactive.Paginator;
using PassiveBOT.Handlers;

namespace PassiveBOT.Commands.OwnerCmds
{
    [Preconditions.BotModerator]
    public class BotModerator : InteractiveBase
    {
        [Command("PartnerList+", RunMode = RunMode.Async)]
        [Summary("PartnerList+")]
        [Remarks("Get a list of all partner servers")]
        public async Task PList()
        {
            var pages = new List<string>();
            var currentpage = "";
            foreach (var guild in TimerService.AcceptedServers)
                if (Context.Client.GetGuild(guild) is SocketGuild guildfull)
                {
                    var guildobj = GuildConfig.GetServer(guildfull);
                    if (currentpage.Length > 1000)
                    {
                        pages.Add(currentpage);
                        currentpage = "";
                    }

                    currentpage += $"`{guildobj.GuildId} - {(guildobj.PartnerSetup.banned ? "BANNED" : "PUBLIC")}`" +
                                   "\n" +
                                   $"{guildobj.PartnerSetup.Message}" +
                                   "\n-----\n";
                }

            pages.Add(currentpage);
            var msg = new PaginatedMessage
            {
                Title = "Partner Messages",
                Pages = pages.Select(x => new PaginatedMessage.Page
                    {description = x}),
                Color = new Color(114, 137, 218)
            };

            await PagedReplyAsync(msg);
        }

        [Command("PartnerListFull+", RunMode = RunMode.Async)]
        [Summary("PartnerListFull+")]
        [Remarks("Get a complete list of all partner servers")]
        public async Task PListF()
        {
            var pages = new List<string>();
            var currentpage = "";
            foreach (var guild in Context.Client.Guilds)
            {
                try
                {
                    var guildobj = GuildConfig.GetServer(guild);

                    if (guildobj.PartnerSetup.IsPartner)
                    {
                        if (currentpage.Length > 1000)
                        {
                            pages.Add(currentpage);
                            currentpage = "";
                        }

                        currentpage += $"`{guildobj.GuildId} - {(guildobj.PartnerSetup.banned ? "BANNED" : "PUBLIC")}`" +
                                        "\n" +
                                        $"{guildobj.PartnerSetup.Message}" +
                                        "\n-----\n";
                    }

                }
                catch
                {
                    //
                }
            }


            pages.Add(currentpage);
            var msg = new PaginatedMessage
            {
                Title = "Partner Messages",
                Pages = pages.Select(x => new PaginatedMessage.Page
                    { description = x }),
                Color = new Color(114, 137, 218)
            };

            await PagedReplyAsync(msg);
        }


        [Command("PartnerInfo+", RunMode = RunMode.Async)]
        [Summary("PartnerInfo+ <ID>")]
        [Remarks("Get Partner information via guild ID")]
        public async Task Pinfo2(ulong GuildID)
        {
            var Guild = Context.Client.Guilds.FirstOrDefault(x => x.Id == GuildID);
            if (Guild == null)
            {
                await ReplyAsync("Invalid Guild");
                return;
            }
            var guild = GuildConfig.GetServer(Guild);
            var pages = new List<PaginatedMessage.Page>();
            var PChannel = Guild.Channels.FirstOrDefault(x => x.Id == guild.PartnerSetup.PartherChannel);
            var Pchannelname = PChannel == null ? "Null" : PChannel.Name;
            pages.Add(new PaginatedMessage.Page
            {
                dynamictitle = "Info",
                description = $"{Guild.Name} - [{Guild.Id}]\n" +
                          $"Is Partner: { guild.PartnerSetup.IsPartner}\n" +
                          $"Is Banned: {guild.PartnerSetup.banned}\n" +
                          $"Partner Channel ID: {guild.PartnerSetup.PartherChannel}\n" +
                          $"Partner Channel Name: {Pchannelname}\n"
            });
            pages.Add(new PaginatedMessage.Page
            {
                dynamictitle = $"Server Message",
                description = $"---\n" +
                          $"{guild.PartnerSetup.Message}" +
                          $"\n---"
            });
            if (PChannel != null)
            {
                var ChannelOverWrites = PChannel.PermissionOverwrites;
                var Checking = "";
                foreach (var OverWrite in ChannelOverWrites)
                {
                    try
                    {
                        var Name = "N/A";
                        if (OverWrite.TargetType == PermissionTarget.Role)
                        {
                            var Role = Guild.Roles.FirstOrDefault(x => x.Id == OverWrite.TargetId);
                            if (Role != null)
                            {
                                Name = Role.Name;
                            }
                        }
                        else
                        {
                            var user = Guild.Users.FirstOrDefault(x => x.Id == OverWrite.TargetId);
                            if (user != null)
                            {
                                Name = user.Username;
                            }
                        }

                        if (OverWrite.Permissions.ReadMessages == PermValue.Deny)
                        {
                            Checking += $"{Name} Cannot Read Msgs.\n";
                        }

                        if (OverWrite.Permissions.ReadMessageHistory == PermValue.Deny)
                        {
                            Checking += $"{Name} Cannot Read History.\n";
                        }
                        
                    }
                    catch
                    {
                        //
                    }

                }

                if (Checking != "")
                {
                    pages.Add(new PaginatedMessage.Page
                    {
                        dynamictitle = "Partner Channel Perms",
                        description = $"---\n" +
                                  $"{Checking}" +
                                  $"\n---"
                    });
                }

            }
            var msg = new PaginatedMessage
            {
                Title = "Partner Messages",
                Pages = pages,
                Color = new Color(114, 137, 218)
            };

            await PagedReplyAsync(msg);
        }

        [Command("BanPartner+")]
        [Summary("BanPartner+ <guildID>")]
        [Remarks("Ban a partner from the partners program")]
        public async Task BanPartner(ulong GuildId = 0)
        {
            var guildobj = GuildConfig.GetServer(Context.Client.GetGuild(GuildId));
            guildobj.PartnerSetup.banned = true;
            GuildConfig.SaveServer(guildobj);
            await ReplyAsync("Guild Partnership Banned for:\n" +
                             $"{guildobj.PartnerSetup.Message}");
        }

        [Command("UnbanPartner+")]
        [Summary("UnbanPartner+ <guildID>")]
        [Remarks("Ban a partner from the partners program")]
        public async Task UnBanPartner(ulong GuildId = 0)
        {
            var guildobj = GuildConfig.GetServer(Context.Client.GetGuild(GuildId));
            guildobj.PartnerSetup.banned = false;
            guildobj.PartnerSetup.Message = "";
            GuildConfig.SaveServer(guildobj);
            await ReplyAsync("Guild Partnership Unbanned. Message has been reset");
        }

        [Command("ViewRedditCache+")]
        [Summary("ViewRedditCache+")]
        [Remarks("Get a list of cached subreddits")]
        public async Task RCache()
        {
            var pages = new List<string>();
            var curpage = "";
            foreach (var sub in CommandHandler.SubReddits)
            {
                curpage += $"__{sub.title}__\n" +
                           $"Cached Posts: {sub.Posts.Count}\n" +
                           $"NSFW Posts: {sub.Posts.Count(x => x.NSFW)}\n" +
                           $"Last Refresh: {sub.LastUpdate}\n" +
                           $"Hits Since Refresh: {sub.Hits}\n";
                if (curpage.Length > 400)
                {
                    pages.Add(curpage);
                    curpage = "";
                }
            }
            pages.Add(curpage);
            var msg = new PaginatedMessage
            {
                Title = "Reddit Cache",
                Pages = pages.Select(x => new PaginatedMessage.Page
                {
                    description = x
                }),
                Color = new Color(114, 137, 218)
            };

            await PagedReplyAsync(msg);
        }

        [Command("ClearRedditCache+")]
        [Summary("ClearRedditCache+")]
        [Remarks("Clear all cached subreddits")]
        public async Task CCache()
        {
            CommandHandler.SubReddits = new List<CommandHandler.SubReddit>();
            await ReplyAsync("SubReddit Cache Cleared");
        }

        [Command("GetCMDUses+")]
        [Summary("GetCMDUses+")]
        [Remarks("Get a list of all commands that have been used since restart")]
        public async Task CUses()
        {
            var pages = new List<string>();
            var curpage = "";
            foreach (var cmd in CommandHandler.CommandUses.OrderByDescending(x => x.Uses))
            {
                curpage += $"{cmd.Name} - {cmd.Uses}\n";
                if (curpage.Length > 400)
                {
                    pages.Add(curpage);
                    curpage = "";
                }
            }
            pages.Add(curpage);
            var msg = new PaginatedMessage
            {
                Title = "Command Uses",
                Pages = pages.Select(x => new PaginatedMessage.Page
                {
                    description = x
                }),
                Color = new Color(114, 137, 218)
            };

            await PagedReplyAsync(msg);
        }
    }
}