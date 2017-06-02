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
            _client.MessageReceived += Auto;
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

            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasStringPrefix(Config.Load().Prefix, ref argPos))) return;
            if (message.HasStringPrefix(Config.Load().Prefix + Config.Load().Prefix, ref argPos) || message.ToString() == ".")
            {
                return;
            }
            var result = await _commands.ExecuteAsync(context, argPos, Provider);

            #region shorten
            var str = context.Message.ToString();
            var use = context.User.Username;
            string server;
            if (context.Channel is IPrivateChannel)
                server = "Direct Message "; //because direct messages have no guild name define it as Direct Message
            else
            {
                var gui = context.Guild.ToString();
                var guild = $"{gui}                 ";
                server = guild.Substring(0, 15);
            }
            var string2 = $"{str}               ";
            var msg = string2.Substring(0, 15);
            var use2 = $"{use}                  ";
            var user = use2.Substring(0, 15);
            #endregion shorten

            var lines = File.ReadAllLines(AppContext.BaseDirectory + @"moderation\error\logging.txt");
            var errlog = lines.ToList();

            if (!result.IsSuccess)
                if (errlog.Contains(context.Guild.Id.ToString()))
                {
                    await context.Channel.SendMessageAsync($"​**COMMAND: **{msg} \n**ERROR: **{result.ErrorReason}"); //if in server error responses are enabled reply on error
                    await ColourLog.ColourError($"{msg} | Server: {server} | ${result.ErrorReason}"); 
                }
                else
                {
                    await ColourLog.ColourError($"{msg} | Server: {server} | ${result.ErrorReason}"); // log errors as arrors
                }
            else
                await ColourLog.ColourInfo($"{msg} | Server: {server} | User: {user}"); //if there is no error log normally
        }

        #region autoresponse

        public async Task Auto(SocketMessage parameterMessage)
        {
            var message = parameterMessage as SocketUserMessage;
            if (message == null) return;
            var argPos = 0;
            var context = new CommandContext(_client, message);

            var lines = File.ReadAllLines(AppContext.BaseDirectory + @"moderation\prefix\nopre.txt");
            var result = lines.ToList();

            if (context.User.IsBot || context.Channel is IPrivateChannel ||
                result.Contains(context.Guild.Id.ToString()))
            {
            }
            else
            {
                var rand = new Random();
                var val = rand.Next(0, 100);
                if (val >= 90)
                    if (message.HasStringPrefix("( ͡° ͜ʖ ͡°)", ref argPos))
                    {
                        await context.Channel.SendMessageAsync("(:eye: ͜ʖ :eye:)");
                    }
                    else if (message.HasStringPrefix("lol", ref argPos))
                    {
                        await context.Channel.SendMessageAsync("lol");
                    }
                    else if (message.HasStringPrefix("lel", ref argPos))
                    {
                        await context.Channel.SendMessageAsync("lel");
                    }
                    else if (message.HasStringPrefix(":3", ref argPos))
                    {
                        await context.Channel.SendMessageAsync("meow");
                    }
                    else if (message.HasStringPrefix("┬─┬ノ(ಠ_ಠノ)", ref argPos) ||
                             message.HasStringPrefix("┬─┬ ノ( ゜-゜ノ)", ref argPos))
                    {
                        var embed = new EmbedBuilder
                        {
                            ImageUrl = "https://media.giphy.com/media/Pch8FiF08bc1G/giphy.gif"
                        };
                        await context.Channel.SendMessageAsync("\u200B" + "(╯°□°）╯︵ ┻━┻ NO! ", false, embed.Build());
                    }
                    else if (message.HasStringPrefix("(╯°□°）╯︵ ┻━┻", ref argPos))
                    {
                        var embed = new EmbedBuilder
                        {
                            ImageUrl = "https://media.giphy.com/media/Pch8FiF08bc1G/giphy.gif"
                        };
                        await context.Channel.SendMessageAsync("\u200B" + "(╯°□°）╯︵ ┻━┻ FLIP ALL THE TABLES! ", false,
                            embed.Build());
                    }
            }
        }
        
        #endregion autoresponse
    }

    
}