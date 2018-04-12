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
        private readonly DiscordSocketClient client;
        public List<Delays> AntiRLDelays = new List<Delays>();
        public class Delays
        {
            public DateTime _delay { get; set; } = DateTime.UtcNow;
            public ulong GuildID { get; set; }
        }

        public IServiceProvider Provider { get; }
        public EventHandler(IServiceProvider provider)
        {
            Provider = provider;
            client = Provider.GetService<DiscordSocketClient>();

            client.JoinedGuild += NewGuildMessage;


            client.UserJoined += WelcomeMessage;
            client.UserLeft += GoodbyeMessage;

            //user
            client.UserLeft += UserLeftEvent;
            client.UserJoined += UserJoinedEvent;
            client.UserBanned += UserBannedEvent;
            client.UserUnbanned += UserUnbannedEvent;
            //Message
            client.MessageDeleted += MessageDeletedEvent;
            client.MessageUpdated += MessageUpdatedEvent;
            //Channel
            client.ChannelCreated += ChannelCreatedEvent;
            client.ChannelDestroyed += ChannelDeletedEvent;
            client.ChannelUpdated += ChannelUpdatedEvent;
        }

        public async Task SendMessage(SocketGuild guild, GuildConfig gobject, EmbedBuilder embed, string txt = "")
        {
            var delay = AntiRLDelays.FirstOrDefault(x => x.GuildID == guild.Id);
            if (delay != null)
            {
                if (delay._delay > DateTime.UtcNow)
                    return;
                delay._delay = DateTime.UtcNow.AddSeconds(2);
            }
            else
            {
                AntiRLDelays.Add(new Delays
                {
                    _delay = DateTime.UtcNow.AddSeconds(2),
                    GuildID = guild.Id
                });
            }
            try
            {
                if (gobject.EventChannel != 0)
                {
                    await ((ITextChannel) guild.GetChannel(gobject.EventChannel)).SendMessageAsync(txt, false, embed.Build());
                }
            }
            catch
            {
                //
            }

        }

        
        public async Task MessageUpdatedEvent(Cacheable<IMessage, ulong> messageOld, SocketMessage messageNew,
            ISocketMessageChannel cchannel)
        {
            if (messageNew.Author.IsBot)
                return;

            if (string.Equals(messageOld.Value.Content, messageNew.Content, StringComparison.CurrentCultureIgnoreCase))
                return;

            if (messageOld.Value?.Embeds.Count > 0 || messageNew.Embeds.Count > 0)
                return;

            var guild = ((SocketGuildChannel) cchannel).Guild;

            var guildobj = GuildConfig.GetServer(guild);
            if (guildobj.EventLogging)
            {
                var embed = new EmbedBuilder
                {
                    Title = "Message Updated",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME"
                    },
                    Color = Color.Blue
                };
                embed.AddField("Old Message:", $"{messageOld.Value.Content}");
                embed.AddField("New Message:", $"{messageNew.Content}");
                embed.AddField("Info",
                    $"Author: {messageNew.Author.Username}\n" +
                    $"Author ID: {messageNew.Author.Id}\n" +
                    $"Channel: {messageNew.Channel.Name}\n" +
                    $"Embeds: {messageNew.Embeds.Any()}");

                await SendMessage(guild, guildobj, embed);
            }
        }

        public async Task ChannelUpdatedEvent(SocketChannel s1, SocketChannel s2)
        {
            var ChannelBefore = s1 as SocketGuildChannel;
            var ChannelAfter = s2 as SocketGuildChannel;
            if (ChannelBefore != null)
            {
                var guild = ChannelBefore.Guild;
                var gChannel = ChannelAfter;
                var guildobj = GuildConfig.GetServer(guild);
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
                }
            }
        }

        public async Task ChannelDeletedEvent(SocketChannel sChannel)
        {
            var guild = ((SocketGuildChannel) sChannel).Guild;
            var guildobj = GuildConfig.GetServer(guild);
            if (guildobj.EventLogging)
            {
                var embed = new EmbedBuilder
                {
                    Title = "Channel Deleted",
                    Description = ((SocketGuildChannel) sChannel)?.Name,
                    Color = Color.DarkTeal,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME"
                    }
                };
                await SendMessage(guild, guildobj, embed);
            }
        }

        public async Task ChannelCreatedEvent(SocketChannel sChannel)
        {
            var guild = ((SocketGuildChannel) sChannel).Guild;
            var gChannel = (SocketGuildChannel) sChannel;
            var guildobj = GuildConfig.GetServer(guild);
            if (guildobj.EventLogging)
            {
                var embed = new EmbedBuilder();
                embed.AddField("Channel Created", $"{gChannel.Name}");
                embed.WithFooter(x =>
                {
                    x.WithText($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME");
                });
                embed.Color = Color.Green;

                await SendMessage(guild, guildobj, embed);
            }
        }

        public async Task MessageDeletedEvent(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            var guild = ((SocketGuildChannel) channel).Guild;
            var guildobj = GuildConfig.GetServer(guild);
            if (guildobj.EventLogging)
            {


                var embed = new EmbedBuilder();
                try
                {
                    embed.AddField("Message Deleted", $"Message: {message.Value.Content}\n" +
                                                      $"Author: {message.Value.Author}\n" +
                                                      $"Channel: {channel.Name}");
                }
                catch
                {
                    embed.AddField("Message Deleted", "Message was unable to be retrieved\n" +
                                                      $"Channel: {channel.Name}");
                }

                embed.WithFooter(x =>
                {
                    x.WithText($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME");
                });
                embed.Color = Color.Green;

                await SendMessage(guild, guildobj, embed);
            }
        }

        public async Task UserUnbannedEvent(SocketUser user, SocketGuild guild)
        {
            var guildobj = GuildConfig.GetServer(guild);
            if (guildobj.EventLogging)
            {
                var embed = new EmbedBuilder();
                embed.AddField("User UnBanned", $"Username: {user.Username}");
                embed.WithFooter(x =>
                {
                    x.WithText($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME");
                });
                embed.Color = Color.Green;

                await SendMessage(guild, guildobj, embed);
            }
        }

        public async Task UserBannedEvent(SocketUser user, SocketGuild guild)
        {
            var guildobj = GuildConfig.GetServer(guild);
            if (guildobj.EventLogging)
            {
                var embed = new EmbedBuilder();
                embed.AddField("User Banned", $"Username: {user.Username}");
                embed.WithFooter(x =>
                {
                    x.WithText($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME");
                });
                embed.Color = Color.Red;

                await SendMessage(guild, guildobj, embed);
            }
        }

        public async Task UserJoinedEvent(SocketGuildUser user)
        {
            var guildobj = GuildConfig.GetServer(user.Guild);
            if (guildobj.antiraid)
            {
                IRole role;
                if (user.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, "PB-RAID", StringComparison.CurrentCultureIgnoreCase)) is IRole Role)
                {
                    await Role.ModifyAsync(x => x.Permissions = new GuildPermissions(readMessageHistory: true, readMessages: true));
                    role = Role;
                }
                else
                {
                    role = await user.Guild.CreateRoleAsync("PB-RAID", new GuildPermissions(readMessageHistory: true, readMessages: true));
                }

                await user.AddRoleAsync(role);
            }
            if (guildobj.EventLogging)
            {
                var embed = new EmbedBuilder
                {
                    Title = "User Joined",
                    Description = $"{user.Mention} {user.Username}#{user.Discriminator}\n" +
                                  $"ID: {user.Id}",
                    ThumbnailUrl = user.GetAvatarUrl(),
                    Color = Color.Green,
                    Footer = new EmbedFooterBuilder
                    {Text = $"{DateTime.UtcNow} UTC TIME"}
                    
                };
                await SendMessage(user.Guild, guildobj, embed);
            }
        }

        public async Task UserLeftEvent(SocketGuildUser user)
        {
            var guildobj = GuildConfig.GetServer(user.Guild);
            if (guildobj.EventLogging)
            {
                var embed = new EmbedBuilder
                {
                    Title = "User Left",
                    Description = $"{user.Mention} {user.Username}#{user.Discriminator}\n" +
                                  $"ID: {user.Id}",
                    ThumbnailUrl = user.GetAvatarUrl(),
                    Color = Color.Red,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"{DateTime.UtcNow} UTC TIME"
                    }

                };
                await SendMessage(user.Guild, guildobj, embed);
            }
        }

        public static async Task WelcomeMessage(SocketGuildUser user)
        {
            var guildobj = GuildConfig.GetServer(user.Guild);
            if (!guildobj.WelcomeEvent) return;
            var wmessage = guildobj.WelcomeMessage;
            if (wmessage == null) return;
            var embed = new EmbedBuilder
            {
                Title = $"Welcome {user.Username}",
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

        public async Task GoodbyeMessage(SocketGuildUser user)
        {
            var guildobj = GuildConfig.GetServer(user.Guild);
            if (!guildobj.GoodbyeEvent) return;
            var embed = new EmbedBuilder
            {
                Title = $"Goodbye {user.Username}",
                Description = guildobj.GoodbyeMessage
            };
            if (guildobj.GoodByeChannel != 0)
            {
                await user.Guild.GetTextChannel(guildobj.GoodByeChannel).SendMessageAsync($"", false, embed.Build());
            }
            else
            {
                await user.Guild.DefaultChannel.SendMessageAsync($"", false, embed.Build());
            }
        }

        public async Task NewGuildMessage(SocketGuild guild)
        {
            var config = Path.Combine(AppContext.BaseDirectory + $"setup/server/{guild.Id}.json");
            if (!File.Exists(config))
                GuildConfig.Setup(guild);

            await guild.DefaultChannel.SendMessageAsync(
                $"Hi, I'm PassiveBOT. To see a list of my commands type `{Load.Pre}help` and for some statistics about me type `{Load.Pre}info`\n" +
                "I am able to do tags, moderation, memes & more!!!!!");

            try
            {
                var DblApi = new DiscordNetDblApi(client, Config.Load().DBLtoken);
                var me = await DblApi.GetMeAsync();
                await me.UpdateStatsAsync(client.Guilds.Count);
            }
            catch
            {
                //
            }
        }
    }
}