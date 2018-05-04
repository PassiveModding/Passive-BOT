using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Configuration;

namespace PassiveBOT.Commands.ServerSetup
{
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    public class GamblingSetup : ModuleBase
    {
        [Command("setCurrencyName")]
        [Summary("setCurrencyName <name>")]
        [Remarks("set the name of the server's currency (ie. Coins)")]
        public async Task SCurrencyName([Remainder] string name = null)
        {
            var guildobj = GuildConfig.GetServer(Context.Guild);
            if (name == null)
            {
                await ReplyAsync("Currency Name has been set to default -> `Coins`");
                guildobj.Gambling.settings.CurrencyName = "Coins";
            }
            else
            {
                await ReplyAsync($"Currency Name has been set to -> `{name}`");
                guildobj.Gambling.settings.CurrencyName = name;
            }
            GuildConfig.SaveServer(guildobj);
        }
    }
}
