using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBotsList.Api.Extensions.DiscordNet;
using Microsoft.Extensions.DependencyInjection;
using PassiveBOT.Configuration;

namespace PassiveBOT.Handlers
{
    public class EventHandler
    {
        private readonly List<Delays> AntiRLDelays = new List<Delays>();
        private readonly DiscordSocketClient client;

        public EventHandler(IServiceProvider provider)
        {
            Provider = provider;
            client = Provider.GetService<DiscordSocketClient>();
            client.Ready += Client_Ready;


            client.JoinedGuild += NewGuildMessage;


            client.UserJoined += WelcomeMessage;
            client.UserLeft += GoodbyeMessage;

            client.ChannelUpdated += ChannelUpdatedEvent;
        }

        private IServiceProvider Provider { get; }

        private async Task Client_Ready()
        {
            await client.SetGameAsync($"{Config.Load().Prefix}help / {Load.Gamesite}");
        }

        private async Task ChannelUpdatedEvent(SocketChannel s1, SocketChannel s2)
        {
            var ChannelBefore = s1 as SocketGuildChannel;
            var ChannelAfter = s2 as SocketGuildChannel;
            if (ChannelBefore != null)
            {
                var guild = ChannelBefore.Guild;
                var gChannel = ChannelAfter;
                var guildobj = GuildConfig.GetServer(guild);
                if (guildobj.PartnerSetup.IsPartner && !guildobj.PartnerSetup.banned &&
                    ChannelBefore.Id == guildobj.PartnerSetup.PartherChannel)
                {
                    var changes = ChannelAfter.PermissionOverwrites.Where(x => x.TargetType == PermissionTarget.Role &&
                                                                               ChannelBefore.PermissionOverwrites
                                                                                   .FirstOrDefault(y =>
                                                                                       y.TargetId == x.TargetId &&
                                                                                       (y.Permissions.AllowValue !=
                                                                                        x.Permissions.AllowValue ||
                                                                                        y.Permissions.DenyValue !=
                                                                                        x.Permissions.DenyValue))
                                                                                   .TargetId == x.TargetId).ToList();

                    var userlist = ((SocketTextChannel) s2).Users;
                    var userstring = $"Users Visible/Total Users: {userlist.Count} / {guild.Users.Count}\n" +
                                     $"Percent Visible: {(double) userlist.Count / guild.Users.Count * 100}%";

                    var homeserver = Homeserver.Load();
                    var embed = new EmbedBuilder
                    {
                        Title = $"Partner Channel Updated",
                        Description =
                            $"{(changes.Any() ? string.Join("\n", changes.Select(x => $"Role: {guild.GetRole(x.TargetId)?.Name}\n" + $"Read Messages: {x.Permissions.ReadMessages}\n" + $"Read History: {x.Permissions.ReadMessageHistory}\n")) : "No Role Permissions Detected")}\n\n" +
                            $"__**Channel Visibility**__\n" +
                            $"{userstring}\n" +
                            $"__**Server Info**__\n" +
                            $"Guild ID: {guild.Id}\n" +
                            $"Guild Name: {guild.Name}\n" +
                            $"Channel name: {ChannelAfter.Name}",
                        Color = Color.Blue,
                        Footer = new EmbedFooterBuilder
                        {
                            Text = $"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME"
                        }
                    };
                    var channel = client.GetChannel(homeserver.PartnerUpdates);
                    if (channel != null && !string.IsNullOrEmpty(embed.Description))
                        await ((ITextChannel) channel).SendMessageAsync("", false, embed.Build());
                }
                /*
                if (guildobj.EventLogging)
                {
                    if (ChannelAfter != null && ChannelBefore.Position != ChannelAfter.Position)
                        return;
                    var embed = new EmbedBuilder
                    {
                        Title = "Channel Updated",
                        Description = gChannel.Name,
                        Color = Color.Blue,
                        Footer = new EmbedFooterBuilder
                        {
                            Text = $"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME"
                        }
                    };
                    await SendMessage(guild, guildobj, embed);
                }*/
            }
        }
        
        private static async Task WelcomeMessage(SocketGuildUser user)
        {
            var guildobj = GuildConfig.GetServer(user.Guild);
            if (!guildobj.WelcomeEvent) return;
            var wmessage = guildobj.WelcomeMessage;
            if (wmessage == null) return;
            var embed = new EmbedBuilder
            {
                Title = $"Welcome {user.Username}",
                ThumbnailUrl = user.GetAvatarUrl(),
                Description = wmessage,
                Color = Color.Blue,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Users: {user.Guild.MemberCount}"
                }
            };
            if (guildobj.WelcomeChannel != 0)
            {
                var channel = user.Guild.GetTextChannel(guildobj.WelcomeChannel);
                await channel.SendMessageAsync($"{user.Mention}", false, embed.Build());
            }
            else
            {
                await user.Guild.DefaultChannel.SendMessageAsync($"{user.Mention}", false, embed.Build());
            }
        }

        private async Task GoodbyeMessage(SocketGuildUser user)
        {
            var guildobj = GuildConfig.GetServer(user.Guild);
            if (!guildobj.GoodbyeEvent) return;
            var embed = new EmbedBuilder
            {
                Title = $"Goodbye {user.Username}",
                ThumbnailUrl = user.GetAvatarUrl(),
                Description = guildobj.GoodbyeMessage
            };
            if (guildobj.GoodByeChannel != 0)
                await user.Guild.GetTextChannel(guildobj.GoodByeChannel).SendMessageAsync($"", false, embed.Build());
            else
                await user.Guild.DefaultChannel.SendMessageAsync($"", false, embed.Build());
        }

        private async Task NewGuildMessage(SocketGuild guild)
        {
            var config = Path.Combine(AppContext.BaseDirectory + $"setup/server/{guild.Id}.json");
            if (!File.Exists(config))
                GuildConfig.Setup(guild);

            await guild.DefaultChannel.SendMessageAsync(
                $"Hi, I'm PassiveBOT. To see a list of my commands type `{Load.Pre}help` and for some statistics about me type `{Load.Pre}info`\n" +
                "I am able to do tags, moderation, memes & more!!!!!");

            try
            {
                var DblApi = new DiscordNetDblApi(client, Tokens.Load().DiscordBotsListToken);
                var me = await DblApi.GetMeAsync();
                await me.UpdateStatsAsync(client.Guilds.Count);
            }
            catch
            {
                //
            }
        }

        public class Delays
        {
            public DateTime _delay { get; set; } = DateTime.UtcNow;
            public ulong GuildID { get; set; }
        }
    }
}