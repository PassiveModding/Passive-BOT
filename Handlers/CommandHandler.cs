using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PassiveBOT.Discord.Context;
using PassiveBOT.Discord.Extensions;
using PassiveBOT.Discord.TypeReaders;
using PassiveBOT.Models;

namespace PassiveBOT.Handlers
{
    public class CommandHandler
    {
        public static IServiceProvider Provider;
        public static Dictionary<ulong, LanguageMap.languagecode> Translated = new Dictionary<ulong, LanguageMap.languagecode>();
        public static RuntimeStats Stats = new RuntimeStats();

        public static List<Connect4Lobby> Connect4List = new List<Connect4Lobby>();
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        public CommandHandler(IServiceProvider provider)
        {
            Provider = provider;
            _client = Provider.GetService<DiscordSocketClient>();
            _commands = new CommandService();
            Config = ConfigModel.Load();

            _commands.AddTypeReader(typeof(Emoji), new EmojiTypeReader());
            _client.MessageReceived += DoCommand;
            _client.Ready += _client_Ready;
            _client.JoinedGuild += _client_JoinedGuild;

            //Translate Reactions
            _client.ReactionAdded += _client_ReactionAdded;

            //Welcome and Joined Events
            _client.UserJoined += _client_UserJoined;
            _client.UserLeft += _client_UserLeftAsync;
        }

        //public static List<GuildMsgx> GuildMsgs { get; set; } = new List<GuildMsgx>();
        public static ConfigModel Config { get; set; } = ConfigModel.Load();

        private static async Task _client_ReactionAdded(Cacheable<IUserMessage, ulong> Message, ISocketMessageChannel Channel, SocketReaction Reaction)
        {
            try
            {
                LogHandler.LogMessage("Reaction Detected", LogSeverity.Verbose);
                if (Message.HasValue)
                {
                    if (Message.Value.Author.IsBot || Reaction.User.Value.IsBot || Message.Value.Embeds.Any())
                    {
                        return;
                    }

                    var guild = DatabaseHandler.GetGuild((Channel as IGuildChannel).GuildId);
                    if (!guild.Settings.Translate.EasyTranslate)
                    {
                        return;
                    }

                    //Check custom matches first
                    var languagetype = guild.Settings.Translate.Custompairs.FirstOrDefault(x => x.EmoteMatches.Any(val => val == Reaction.Emote.Name));

                    if (languagetype == null)
                    {
                        //If no custom matches, check default matches
                        languagetype = LanguageMap.Map.FirstOrDefault(x => x.EmoteMatches.Any(val => val == Reaction.Emote.Name));
                        if (languagetype == null)
                        {
                            return;
                        }
                    }

                    if (Translated.Any(x => x.Key == Reaction.MessageId && x.Value == languagetype.Language))
                    {
                        return;
                    }

                    var language = languagetype.Language.ToString();
                    if (language == "zh_CN")
                    {
                        language = "zh-CN";
                    }

                    if (language == "zh_TW")
                    {
                        language = "zh-TW";
                    }

                    if (language == "_is")
                    {
                        language = "is";
                    }

                    var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl={language}&dt=t&ie=UTF-8&oe=UTF-8&q={Uri.EscapeDataString(Message.Value.Content)}";
                    var embed = new EmbedBuilder
                    {
                        Title = "Translate",
                        Color = Color.Blue
                    };


                    var client = new WebClient {Encoding = Encoding.UTF8};

                    var stream = client.OpenRead(url);
                    var reader = new StreamReader(stream ?? throw new InvalidOperationException());
                    var content = reader.ReadToEnd();
                    dynamic file = JsonConvert.DeserializeObject(content);
                    var original = Message.Value.Content;
                    if (original.Length > 1024)
                    {
                        original = original.Substring(0, 1020) + "...";
                    }

                    var response = TranslateMethods.HandleReponse(file);
                    if (response.ToString().Length > 1024)
                    {
                        response = response.Substring(0, 1020) + "...";
                    }

                    embed.AddField($"Translated [{language} || {Reaction.Emote}]", $"{response}", true);
                    embed.AddField($"Original [{file[2]}]", $"{original}", true);
                    embed.AddField("Info", $"Original Author: {Message.Value.Author}\n" +
                                           $"Reactor: {Reaction.User.Value}", true);
                    if (guild.Settings.Translate.DMTranslations)
                    {
                        await Reaction.User.Value.SendMessageAsync("", false, embed.Build());
                    }
                    else
                    {
                        await Channel.SendMessageAsync("", false, embed.Build());
                        Translated.Add(Reaction.MessageId, languagetype.Language);
                    }

                    client.Dispose();
                }
            }
            catch (Exception e)
            {
                LogHandler.LogMessage(e.ToString(), LogSeverity.Error);
            }
        }

        private static async Task _client_UserLeftAsync(SocketGuildUser User)
        {
            await EventTriggers._client_UserLeft(User);
        }

