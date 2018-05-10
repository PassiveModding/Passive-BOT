using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBotsList.Api.Extensions.DiscordNet;
using Microsoft.Extensions.DependencyInjection;
using PassiveBOT.Configuration;

namespace PassiveBOT.Handlers
{
    public class EventHandler
    {
        private readonly List<Delays> AntiRLDelays = new List<Delays>();
        private readonly DiscordSocketClient client;

        public EventHandler(IServiceProvider provider)
        {
            Provider = provider;
            client = Provider.GetService<DiscordSocketClient>();
            client.Ready += Client_Ready;


            client.JoinedGuild += NewGuildMessage;


            client.UserJoined += WelcomeMessage;
            client.UserLeft += GoodbyeMessage;

            //user
            client.GuildMemberUpdated += Client_GuildMemberUpdated;

            client.UserLeft += UserLeftEvent;
            client.UserJoined += UserJoinedEvent;
            client.UserBanned += UserBannedEvent;
            client.UserUnbanned += UserUnbannedEvent;
            //Message
            client.MessageDeleted += MessageDeletedEvent;
            client.MessageUpdated += MessageUpdatedEvent;
            //Channel
            client.ChannelCreated += ChannelCreatedEvent;
            client.ChannelDestroyed += ChannelDeletedEvent;
            client.ChannelUpdated += ChannelUpdatedEvent;
        }

        private IServiceProvider Provider { get; }

        private async Task Client_Ready()
        {
            await client.SetGameAsync($"{Config.Load().Prefix}help / {Load.Gamesite}");
        }

        private async Task Client_GuildMemberUpdated(SocketGuildUser UserBefore, SocketGuildUser UserAfter)
        {
            var HS = Homeserver.Load();
            var logmsg = "";

            if (UserBefore.Nickname != UserAfter.Nickname)
            {
                var UserAliases = HS.Aliases.FirstOrDefault(x => x.UserID == UserBefore.Id);

                if (UserAliases == null)
                {
                    HS.Aliases.Add(new Homeserver.Alias
                    {
                        UserID = UserBefore.Id,
                        UserName = UserAfter.Username,
                        Guilds = new List<Homeserver.Alias.guild>
                        {
                            new Homeserver.Alias.guild
                            {
                                GuildID = UserBefore.Guild.Id,
                                GuildName = UserBefore.Guild.Name,
                                GuildAliases = new List<Homeserver.Alias.guild.GuildAlias>
                                {
                                    new Homeserver.Alias.guild.GuildAlias
                                    {
                                        DateChanged = DateTime.UtcNow,
                                        Name = UserAfter.Nickname ?? UserAfter.Username
                                    }
                                }
                            }
                        }
                    });
                }
                else
                {
                    var guild = UserAliases.Guilds.FirstOrDefault(x => x.GuildID == UserBefore.Guild.Id);
                    if (guild == null)
                        UserAliases.Guilds.Add(new Homeserver.Alias.guild
                        {
                            GuildName = UserBefore.Guild.Name,
                            GuildID = UserBefore.Guild.Id,
                            GuildAliases = new List<Homeserver.Alias.guild.GuildAlias>
                            {
                                new Homeserver.Alias.guild.GuildAlias
                                {
                                    DateChanged = DateTime.UtcNow,
                                    Name = UserAfter.Nickname ?? UserAfter.Username
                                }
                            }
                        });
                    else
                        guild.GuildAliases.Add(new Homeserver.Alias.guild.GuildAlias
                        {
                            DateChanged = DateTime.UtcNow,
                            Name = UserAfter.Nickname ?? UserAfter.Username
                        });
                }

                logmsg += $"__**NickName Updated**__\n" +
                          $"OLD: {UserBefore.Nickname ?? UserBefore.Username}\n" +
                          $"AFTER: {UserAfter.Nickname ?? UserAfter.Username}\n";
            }

            if (UserBefore.Roles.Count < UserAfter.Roles.Count)
            {
                var result = UserAfter.Roles.Where(b => UserBefore.Roles.All(a => b.Id != a.Id)).ToList();
                logmsg += $"__**Role Added**__\n" +
                          $"{result[0].Name}\n";
            }
            else if (UserBefore.Roles.Count > UserAfter.Roles.Count)
            {
                var result = UserBefore.Roles.Where(b => UserAfter.Roles.All(a => b.Id != a.Id)).ToList();
                logmsg += $"__**Role Removed**__\n" +
                          $"{result[0].Name}\n";
            }

            if (logmsg == "") return;
            var GuildConfig = Configuration.GuildConfig.GetServer(UserAfter.Guild);
            if (GuildConfig.EventLogging && GuildConfig.EventChannel != 0)
            {
                var embed = new EmbedBuilder
                {
                    Title = $"User Updated",
                    Description = $"**User:** {UserAfter.Mention}\n" +
                                  $"**ID:** {UserAfter.Id}\n\n" + logmsg,
                    ThumbnailUrl = UserAfter.GetAvatarUrl(),
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC"
                    },
                    Color = Color.Blue
                };
                await SendMessage(UserAfter.Guild, GuildConfig, embed);
            }

