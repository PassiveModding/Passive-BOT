namespace PassiveBOT.Modules.GuildCommands
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using global::Discord;
    using global::Discord.Addons.Interactive;
    using global::Discord.Commands;

    using PassiveBOT.Discord.Context;
    using PassiveBOT.Discord.Extensions;

    /// <summary>
    /// The levels commands
    /// </summary>
    [RequireContext(ContextType.Guild)]
    public class Levels : Base
    {
        /// <summary>
        /// Shows the users rank
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Throws if invalid user/no rank
        /// </exception>
        [Command("Level")]
        [Alias("Rank")]
        [Summary("Find the level of a user")]
        [Remarks("Will default to the current user if none specified")]
        public async Task Rank(IUser user = null)
        {
            var levelUser = Context.Server.Levels.Users.FirstOrDefault(x => x.UserID == (user?.Id ?? Context.User.Id));
            if (levelUser == null)
            {
                throw new Exception("Error, Missing User");
            }

            var embed = new EmbedBuilder
            {
                Title = $"{user?.Username ?? Context.User.Username}'s Rank",
                Color = Color.Purple,
                ThumbnailUrl = user != null ? user.GetAvatarUrl() : Context.User.GetAvatarUrl()
            };
            embed.AddField("Level", $"{levelUser.Level - 1}", true);
            embed.AddField("XP", $"{levelUser.XP}", true);
            embed.AddField("Rank", $"#{Context.Server.Levels.Users.OrderByDescending(x => x.XP).ToList().FindIndex(u => u == levelUser) + 1}", true);
            await ReplyAsync(embed);
            Context.Server.Save();
        }

        /// <summary>
        /// The leader board.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("LeaderBoard")]
        [Summary("Display the LeaderBoard")]
        public async Task LeaderBoard()
        {
            var users = Context.Server.Levels.Users.OrderByDescending(x => x.XP).Where(x => Context.Guild.GetUser(x.UserID) != null).Take(100).ToList();
            var rgx = new Regex("[^a-zA-Z0-9 -#]");
            var list = users.Select(x => $"`{$"#{users.IndexOf(x) + 1} - {rgx.Replace(Context.Guild.GetUser(x.UserID).ToString(), "")}".PadRight(40)}\u200B || LV: {x.Level - 1} XP: {x.XP}`").ToList();
            var pages = TextManagement.SplitList(list, 20).Select(x => new PaginatedMessage.Page
            {
                Description = string.Join("\n", x)
            });
            var pager = new PaginatedMessage
            {
                Title = $"{Context.Guild.Name} Levels",
                Pages = pages,
                Color = Color.DarkRed
            };
            await PagedReplyAsync(pager, new ReactionList
                                             {
                                                 Forward = true,
                                                 Backward = true
                                             });
        }

        /// <summary>
        /// The leader board.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("Ranks")]
        [Summary("Display the Level Rewards")]
        public async Task Ranks()
        {
            if (!Context.Server.Levels.RewardRoles.Any())
            {
                throw new Exception("There are no ranks in this server");
            }

            await SimpleEmbedAsync(string.Join("\n", Context.Server.Levels.RewardRoles.OrderByDescending(x => x.Requirement).Select(x => Context.Guild.GetRole(x.RoleID)?.Mention).Where(x => x != null)));
        }
    }
}