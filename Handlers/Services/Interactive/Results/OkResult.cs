using Discord.Commands;

namespace PassiveBOT.Handlers.Services.Interactive.Results
{
    public class OkResult : RuntimeResult
    {
        public OkResult(string reason = null) : base(null, reason)
        {
        }
    }
}