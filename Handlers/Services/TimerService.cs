using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Discord;
using Discord.WebSocket;
using PassiveBOT.Configuration;

namespace PassiveBOT.Handlers.Services
{
    public class TimerService
    {
        public static List<ulong> AcceptedServers = new List<ulong>();
        public static DateTime LastFireTime = DateTime.MinValue;
        public static int FirePreiod = 60;
        private readonly Timer _timer;
        private readonly Random rndshuffle = new Random();


        public TimerService(DiscordSocketClient client)
        {
            _timer = new Timer(async _ =>
                {
                    try
                    {
                        //First filter out any servers which do not appear in the bot's guild list (ie. They removed the bot from the server)
                        AcceptedServers = AcceptedServers.Where(x => client.Guilds.Any(y => y.Id == x)).ToList();
                        var newlist = AcceptedServers.ToList();
                        //Randomly Shuffle in order to provide a level of randomness in which servers get shared where. We also ensure that newlist isn't being modified during the loop by casting it to a second list.
                        foreach (var guildid in newlist.ToList().OrderBy(x => rndshuffle.Next()))
                            try
                            {
                                //Try to get the server's saved config
                                var guildobj = GuildConfig.GetServer(client.GetGuild(guildid));
                                //Filter out servers which are either banned or do not use the partner program
                                if (!guildobj.PartnerSetup.IsPartner) continue;
                                if (guildobj.PartnerSetup.banned) continue;
                                //Ensure that the pertner channel still exists
                                if (client.GetChannel(guildobj.PartnerSetup.PartherChannel) is IMessageChannel channel)
                                {
                                    try
                                    {
                                        //make sure we filter out the current server in the random list so that servers dont receive their own pertner message
                                        var newitems = newlist.Where(x => x != guildid).ToList();
                                        //next select a random server from the previous list to share
                                        var newitem = newitems[new Random().Next(0, newitems.Count)];

                                        var selectedguild = GuildConfig.GetServer(client.GetGuild(newitem)).PartnerSetup;
                                        //ensure the selected guild is not banned
                                        if (selectedguild.banned) continue;
                                        //also ensure that the selected guild is using the partner channel, it is a legitimate channel and their message isn't empty
                                        if (!selectedguild.IsPartner || !(client.GetChannel(selectedguild.PartherChannel) is IGuildChannel otherchannel) || selectedguild.Message == null) continue;
                                        //In order to filter out people trying to cheat the systen, we filter out servers where less than 90% of users can actually see the channel
                                        if ((decimal) ((SocketTextChannel) otherchannel).Users.Count /
                                            ((SocketGuild) otherchannel.Guild).Users.Count*100 < 90)
                                        {
                                            //Ideally we notify the infringing guild that they are being ignored in the partner program until they change their settings.
                                            await ((ITextChannel)otherchannel).SendMessageAsync($"{(decimal)((SocketTextChannel)otherchannel).Users.Count * 100 / ((SocketGuild)otherchannel.Guild).Users.Count}% Visibility - The partner channel is currently inactive as less that 90% of this server's users can view the channel. You can fix this by ensuring that all roles have permissions to view messages and message history in the channel settings");
                                            continue;
                                        }

                                        //Generate an embed with the partner message from newitem
                                        var embed = new EmbedBuilder
                                        {
                                            Title = otherchannel.Guild.Name,
                                            Description = selectedguild.Message,
                                            ThumbnailUrl = otherchannel.Guild.IconUrl,
                                            ImageUrl = selectedguild.ImageUrl,
                                            Color = Color.Green,
                                            Footer = new EmbedFooterBuilder
                                            {
                                                Text = selectedguild.showusercount
                                                    ? $"User Count: {((SocketGuild) otherchannel.Guild).MemberCount}"
                                                    : null
                                            }
                                        };
                                        //send the partner message to the original server
                                        await channel.SendMessageAsync("", false, embed.Build());
                                        //remove the given item from the list so we don't have any repeats
                                        newlist.Remove(newitem);
                                    }
                                    catch //(Exception e)
                                    {
                                        //Console.WriteLine(e);
                                    }
                                }
                                else
                                {
                                    //auto-disable servers with a missing partner channel
                                    guildobj.PartnerSetup.IsPartner = false;
                                    GuildConfig.SaveServer(guildobj);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    LastFireTime = DateTime.UtcNow;
                },
                null, TimeSpan.Zero, TimeSpan.FromMinutes(FirePreiod));
        }

        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Restart()
        {
            _timer.Change(TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(FirePreiod));
        }
    }

    /*public class TimerModule : ModuleBase
    {
        private readonly TimerService _service;

        public TimerModule(TimerService service) // Make sure to configure your DI with your TimerService instance
        {
            _service = service;
        }

        public void StopCmd()
        {
            _service.Stop();
        }

        public void RestartCmd()
        {
            _service.Restart();
        }
    }*/
}