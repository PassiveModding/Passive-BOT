using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Discord.Context;
using PassiveBOT.Discord.Context.Interactive.Paginator;
using PassiveBOT.Discord.Extensions;

namespace PassiveBOT.Modules.Info
{
    [RequireContext(ContextType.Guild)]
    public class Levels : Base
    {
        [Command("Rank")]
        [Summary("Rank <@user>")]
        [Remarks("Find your rank")]
        public async Task Rank(IUser user = null)
        {
            var luser = Context.Server.Levels.Users.FirstOrDefault(x => x.UserID == (user?.Id ?? Context.User.Id));
            if (luser == null)
            {
                throw new Exception("Error, Mising User");
            }

            var embed = new EmbedBuilder
            {
                Title = $"{user?.Username ?? Context.User.Username}'s Rank",
                Color = Color.Purple,
                ThumbnailUrl = user != null ? user.GetAvatarUrl() : Context.User.GetAvatarUrl()
            };
            embed.AddField("Level", $"{luser.Level - 1}",true);
            embed.AddField("XP", $"{luser.XP}", true);
            embed.AddField("Rank", $"#{Context.Server.Levels.Users.OrderByDescending(x => x.XP).ToList().FindIndex(u => u == luser) + 1}", true);
            await SendEmbedAsync(embed);
            Context.Server.Save();
        }

        [Command("Leaderboard")]
        [Summary("Leaderboard")]
        [Remarks("Display the leaderboard")]
        public async Task Leaderboard()
        {
            var users = Context.Server.Levels.Users.OrderByDescending(x => x.XP).Where(x => Context.Socket.Guild.GetUser(x.UserID) != null).ToList();
            var rgx = new Regex("[^a-zA-Z0-9 -#]");
            var stringlist = users.Select(x => $"`{$"#{users.IndexOf(x) + 1} - {rgx.Replace(Context.Socket.Guild.GetUser(x.UserID).ToString(), "")}".PadRight(40)}\u200B || LV: {x.Level - 1} XP: {x.XP}`").ToList();
            var pages = TextManagement.splitList(stringlist, 20).Select(x => new PaginatedMessage.Page
            {
                description = string.Join("\n", x)
            });
            var Pager = new PaginatedMessage
            {
                Title = $"{Context.Guild.Name} Levels",
                Pages = pages
            };
            await PagedReplyAsync(Pager);
        }
    }
}
