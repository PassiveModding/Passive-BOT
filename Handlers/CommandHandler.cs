using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PassiveBOT.Configuration;
using PassiveBOT.Services;
using Color = System.Drawing.Color;

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

            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) ||
                  message.HasStringPrefix(Load.Pre, ref argPos))) return;
            if (message.HasStringPrefix(Load.Pre + Load.Pre, ref argPos) || message.ToString() == Load.Pre) return;
            if (context.User.IsBot)
                return;

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
                    Color.Teal); //if there is no error log normally
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
                var channel = user.Guild.GetTextChannel(wchan);
                await channel.SendMessageAsync($"{user.Mention}: {wmessage}");
            }
            else
            {
                await user.Guild.DefaultChannel.SendMessageAsync($"{user.Mention}: {wmessage}");
            }
        }
    }
}