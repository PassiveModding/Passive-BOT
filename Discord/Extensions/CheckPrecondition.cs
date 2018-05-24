using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;

namespace PassiveBOT.Discord.Extensions
{
    public class CheckPrecondition
    {
        public static bool preconditioncheck(ICommandContext context, CommandInfo cmd, IServiceProvider provider)
        {
            return cmd.CheckPreconditionsAsync(context, provider).Result.IsSuccess;
        }
    }
}
