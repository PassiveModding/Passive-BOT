using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace PassiveBOT.preconditions

{
    //from Discord.Addons (slightly edited)
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RatelimitAttribute : PreconditionAttribute
    {
        private readonly uint _invokeLimit;
        private readonly TimeSpan _invokeLimitPeriod;
        private readonly Dictionary<ulong, CommandTimeout> _invokeTracker = new Dictionary<ulong, CommandTimeout>();
        private readonly bool _noLimitInDMs;

        public RatelimitAttribute(uint times, double period, Measure measure, bool noLimitInDMs = false)
        {
            _invokeLimit = times;
            _noLimitInDMs = noLimitInDMs;

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
                default:
                    throw new ArgumentOutOfRangeException(nameof(measure), measure, null);
            }
        }

        public RatelimitAttribute(uint times, TimeSpan period, bool noLimitInDMs = false)
        {
            _invokeLimit = times;
            _noLimitInDMs = noLimitInDMs;
            _invokeLimitPeriod = period;
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command,
            IServiceProvider prov)
        {
            if (context.Channel is IPrivateChannel && _noLimitInDMs)
                return Task.FromResult(PreconditionResult.FromSuccess());

            var now = DateTime.UtcNow;
            var timeout = _invokeTracker.TryGetValue(context.User.Id, out
                              var t) && now - t.FirstInvoke < _invokeLimitPeriod
                ? t
                : new CommandTimeout(now);

            timeout.TimesInvoked++;

            string timeoutstr;
            var timeleft = _invokeLimitPeriod.Subtract(now - timeout.FirstInvoke);
            if (timeleft.Minutes > 0)
                if (timeleft.Hours > 0)
                    timeoutstr = timeleft.Days > 0 ? $"{timeleft.Days} Days" : $"{timeleft.Hours} Hours";
                else
                    timeoutstr = $"{timeleft.Minutes} Minutes";
            else
                timeoutstr = $"{timeleft.Seconds} Seconds";

            if (timeout.TimesInvoked > _invokeLimit)
                return Task.FromResult(PreconditionResult.FromError($"Timeout for another {timeoutstr}"));
            _invokeTracker[context.User.Id] = timeout;
            return Task.FromResult(PreconditionResult.FromSuccess());
        }

        private class CommandTimeout
        {
            public CommandTimeout(DateTime timeStarted)
            {
                FirstInvoke = timeStarted;
            }

            public uint TimesInvoked { get; set; }

            public DateTime FirstInvoke { get; }
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