namespace PassiveBOT.Modules.GuildCommands.ServerSetup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using global::Discord;
    using global::Discord.Addons.Interactive;
    using global::Discord.Commands;

    using PassiveBOT.Discord.Context;
    using PassiveBOT.Discord.Extensions;
    using PassiveBOT.Discord.Preconditions;
    using PassiveBOT.Models;

    /// <summary>
    /// The custom channel setup.
    /// </summary>
    [RequireAdmin]
    [Group("Channel")]
    [RequireContext(ContextType.Guild)]
    public class ChannelSetup : Base
    {
        /// <summary>
        /// The media channel setup
        /// </summary>
        [Group("Media")]
        [Summary("Media Channels automatically delete all messages that do not have a URL or attachment")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public class MediaChannel : Base
        {
            /// <summary>
            /// Adds a media channel
            /// </summary>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [Command("AddChannel")]
            [Summary("Add the current channel to the media channels list")]
            public Task AddAsync()
            {
                Context.Server.CustomChannel.MediaChannels.Add(new GuildModel.CustomChannels.MediaChannel { ChannelID = Context.Channel.Id, Enabled = true, ExemptRoles = new List<ulong>() });
                Context.Server.Save();
                return SimpleEmbedAsync($"{Context.Channel.Name} is now a media channel. All messages without URLs or Attachments will be deleted");
            }

            /// <summary>
            /// Removes a media channel
            /// </summary>
            /// <param name="channel">
            /// The channel.
            /// </param>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [Command("RemoveChannel")]
            [Summary("Remove the channel from the media channels list")]
            public async Task RemoveAsync(ITextChannel channel = null)
            {
                if (channel == null)
                {
                    channel = Context.Channel as ITextChannel;
                }

                var match = Context.Server.CustomChannel.MediaChannels.FirstOrDefault(x => x.ChannelID == channel.Id);
                if (match != null)
                {
                    Context.Server.CustomChannel.MediaChannels.Remove(match);
                    Context.Server.Save();
                    await SimpleEmbedAsync($"{Context.Channel.Name} is no longer a media channel");
                }
            }

            /// <summary>
            /// Adds a media exempt role in the specified channel
            /// </summary>
            /// <param name="channel">
            /// The channel.
            /// </param>
            /// <param name="role">
            /// The exempt role.
            /// </param>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [Command("Exempt")]
            [Summary("Set a specific role to not be checked my media channel restrictions")]
            public Task MediaExemptAsync(ITextChannel channel, IRole role)
            {
                var match = Context.Server.CustomChannel.MediaChannels.FirstOrDefault(x => x.ChannelID == channel.Id);
                if (match == null)
                {
                    throw new Exception("channel is not a media channel");
                }

                match.ExemptRoles.Add(role.Id);
                Context.Server.Save();
                return SimpleEmbedAsync($"{role.Name} will no longer be checked in this media channel");
            }

            /// <summary>
            /// Adds a media exempt role in the current channel
            /// </summary>
            /// <param name="role">
            /// The role.
            /// </param>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [Command("Exempt")]
            [Summary("Set a specific role to not be checked my media channel restrictions")]
            public Task MediaExemptAsync(IRole role)
            {
                return MediaExemptAsync(Context.Channel as ITextChannel, role);
            }

            /// <summary>
            /// Deletes a media exempt role in the specified channel
            /// </summary>
            /// <param name="channel">
            /// The channel.
            /// </param>
            /// <param name="role">
            /// The exempt role.
            /// </param>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [Command("RemoveExempt")]
            [Summary("Remove a Media Exempt role in the specified channel")]
            public async Task DelMediaExemptAsync(ITextChannel channel, IRole role)
            {
                var match = Context.Server.CustomChannel.MediaChannels.FirstOrDefault(x => x.ChannelID == channel.Id);
                if (match != null)
                {
                    if (match.ExemptRoles.Contains(role.Id))
                    {
                        match.ExemptRoles.Remove(role.Id);
                        Context.Server.Save();
                        await SimpleEmbedAsync($"{role.Name} has been removed from the exempt roles");
                    }
                }
            }

            /// <summary>
            /// Deletes a media exempt role
            /// </summary>
            /// <param name="role">
            /// The role.
            /// </param>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [Command("RemoveExempt")]
            [Summary("Remove a Media Exempt role in the current channel")]
            public Task DelMediaExemptAsync(IRole role)
            {
                return DelMediaExemptAsync(Context.Channel as ITextChannel, role);
            }

            /// <summary>
            /// The list.
            /// </summary>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            /// <exception cref="Exception">
            /// Throws if no channels set up
            /// </exception>
            [Command("List")]
            [Summary("List all Media Channels in the Server")]
            public Task ListAsync()
            {
                if (!Context.Server.CustomChannel.MediaChannels.Any())
                {
                    throw new Exception("This server has no Media Channels set up.");
                }

                var list = Context.Server.CustomChannel.MediaChannels.Where(x => Context.Guild.GetChannel(x.ChannelID) != null && x.Enabled).Select(m => new EmbedFieldBuilder
                {
                    Name = $"{Context.Guild.GetChannel(m.ChannelID)}",
                    Value = $"Enabled: {m.Enabled}\n" +
                             "Exempt Roles: \n" +
                             $"{string.Join("\n", m.ExemptRoles.Select(x => Context.Guild.GetRole(x)).Where(x => x != null).Select(x => x.Mention))}"
                }).ToList();
                var pager = new PaginatedMessage
                {
                    Title = "Media Channels",
                    Pages = TextManagement.SplitList(list, 5).Select(x => new PaginatedMessage.Page
                    {
                        Fields = x
                    })
                };
                return PagedReplyAsync(pager, new ReactionList
                                                  {
                                                      Forward = true,
                                                      Backward = true,
                                                      Trash = true
                                                  });
            }
        }

        /// <summary>
        /// The auto message channel setup
        /// </summary>
        [Group("AutoMessage")]
        [Summary("AutoMessage channels will send a specific message every x messages sent in the channel.")]
        public class AutoMessage : Base
        {
            /// <summary>
            /// Toggles a media channel
            /// </summary>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [Command("Toggle")]
            [Summary("Toggle the use of Auto-Messages in the current channel")]
            public async Task ToggleAsync()
            {
                var channel = Context.Server.CustomChannel.AutoMessageChannels.FirstOrDefault(x => x.ChannelID == Context.Channel.Id);
                if (channel == null)
                {
                    Context.Server.CustomChannel.AutoMessageChannels.Add(new GuildModel.CustomChannels.AutoMessageChannel
                    {
                        ChannelID = Context.Channel.Id,
                        Enabled = true,
                        Limit = 100,
                        Count = 0
                    });
                    channel = Context.Server.CustomChannel.AutoMessageChannels.FirstOrDefault(x => x.ChannelID == Context.Channel.Id);
                }
                else
                {
                    channel.Enabled = !channel.Enabled;
                }

                await SimpleEmbedAsync($"AutoMessaging Enabled in {Context.Channel.Name}: {channel.Enabled}");
                Context.Server.Save();
            }

            /// <summary>
            /// Sets a message for the media channel
            /// </summary>
            /// <param name="message">
            /// The message.
            /// </param>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            /// <exception cref="Exception">
            /// Throws if the message is too long or channel is not enabled
            /// </exception>
            [Command("Message")]
            [Summary("Set the AutoMessage for the current channel.")]
            public async Task SetMessageAsync([Remainder] string message = null)
            {
                var channel = Context.Server.CustomChannel.AutoMessageChannels.FirstOrDefault(x => x.ChannelID == Context.Channel.Id);
                if (channel == null)
                {
                    throw new Exception("Please use the toggle command to initiate an Auto Message in this channel first.");
                }

                if (message.Length > 512)
                {
                    throw new Exception($"Auto Message character limit exceeded. Please limit it to 512 characters. Current: {message.Length}");
                }

                channel.Message = message;
                await SimpleEmbedAsync("This channel's Auto Message is now:\n" +
                                       $"{message}");
                Context.Server.Save();
            }

            /// <summary>
            /// Sets the amount of messages between auto-messages
            /// </summary>
            /// <param name="limit">
            /// The limit.
            /// </param>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            /// <exception cref="Exception">
            /// throws if the channel is not an auto-message channel
            /// </exception>
            [Command("Limit")]
            [Summary("Set number of messages between each Auto Message")]
            public async Task SetLimitAsync(int limit)
            {
                var channel = Context.Server.CustomChannel.AutoMessageChannels.FirstOrDefault(x => x.ChannelID == Context.Channel.Id);
                if (channel == null)
                {
                    throw new Exception("Please use the toggle command to initiate an Auto Message in this channel first.");
                }

                channel.Limit = limit;
                await SimpleEmbedAsync($"Auto Messages will be sent every {limit} messages in this channel");
                Context.Server.Save();
            }

            /// <summary>
            /// The list.
            /// </summary>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            /// <exception cref="Exception">
            /// throws if there are no channels
            /// </exception>
            [Command("List")]
            [Summary("List all Auto Messages in the Server")]
            public Task ListAsync()
            {
                if (!Context.Server.CustomChannel.AutoMessageChannels.Any())
                {
                    throw new Exception("This server has no Auto Messages set up.");
                }

                var list = Context.Server.CustomChannel.AutoMessageChannels.Where(x => Context.Guild.GetChannel(x.ChannelID) != null && x.Message != null && x.Enabled).Select(am => new EmbedFieldBuilder
                {
                    Name = $"{Context.Guild.GetChannel(am.ChannelID)}",
                    Value = $"Enabled: {am.Enabled}\n" +
                            $"Limit: {am.Limit}\n" +
                            $"Count: {am.Count}\n" +
                            "Message:\n" +
                            $"{am.Message ?? "N/A"}"
                }).ToList();
                var pager = new PaginatedMessage
                {
                    Title = "AutoMessage Channels",
                    Pages = TextManagement.SplitList(list, 5).Select(x => new PaginatedMessage.Page
                    {
                        Fields = x
                    })
                };
                return PagedReplyAsync(pager, new ReactionList
                                                  {
                                                      Forward = true,
                                                      Backward = true,
                                                      Trash = true
                                                  });
            }
        }
    }
}
