using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Handlers;

namespace PassiveBOT.Discord.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireAdmin : PreconditionAttribute
    {
        private readonly bool _allowAdministrator;

        public RequireAdmin(bool AllowAdminPermission = true)
        {
            _allowAdministrator = AllowAdminPermission;
        }

        /// <summary>
        /// This will check wether or not a user has permissions to use a command/module
        /// </summary>
        /// <param name="context"></param>
        /// <param name="command"></param>
        /// <param name="services"></param>
        /// <returns></returns>
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            if (context.Channel is IDMChannel) return Task.FromResult(PreconditionResult.FromSuccess());

            //Check wether or not the user is the Bot owner.
            var own = context.Client.GetApplicationInfoAsync();
            if (own.Result.Owner.Id == context.User.Id)
                return Task.FromResult(PreconditionResult.FromSuccess());

            //If we have allow admin permissions toggled on we allow users who have the permissions in the server
            var guser = (IGuildUser)context.User;
            var guild = DatabaseHandler.GetGuild(context.Guild.Id);
            if (_allowAdministrator && guser.GuildPermissions.Administrator)
                return Task.FromResult(PreconditionResult.FromSuccess());

            //check to see if the user has an admin role in the server
            if (guild.Moderation.AdminRoleIDs.Any(x => guser.RoleIds.Contains(x)))
                return Task.FromResult(PreconditionResult.FromSuccess());

            //Return an error stating the user is not an admin
            return Task.FromResult(PreconditionResult.FromError("User is Not an Admin!"));
        }
    }
}
