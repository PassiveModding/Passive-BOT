using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Configuration;

namespace PassiveBOT.Commands.ServerSetup
{
    [RequireContext(ContextType.Guild)]
    public class Giveaway : ModuleBase
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

        [Command("GiveAway")]
        [Summary("GiveAway")]
        [Remarks("Check any current giveaways")]
        public async Task GiveawayCheck()
        {
            var server = GuildConfig.GetServer(Context.Guild);
            try
            {
                var u = await Context.Guild.GetUserAsync(server.Comp.Creator);
                var embed = new EmbedBuilder
                {
                    Title = "Giveaway",
                    Description = $"**{server.Comp.Message}**\n\n" +
                                  $"Host: {u.Mention}\n" +
                                  $"Entrants: {server.Comp.Users.Count}"
                };

                await ReplyAsync("", false, embed.Build());
            }
            catch
            {
                await ReplyAsync("ERROR, there is no competition currently");
            }
        }

        [Command("JoinGiveAway")]
        [Summary("JoinGiveAway")]
        [Remarks("Join the current giveaway")]
        public async Task GiveawayJoin()
        {
            var server = GuildConfig.GetServer(Context.Guild);
            if (server.Comp != new GuildConfig.GiveAway())
                if (server.Comp.Users.Contains(Context.User.Id))
                {
                    await ReplyAsync("ERROR, you have already enetred the giveaway");
                }
                else
                {
                    server.Comp.Users.Add(Context.User.Id);
                    GuildConfig.SaveServer(server);

                    await ReplyAsync($"**[{server.Comp.Users.Count}] Success, added to the giveaway**\n" +
                                     $"{server.Comp.Message}");
                }
            else
                await ReplyAsync("ERROR, there is no competition currently");
        }

        [Command("LeaveGiveAway")]
        [Summary("LeaveGiveAway")]
        [Remarks("Leaves the current giveaway")]
        public async Task GiveawayLeave()
        {
            var server = GuildConfig.GetServer(Context.Guild);
            if (server.Comp != new GuildConfig.GiveAway())
                if (server.Comp.Users.Contains(Context.User.Id))
                {
                    server.Comp.Users.Remove(Context.User.Id);
                    await ReplyAsync($"**[{server.Comp.Users.Count}] Success, removed from the giveaway**\n" +
                                     $"{server.Comp.Message}");

                    GuildConfig.SaveServer(server);
                }
                else
                {
                    await ReplyAsync("ERROR, not in the giveaway");
                }
            else
                await ReplyAsync("ERROR, there is no competition currently");
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