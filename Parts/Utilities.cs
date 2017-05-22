using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Discord;
using Discord.WebSocket;

namespace PassiveBOT
{
    public class Utilities
    {
        public static IRole ManageRole(SocketGuild Guild)
        {
            foreach (SocketRole Role in Guild.Roles)
                if (Role.Permissions.ManageRoles)
                    return Role;
            return null;
        }

        public static IRole GetMutedRole(SocketGuild Guild)
        {
            foreach (SocketRole Role in Guild.Roles)
                if (Role.Name == "Muted")
                    return Role;
            return null;
        }

        private static async Task<IGuildUser> GetUserByName(IGuild Guild, String Name)
        {
            IReadOnlyCollection<IGuildUser> users = await Guild.GetUsersAsync();
            foreach (IGuildUser user in users)
            {
                if (user.Username.Contains(Name))
                    return user;
                else if (user.Nickname != null && user.Nickname.Contains(Name))
                    return user;
            }
            return null;
        }

        public static async Task<IGuildUser> GetUser(IGuild Guild, String Input)
        {
            IGuildUser nameResult = await GetUserByName(Guild, Input);
            if (nameResult != null)
                return nameResult;
            else
                return null;
        }
    }
}