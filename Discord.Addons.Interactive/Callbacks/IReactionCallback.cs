using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Discord.Addons.Interactive.Criteria;

namespace PassiveBOT.Discord.Addons.Interactive.Callbacks
{
    public interface IReactionCallback
    {
        RunMode RunMode { get; }
        ICriterion<SocketReaction> Criterion { get; }
        TimeSpan? Timeout { get; }
        SocketCommandContext Context { get; }

        Task<bool> HandleCallbackAsync(SocketReaction reaction);
    }
}