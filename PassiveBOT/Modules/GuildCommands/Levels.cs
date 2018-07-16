namespace PassiveBOT.Modules.GuildCommands
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Addons.Interactive;
    using Discord.Commands;

    using PassiveBOT.Context;
    using PassiveBOT.Extensions;
    using PassiveBOT.Services;

    /// <summary>
    ///     The levels commands
    /// </summary>
    [RequireContext(ContextType.Guild)]
    [Summary("Level information for the server")]
    public class Levels : Base
    {
        public Levels(LevelService service)
        {
            Service = service;
        }

        private static LevelService Service { get; set; }

        /// <summary>
        ///     The leader board.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("LeaderBoard", RunMode = RunMode.Async)]
        [Summary("Display the LeaderBoard")]
        public Task LeaderBoardAsync()
        {
            var l = Service.GetLevelSetup(Context.Guild.Id);
            var users = l.Users.OrderByDescending(x => x.Value.XP).Where(x => Context.Guild.GetUser(x.Key) != null).Take(100).ToList();
            var rgx = new Regex("[^a-zA-Z0-9 -#]");
            var list = users.Select(x => $"`{$"#{users.IndexOf(x) + 1} - {rgx.Replace(Context.Guild.GetUser(x.Key).ToString(), string.Empty)}".PadRight(40)}\u200B || LV: {x.Value.Level - 1} XP: {x.Value.XP}`").ToList();
            var pages = list.SplitList(20).Select(x => new PaginatedMessage.Page { Description = string.Join("\n", x) });
            var pager = new PaginatedMessage { Title = $"{Context.Guild.Name} Levels", Pages = pages, Color = Color.DarkRed };
            return PagedReplyAsync(pager, new ReactionList { Forward = true, Backward = true });
        }

        /// <summary>
        ///     Shows the users rank
        /// </summary>
        /// <param name="user">
        ///     The user.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        /// <exception cref="Exception">
        ///     Throws if invalid user/no rank
        /// </exception>
        [Command("Level", RunMode = RunMode.Async)]
        [Alias("Rank")]
        [Summary("Find the level of a user")]
        [Remarks("Will default to the current user if none specified")]
        public Task RankAsync(IUser user = null)
        {
            var l = Service.GetLevelSetup(Context.Guild.Id);
            l.Users.TryGetValue(user?.Id ?? Context.User.Id, out var levelUser);
            if (levelUser == null)
            {
                throw new Exception("Error, Missing User");
            }

            var embed = new EmbedBuilder { Title = $"{user?.Username ?? Context.User.Username}'s Rank", Color = Color.Purple, ThumbnailUrl = user != null ? user.GetAvatarUrl() : Context.User.GetAvatarUrl() };

            var requiredXP = (levelUser.Level * 50) + ((levelUser.Level * levelUser.Level) * 25);
            var currentLvXP = ((levelUser.Level - 1) * 50) + (((levelUser.Level - 1) * (levelUser.Level - 1)) * 25);
            var progressInt = ((float)(requiredXP - levelUser.XP) / (requiredXP - currentLvXP)) * 10;
            var progressString = "`|";
            for (var i = 0; i < 10; i++)
            {
                if (progressInt > i)
                {
                    progressString += "x";
                }
                else
                {
                    progressString += "_";
                }
            }

            progressString += $"|` {(int)progressInt * 10}%";

            embed.AddField("Level", $"{levelUser.Level - 1}", true);
            embed.AddField("XP", $"{levelUser.XP}/{requiredXP}", true);
            embed.AddField("Rank", $"#{l.Users.OrderByDescending(x => x.Value.XP).Where(x => Context.Guild.GetUser(x.Key) != null).ToList().FindIndex(u => u.Key == levelUser.UserID) + 1}", true);
            embed.AddField("Progress Left", progressString);
            return ReplyAsync(embed);
        }

        /// <summary>
        ///     The leader board.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("Ranks", RunMode = RunMode.Async)]
        [Summary("Display the Level Rewards")]
        public Task RanksAsync()
        {
            var l = Service.GetLevelSetup(Context.Guild.Id);
            if (!l.RewardRoles.Any())
            {
                throw new Exception("There are no ranks in this server");
            }

            return SimpleEmbedAsync(string.Join("\n", l.RewardRoles.OrderByDescending(x => x.Requirement).Where(x => Context.Guild.GetRole(x.RoleID) != null).Select(x => $"{x.Requirement} - {Context.Guild.GetRole(x.RoleID).Mention}")));
        }
    }
}