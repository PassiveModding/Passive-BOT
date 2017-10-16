using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace PassiveBOT.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ServerOwner : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command,
            IServiceProvider prov)
        {
            if (context.Guild.OwnerId == context.User.Id)
                return await Task.FromResult(PreconditionResult.FromSuccess());

            return await Task.FromResult(
                PreconditionResult.FromError(
                    "This Command can only be performed by the server owner"));
        }
    }
}