        private static async Task _client_UserJoined(SocketGuildUser User)
        {
            await EventTriggers._client_UserJoined(User);
        }

        private Task _client_JoinedGuild(SocketGuild Guild)
        {
            if (DatabaseHandler.GetGuild(Guild.Id) == null)
            {
                DatabaseHandler.InsertGuildObject(new GuildModel
                {
                    ID = Guild.Id
                });

            }
            return Task.CompletedTask;
        }

        private Task _client_Ready()
        {
            var inv = $"https://discordapp.com/oauth2/authorize?client_id={_client.CurrentUser.Id}&scope=bot&permissions=2146958591";
            LogHandler.LogMessage($"Invite: {inv}");
            DatabaseHandler.DatabaseInitialise(_client);
            /*
            var dblist = DatabaseHandler.GetFullConfig();
            foreach (var guild in _client.Guilds.Where(g => dblist.All(x => x.ID != g.Id)))
            {
                DatabaseHandler.AddGuild(guild.Id);
            }
            */
            return Task.CompletedTask;
        }

        public async Task ConfigureAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task<Context> DoAutoMessage(Context Context)
        {
            if (Context.Channel is IDMChannel)
            {
                return Context;
            }

            var AutomessageChannel = Context.Server.AutoMessage.AutoMessageChannels.FirstOrDefault(x => x.ChannelID == Context.Channel.Id);
            if (AutomessageChannel == null)
            {
                return Context;
            }

            if (!AutomessageChannel.Enabled)
            {
                return Context;
            }

            AutomessageChannel.Count++;

            if (AutomessageChannel.Count >= AutomessageChannel.Limit)
            {
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder
                {
                    Title = "Auto Message",
                    Color = Color.Green,
                    Description = AutomessageChannel.Message
                }.Build());
                AutomessageChannel.Count = 0;
            }

            Context.Server.Save();
            return Context;
        }

        public async Task<Context> DoLevels(Context Context)
        {
            if (Context.Channel is IDMChannel)
            {
                return Context;
            }

            if (!Context.Server.Levels.Settings.Enabled) return Context;

            var luser = Context.Server.Levels.Users.FirstOrDefault(x => x.UserID == Context.User.Id);
            if (luser == null)
            {
                Context.Server.Levels.Users.Add(new GuildModel.levelling.luser
                {
                    Level = 1,
                    UserID = Context.User.Id,
                    XP = 0,
                    LastUpdate = DateTime.UtcNow + TimeSpan.FromMinutes(1)
                });
            }
            else
            {
                if (luser.LastUpdate > DateTime.UtcNow)
                {
                    return Context;
                }


                luser.XP += 10;
                luser.LastUpdate = DateTime.UtcNow + TimeSpan.FromMinutes(1);

                var requiredxp = luser.Level * 50 + luser.Level * luser.Level * 25;
                if (luser.XP > requiredxp)
                {
                    luser.Level++;
                    string roleadded = null;
                    if (Context.Server.Levels.RewardRoles.Any())
                    {
                        var rolesavailable = Context.Server.Levels.RewardRoles.Where(x => x.Requirement <= luser.Level - 1).ToList();
                        var roletoreceive = new List<GuildModel.levelling.levelreward>();
                        if (rolesavailable.Any())
                        {
                            if (Context.Server.Levels.Settings.IncrementLevelRewards)
                            {
                                var maxrole = rolesavailable.Max(x => x.Requirement);
                                roletoreceive.Add(rolesavailable.FirstOrDefault(x => x.Requirement == maxrole));
                            }
                            else
                            {
                                roletoreceive = rolesavailable;
                            }
                        }

                        if (roletoreceive.Count != 0)
                        {
                            foreach (var role in roletoreceive)
                            {
                                if (((IGuildUser) Context.User).RoleIds.Contains(role.RoleID)) continue;
                                var grole = Context.Guild.GetRole(role.RoleID);
                                if (grole != null)
                                {
                                    try
                                    {
                                        await ((SocketGuildUser) Context.User).AddRoleAsync(grole);
                                        roleadded += $"Role Reward: {grole.Name}\n";
                                    }
                                    catch
                                    {
                                        //
                                    }
                                }
                                else
                                {
                                    Context.Server.Levels.RewardRoles.Remove(role);
                                }
                            }

                            if (roletoreceive.Count != rolesavailable.Count && roletoreceive.Count == 1)
                            {
                                try
                                {
                                    rolesavailable.Remove(roletoreceive.First());
                                    var roles = rolesavailable.Select(x => Context.Guild.GetRole(x.RoleID)).Where(x => x != null);

                                    await ((SocketGuildUser) Context.User).RemoveRolesAsync(roles);
                                }
                                catch
                                {
                                    //
                                }
                            }
                        }
                    }

                    var embed = new EmbedBuilder
                    {
                        Title = $"{Context.User.Username} Levelled Up!",
                        ThumbnailUrl = Context.User.GetAvatarUrl(),
                        Description = $"Level: {luser.Level - 1}\n" +
                                      $"{roleadded}" +
                                      $"XP: {requiredxp}\n" +
                                      $"Next Level At: {luser.Level * 50 + luser.Level * luser.Level * 25} XP",
                        Color = Color.Blue
                    };
                    if (Context.Server.Levels.Settings.UseLogChannel)
                    {
                        try
                        {
                            if (Context.Socket.Guild.GetChannel(Context.Server.Levels.Settings.LogChannelID) is IMessageChannel chan)
                            {
                                await chan.SendMessageAsync("", false, embed.Build());
                            }
                        }
                        catch
                        {
                            //
                        }
                    }

                    if (Context.Server.Levels.Settings.ReplyLevelUps)
                    {
                        try
                        {
                            await Context.Channel.SendMessageAsync("", false, embed.Build());
                        }
                        catch
                        {
                            //
                        }
                    }

                    if (Context.Server.Levels.Settings.DMLevelUps)
                    {
                        try
                        {
                            embed.Title = $"You Levelled up in {Context.Guild.Name}!";
                            await Context.User.SendMessageAsync("", false, embed.Build());
                        }
                        catch
                        {
                            //
                        }
                    }
                }
            }

            Context.Server.Save();
            return Context;
        }

