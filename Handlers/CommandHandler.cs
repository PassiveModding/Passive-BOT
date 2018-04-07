using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ApiAiSDK;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PassiveBOT.Configuration;
using PassiveBOT.strings;

namespace PassiveBOT.Handlers
{
    public class CommandHandler
    {
        private readonly ApiAi _apiAi;
        private readonly DiscordShardedClient _client;
        private readonly CommandService _commands;


        public IServiceProvider Provider;

        public CommandHandler(IServiceProvider provider)
        {
            Provider = provider;
            _client = Provider.GetService<DiscordShardedClient>();
            _commands = new CommandService();


            var config = new AIConfiguration(Config.Load().dialogueflow, SupportedLanguage.English);
            _apiAi = new ApiAi(config);

            _client.MessageReceived += DoCommand;
        }

        public async Task ConfigureAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public class NoSpam
        {
            public ulong ID { get; set; }
            public string LastMessage { get; set; }
            public DateTime LastMessageDate { get; set; }
        }


        public async Task DoCommand(SocketMessage parameterMessage)
        {
            Load.Messages++;
            if (!(parameterMessage is SocketUserMessage message)) return;
            var argPos = 0;
            var context = new ShardedCommandContext(_client, message); //new CommandContext(_client, message);

            if (context.User.IsBot)
                return;

            if (message.Content.Contains("discord.gg"))
                try
                {
                    if (context.Channel is IGuildChannel)
                        if (GuildConfig.Load(context.Guild.Id).Invite &&
                            !((SocketGuildUser) context.User).GuildPermissions.Administrator)
                            if (!((IGuildUser) context.User).RoleIds
                                .Intersect(GuildConfig.Load(context.Guild.Id).InviteExcempt).Any())
                            {
                                await message.DeleteAsync();
                                await context.Channel.SendMessageAsync(
                                    $"{context.User.Mention} - Pls Daddy, no sending invite links... the admins might get angry");
                                //if
                                // 1. The server Has Invite Deletions turned on
                                // 2. The user is not an admin
                                // 3. The user does not have one of the invite excempt roles
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
                            !((SocketGuildUser) context.User).GuildPermissions.Administrator)
                            if (!((IGuildUser) context.User).RoleIds
                                .Intersect(GuildConfig.Load(context.Guild.Id).InviteExcempt).Any())
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
                                //if
                                // 1. The server Has Mention Deletions turned on
                                // 2. The user is not an admin
                                // 3. The user does not have one of the mention excempt roles
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
                    !((IGuildUser) context.User).GuildPermissions.Administrator)
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

            try
            {
                if (!(context.Channel is IDMChannel))
                {
                    if (File.Exists(Path.Combine(AppContext.BaseDirectory, $"setup/server/{context.Guild.Id}.json")) && GuildConfig.Load(context.Guild.Id).AutoMessage.Any(x => x.channelID == context.Channel.Id))
                    {
                        var serverobj = GuildConfig.Load(context.Guild.Id);
                        var chan = serverobj.AutoMessage.First(x => x.channelID == context.Channel.Id);
                        if (chan.enabled)
                        {
                            chan.messages++;
                            if (chan.messages >= chan.sendlimit)
                            {
                                var embed = new EmbedBuilder();
                                embed.AddField("AutoMessage", chan.automessage);
                                embed.Color = Color.Green;
                                await context.Channel.SendMessageAsync("", false, embed.Build());
                                chan.messages = 0;
                            }
                            GuildConfig.SaveServer(serverobj, context.Guild);
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }


            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) ||
                  message.HasStringPrefix(Load.Pre, ref argPos) ||
                  message.HasStringPrefix(GuildConfig.Load(context.Guild.Id).Prefix, ref argPos))) return;

            if (message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var newmessage = Regex.Replace(context.Message.Content, @"^\!?<@[0-9]+>\s*", "",
                    RegexOptions.Multiline);
                var response = _apiAi.TextRequest(newmessage);
                if (response.Result.Fulfillment.Speech != "")
                    await context.Channel.SendMessageAsync(response.Result.Fulfillment.Speech);
                return;
            }


            var result = await _commands.ExecuteAsync(context, argPos, Provider);
            var commandsuccess = result.IsSuccess;


            string server;
            if (context.Channel is IPrivateChannel)
                server = "Direct Message "; //because direct messages have no guild name define it as Direct Message
            else
                server = context.Guild.Name;


            if (!commandsuccess)
            {
                try
                {
                    if (!(result.ErrorReason == "Unknown command." ||
                          result.ErrorReason == "The input text has too many parameters." ||
                          result.ErrorReason == "The input text has too few parameters." ||
                          result.ErrorReason == "Timeout" ||
                          result.ErrorReason == "This command may only be invoked in an NSFW channel." ||
                          result.ErrorReason == "Command can only be run by the owner of the bot" ||
                          result.ErrorReason == "This command is locked to NSFW Channels. Pervert."))
                    {
                        var s = Homeserver.Load().Error;
                        var c = await (context.Client as IDiscordClient).GetChannelAsync(s);
                        var embed = new EmbedBuilder();
                        embed.AddField("ERROR", context.Message);
                        embed.AddField("Reason", result.ErrorReason);
                        embed.WithFooter(x => { x.Text = $"{context.Message.CreatedAt} || {context.Guild.Name}"; });
                        embed.Color = Color.Red;
                        await ((ITextChannel) c).SendMessageAsync("", false, embed.Build());
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
                    $"{context.Message}", 'S', $"{server}", 'U', $"{context.User}"); //if there is no error log normally

                Load.Commands++;
            }
        }
    }
}