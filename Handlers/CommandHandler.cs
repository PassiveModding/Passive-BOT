using System.Threading.Tasks;
using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using System;
using Discord;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using PassiveBOT.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PassiveBOT
{
    public class CommandHandler
    {
        private CommandService _commands;
        private DiscordSocketClient _client;
        public IServiceProvider _provider;


        public async Task ConfigureAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public CommandHandler(IServiceProvider provider)
        {
            _provider = provider;
            _client = _provider.GetService<DiscordSocketClient>();
            _commands = new CommandService();

            _client.MessageReceived += DoCommand;
            _client.MessageReceived += Auto;
        }

        public async Task DoCommand(SocketMessage parameterMessage)
        {
            var message = parameterMessage as SocketUserMessage;
            if (message == null) return;
            int argPos = 0;
            var context = new CommandContext(_client, message);
            
            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasStringPrefix(Config.Load().Prefix, ref argPos))) return;
            var result = await _commands.ExecuteAsync(context, argPos, _provider);

            #region shorten
            string str = context.Message.ToString();
            string gui = "";
            string cha = context.Channel.ToString();
            string use = context.User.Username.ToString();

            if (str.Length > 15)
                str = str.Substring(0, 15);
            else if (str.Length < 15)
            {
                str = str + "               .";
                str = str.Substring(0, 15);
            }
            if (context.Channel is IPrivateChannel)
                gui = "Direct Message ";
            else
            {
                gui = context.Guild.ToString();
                if (gui.Length > 15)
                    gui = gui.Substring(0, 15);
                else if (gui.Length < 15)
                {
                    gui = gui + "               .";
                    gui = gui.Substring(0, 15);
                }
            }
            if (cha.Length > 15)
                cha = cha.Substring(0, 15);
            else if (cha.Length < 15)
            {
                cha = cha + "               .";
                cha = cha.Substring(0, 15);
            }
            if (use.Length > 15)
                use = use.Substring(0, 15);
            else if (use.Length < 15)
            {
                use = use + "               .";
                use = use.Substring(0, 15);
            }

            #endregion shorten
            if (!result.IsSuccess)
            {
                await Handlers.LogHandler.LogErrorAsync($"{str} | Server: {gui}", $"{result.ErrorReason}");
            }
            else
            {
                await Handlers.LogHandler.LogAsync($"{str} | Server: {gui} | User: {use}");
            }
        }

        #region autoresponse
        public async Task Auto(SocketMessage parameterMessage)
        {
            var message = parameterMessage as SocketUserMessage;
            if (message == null) return;
            int argPos = 0;
            var context = new CommandContext(_client, message);

            var lines = File.ReadAllLines(AppContext.BaseDirectory + @"moderation\prefix\nopre.txt");
            List<string> result = lines.ToList();

            if (context.User.IsBot || context.Channel is IPrivateChannel || result.Contains(context.Guild.Id.ToString())) { }
            else
            {
                Random rand = new Random();
                int val = (rand.Next(0, 100));
                if (val >= 90)
                {
                    if (message.HasStringPrefix("( ͡° ͜ʖ ͡°)", ref argPos))
                        await context.Channel.SendMessageAsync("(:eye: ͜ʖ :eye:)");
                    else if (message.HasStringPrefix("lol", ref argPos))
                        await context.Channel.SendMessageAsync("lol");
                    else if (message.HasStringPrefix("lel", ref argPos))
                        await context.Channel.SendMessageAsync("lel");
                    else if (message.HasStringPrefix(":3", ref argPos))
                        await context.Channel.SendMessageAsync("meow");
                    else if (message.HasStringPrefix("._.", ref argPos))
                        await context.Channel.SendMessageAsync("._.");
                    else if (message.HasStringPrefix("┬─┬ノ(ಠ_ಠノ)", ref argPos) || message.HasStringPrefix("┬─┬ ノ( ゜-゜ノ)", ref argPos))
                    {
                        var embed = new EmbedBuilder()
                        {
                            ImageUrl = "https://media.giphy.com/media/Pch8FiF08bc1G/giphy.gif"
                        };
                        await context.Channel.SendMessageAsync("\u200B" + "(╯°□°）╯︵ ┻━┻ NO! ", false, embed.Build());
                        return;
                    }
                    else if (message.HasStringPrefix("(╯°□°）╯︵ ┻━┻", ref argPos))
                    {
                        var embed = new EmbedBuilder()
                        {
                            ImageUrl = "https://media.giphy.com/media/Pch8FiF08bc1G/giphy.gif"
                        };
                        await context.Channel.SendMessageAsync("\u200B" + "(╯°□°）╯︵ ┻━┻ FLIP ALL THE TABLES! ", false, embed.Build());
                        return;
                    }

                }


            }
        }

    }
    #endregion autoresponse
}