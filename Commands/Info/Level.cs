using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Configuration;
using PassiveBOT.Handlers;
using PassiveBOT.Handlers.Services.Interactive;
using PassiveBOT.Handlers.Services.Interactive.Paginator;

namespace PassiveBOT.Commands.Info
{
    [RequireContext(ContextType.Guild)]
    public class Level : InteractiveBase
    {
        [Command("ShowLevel")]
        [Summary("ShowLevel <@user>")]
        [Remarks("Show a user's level and XP")]
        public async Task SetLevelChannel(IUser user = null)
        {
            if (user == null) user = Context.User;
            var userobj = CommandHandler.Levels.FirstOrDefault(x => x.GuildID == Context.Guild.Id)?.Users.FirstOrDefault(x => x.userID == user.Id);

            if (userobj != null)
            {
                var embed = new EmbedBuilder
                {
                    Title = $"User Level for {user.Username}#{user.Discriminator}",
                    Description = $"Level: {userobj.level - 1}\n" +
                                  $"XP: {userobj.xp}"
                };

                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                await ReplyAsync($"User not in levelling system!");
            }
        }

        [Command("ShowLevelRewards")]
        [Summary("ShowLevelRewards")]
        [Remarks("Show rewards available in this server for different levels")]
        public async Task ShowLVRewards()
        {
            var GuildObj = GuildConfig.GetServer(Context.Guild);

            var embed = new EmbedBuilder
            {
                Title = $"Level Rewards in {Context.Guild.Name}",
                Description =
                    $"Reward Roles:\n {(GuildObj.Levels.LevelRoles.Any() ? string.Join("\n", GuildObj.Levels.LevelRoles.Where(lr => Context.Guild.Roles.Select(x => x.Id).Contains(lr.RoleID)).Select(x => $"{Context.Guild.GetRole(x.RoleID)?.Mention ?? "ERROR"} LV: {x.LevelToEnter}")) : "N/A")}"
            };
            await ReplyAsync("", false, embed.Build());
        }

        [Command("ShowLeaderboard")]
        [Summary("ShowLeaderboard")]
        [Remarks("Show the user level leaderboard for this server")]
        public async Task ShowLeaderboard()
        {
            var userlist = new List<PaginatedMessage.Page>();
            var userindex = 1;
            var desc = new StringBuilder();

            foreach (var user in CommandHandler.Levels.FirstOrDefault(x => x.GuildID == Context.Guild.Id).Users.OrderByDescending(x => x.xp))
            {
                var guser = Context.Guild.GetUser(user.userID);
                if (guser == null) continue;
                desc.Append($"`{userindex}` {guser.Username} `LV: {user.level - 1} XP: {user.xp}`\n");
                userindex++;
                if (desc.ToString().Split("\n").Length > 20)
                {
                    userlist.Add(new PaginatedMessage.Page
                    {
                        description = desc.ToString()
                    });
                    desc.Clear();
                }
            }

            userlist.Add(new PaginatedMessage.Page
            {
                description = desc.ToString()
            });
            var msg = new PaginatedMessage
            {
                Title = "User Levels Leaderboard",
                Pages = userlist
            };

            await PagedReplyAsync(msg);
        }
    }
}