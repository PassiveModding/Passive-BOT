using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
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
        [Command("PartnerList+")]
        [Summary("PartnerList+")]
        [Remarks("Set the PartnerUpdatesChannel")]
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
            pages.Add("PassiveBOT <3");
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
                Pages = pages,
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
    }
}