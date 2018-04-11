using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ApiAiSDK;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PassiveBOT.Configuration;
using PassiveBOT.strings;
using RedditSharp.Things;

namespace PassiveBOT.Handlers
{
    public class CommandHandler
    {
        public class CMD
        {
            public string Name { get; set; }
            public int Uses { get; set; }
        }

        public static List<CMD> CommandUses = new List<CMD>();

        public static List<SubReddit> SubReddits = new List<SubReddit>();
        private readonly ApiAi _apiAi;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly TimerService _service;
        private bool DoOnce;
        public IServiceProvider Provider;

        public CommandHandler(IServiceProvider provider)
        {
            Provider = provider;
            _client = Provider.GetService<DiscordSocketClient>();
            _commands = new CommandService();
            _service = provider.GetService<TimerService>();

            var config = new AIConfiguration(Config.Load().dialogueflow, SupportedLanguage.English);
            _apiAi = new ApiAi(config);

            _client.MessageReceived += DoCommand;
            _client.Ready += _client_Ready;
        }

        private Task _client_Ready()
        {
            var inv =
                $"https://discordapp.com/oauth2/authorize?client_id={_client.CurrentUser.Id}&scope=bot&permissions=2146958591";
            ColourLog.LogInfo($"Invite: {inv}");
            return Task.CompletedTask;
        }

        public async Task ConfigureAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        /*public class NoSpam
        {
            public ulong ID { get; set; }
            public string LastMessage { get; set; }
            public DateTime LastMessageDate { get; set; }
        }*/

        public async Task AutoMessage(SocketUserMessage message, SocketCommandContext context)
        {
            try
            {
                if (!(context.Channel is IDMChannel))
                    if (File.Exists(Path.Combine(AppContext.BaseDirectory, $"setup/server/{context.Guild.Id}.json")) &&
                        GuildConfig.GetServer(context.Guild).AutoMessage.Any(x => x.channelID == context.Channel.Id))
                    {
                        var serverobj = GuildConfig.GetServer(context.Guild);
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

                            GuildConfig.SaveServer(serverobj);
                        }
                    }
            }
            catch
            {
                //
            }
        }

        public async Task CheckMessage(SocketUserMessage message, SocketCommandContext context)
        {
            if (message.Content.Contains("discord.gg"))
                try
                {
                    if (context.Channel is IGuildChannel)
                        if (GuildConfig.GetServer(context.Guild).Invite &&
                            !((SocketGuildUser) context.User).GuildPermissions.Administrator)
                            if (!((IGuildUser) context.User).RoleIds
                                .Intersect(GuildConfig.GetServer(context.Guild).InviteExcempt).Any())
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
                        if (GuildConfig.GetServer(context.Guild).MentionAll &&
                            !((SocketGuildUser) context.User).GuildPermissions.Administrator)
                            if (!((IGuildUser) context.User).RoleIds
                                .Intersect(GuildConfig.GetServer(context.Guild).InviteExcempt).Any())
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
                var blacklistdetected = false;
                if (GuildConfig.GetServer(context.Guild).BlacklistBetterFilter)
                {
                    if (GuildConfig.GetServer(context.Guild).Blacklist.Any(x =>
                            ProfanityFilter.doreplacements(ProfanityFilter.RemoveDiacritics(context.Message.Content))
                                .ToLower().Contains(x.ToLower())) &&
                        !((IGuildUser) context.User).GuildPermissions.Administrator)
                        blacklistdetected = true;
                }
                else
                {
                    if (GuildConfig.GetServer(context.Guild).Blacklist
                            .Any(b => context.Message.Content.ToLower().Contains(b.ToLower())) &&
                        !((IGuildUser) context.User).GuildPermissions.Administrator)
                        blacklistdetected = true;
                }

                if (blacklistdetected)
                {
                    await message.DeleteAsync();
                    var blmessage = "";
                    try
                    {
                        blmessage = GuildConfig.GetServer(context.Guild).BlacklistMessage;
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
        }

        public void InitialisePartnerProgram()
        {
            if (DoOnce) return;
            try
            {
                foreach (var guild in _client.Guilds)
                    try
                    {
                        //GuildConfig.Setup(guild);
                        var guildconfig = GuildConfig.GetServer(guild);
                        if (guildconfig.PartnerSetup.PartherChannel != 0)
                            if (guildconfig.PartnerSetup.IsPartner &&
                                _client.GetChannel(guildconfig.PartnerSetup.PartherChannel) is IMessageChannel)
                                TimerService.AcceptedServers.Add(guild.Id);
                    }
                    catch //(Exception e)
                    {
                        //Console.WriteLine(e);
                    }

                _service.Restart();
            }
            catch //(Exception e)
            {
                //Console.WriteLine(e);
            }

            DoOnce = true;
        }

        public async Task DoCommand(SocketMessage parameterMessage)
        {
            Load.Messages++;
            if (!(parameterMessage is SocketUserMessage message)) return;
            var argPos = 0;
            var context = new SocketCommandContext(_client, message); //new CommandContext(_client, message);
            if (context.User.IsBot) return;

            InitialisePartnerProgram();
            await CheckMessage(message, context);
            await AutoMessage(message, context);

            if (message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var newmessage = Regex.Replace(context.Message.Content, @"^\!?<@[0-9]+>\s*", "",
                    RegexOptions.Multiline);
                try
                {
                    var response = _apiAi.TextRequest(newmessage);
                    if (response.Result.Fulfillment.Speech != "")
                        await context.Channel.SendMessageAsync(response.Result.Fulfillment.Speech);
                }
                catch
                {
                    //
                }

                return;
            }

            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) ||
                  message.HasStringPrefix(Load.Pre, ref argPos) ||
                  message.HasStringPrefix(GuildConfig.GetServer(context.Guild).Prefix, ref argPos))) return;

            var result = await _commands.ExecuteAsync(context, argPos, Provider);
            var commandsuccess = result.IsSuccess;

            var server = context.Channel is IPrivateChannel ? "Direct Message " : context.Guild.Name;

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
                        var c = context.Client.GetChannel(s);
                        var embed = new EmbedBuilder
                        {
                            Title = $"ERROR: {context.Message}",
                            Description = $"REASON:\n" +
                                          $"{result.ErrorReason}"
                        };
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
                try
                {
                    await context.Message.DeleteAsync();
                }
                catch
                {
                    //
                }

                await ColourLog.In3Error($"{context.Message}", 'S', $"{context.Guild.Name}", 'E',
                    $"{result.ErrorReason}"); // log errors as arrors
            }
            else
            {
                await ColourLog.In3(
                    $"{context.Message}", 'S', $"{server}", 'U', $"{context.User}"); //if there is no error log normally

                Load.Commands++;
                var srch = _commands.Search(context, argPos);
                if (srch.IsSuccess)
                {
                    var name = srch.Commands.Select(x => x.Command.Name).FirstOrDefault();
                    if (name != null)
                    {
                        if (CommandUses.Any(x => x.Name.ToLower() == name.ToLower()))
                        {
                            var cmd = CommandUses.First(x => x.Name.ToLower() == name.ToLower());
                            cmd.Uses++;
                        }
                        else
                        {
                            CommandUses.Add(new CMD
                            {
                                Name = name.ToLower(),
                                Uses = 1
                            });
                        }
                    }
                }
            }
        }

        public class SubReddit
        {
            public string title { get; set; }
            public List<Post> Posts { get; set; }
            public DateTime LastUpdate { get; set; }
            public int Hits { get; set; } = 0;
        }
    }
}