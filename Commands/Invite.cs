using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace PassiveBOT.Commands
{
    public class Invite : ModuleBase
    {
        [Command("invite")]
        [Summary("invite")]
        [Remarks("Returns the OAuth2 Invite URL of the bot")]
        public async Task InviteBot()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            await ReplyAsync(
                $"A user with `MANAGE_SERVER` can invite me to your server here: <https://discordapp.com/oauth2/authorize?client_id={application.Id}&scope=bot&permissions=2146958591>");
        }
    }

}
