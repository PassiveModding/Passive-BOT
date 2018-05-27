﻿using System;
using System.Collections.Generic;
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
        public static IServiceProvider Provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        public static List<GuildMsgx> GuildMsgs { get; set; } = new List<GuildMsgx>();
        public static ConfigModel Config { get; set; } = ConfigModel.Load();
        public class GuildMsgx
        {
            public ulong GuildID { get; set; }
            public List<DateTime> Times { get; set; } = new List<DateTime>();
        }


        public CommandHandler(IServiceProvider provider)
        {
            Provider = provider;
            _client = Provider.GetService<DiscordSocketClient>();
            _commands = new CommandService();
            Config = ConfigModel.Load();

            _client.MessageReceived += DoCommand;
            _client.Ready += _client_Ready;
            _client.JoinedGuild += _client_JoinedGuild;

            //Welcome and Joined Events
            _client.UserJoined += _client_UserJoined;
            _client.UserLeft += _client_UserLeftAsync;
        }

        private async Task _client_UserLeftAsync(SocketGuildUser User)
        {
            await Discord.Extensions.EventTriggers._client_UserLeft(User);
        }

        private async Task _client_UserJoined(SocketGuildUser User)
        {
            await Discord.Extensions.EventTriggers._client_UserJoined(User);
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

        public async Task<Context> DoLevels(Context Context)
        {
            if (Context.Channel is IDMChannel)
            {
                LogHandler.LogMessage("Level Check Ignored");
                return Context;
            }
            
            if (Context.Server.Levels.Settings.Enabled)
            {
                LogHandler.LogMessage("Running Level Check");
                var luser = Context.Server.Levels.Users.FirstOrDefault(x => x.UserID == Context.User.Id);
                if (luser == null)
                {
                    Context.Server.Levels.Users.Add(new GuildModel.levelling.luser
                    {
                        Level = 1,
                        UserID = Context.User.Id,
                        XP = 0
                    });
                    luser = Context.Server.Levels.Users.FirstOrDefault(x => x.UserID == Context.User.Id);
                }

                luser.XP += 10;
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
                                if (((IGuildUser)Context.User).RoleIds.Contains(role.RoleID)) continue;
                                var grole = Context.Guild.GetRole(role.RoleID);
                                if (grole != null)
                                {
                                    try
                                    {
                                        await ((SocketGuildUser)Context.User).AddRoleAsync(grole);
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

                                    await ((SocketGuildUser)Context.User).RemoveRolesAsync(roles);
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
                    if (Context.Server.Levels.Settings.ReplyLevelUps)
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
                Context.Server.Save();
            }
            LogHandler.LogMessage("Level Check Complete");
            return Context;
        }
        private async Task DoCommand(SocketMessage parameterMessage)
        {
            if (!(parameterMessage is SocketUserMessage message)) return;
            var argPos = 0;
            var context = new Context(_client, message, Provider);
            if (context.User.IsBot) return;

            if (context.Channel is IDMChannel)
            {
                if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasStringPrefix(Config.Prefix, ref argPos))) return;
            }
            else
            {
                if (GuildMsgs.Any(x => x.GuildID == context.Guild.Id))
                {
                    var g = GuildMsgs.FirstOrDefault(x => x.GuildID == context.Guild.Id);
                    g.Times.Add(DateTime.UtcNow);
                }
                else
                {
                    GuildMsgs.Add(new GuildMsgx
                    {
                        GuildID = context.Guild.Id,
                        Times = new List<DateTime>
                        {
                            DateTime.UtcNow
                        }
                    });
                }
                context = await DoLevels(context);
                if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) && !context.Server.Settings.Prefix.DenyMentionPrefix ||
                      message.HasStringPrefix(Config.Prefix, ref argPos) && !context.Server.Settings.Prefix.DenyDefaultPrefix ||
                      context.Server.Settings.Prefix.CustomPrefix != null && message.HasStringPrefix(context.Server.Settings.Prefix.CustomPrefix, ref argPos)))
                {
                    return;
                }
            }
            

            var result = await _commands.ExecuteAsync(context, argPos, Provider);

            if (result.IsSuccess)
            {
                LogHandler.LogMessage($"{context.Message.Content}");
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