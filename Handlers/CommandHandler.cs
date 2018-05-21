using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PassiveBOT.Discord.Context;
using PassiveBOT.Models;

namespace PassiveBOT.Handlers
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider Provider;

        public CommandHandler(IServiceProvider provider)
        {
            Provider = provider;
            _client = Provider.GetService<DiscordSocketClient>();
            _commands = new CommandService();

            _client.MessageReceived += DoCommand;
            _client.Ready += _client_Ready;
            _client.JoinedGuild += _client_JoinedGuild;
        }

        private Task _client_JoinedGuild(SocketGuild Guild)
        {
            var dblist = DatabaseHandler.GetFullConfig();
            if (dblist.All(x => x.ID != Guild.Id))
            {
                foreach (var missingguild in _client.Guilds.Where(g => dblist.All(x => x.ID != g.Id)))
                {
                    DatabaseHandler.AddGuild(missingguild.Id);
                }
            }
            return Task.CompletedTask;
        }

        private Task _client_Ready()
        {
            var inv = $"https://discordapp.com/oauth2/authorize?client_id={_client.CurrentUser.Id}&scope=bot&permissions=2146958591";
            LogHandler.LogMessage($"Invite: {inv}");
            DatabaseHandler.DatabaseInitialise(_client);
            var dblist = DatabaseHandler.GetFullConfig();
            foreach (var guild in _client.Guilds.Where(g => dblist.All(x => x.ID != g.Id)))
            {
                DatabaseHandler.AddGuild(guild.Id);
            }
            return Task.CompletedTask;
        }

        public async Task ConfigureAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task DoCommand(SocketMessage parameterMessage)
        {
            if (!(parameterMessage is SocketUserMessage message)) return;
            var argPos = 0;
            var context = new Context(_client, message, Provider);
            if (context.User.IsBot) return;

            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasStringPrefix(ConfigModel.Load().Prefix, ref argPos))) return;

            var result = await _commands.ExecuteAsync(context, argPos, Provider);

            if (result.IsSuccess)
            {
                LogHandler.LogMessage($"{context.Message.Content}");
            }
            else
            {
                try
                {
                    var desc = "";
                    if (result.Error != CommandError.UnknownCommand)
                    {
                        var srch = _commands.Search(context, argPos);
                        var cmd = srch.Commands.FirstOrDefault();

                        desc += $"**Command Name:** `{cmd.Command.Name}`\n";
                        if (cmd.Command.Parameters.Any())
                        {
                            desc += $"**Parameters:** {string.Join(" ", cmd.Command.Parameters.Select(x => x.IsOptional ? $" `<(Optional){x.Name}>` " : $" `<{x.Name}>` "))}\n";
                        }
                        else
                        {
                            desc += "**Parameters:** N/A\n";
                        }

                        desc += $"**Summary:** `{cmd.Command?.Summary ?? "N/A"}`\n" +
                                $"**Remarks:** `{cmd.Command?.Remarks ?? "N/A"}`\n";

                        if (cmd.Command.Aliases.Any())
                        {
                            desc += $"**Aliases:** {string.Join(" ", cmd.Command.Aliases.Select(x => $"`{x}`"))}\n";
                        }

                        desc += "**Error Reason:**\n" +
                                $"{result.ErrorReason}";
                    }
                    else
                    {
                        desc += "**Command:** N/A";
                    }

                    var errmsg = new EmbedBuilder
                    {
                        Title = $"Error, {context.User.Username}",
                        Description = desc
                    };

                    try
                    {
                        await context.Channel.SendMessageAsync("", false, errmsg.Build());
                    }
                    catch
                    {
                        //
                    }

                    LogHandler.LogMessage(context.Message.Content, LogSeverity.Error);
                }
                catch (Exception e)
                {
                    LogHandler.LogMessage(e.ToString(), LogSeverity.Error);
                }
            }
        }
    }
}