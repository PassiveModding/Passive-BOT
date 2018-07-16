namespace PassiveBOT.Preconditions
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Commands;

    using Microsoft.Extensions.DependencyInjection;

    using PassiveBOT.Handlers;
    using PassiveBOT.Models;
    using PassiveBOT.Services;

    /// <summary>
    ///     The require admin precondition
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class UsingCustomChannels : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Channel is IDMChannel)
            {
                return Task.FromResult(PreconditionResult.FromError("This is a guild only command"));
            }

            if (services.GetRequiredService<ChannelService>().GetCustomChannels(context.Guild.Id) == null)
            {
                return Task.FromResult(PreconditionResult.FromError("Custom Channels are disabled in the current server."));
            }

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}