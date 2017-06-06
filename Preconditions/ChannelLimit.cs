using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace PassiveBOT.preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RequireChannel : PreconditionAttribute
    {
        private readonly string _name;

        public RequireChannel(string name)
        {
            _name = name;
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command,
            IServiceProvider prov)
        {
            if (context.Channel.Name == _name || context.Channel.Name.StartsWith(_name + "-") ||
                context.Channel is IDMChannel)
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(
                PreconditionResult.FromError(
                    $"Command is locked to channels titled `{_name}` or that start with `{_name}-`"));
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class BanChannel : PreconditionAttribute
    {
        private readonly string _name;

        public BanChannel(string name)
        {
            _name = name;
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command,
            IServiceProvider prov)
        {
            if (context.Channel.Name == _name || context.Channel.Name.StartsWith(_name + "-"))
                return Task.FromResult(
                    PreconditionResult.FromError(
                        $"Command is banned from being used in `{_name}` and channels starting with `{_name}-`"));
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}