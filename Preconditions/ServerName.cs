using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System.Linq;
using System.IO;

namespace PassiveBOT
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class NoPrefix : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            var lines = File.ReadAllLines(AppContext.BaseDirectory + $"moderation/prefix/nopre.txt");
            List<string> result = lines.ToList();
            var id = context.Guild.Id.ToString();
            if (result.Contains(id))
                return Task.FromResult(PreconditionResult.FromError("Command is disabled on this server"));
            else
                return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}