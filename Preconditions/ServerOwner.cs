using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace PassiveBOT.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ServerOwner : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command,
            IServiceProvider prov)
        {
            if (context.Channel is IDMChannel) return await Task.FromResult(PreconditionResult.FromSuccess());

            var own = context.Client.GetApplicationInfoAsync();
            if (own.Result.Owner.Id == context.User.Id)
                return await Task.FromResult(PreconditionResult.FromSuccess());

            if (context.Guild.OwnerId == context.User.Id)
                return await Task.FromResult(PreconditionResult.FromSuccess());

            return await Task.FromResult(
                PreconditionResult.FromError(
                    "This Command can only be performed by the server owner"));
        }
    }
}