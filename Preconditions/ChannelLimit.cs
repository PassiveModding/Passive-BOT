using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace PassiveBOT.preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class NsfWchat : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command,
            IServiceProvider prov)
        {
            if (context.Channel.Name == "nsfw" || context.Channel.Name.StartsWith("nsfw-") ||
                context.Channel is IDMChannel)
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(
                PreconditionResult.FromError("Command does not function in channels without the title #nsfw"));
        }
    }
}