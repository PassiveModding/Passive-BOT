using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;

namespace PassiveBOT
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RatelimitAttribute : PreconditionAttribute
    {
        private readonly uint _invokeLimit;
        private readonly bool _noLimitInDMs;
        private readonly TimeSpan _invokeLimitPeriod;
        private readonly Dictionary<ulong, CommandTimeout> _invokeTracker = new Dictionary<ulong, CommandTimeout>();

        public RatelimitAttribute(uint times, double period, Measure measure, bool noLimitInDMs = false)
        {
            _invokeLimit = times;
            _noLimitInDMs = noLimitInDMs;

            //TODO: C# 7 candidate switch expression
            switch (measure)
            {
                case Measure.Days:
                    _invokeLimitPeriod = TimeSpan.FromDays(period);
                    break;
                case Measure.Hours:
                    _invokeLimitPeriod = TimeSpan.FromHours(period);
                    break;
                case Measure.Minutes:
                    _invokeLimitPeriod = TimeSpan.FromMinutes(period);
                    break;
                case Measure.Seconds:
                    _invokeLimitPeriod = TimeSpan.FromSeconds(period);
                    break;
                case Measure.Milliseconds:
                    _invokeLimitPeriod = TimeSpan.FromMilliseconds(period);
                    break;
            }
        }

        public RatelimitAttribute(uint times, TimeSpan period, bool noLimitInDMs = false)
        {
            _invokeLimit = times;
            _noLimitInDMs = noLimitInDMs;
            _invokeLimitPeriod = period;
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            if (context.Channel is IPrivateChannel && _noLimitInDMs)
                return Task.FromResult(PreconditionResult.FromSuccess());

            var now = DateTime.UtcNow;
            var timeout = (_invokeTracker.TryGetValue(context.User.Id, out
             var t) && ((now - t.FirstInvoke) < _invokeLimitPeriod)) ? t : new CommandTimeout(now);

            timeout.TimesInvoked++;

            if (timeout.TimesInvoked <= _invokeLimit)
            {
                _invokeTracker[context.User.Id] = timeout;
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            else
            {
                return Task.FromResult(PreconditionResult.FromError("Timeout"));
            }
        }

        private class CommandTimeout
        {
            public uint TimesInvoked
            {
                get;
                set;
            }
            public DateTime FirstInvoke
            {
                get;
            }

            public CommandTimeout(DateTime timeStarted)
            {
                FirstInvoke = timeStarted;
            }
        }
    }

    public enum Measure
    {

        Days,

        Hours,

        Minutes,

        Seconds,
 
        Milliseconds
    }
}