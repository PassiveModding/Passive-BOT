using System;
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


            _client.Ready += _client_Ready;
            _client.MessageReceived += DoCommand;
        }

        public async Task _client_Ready()
        {
            foreach (var guildd in _client.Guilds)
            {
                var guild = guildd.Id;
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
            Load.Messages++;
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
                if (GuildConfig.Load(context.Guild.Id).Blacklist
                        .Any(b => context.Message.Content.ToLower().Contains(b.ToLower())) &&
                    !(context.User as IGuildUser).GuildPermissions.Administrator)
                {
                    await message.DeleteAsync();
                    var blmessage = "";
                    try
                    {
                        blmessage = GuildConfig.Load(context.Guild.Id).BlacklistMessage;
                    }
                    catch
                    {
                        //
                    }
                    if (blmessage != "")
                    {
                        var r = await context.Channel.SendMessageAsync(blmessage);
                        await Task.Delay(5000);
                        await r.DeleteAsync();
                    }
                }
            }
            catch
            {
                //
            }


            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) ||
                  message.HasStringPrefix(Load.Pre, ref argPos) ||
                  message.HasStringPrefix(GuildConfig.Load(context.Guild.Id).Prefix, ref argPos))) return;

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
                try
                {
                    if (!(result.ErrorReason == "Unknown command." ||
                          result.ErrorReason == "The input text has too many parameters." ||
                          result.ErrorReason == "The input text has too few parameters." ||
                          result.ErrorReason == "Timeout" ||
                          result.ErrorReason == "This command may only be invoked in an NSFW channel." ||
                          result.ErrorReason == "Command can only be run by the owner of the bot"))
                    {
                        var s = Homeserver.Load().Error;
                        var c = await (context.Client as IDiscordClient).GetChannelAsync(s);
                        var embed = new EmbedBuilder();
                        embed.AddField($"ERROR", context.Message);
                        embed.AddField("Reason", result.ErrorReason);
                        embed.WithFooter(x => { x.Text = $"{context.Message.CreatedAt} || {context.Guild.Name}"; });
                        embed.Color = Color.Red;
                        await (c as ITextChannel).SendMessageAsync("", false, embed.Build());
                    }
                }
                catch
                {
                    //
                }
                var errmessage = await context.Channel.SendMessageAsync(
                    $"​**COMMAND: **{context.Message} \n**ERROR: **{result.ErrorReason}"); //if in server error responses are enabled reply on error
                await Task.Delay(5000);
                await errmessage.DeleteAsync();
                await context.Message.DeleteAsync();
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
    }
}