using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PassiveBOT.Configuration;
using PassiveBOT.strings;
using PassiveBOT.Services;

namespace PassiveBOT.Handlers
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly RssService _rss;
        public IServiceProvider Provider;

        public CommandHandler(IServiceProvider provider)
        {
            Provider = provider;
            _rss = Provider.GetService<RssService>();
            _client = Provider.GetService<DiscordSocketClient>();
            _commands = new CommandService();

            _client.MessageReceived += DoCommand;
            _client.JoinedGuild += _client_JoinedGuild;
            _client.UserJoined += UserJoinedAsync;
            _client.Ready += _client_Ready;
            _client.UserLeft += _client_UserLeft;


            //user
            _client.UserLeft += UserLeftEvent;
            _client.UserJoined += UserJoinedEvent;
            _client.UserBanned += UserBannedEvent;
            _client.UserUnbanned += UserUnbannedEvent;
            //Message
            _client.MessageDeleted += MessageDeletedEvent;
            _client.MessageUpdated += MessageUpdatedEvent;
            //Channel
            _client.ChannelCreated += ChannelCreatedEvent;
            _client.ChannelDestroyed += ChannelDeletedEvent;
            _client.ChannelUpdated += ChannelUpdatedEvent;
        }

        private async Task MessageUpdatedEvent(Cacheable<IMessage, ulong> messageOld, SocketMessage messageNew,
            ISocketMessageChannel Channel)
        {
            var guild = (Channel as SocketGuildChannel).Guild;
            if (messageNew.Author.IsBot) return;
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
                    await(channel as ITextChannel).SendMessageAsync("", false, embed.Build());
                }
            }
        }

        private async Task ChannelUpdatedEvent(SocketChannel sChannel1, SocketChannel sChannel2)
        {
            var guild = (sChannel1 as SocketGuildChannel).Guild;
            var gChannel = sChannel1 as SocketGuildChannel;
            if (GuildConfig.Load(guild.Id).EventLogging)
            {
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

        private async Task ChannelDeletedEvent(SocketChannel sChannel)
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
                    await(channel as ITextChannel).SendMessageAsync("", false, embed.Build());
                }
            }
        }

        private async Task ChannelCreatedEvent(SocketChannel sChannel)
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

        private async Task MessageDeletedEvent(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            var guild = (channel as SocketGuildChannel).Guild;
            if (GuildConfig.Load(guild.Id).EventLogging)
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
                var cchannel = guild.GetChannel(GuildConfig.Load(guild.Id).EventChannel);
                await (cchannel as ITextChannel).SendMessageAsync("", false, embed.Build());
            }
        }

        private async Task UserUnbannedEvent(SocketUser user, SocketGuild guild)
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

        private async Task UserBannedEvent(SocketUser user, SocketGuild guild)
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
                    await(channel as ITextChannel).SendMessageAsync("", false, embed.Build());
                }
            }
        }

        private async Task UserJoinedEvent(SocketGuildUser user)
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

        private async Task UserLeftEvent(SocketGuildUser user)
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

        private async Task _client_UserLeft(SocketGuildUser user)
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

        public async Task _client_Ready()
        {
            var config = Path.Combine(AppContext.BaseDirectory + "setup/server");
            var dirs = Directory.GetDirectories(config);
            var list = dirs.Select(d => Convert.ToUInt64(Path.GetFileName(d))).ToList();

            foreach (var guild in list)
            {
                var server = _client.GetGuild(guild);
                try
                {
                    var channel = GuildConfig.Load(guild).RssChannel;
                    var chan = server.GetTextChannel(channel);
                    var feed = GuildConfig.Load(guild).Rss;
                    await _rss.Rss(feed, chan);
                }
                catch
                {
                    //
                }
            }
        }

        public async Task ConfigureAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }


        public async Task DoCommand(SocketMessage parameterMessage)
        {
            var message = parameterMessage as SocketUserMessage;
            if (message == null) return;
            var argPos = 0;
            var context = new SocketCommandContext(_client, message); //new CommandContext(_client, message);

            if (context.User.IsBot)
                return;

            if (message.Content.Contains("discord.gg"))
                try
                {
                    if (context.Channel is IGuildChannel)
                        if (GuildConfig.Load(context.Guild.Id).Invite &&
                            !(context.User as SocketGuildUser).GuildPermissions.Administrator)
                        {
                            await message.DeleteAsync();
                            await context.Channel.SendMessageAsync(
                                $"{context.User.Mention} - Pls Daddy, no sending invite links... the admins might get angry");
                        }
                }
                catch
                {
                    //
                }
            if (message.Content.Contains("@everyone") || message.Content.Contains("@here"))
                try
                {
                    if (context.Channel is IGuildChannel)
                        if (GuildConfig.Load(context.Guild.Id).MentionAll &&
                            !(context.User as SocketGuildUser).GuildPermissions.Administrator)
                        {
                            await message.DeleteAsync();

                            var rnd = new Random();
                            var res = rnd.Next(0, FunStr.Everyone.Length);
                            var emb = new EmbedBuilder
                            {
                                Title = $"{context.User} - the admins might get angry",
                                ImageUrl = FunStr.Everyone[res]
                            };
                            await context.Channel.SendMessageAsync("", false, emb.Build());
                        }
                }
                catch
                {
                    //
                }
            try
            {
                if (GuildConfig.Load(context.Guild.Id).Blacklist.Any(b => context.Message.Content.Contains(b)) &&
                    !(context.User as IGuildUser).GuildPermissions.Administrator)
                {
                    await message.DeleteAsync();
                    var r = await context.Channel.SendMessageAsync("Passive Police say NO!");

                    await Task.Delay(5000);
                    await r.DeleteAsync();
                }
            }
            catch
            {
                //
            }


            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) ||
                  message.HasStringPrefix(Load.Pre, ref argPos))) return;
            if (message.HasStringPrefix(Load.Pre + Load.Pre, ref argPos) || message.ToString() == Load.Pre) return;


            var result = await _commands.ExecuteAsync(context, argPos, Provider);

            var commandsuccess = result.IsSuccess;

            bool errlog;

            try
            {
                errlog = GuildConfig.Load(context.Guild.Id).ErrorLog;
            }
            catch
            {
                errlog = false;
            }


            string server;
            if (context.Channel is IPrivateChannel)
                server = "Direct Message "; //because direct messages have no guild name define it as Direct Message
            else
                server = context.Guild.ToString();


            if (!commandsuccess)
            {
                if (errlog)
                    await context.Channel.SendMessageAsync(
                        $"​**COMMAND: **{context.Message} \n**ERROR: **{result.ErrorReason}"); //if in server error responses are enabled reply on error
                await ColourLog.In3Error($"{context.Message}", 'S', $"{context.Guild.Name}", 'E',
                    $"{result.ErrorReason}"); // log errors as arrors
            }
            else
            {
                await ColourLog.In3(
                    $"{context.Message}", 'S', $"{server}", 'U', $"{context.User}",
                    System.Drawing.Color.Teal); //if there is no error log normally
            }
        }

        public async Task _client_JoinedGuild(SocketGuild guild)
        {
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, $"setup/server/{guild.Id}/tags/")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, $"setup/server/{guild.Id}/tags/"));
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, $"setup/server/{guild.Id}/music/")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, $"setup/server/{guild.Id}/music/"));

            var config = Path.Combine(AppContext.BaseDirectory + $"setup/server/{guild.Id}/config.json");
            if (!File.Exists(config))
                GuildConfig.Setup(guild);

            await guild.DefaultChannel.SendMessageAsync(
                $"Hi, I'm PassiveBOT. To see a list of my commands type `{Load.Pre}help` and for some statistics about me type `{Load.Pre}info`\n" +
                "I am able to do music, tags, moderation, memes & more!!!!!");
        }

        public static async Task UserJoinedAsync(SocketGuildUser user)
        {
            var id = user.Guild.Id;
            var wevent = GuildConfig.Load(id).WelcomeEvent;
            if (!wevent) return;
            var wchan = GuildConfig.Load(id).WelcomeChannel;
            var wmessage = GuildConfig.Load(id).WelcomeMessage;
            if (wchan != 0)
            {
                var embed = new EmbedBuilder();
                embed.AddField($"Welcome {user.Mention}", $"{wmessage}");
                var channel = user.Guild.GetTextChannel(wchan);
                await channel.SendMessageAsync($"", false, embed.Build());
            }
            else
            {
                var embed = new EmbedBuilder();
                embed.AddField($"Welcome {user.Mention}", $"{wmessage}");
                await user.Guild.DefaultChannel.SendMessageAsync($"", false, embed.Build());
            }
        }
    }
}