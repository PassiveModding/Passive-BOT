using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace PassiveBOT.Discord.Addons.Interactive.Criteria
{
    public class EnsureFromUserCriterion : ICriterion<SocketMessage>
    {
        private readonly ulong _id;

        public EnsureFromUserCriterion(ulong id)
        {
            _id = id;
        }

        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketMessage parameter)
        {
            var ok = _id == parameter.Author.Id;
            return Task.FromResult(ok);
        }
    }
}