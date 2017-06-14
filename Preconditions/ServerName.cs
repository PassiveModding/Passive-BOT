using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace PassiveBOT.Preconditions

{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class NoPrefix : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command,
            IServiceProvider prov)
        {
            var lines = File.ReadAllLines(AppContext.BaseDirectory + "setup/moderation/prefix/nopre.txt");
            var result = lines.ToList();
            var id = context.Guild.Id.ToString();
            if (result.Contains(id))
                return Task.FromResult(PreconditionResult.FromError("Command is disabled on this server"));
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}