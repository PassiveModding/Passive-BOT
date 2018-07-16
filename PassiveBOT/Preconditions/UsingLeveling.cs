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
    public class UsingLeveling : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Channel is IDMChannel)
            {
                return Task.FromResult(PreconditionResult.FromError("Administrator permissions are only accessible through a guild."));
            }

            if (services.GetRequiredService<LevelService>().GetLevelSetup(context.Guild.Id) == null)
            {
                return Task.FromResult(PreconditionResult.FromError("Leveling is disabled in the current server."));
            }

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}