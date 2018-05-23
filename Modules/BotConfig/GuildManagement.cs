using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using PassiveBOT.Discord.Context;
using PassiveBOT.Discord.Context.Interactive.Paginator;
using PassiveBOT.Handlers;
using PassiveBOT.Models;

namespace PassiveBOT.Modules.BotConfig
{
    public class GuildManagement : Base
    {
        [Command("ConfigObj")]
        [Summary("ConfigObj")]
        [Remarks("x")]
        public async Task SetHS()
        {
            var pages = new List<PaginatedMessage.Page>();
            foreach (var guild in DatabaseHandler.GetFullConfig())
            {
                pages.Add(new PaginatedMessage.Page
                {
                    description = guild.ID.ToString()
                });
            }

            var pager = new PaginatedMessage
            {
                Pages = pages
            };
            await PagedReplyAsync(pager);
        }
    }
}
