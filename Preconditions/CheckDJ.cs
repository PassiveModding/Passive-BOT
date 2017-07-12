using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Configuration;

namespace PassiveBOT.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CheckDj : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            var id = context.Guild.Id;
            var role = GuildConfig.Load(id).DjRoleId;
            if (role == 0)
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            var users = (context.Guild as SocketGuild).Users;
            foreach (var user in users)
                if (user.Roles.Contains((context.Guild as SocketGuild).GetRole(role)))
                    if (context.User == user)
                        return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(PreconditionResult.FromError("User is Not DJ"));
        }
    }
}