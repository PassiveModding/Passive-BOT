using System.Threading.Tasks;
using Discord.Commands;

namespace PassiveBOT.Handlers.Services.Interactive.Criteria
{
    public interface ICriterion<T>
    {
        Task<bool> JudgeAsync(SocketCommandContext sourceContext, T parameter);
    }
}