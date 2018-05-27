using System.Linq;
using Discord;

namespace PassiveBOT.Discord.Extensions
{
    public class CheckAdmin
    {
        public static bool IsAdmin(Context.Context context)
        {
            return (context.User as IGuildUser).RoleIds.Any(x => context.Server.Moderation.AdminRoleIDs.Contains(x)) || (context.User as IGuildUser).GuildPermissions.Administrator;
        }
    }
}