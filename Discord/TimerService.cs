using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PassiveBOT.Discord.Extensions;
using PassiveBOT.Handlers;
using PassiveBOT.Models;

namespace PassiveBOT.Discord
{
    public class TimerService
    {
        public static DateTime LastFireTime = DateTime.MinValue;
        public static int FirePreiod = 30;
        private readonly Timer _timer;
        private readonly Random rndshuffle = new Random();

        public TimerService(DiscordSocketClient client)
        {
            _client = client;
            _timer = new Timer(async _ =>
                {
                    try
                    {
                        try
                        {
                            var rnd = new Random();
                            switch (rnd.Next(0, 6))
                            {
                                case 0:
                                    await client.SetGameAsync($"{CommandHandler.Config.Prefix}help // {client.Guilds.Count} Guilds!");
                                    break;
                                case 1:
                                    await client.SetGameAsync($"{CommandHandler.Config.Prefix}help // {client.Guilds.Sum(x => x.MemberCount)} Users!");
                                    break;
                                case 2:
                                    await client.SetGameAsync($"{CommandHandler.Config.Prefix}help // {CommandHandler.Config.SupportServer}");
                                    break;
                                case 3:
                                    await client.SetGameAsync($"{CommandHandler.Config.Prefix}help // Partnering People!");
                                    break;
                                case 4:
                                    await client.SetGameAsync($"{CommandHandler.Config.Prefix}help // Better than tinder");
                                    break;
                                case 5:
                                    await client.SetGameAsync($"{CommandHandler.Config.Prefix}help // Hot Stuff!");
                                    break;
                            }
                        }
                        catch
                        {
                            //
                        }
                        await Partner();
                    }
                    catch (Exception e)
                    {
                        LogHandler.LogMessage($"Partner Error:\n" +
                                              $"{e}", LogSeverity.Error);
                    }

                    LastFireTime = DateTime.UtcNow;
                },
                null, TimeSpan.Zero, TimeSpan.FromMinutes(FirePreiod));
        }

        public DiscordSocketClient _client { get; set; }

        public async Task Partner()
        {
            var glist = DatabaseHandler.GetFullConfig().Where(x => !x.Partner.Settings.Banned &&
                                                                   x.Partner.Settings.Enabled && x.Partner.Settings.ChannelID != 0 &&
                                                                   x.Partner.Message.Content != null &&
                                                                   _client.GetChannel(x.Partner.Settings.ChannelID) != null &&
                                                                   _client.Guilds.Any(g => g.Id == x.ID))
                .ToList();
            var reduced_glist = glist.ToList();
            foreach (var receiverguildconfig in glist)
            {
                try
                {
                    var receiverguild = _client.GetGuild(receiverguildconfig.ID);
                    if (!(_client.GetChannel(receiverguildconfig.Partner.Settings.ChannelID) is SocketTextChannel receiverchannel)) continue;
                    var messageguild = reduced_glist.OrderByDescending(x => rndshuffle.Next()).FirstOrDefault(x => x.ID != receiverguild.Id);
                    if (messageguild == null) continue;
                    if (!(_client.GetChannel(messageguild.Partner.Settings.ChannelID) is SocketTextChannel messagechannel)) continue;

                    if ((decimal) messagechannel.Users.Count / messagechannel.Guild.Users.Count * 100 < 90)
                    {
                        await messagechannel.SendMessageAsync("", false, new EmbedBuilder
                        {
                            Description = "NOTICE:\n" +
                                          "This server's partner message was not shared to any other guilds because this channel's visibility is less than 90%\n" +
                                          "Please change the role settings of this channel to ensure all roles have the `read messages` permission"
                        }.Build());
                    }
                    else
                    {
                        try
                        {
                            await receiverchannel.SendMessageAsync("", false, GeneratePartnerMessage.GenerateMessage(messageguild, messagechannel.Guild).Build());
                            messageguild.Partner.Stats.ServersReached++;
                            messageguild.Partner.Stats.UsersReached = messageguild.Partner.Stats.UsersReached + receiverguild.MemberCount;
                            messageguild.Save();
                            await Task.Delay(500);
                        }
                        catch (Exception e)
                        {
                            LogHandler.LogMessage($"Partner Message Send Error in:\n" +
                                                  $"S:{receiverguild.Name} [{receiverguild.Id}] : C:{receiverchannel.Name}\n" +
                                                  $"{e}", LogSeverity.Error);
                        }
                    }

                    reduced_glist.Remove(messageguild);
                }
                catch (Exception e)
                {
                    LogHandler.LogMessage($"Partner Event Error for Guild: [{receiverguildconfig.ID}]\n" +
                                          $"{e}", LogSeverity.Error);
                }
            }
        }

        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Restart()
        {
            _timer.Change(TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(FirePreiod));
        }

        public void ChangeRate(int newperiod = 60)
        {
            FirePreiod = newperiod;
            _timer.Change(TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(FirePreiod));
        }
    }
}