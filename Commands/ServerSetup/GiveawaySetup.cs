using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Discord;
using Discord.Commands;
using PassiveBOT.Configuration;
using PassiveBOT.Preconditions;

namespace PassiveBOT.Commands.ServerSetup
{
    [RequireModerator]
    [RequireContext(ContextType.Guild)]
    public class GiveawaySetup : ModuleBase
    {
        [Command("CreateGiveAway")]
        [Summary("GiveAway <description>")]
        [Remarks("Creates a giveaway with the given title/description")]
        public async Task CreateGiveaway([Remainder] string description)
        {
            var server = GuildConfig.GetServer(Context.Guild);

            var comp = new GuildConfig().Comp;
            comp.Message = description;
            comp.Users = new List<ulong>();
            comp.Creator = Context.User.Id;
            server.Comp = comp;

            GuildConfig.SaveServer(server);

            await ReplyAsync("GiveAway Created.");
        }

        [Command("PickGiveAway")]
        [Summary("PickGiveAway")]
        [Remarks("Picks the current giveaway")]
        public async Task GiveawayPick()
        {
            var server = GuildConfig.GetServer(Context.Guild);
            if (server.Comp != new GuildConfig.GiveAway())
            {
                if (server.Comp.Creator == Context.User.Id)
                {
                    var rnd = new Random().Next(0, server.Comp.Users.Count);
                    var winner = await Context.Guild.GetUserAsync(server.Comp.Users[rnd]);

                    if (winner == null)
                    {
                        await ReplyAsync("ERROR, Winner is Unavailable");
                    }
                    else
                    {
                        var embed = new EmbedBuilder
                        {
                            Title = "Winner!!!",
                            Description = $"Congratulations {winner.Mention}, You have won:\n\n" +
                                          $"{server.Comp.Message}\n\n" +
                                          $"Please Contact {Context.User.Mention} or the Giveaway host!!!"
                        };

                        await ReplyAsync("", false, embed.Build());

                        server.Comp = new GuildConfig.GiveAway();
                        GuildConfig.SaveServer(server);
                    }
                }
            }
            else
            {
                await ReplyAsync("ERROR, there is no competition currently");
            }
        }
    }
}