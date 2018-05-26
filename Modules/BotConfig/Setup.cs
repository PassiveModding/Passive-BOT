using System.Threading.Tasks;
using Discord.Commands;
using PassiveBOT.Discord.Context;
using PassiveBOT.Models;

namespace PassiveBOT.Modules.BotConfig
{
    [RequireOwner]
    [RequireContext(ContextType.Guild)]
    public class Setup : Base
    {
        [Command("SetHomeServer")]
        [Summary("SetHomeServer")]
        [Remarks("Set home server ID")]
        public async Task SetHS()
        {
            var hs = HomeModel.Load();
            hs.ID = Context.Guild.Id;
            hs.Save();
            await SimpleEmbedAsync($"Homeserver Saved!");
        }

        [Command("TogglePartnerLog")]
        [Summary("TogglePartnerLog")]
        [Remarks("Toggle partner logging")]
        public async Task Toggle()
        {
            var hs = HomeModel.Load();
            hs.Logging.LogPartnerChanges = !hs.Logging.LogPartnerChanges;
            hs.Save();
            await SimpleEmbedAsync($"Log Partner Events: {hs.Logging.LogPartnerChanges}");
        }

        [Command("SetPartnerLogChannel")]
        [Summary("SetPartnerLogChannel")]
        [Remarks("Set channel to log partner changed")]
        public async Task SerParther()
        {
            var hs = HomeModel.Load();
            hs.Logging.PartnerLogChannel = Context.Channel.Id;
            hs.Save();
            await SimpleEmbedAsync($"Partner events will be logged in: {Context.Channel.Name}");
        }
    }
}