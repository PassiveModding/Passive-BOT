using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Handlers;

namespace PassiveBOT.Commands
{
    public class Test : ModuleBase
    {
        [Command("Users")]
        public async Task Ebuild()
        {
            var botlist = (Context.Guild as SocketGuild).Users.Count(x => x.IsBot);
            var mem = (Context.Guild as SocketGuild).MemberCount;
            var guildusers = mem - botlist;
            var application = await Context.Client.GetApplicationInfoAsync();

            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x.Name = Context.Guild.Name;
                    x.IconUrl = Context.Guild.IconUrl;
                })
                .AddInlineField("Total Members", mem)
                .AddInlineField("Total Bots", botlist)
                .AddInlineField("Total Users", guildusers)
                .AddField("Links", "[Forums](https://goo.gl/s3BZTw) \n[Invite](https://discordapp.com/oauth2/authorize?client_id={application.Id}&scope=bot&permissions=2146958591)\n[Main Server](https://discord.gg/ZKXqt2a) \n[Testing Server](https://discord.gg/bmXfBQM)")
                .WithFooter(x =>
                {
                    x.WithText("PassiveBOT");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, embed);
        }
    }
}