            Homeserver.SaveHome(HS);
        }

        private async Task SendMessage(SocketGuild guild, GuildConfig gobject, EmbedBuilder embed, string txt = "")
        {
            var delay = AntiRLDelays.FirstOrDefault(x => x.GuildID == guild.Id);
            if (delay != null)
            {
                if (delay._delay > DateTime.UtcNow)
                    return;
                delay._delay = DateTime.UtcNow.AddSeconds(2);
            }
            else
            {
                AntiRLDelays.Add(new Delays
                {
                    _delay = DateTime.UtcNow.AddSeconds(2),
                    GuildID = guild.Id
                });
            }

            try
            {
                if (gobject.EventChannel != 0)
                    await ((ITextChannel) guild.GetChannel(gobject.EventChannel)).SendMessageAsync(txt, false,
                        embed.Build());
            }
            catch
            {
                //
            }
        }


        private async Task MessageUpdatedEvent(Cacheable<IMessage, ulong> messageOld, SocketMessage messageNew,
            ISocketMessageChannel cchannel)
        {
            if (messageNew.Author.IsBot)
                return;

            if (string.Equals(messageOld.Value.Content, messageNew.Content, StringComparison.CurrentCultureIgnoreCase))
                return;

            if (messageOld.Value?.Embeds.Count > 0 || messageNew.Embeds.Count > 0)
                return;

            var guild = ((SocketGuildChannel) cchannel).Guild;

            var guildobj = GuildConfig.GetServer(guild);
            if (guildobj.EventLogging)
            {
                var embed = new EmbedBuilder
                {
                    Title = "Message Updated",
                    ThumbnailUrl = messageNew.Author.GetAvatarUrl(),
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME"
                    },
                    Color = Color.Blue
                };
                embed.AddField("Old Message:", $"{messageOld.Value.Content}");
                embed.AddField("New Message:", $"{messageNew.Content}");
                embed.AddField("Info",
                    $"**Author:** {messageNew.Author.Username}\n" +
                    $"**Author ID:** {messageNew.Author.Id}\n" +
                    $"**Channel:** {messageNew.Channel.Name}\n" +
                    $"**Embeds:** {messageNew.Embeds.Any()}");

                await SendMessage(guild, guildobj, embed);
            }
        }

        private async Task ChannelUpdatedEvent(SocketChannel s1, SocketChannel s2)
        {
            var ChannelBefore = s1 as SocketGuildChannel;
            var ChannelAfter = s2 as SocketGuildChannel;
            if (ChannelBefore != null)
            {
                var guild = ChannelBefore.Guild;
                var gChannel = ChannelAfter;
                var guildobj = GuildConfig.GetServer(guild);
                if (guildobj.PartnerSetup.IsPartner && !guildobj.PartnerSetup.banned &&
                    ChannelBefore.Id == guildobj.PartnerSetup.PartherChannel)
                {
                    var changes = ChannelAfter.PermissionOverwrites.Where(x => x.TargetType == PermissionTarget.Role &&
                                                                               ChannelBefore.PermissionOverwrites
                                                                                   .FirstOrDefault(y =>
                                                                                       y.TargetId == x.TargetId &&
                                                                                       (y.Permissions.AllowValue !=
                                                                                        x.Permissions.AllowValue ||
                                                                                        y.Permissions.DenyValue !=
                                                                                        x.Permissions.DenyValue))
                                                                                   .TargetId == x.TargetId).ToList();

                    var userlist = ((SocketTextChannel) s2).Users;
                    var userstring = $"Users Visible/Total Users: {userlist.Count} / {guild.Users.Count}\n" +
                                     $"Percent Visible: {(double) userlist.Count / guild.Users.Count * 100}%";

                    var homeserver = Homeserver.Load();
                    var embed = new EmbedBuilder
                    {
                        Title = $"Partner Channel Updated",
                        Description =
                            $"{(changes.Any() ? string.Join("\n", changes.Select(x => $"Role: {guild.GetRole(x.TargetId)?.Name}\n" + $"Read Messages: {x.Permissions.ReadMessages}\n" + $"Read History: {x.Permissions.ReadMessageHistory}\n")) : "No Role Permissions Detected")}\n\n" +
                            $"__**Channel Visibility**__\n" +
                            $"{userstring}\n" +
                            $"__**Server Info**__\n" +
                            $"Guild ID: {guild.Id}\n" +
                            $"Guild Name: {guild.Name}\n" +
                            $"Channel name: {ChannelAfter.Name}",
                        Color = Color.Blue,
                        Footer = new EmbedFooterBuilder
                        {
                            Text = $"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME"
                        }
                    };
                    var channel = client.GetChannel(homeserver.PartnerUpdates);
                    if (channel != null && !string.IsNullOrEmpty(embed.Description))
                        await ((ITextChannel) channel).SendMessageAsync("", false, embed.Build());
                }

                if (guildobj.EventLogging)
                {
                    if (ChannelAfter != null && ChannelBefore.Position != ChannelAfter.Position)
                        return;
                    var embed = new EmbedBuilder
                    {
                        Title = "Channel Updated",
                        Description = gChannel.Name,
                        Color = Color.Blue,
                        Footer = new EmbedFooterBuilder
                        {
                            Text = $"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME"
                        }
                    };
                    await SendMessage(guild, guildobj, embed);
                }
            }
        }

        private async Task ChannelDeletedEvent(SocketChannel sChannel)
        {
            var guild = ((SocketGuildChannel) sChannel).Guild;
            var guildobj = GuildConfig.GetServer(guild);
            if (guildobj.EventLogging)
            {
                var embed = new EmbedBuilder
                {
                    Title = "Channel Deleted",
                    Description = ((SocketGuildChannel) sChannel)?.Name,
                    Color = Color.DarkTeal,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME"
                    }
                };
                await SendMessage(guild, guildobj, embed);
            }
        }

        private async Task ChannelCreatedEvent(SocketChannel sChannel)
        {
            var guild = ((SocketGuildChannel) sChannel).Guild;
            var guildobj = GuildConfig.GetServer(guild);
            bool? mutesuccess = null;
            if (guildobj.RoleConfigurations.MutedRole != 0 && sChannel is ITextChannel channel)
            {
                var mutedrole = guild.GetRole(guildobj.RoleConfigurations.MutedRole);
                if (mutedrole != null)
                    try
                    {
                        var unverifiedPerms =
                            new OverwritePermissions(sendMessages: PermValue.Deny, addReactions: PermValue.Deny);
                        await channel.AddPermissionOverwriteAsync(mutedrole, unverifiedPerms);
                        mutesuccess = true;
                    }
                    catch
                    {
                        mutesuccess = false;
                    }
            }

            if (guildobj.EventLogging)
            {
                var desc = ((SocketGuildChannel) sChannel)?.Name;
                if (mutesuccess != null)
                    desc = $"{((SocketGuildChannel) sChannel).Name}\n\n" +
                           $"Muted Role Added: {mutesuccess}";

                var embed = new EmbedBuilder
                {
                    Title = "Channel Created",
                    Description = desc,
                    Color = Color.Green,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME"
                    }
                };
                await SendMessage(guild, guildobj, embed);
            }
        }

        private async Task MessageDeletedEvent(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            var guild = ((SocketGuildChannel) channel).Guild;
            var guildobj = GuildConfig.GetServer(guild);
            if (guildobj.EventLogging)
            {
                var embed = new EmbedBuilder();
                try
                {
                    embed.AddField("Message Deleted", $"Message: {message.Value.Content}\n" +
                                                      $"Author: {message.Value.Author}\n" +
                                                      $"Channel: {channel.Name}");
                }
                catch
                {
                    embed.AddField("Message Deleted", "Message was unable to be retrieved\n" +
                                                      $"Channel: {channel.Name}");
                }

                embed.WithFooter(x =>
                {
                    x.WithText($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME");
                });
                embed.Color = Color.DarkTeal;

                await SendMessage(guild, guildobj, embed);
            }
        }

        private async Task UserUnbannedEvent(SocketUser user, SocketGuild guild)
        {
            var guildobj = GuildConfig.GetServer(guild);
            if (guildobj.EventLogging)
            {
                var embed = new EmbedBuilder
                {
                    Title = "User Unbanned",
                    ThumbnailUrl = user.GetAvatarUrl(),
                    Description = $"**Username:** {user.Username}",
                    Color = Color.DarkTeal,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME"
                    }
                };
                await SendMessage(guild, guildobj, embed);
            }
        }

        private async Task UserBannedEvent(SocketUser user, SocketGuild guild)
        {
            var guildobj = GuildConfig.GetServer(guild);
            if (guildobj.EventLogging)
            {
                var embed = new EmbedBuilder
                {
                    Title = "User Banned",
                    ThumbnailUrl = user.GetAvatarUrl(),
                    Description = $"**Username:** {user.Username}",
                    Color = Color.Red,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC TIME"
                    }
                };

                await SendMessage(guild, guildobj, embed);
            }
        }

        private async Task UserJoinedEvent(SocketGuildUser user)
        {
            var guildobj = GuildConfig.GetServer(user.Guild);
            if (guildobj.Antispams.Antispam.antiraid)
            {
                IRole role;
                if (user.Guild.Roles.FirstOrDefault(x =>
                    string.Equals(x.Name, "PB-RAID", StringComparison.CurrentCultureIgnoreCase)) is IRole Role)
                {
                    await Role.ModifyAsync(x =>
                        x.Permissions = new GuildPermissions(readMessageHistory: true, readMessages: true));
                    role = Role;
                }
                else
                {
                    role = await user.Guild.CreateRoleAsync("PB-RAID",
                        new GuildPermissions(readMessageHistory: true, readMessages: true));
                }

                await user.AddRoleAsync(role);
            }

            if (guildobj.EventLogging)
            {
                var embed = new EmbedBuilder
                {
                    Title = "User Joined",
                    Description = $"{user.Mention} {user.Username}#{user.Discriminator}\n" +
                                  $"ID: {user.Id}",
                    ThumbnailUrl = user.GetAvatarUrl(),
                    Color = Color.Green,
                    Footer = new EmbedFooterBuilder
                        {Text = $"{DateTime.UtcNow} UTC TIME"}
                };
                await SendMessage(user.Guild, guildobj, embed);
            }
        }

        private async Task UserLeftEvent(SocketGuildUser user)
        {
            var guildobj = GuildConfig.GetServer(user.Guild);
            if (guildobj.EventLogging)
            {
                var embed = new EmbedBuilder
                {
                    Title = "User Left",
                    Description = $"{user.Mention} {user.Username}#{user.Discriminator}\n" +
                                  $"ID: {user.Id}",
                    ThumbnailUrl = user.GetAvatarUrl(),
                    Color = Color.Red,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"{DateTime.UtcNow} UTC TIME"
                    }
                };
                await SendMessage(user.Guild, guildobj, embed);
            }
        }

        private static async Task WelcomeMessage(SocketGuildUser user)
        {
            var guildobj = GuildConfig.GetServer(user.Guild);
            if (!guildobj.WelcomeEvent) return;
            var wmessage = guildobj.WelcomeMessage;
            if (wmessage == null) return;
            var embed = new EmbedBuilder
            {
                Title = $"Welcome {user.Username}",
                ThumbnailUrl = user.GetAvatarUrl(),
                Description = wmessage,
                Color = Color.Blue,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Users: {user.Guild.MemberCount}"
                }
            };
            if (guildobj.WelcomeChannel != 0)
            {
                var channel = user.Guild.GetTextChannel(guildobj.WelcomeChannel);
                await channel.SendMessageAsync($"{user.Mention}", false, embed.Build());
            }
            else
            {
                await user.Guild.DefaultChannel.SendMessageAsync($"{user.Mention}", false, embed.Build());
            }
        }

        private async Task GoodbyeMessage(SocketGuildUser user)
        {
            var guildobj = GuildConfig.GetServer(user.Guild);
            if (!guildobj.GoodbyeEvent) return;
            var embed = new EmbedBuilder
            {
                Title = $"Goodbye {user.Username}",
                ThumbnailUrl = user.GetAvatarUrl(),
                Description = guildobj.GoodbyeMessage
            };
            if (guildobj.GoodByeChannel != 0)
                await user.Guild.GetTextChannel(guildobj.GoodByeChannel).SendMessageAsync($"", false, embed.Build());
            else
                await user.Guild.DefaultChannel.SendMessageAsync($"", false, embed.Build());
        }

        private async Task NewGuildMessage(SocketGuild guild)
        {
            var config = Path.Combine(AppContext.BaseDirectory + $"setup/server/{guild.Id}.json");
            if (!File.Exists(config))
                GuildConfig.Setup(guild);

            await guild.DefaultChannel.SendMessageAsync(
                $"Hi, I'm PassiveBOT. To see a list of my commands type `{Load.Pre}help` and for some statistics about me type `{Load.Pre}info`\n" +
                "I am able to do tags, moderation, memes & more!!!!!");

            try
            {
                var DblApi = new DiscordNetDblApi(client, Tokens.Load().DiscordBotsListToken);
                var me = await DblApi.GetMeAsync();
                await me.UpdateStatsAsync(client.Guilds.Count);
            }
            catch
            {
                //
            }
        }

        public class Delays
        {
            public DateTime _delay { get; set; } = DateTime.UtcNow;
            public ulong GuildID { get; set; }
        }
    }
}