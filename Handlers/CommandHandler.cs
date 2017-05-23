using System.Threading.Tasks;
using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using System;
using Discord;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using PassiveBOT.Services;

namespace PassiveBOT
{
    public class CommandHandler
    {
        private CommandService commands;
        private DiscordSocketClient client;
        public IDependencyMap Map;


        public async Task ConfigureAsync()
        {
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task Install(IDependencyMap _map)
        {
            client = _map.Get<DiscordSocketClient>();
            commands = new CommandService();

            await commands.AddModulesAsync(Assembly.GetEntryAssembly());

            client.MessageReceived += HandleCommand;
            client.MessageReceived += Auto;
        }

        public async Task HandleCommand(SocketMessage parameterMessage)
        {
            var message = parameterMessage as SocketUserMessage;
            if (message == null) return;
            int argPos = 0;
            var context = new CommandContext(client, message);


            if (!(message.HasMentionPrefix(client.CurrentUser, ref argPos) || message.HasStringPrefix(Config.Load().Prefix, ref argPos))) return;


            var result = await commands.ExecuteAsync(context, argPos, Map);

            var Context = new CommandContext(client, message);
            #region shorten
            string str = Context.Message.ToString();
            if (str.Length > 15)
                str = str.Substring(0, 15);
            else if (str.Length < 15)
            {
                str = str + "               .";
                str = str.Substring(0, 15);
            }
            string gui = "";
            if (context.Channel is IPrivateChannel)
            {
                gui = "Direct Message";
                if (gui.Length > 15)
                    gui = gui.Substring(0, 15);
                else if (gui.Length < 15)
                {
                    gui = gui + "               .";
                    gui = gui.Substring(0, 15);
                }
            }
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


            string cha = context.Channel.ToString();
            if (cha.Length > 15)
                cha = cha.Substring(0, 15);
            else if (cha.Length < 15)
            {
                cha = cha + "               .";
                cha = cha.Substring(0, 15);
            }

            string use = context.User.Username.ToString();
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
                await Program.Log(new LogMessage(LogSeverity.Info, "Error",
                   $"{str} | Server: {gui} | {result.ErrorReason}"));
            }
            else
            {
                await Program.Log(new LogMessage(LogSeverity.Info, "Command",
                    $"{str} | Server: {gui} | User: {use}"));
            }
        }

        #region autoresponse
        public async Task Auto(SocketMessage parameterMessage)
        {
            var message = parameterMessage as SocketUserMessage;
            if (message == null) return;
            int argPos = 0;
            var context = new CommandContext(client, message);

            var lines = File.ReadAllLines(AppContext.BaseDirectory + @"moderation\prefix\nopre.txt");
            List<string> result = lines.ToList();

            if (context.User.IsBot)
            {
                return;
            }
            else if (context.Channel is IPrivateChannel)
            {

            }
            else if (result.Contains(context.Guild.Id.ToString())) { }
            else
            {
                Random rand = new Random();
                int val = (rand.Next(0, 100));
                if (val >= 90)
                {
                    if (message.HasStringPrefix("( ͡° ͜ʖ ͡°)", ref argPos))
                    {
                        await context.Channel.SendMessageAsync("(:eye: ͜ʖ :eye:)");
                    }
                    else if (message.HasStringPrefix("lol", ref argPos))
                    {
                        await context.Channel.SendMessageAsync("lol");
                        return;
                    }
                    else if (message.HasStringPrefix("lel", ref argPos))
                    {
                        await context.Channel.SendMessageAsync("lel");
                        return;
                    }
                    else if (message.HasStringPrefix(":3", ref argPos))
                    {
                        await context.Channel.SendMessageAsync("meow");
                        return;
                    }
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
                    else if (message.HasStringPrefix("._.", ref argPos))
                    {
                        await context.Channel.SendMessageAsync("._.");
                        return;
                    }
                    else
                    {
                        return;
                    }
                }


            }
        }

    }
    #endregion autoresponse
}