using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Configuration;
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
            var GuildObj = GuildConfig.GetServer(Context.Guild);
            var userobj = GuildObj.Levels.Users.FirstOrDefault(x => x.userID == user.Id);

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

        [Command("ShowLeaderboard")]
        [Summary("ShowLeaderboard")]
        [Remarks("Show the user level leaderboard for this server")]
        public async Task ShowLeaderboard()
        {
            var GuildObj = GuildConfig.GetServer(Context.Guild);
            var userlist = new List<PaginatedMessage.Page>();
            var userindex = 1;
            var desc = "";

            foreach (var user in GuildObj.Levels.Users.OrderByDescending(x => x.xp))
            {
                var guser = Context.Guild.GetUser(user.userID);
                if (guser == null) continue;
                desc += $"`{userindex}` {guser.Username} `LV: {user.level - 1} XP: {user.xp}`\n";
                userindex++;
                if (desc.Split("\n").Length > 20)
                {
                    userlist.Add(new PaginatedMessage.Page
                    {
                        description = desc
                    });
                    desc = "";
                }
            }

            userlist.Add(new PaginatedMessage.Page
            {
                description = desc
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