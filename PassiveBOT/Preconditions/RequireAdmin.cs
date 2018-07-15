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

    /// <summary>
    /// The require admin precondition
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireAdmin : PreconditionAttribute
    {
        /// <summary>
        /// The _allow administrator.
        /// </summary>
        private readonly bool allowAdministrator;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequireAdmin"/> class.
        /// </summary>
        /// <param name="allowAdminPermission">
        /// The allow admin permission.
        /// </param>
        public RequireAdmin(bool allowAdminPermission = true)
        {
            allowAdministrator = allowAdminPermission;
        }

        /// <summary>
        ///     This will check whether or not a user has permissions to use a command/module
        /// </summary>
        /// <param name="context">The message context</param>
        /// <param name="command">The command being invoked</param>
        /// <param name="services">The service provider</param>
        /// ///
        /// <returns>
        /// A precondition result dictating whether or not the command is allowed to run.
        /// </returns>
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Channel is IDMChannel)
            {
                return Task.FromResult(PreconditionResult.FromError("Administrator permissions are only accessible through a guild."));
            }

            // Check whether or not the user is the Bot owner.
            var own = context.Client.GetApplicationInfoAsync();
            if (own.Result.Owner.Id == context.User.Id)
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }

            // If we have allow admin permissions toggled on we allow users who have the permissions in the server
            var guildUser = context.User as IGuildUser;
            var guild = services.GetRequiredService<DatabaseHandler>().Execute<GuildModel>(DatabaseHandler.Operation.LOAD, null, context.Guild.Id);
            if (allowAdministrator && guildUser.GuildPermissions.Administrator)
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }

            // check to see if the user has an admin role in the server
            // Return an error stating the user is not an admin
            return Task.FromResult(guild.Moderation.AdminRoleIDs.Any(x => guildUser.RoleIds.Contains(x)) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("User is Not an Admin!"));
        }
    }
}