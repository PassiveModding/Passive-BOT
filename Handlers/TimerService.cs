using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Configuration;

namespace PassiveBOT.Handlers
{
    public class TimerService
    {
        public static List<ulong> AcceptedServers = new List<ulong>();
        private readonly Timer _timer;
        public Random rndshuffle = new Random();

        public TimerService(DiscordSocketClient client)
        {
            _timer = new Timer(async _ =>
                {
                    try
                    {
                        AcceptedServers = AcceptedServers.Where(x => client.Guilds.Any(y => y.Id == x)).ToList();
                        var newlist = AcceptedServers.ToList();
                        foreach (var guildid in AcceptedServers.OrderBy(x => rndshuffle.Next()))
                        {
                            try
                            {
                                var guildobj = GuildConfig.GetServer(client.GetGuild(guildid));
                                if (!guildobj.PartnerSetup.IsPartner) continue;
                                if (guildobj.PartnerSetup.banned) continue;
                                if (client.GetChannel(guildobj.PartnerSetup.PartherChannel) is IMessageChannel channel)
                                {
                                    try
                                    {
                                        var newitems = newlist.Where(x => x != guildid).ToList();
                                        var rnd = new Random().Next(0, newitems.Count);
                                        var newitem = newitems[rnd];

                                        var selectedguild = GuildConfig.GetServer(client.GetGuild(newitem)).PartnerSetup;
                                        if (selectedguild.banned) continue;
                                        if (!selectedguild.IsPartner ||
                                            !(client.GetChannel(selectedguild.PartherChannel) is IGuildChannel
                                                otherchannel) || selectedguild.Message == null) continue;
                                        var embed = new EmbedBuilder
                                        {
                                            Title = otherchannel.Guild.Name,
                                            Description = selectedguild.Message,
                                            ThumbnailUrl = otherchannel.Guild.IconUrl,
                                            ImageUrl = selectedguild.ImageUrl,
                                            Color = Color.Green,
                                            Footer = new EmbedFooterBuilder
                                            {
                                                Text = (selectedguild.showusercount ? $"User Count: {((SocketGuild)otherchannel.Guild).MemberCount}" : null)
                                            }
                                        };
                                        await channel.SendMessageAsync("", false, embed.Build());
                                        newlist.Remove(newitem);
                                    }
                                    catch //(Exception e)
                                    {
                                        //Console.WriteLine(e);
                                    }
                                }
                                else
                                {
                                    guildobj.PartnerSetup.IsPartner = false;
                                    GuildConfig.SaveServer(guildobj);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                },
                null, TimeSpan.Zero, TimeSpan.FromMinutes(60));
        }

        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Restart()
        {
            _timer.Change(TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(60));
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