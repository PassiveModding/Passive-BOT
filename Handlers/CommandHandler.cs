using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PassiveBOT.Configuration;

namespace PassiveBOT.Handlers
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        public IServiceProvider Provider;

        public CommandHandler(IServiceProvider provider)
        {
            Provider = provider;
            _client = Provider.GetService<DiscordSocketClient>();
            _commands = new CommandService();

            _client.MessageReceived += DoCommand;
            _client.JoinedGuild += _client_JoinedGuild;
            _client.UserJoined += UserJoinedAsync;
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
            var context = new CommandContext(_client, message);

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
            {
                server = "Direct Message "; //because direct messages have no guild name define it as Direct Message
            }
            else
            {
                server = context.Guild.ToString();
            }


            if (!commandsuccess)
            {
                if (errlog)
                {
                    await context.Channel.SendMessageAsync(
                        $"​**COMMAND: **{context.Message} \n**ERROR: **{result.ErrorReason}"); //if in server error responses are enabled reply on error
                }
                await ColourLog.In3Error($"{context.Message}", 'S', $"{context.Guild.Name}", 'E', $"{result.ErrorReason}"); // log errors as arrors
            }
            else
                await ColourLog.In3(
                    $"{context.Message}",'S', $"{server}",'U', $"{context.User}", System.Drawing.Color.Teal); //if there is no error log normally
        }

        public async Task _client_JoinedGuild(SocketGuild guild)
        {
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, $"setup/server/{guild.Id}/tags/")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, $"setup/server/{guild.Id}/tags/"));
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, $"setup/server/{guild.Id}/music/")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, $"setup/server/{guild.Id}/music/"));

            var config = Path.Combine(AppContext.BaseDirectory + $"setup/server/{guild.Id}/config.json");
            if (!File.Exists(config))
                GuildConfig.Setup(guild.Id, guild.Name);

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