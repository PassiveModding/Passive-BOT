using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public TimerService(DiscordSocketClient client)
        {
            _timer = new Timer(async _ =>
                {
                    var newlist = AcceptedServers.ToList();
                    foreach (var guildid in AcceptedServers)
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
                                ColourLog.LogInfo($"{channel.Name}");
                                if (selectedguild.IsPartner && client.GetChannel(selectedguild.PartherChannel) is IGuildChannel otherchannel && selectedguild.Message != null)
                                {
                                    var embed = new EmbedBuilder
                                    {
                                        Title = otherchannel.Guild.Name,
                                        Description = selectedguild.Message,
                                        ThumbnailUrl = otherchannel.Guild.IconUrl,
                                        Color = Color.Green
                                    };
                                    await channel.SendMessageAsync("", false, embed.Build());
                                    newlist.Remove(newitem);
                                }
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
                },
                null,
                //TimeSpan.FromMinutes(10),  // 4) Time that message should fire after the timer is created
                TimeSpan.Zero,
                TimeSpan.FromMinutes(60)); // 5) Time after which message should repeat (use `Timeout.Infinite` for no repeat)
        }

        public void Stop() // 6) Example to make the timer stop running
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Restart() // 7) Example to restart the timer
        {
            _timer.Change(TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(60));
        }
    }

    public class TimerModule : ModuleBase
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
    }
}