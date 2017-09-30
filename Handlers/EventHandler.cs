using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Discord;
using System.Linq;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PassiveBOT.Configuration;

namespace PassiveBOT.Handlers
{
    public class EventHandler
    {
        private DateTime _delay; //NOTE THIS IS NOT GUILD SPECIFIC YET!

        public EventHandler(IServiceProvider provider)
        {
            Provider = provider;
            var client = Provider.GetService<DiscordSocketClient>();

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

            client.ReactionAdded += Client_ReactionAdded;
        }

        private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg3.Emote.Name == "⭐")
            {
                var guild = (arg3.Channel as IGuildChannel).Guild;
                var starchannelid = GuildConfig.Load(guild.Id).Starboard;
                if (starchannelid != 0)
                {
                    try
                    {
                        if (arg1.Value.Embeds.Any())
                        {
                            return;
                        }

                        var channel = await guild.GetTextChannelAsync(starchannelid);
                        var embed = new EmbedBuilder();
                        embed.WithAuthor(x =>
                        {
                            x.Name = arg1.Value.Author.Username;
                            try
                            {
                                x.IconUrl = arg1.Value.Author.GetAvatarUrl();
                            }
                            catch
                            {
                                //
                            }
                        });
                        if (arg1.Value.Attachments.Count > 0)
                        {
                            var x = arg1.Value.Attachments.First();
                            embed.ImageUrl = x.Url;
                        }
                        embed.Description = $"{arg1.Value.Content}";

                        await channel.SendMessageAsync($"⭐{arg1.Value.Reactions.First(x => x.Key.Name == "⭐").Value.ReactionCount} <#{arg2.Id}>", false, embed.Build());

                    }
                    catch
                    {
                        return;
                    }
                }
            }
        }

        public IServiceProvider Provider { get; }

        public async Task MessageUpdatedEvent(Cacheable<IMessage, ulong> messageOld, SocketMessage messageNew,
            ISocketMessageChannel cchannel)
        {
            var guild = (cchannel as SocketGuildChannel).Guild;
            if (messageNew.Author.IsBot) return;
            if (string.Equals(messageOld.Value.Content, messageNew.Content, StringComparison.CurrentCultureIgnoreCase))
                return;
            if (messageOld.Value.Embeds.Count == 0 && messageNew.Embeds.Count == 1)
                return;
            if (GuildConfig.Load(guild.Id).EventLogging)
            {
                var embed = new EmbedBuilder();
                embed.AddField("Message Updated", $"Author: {messageNew.Author.Username}\n" +
                                                  $"Old Message: {messageOld.Value.Content}\n" +
                                                  $"New Message: {messageNew.Content}");
                embed.WithFooter(x =>
                {
                    x.WithText($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME");
                });
                embed.Color = Color.Green;

                if (GuildConfig.Load(guild.Id).EventChannel != 0)
                {
                    var channel = guild.GetChannel(GuildConfig.Load(guild.Id).EventChannel);
                    await (channel as ITextChannel).SendMessageAsync("", false, embed.Build());
                }
            }
        }

        public async Task ChannelUpdatedEvent(SocketChannel s1, SocketChannel s2)
        {
            var sChannel1 = s1 as SocketGuildChannel;
            var sChannel2 = s2 as SocketGuildChannel;
            var guild = sChannel1.Guild;
            var gChannel = sChannel1;
            if (GuildConfig.Load(guild.Id).EventLogging)
            {
                if (sChannel1.Position != sChannel2.Position)
                    return;
                var embed = new EmbedBuilder();
                embed.AddField("Channel Updated", $"{gChannel.Name}");
                embed.WithFooter(x =>
                {
                    x.WithText($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME");
                });
                embed.Color = Color.Blue;

                if (GuildConfig.Load(guild.Id).EventChannel != 0)
                {
                    var channel = guild.GetChannel(GuildConfig.Load(guild.Id).EventChannel);
                    await (channel as ITextChannel).SendMessageAsync("", false, embed.Build());
                }
            }
        }

        public async Task ChannelDeletedEvent(SocketChannel sChannel)
        {
            var guild = (sChannel as SocketGuildChannel).Guild;
            var gChannel = sChannel as SocketGuildChannel;
            if (GuildConfig.Load(guild.Id).EventLogging)
            {
                var embed = new EmbedBuilder();
                embed.AddField("Channel Deleted", $"{gChannel.Name}");
                embed.WithFooter(x =>
                {
                    x.WithText($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME");
                });
                embed.Color = Color.Green;

                if (GuildConfig.Load(guild.Id).EventChannel != 0)
                {
                    var channel = guild.GetChannel(GuildConfig.Load(guild.Id).EventChannel);
                    await (channel as ITextChannel).SendMessageAsync("", false, embed.Build());
                }
            }
        }

        public async Task ChannelCreatedEvent(SocketChannel sChannel)
        {
            var guild = (sChannel as SocketGuildChannel).Guild;
            var gChannel = sChannel as SocketGuildChannel;
            if (GuildConfig.Load(guild.Id).EventLogging)
            {
                var embed = new EmbedBuilder();
                embed.AddField("Channel Created", $"{gChannel.Name}");
                embed.WithFooter(x =>
                {
                    x.WithText($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME");
                });
                embed.Color = Color.Green;

                if (GuildConfig.Load(guild.Id).EventChannel != 0)
                {
                    var channel = guild.GetChannel(GuildConfig.Load(guild.Id).EventChannel);
                    await (channel as ITextChannel).SendMessageAsync("", false, embed.Build());
                }
            }
        }

        public async Task MessageDeletedEvent(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            var guild = (channel as SocketGuildChannel).Guild;
            if (GuildConfig.Load(guild.Id).EventLogging)
            {
                if (_delay > DateTime.UtcNow)
                    return;
                _delay = DateTime.UtcNow.AddSeconds(2);

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
                var cchannel = guild.GetChannel(GuildConfig.Load(guild.Id).EventChannel);
                await (cchannel as ITextChannel).SendMessageAsync("", false, embed.Build());
            }
        }

        public async Task UserUnbannedEvent(SocketUser user, SocketGuild guild)
        {
            if (GuildConfig.Load(guild.Id).EventLogging)
            {
                var embed = new EmbedBuilder();
                embed.AddField("User UnBanned", $"Username: {user.Username}");
                embed.WithFooter(x =>
                {
                    x.WithText($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME");
                });
                embed.Color = Color.Green;
                if (GuildConfig.Load(guild.Id).EventChannel != 0)
                {
                    var channel = guild.GetChannel(GuildConfig.Load(guild.Id).EventChannel);
                    await (channel as ITextChannel).SendMessageAsync("", false, embed.Build());
                }
            }
        }

        public async Task UserBannedEvent(SocketUser user, SocketGuild guild)
        {
            if (GuildConfig.Load(guild.Id).EventLogging)
            {
                var embed = new EmbedBuilder();
                embed.AddField("User Banned", $"Username: {user.Username}");
                embed.WithFooter(x =>
                {
                    x.WithText($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME");
                });
                embed.Color = Color.Red;
                if (GuildConfig.Load(guild.Id).EventChannel != 0)
                {
                    var channel = guild.GetChannel(GuildConfig.Load(guild.Id).EventChannel);
                    await (channel as ITextChannel).SendMessageAsync("", false, embed.Build());
                }
            }
        }

        public async Task UserJoinedEvent(SocketGuildUser user)
        {
            if (GuildConfig.Load(user.Guild.Id).EventLogging)
            {
                var embed = new EmbedBuilder();
                embed.AddField("User Joined", $"Username: {user.Username}");
                embed.WithFooter(x =>
                {
                    x.WithText($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME");
                });
                embed.Color = Color.Green;
                if (GuildConfig.Load(user.Guild.Id).EventChannel != 0)
                {
                    var channel = user.Guild.GetChannel(GuildConfig.Load(user.Guild.Id).EventChannel);
                    await (channel as ITextChannel).SendMessageAsync("", false, embed.Build());
                }
            }
        }

        public async Task UserLeftEvent(SocketGuildUser user)
        {
            if (GuildConfig.Load(user.Guild.Id).EventLogging)
            {
                var embed = new EmbedBuilder();
                embed.AddField("User Left", $"Username: {user.Username}");
                embed.WithFooter(x =>
                {
                    x.WithText($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME");
                });
                embed.Color = Color.Red;
                if (GuildConfig.Load(user.Guild.Id).EventChannel != 0)
                {
                    var channel = user.Guild.GetChannel(GuildConfig.Load(user.Guild.Id).EventChannel);
                    await (channel as ITextChannel).SendMessageAsync("", false, embed.Build());
                }
            }
        }

        public static async Task WelcomeMessage(SocketGuildUser user)
        {
            var id = user.Guild.Id;
            var wevent = GuildConfig.Load(id).WelcomeEvent;
            if (!wevent) return;
            var wchan = GuildConfig.Load(id).WelcomeChannel;
            var wmessage = GuildConfig.Load(id).WelcomeMessage;
            var embed = new EmbedBuilder();
            if (wchan != 0)
            {
                var channel = user.Guild.GetTextChannel(wchan);

                embed.AddField($"Welcome {user.Username}", wmessage);
                embed.WithColor(Color.Blue);
                await channel.SendMessageAsync($"{user.Mention}", false, embed.Build());
            }
            else
            {
                embed.AddField($"Welcome {user.Username}", wmessage);
                embed.WithColor(Color.Blue);
                await user.Guild.DefaultChannel.SendMessageAsync($"{user.Mention}", false, embed.Build());
            }
        }

        public async Task GoodbyeMessage(SocketGuildUser user)
        {
            var id = user.Guild.Id;
            var gevent = GuildConfig.Load(id).GoodbyeEvent;
            if (!gevent) return;
            var gchan = GuildConfig.Load(id).GoodByeChannel;
            var gmessage = GuildConfig.Load(id).GoodbyeMessage;
            if (gchan != 0)
            {
                var channel = user.Guild.GetTextChannel(gchan);
                //await channel.SendMessageAsync($"{user.Mention}: {gmessage}");

                var embed = new EmbedBuilder();
                embed.AddField($"Goodbye {user.Username}", $"{gmessage}");
                //var channel = user.Guild.GetTextChannel(gchan);
                await channel.SendMessageAsync($"", false, embed.Build());
            }
            else
            {
                var embed = new EmbedBuilder();
                embed.AddField($"Goodbye {user.Username}", $"{gmessage}");
                //await channel.SendMessageAsync($"", false, embed.Build());
                await user.Guild.DefaultChannel.SendMessageAsync($"", false, embed.Build());
            }
        }

        public async Task NewGuildMessage(SocketGuild guild)
        {
            //if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, $"setup/server/{guild.Id}/music/")))
            //    Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, $"setup/server/{guild.Id}/music/"));

            var config = Path.Combine(AppContext.BaseDirectory + $"setup/server/{guild.Id}.json");
            if (!File.Exists(config))
                GuildConfig.Setup(guild);

            await guild.DefaultChannel.SendMessageAsync(
                $"Hi, I'm PassiveBOT. To see a list of my commands type `{Load.Pre}help` and for some statistics about me type `{Load.Pre}info`\n" +
                "I am able to do music, tags, moderation, memes & more!!!!!");
        }
    }
}