using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Models;

namespace PassiveBOT.Discord.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class BotModerator : PreconditionAttribute
    {
        /// <summary>
        ///     This will check wether or not a user has permissions to use a command/module
        /// </summary>
        /// <param name="context"></param>
        /// <param name="command"></param>
        /// <param name="services"></param>
        /// ///
        /// <returns></returns>
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Channel is IDMChannel) return Task.FromResult(PreconditionResult.FromError("User is Not a Bot Moderator!"));

            //Check wether or not the user is the Bot owner.
            var own = context.Client.GetApplicationInfoAsync();
            if (own.Result.Owner.Id == context.User.Id)
                return Task.FromResult(PreconditionResult.FromSuccess());
            var guser = (IGuildUser) context.User;

            var HM = HomeModel.Load();
            if (HM.BotModerator == 0)
            {
                return Task.FromResult(PreconditionResult.FromError("User is Not a Bot Moderator!"));
            }

            if (guser.RoleIds.Any(x => HomeModel.Load().BotModerator == x))
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }

            return Task.FromResult(PreconditionResult.FromError("User is Not a Bot Moderator!"));
        }
    }
}