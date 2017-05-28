using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace PassiveBOT.Configuration
{
    public class Utilities
    {
        public static IRole ManageRole(SocketGuild guild)
        {
            return guild.Roles.FirstOrDefault(role => role.Permissions.ManageRoles);
        }

        public static IRole GetMutedRole(SocketGuild guild)
        {
            return guild.Roles.FirstOrDefault(role => role.Name == "Muted");
        }

        private static async Task<IGuildUser> GetUserByName(IGuild guild, string name)
        {
            var users = await guild.GetUsersAsync();
            foreach (var user in users)
                if (user.Username.Contains(name))
                    return user;
                else if (user.Nickname != null && user.Nickname.Contains(name))
                    return user;
            return null;
        }

        public static async Task<IGuildUser> GetUser(IGuild guild, string input)
        {
            var nameResult = await GetUserByName(guild, input);
            return nameResult;
        }
    }
}