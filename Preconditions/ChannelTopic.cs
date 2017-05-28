using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace PassiveBOT.preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RequireTopic : PreconditionAttribute
    {
        private readonly string _name;

        public RequireTopic(string name)
        {
            _name = name;
        }
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider prov)
        {
            var t = context.Channel as ITextChannel;
            if (t.Topic.Contains($"[{_name}]") || context.Channel is IDMChannel)
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(
                PreconditionResult.FromError($"Command is only available in channels containing `[{_name}]` in their topic"));
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class BanTopic : PreconditionAttribute
    {
        private readonly string _name;

        public BanTopic(string name)
        {
            _name = name;
        }
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider prov)
        {
            var t = context.Channel as ITextChannel;
            return Task.FromResult(t.Topic.Contains($"[{_name}]") ? PreconditionResult.FromError($"Command is disabled in channels containing `[{_name}]` within their topic") : PreconditionResult.FromSuccess());
        }
    }
}