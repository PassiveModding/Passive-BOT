using Discord.Commands;

namespace PassiveBOT.Discord.Addons.Interactive.Results
{
    public class OkResult : RuntimeResult
    {
        public OkResult(string reason = null) : base(null, reason)
        {
        }
    }
}