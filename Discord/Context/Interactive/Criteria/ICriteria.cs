using System.Threading.Tasks;
using Discord.Commands;

namespace PassiveBOT.Discord.Context.Interactive.Criteria
{
    public interface ICriterion<in T>
    {
        Task<bool> JudgeAsync(SocketCommandContext sourceContext, T parameter);
    }
}