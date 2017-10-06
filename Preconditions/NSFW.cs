using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace PassiveBOT.preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class CheckNSFW : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command,
            IServiceProvider prov)
        {
            if (context.Channel.IsNsfw ||
                context.Channel is IDMChannel)
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(
                PreconditionResult.FromError(
                    $"This command is locked to NSFW Channels. Pervert."));
        }
    }
}