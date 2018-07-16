namespace PassiveBOT.Preconditions
{
    using System;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Commands;

    using Microsoft.Extensions.DependencyInjection;

    using PassiveBOT.Handlers;
    using PassiveBOT.Models;

    /// <summary>
    ///     A precondition to check for the guilds owner
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class NsfwAllowed : PreconditionAttribute
    {
        /// <summary>
        ///     This will check whether or not a user has permissions to use a command/module
        /// </summary>
        /// <param name="context">The Command Context</param>
        /// <param name="command">The command being invoked</param>
        /// <param name="services">The service provider</param>
        /// ///
        /// <returns>Success if the user is the owner of the current guild</returns>
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            // If the command is invoked in a DM channel we return an error
            if (context.Channel is IDMChannel)
            {
                return Task.FromResult(PreconditionResult.FromError("This is a guild only command!"));
            }

            var database = services.GetRequiredService<DatabaseHandler>().Execute<GuildModel>(DatabaseHandler.Operation.LOAD, null, context.Guild.Id);

            // Check to see if the current user's ID matches the guild owners 
            return Task.FromResult(database.Settings.Nsfw.Enabled ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("NSFW is disabled in this guild."));
        }
    }
}