        private async Task DoCommand(SocketMessage parameterMessage)
        {
            if (!(parameterMessage is SocketUserMessage message)) return;
            var argPos = 0;
            var context = new Context(_client, message, Provider);
            Stats.MessagesReceived++;
            if (context.User.IsBot) return;

            if (context.Channel is IDMChannel)
            {
                if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasStringPrefix(Config.Prefix, ref argPos))) return;
            }
            else
            {
                if (context.Server == null)
                {
                    DatabaseHandler.InsertGuildObject(new GuildModel
                    {
                        ID = context.Guild.Id
                    });
                    await DoCommand(parameterMessage);
                    return;
                }

                context = await DoLevels(context);
                context = await DoAutoMessage(context);
                if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) && !context.Server.Settings.Prefix.DenyMentionPrefix ||
                      message.HasStringPrefix(Config.Prefix, ref argPos) && !context.Server.Settings.Prefix.DenyDefaultPrefix ||
                      context.Server.Settings.Prefix.CustomPrefix != null && message.HasStringPrefix(context.Server.Settings.Prefix.CustomPrefix, ref argPos)))
                {
                    if (message.HasStringPrefix(Config.Prefix, ref argPos) && context.Server.Settings.Prefix.DenyDefaultPrefix)
                    {
                        if (context.Server.Settings.Prefix.CustomPrefix != null)
                        {
                            //Ensure that if for some reason the server's custom prefix isn't set and but they are denying the default prefix that commands are still allowed
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }


            var result = await _commands.ExecuteAsync(context, argPos, Provider);

            if (result.IsSuccess)
            {
                LogHandler.LogMessage(context);
                Stats.CommandsRan++;
            }
            else
            {
                try
                {
                    string desc;
                    if (result.Error == CommandError.UnknownCommand)
                    {
                        desc = "**Command:** N/A";
                    }
                    else
                    {
                        var srch = _commands.Search(context, argPos);
                        var cmd = srch.Commands.FirstOrDefault();

                        desc = $"**Command Name:** `{cmd.Command.Name}`\n" +
                               $"**Summary:** `{cmd.Command?.Summary ?? "N/A"}`\n" +
                               $"**Remarks:** `{cmd.Command?.Remarks ?? "N/A"}`\n" +
                               $"**Aliases:** {(cmd.Command.Aliases.Any() ? string.Join(" ", cmd.Command.Aliases.Select(x => $"`{x}`")) : "N/A")}\n" +
                               $"**Parameters:** {(cmd.Command.Parameters.Any() ? string.Join(" ", cmd.Command.Parameters.Select(x => x.IsOptional ? $" `<(Optional){x.Name}>` " : $" `<{x.Name}>` ")) : "N/A")}\n" +
                               "**Error Reason**\n" +
                               $"{result.ErrorReason}";
                    }

                    try
                    {
                        await context.Channel.SendMessageAsync("", false, new EmbedBuilder
                        {
                            Title = $"{context.User.Username.ToUpper()} ERROR",
                            Description = desc
                        }.Build());
                    }
                    catch
                    {
                        //
                    }

                    LogHandler.LogMessage(context, result.ErrorReason, LogSeverity.Error);
                }
                catch (Exception e)
                {
                    LogHandler.LogMessage(context, e.ToString(), LogSeverity.Error);
                }
            }
        }

        public class RuntimeStats
        {
            public int MessagesReceived { get; set; }
            public int CommandsRan { get; set; }
        }

        public class Connect4Lobby
        {
            public ulong ChannelID { get; set; }
            public bool Gamerunning { get; set; } = false;
        }

        public class GuildMsgx
        {
            public ulong GuildID { get; set; }
            public List<DateTime> Times { get; set; } = new List<DateTime>();
        }
    }
}