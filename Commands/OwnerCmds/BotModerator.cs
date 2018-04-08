using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Configuration;
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
    }
}