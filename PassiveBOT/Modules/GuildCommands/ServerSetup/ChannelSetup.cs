namespace PassiveBOT.Modules.GuildCommands.ServerSetup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Addons.Interactive;
    using Discord.Commands;

    using PassiveBOT.Context;
    using PassiveBOT.Extensions;
    using PassiveBOT.Preconditions;
    using PassiveBOT.Services;

    /// <summary>
    ///     The custom channel setup.
    /// </summary>
    [RequireAdmin]
    [Group("Channel")]
    [RequireContext(ContextType.Guild)]
    public class ChannelSetup : Base
    {
        public ChannelSetup(ChannelService service)
        {
            Service = service;
        }

        private static ChannelService Service { get; set; }

        /// <summary>
        ///     The auto message channel setup
        /// </summary>
        [Group("AutoMessage")]
        [Summary("AutoMessage channels will send a specific message every x messages sent in the channel.")]
        public class AutoMessage : Base
        {
            /// <summary>
            ///     The list.
            /// </summary>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            /// <exception cref="Exception">
            ///     throws if there are no channels
            /// </exception>
            [Command("List", RunMode = RunMode.Async)]
            [Summary("List all Auto Messages in the Server")]
            public Task ListAsync()
            {
                var c = Service.GetCustomChannels(Context.Guild.Id);
                if (!c.AutoMessageChannels.Any())
                {
                    throw new Exception("This server has no Auto Messages set up.");
                }

                var list = c.AutoMessageChannels.Where(x => Context.Guild.GetChannel(x.Key) != null && x.Value.Message != null && x.Value.Enabled).Select(am => new EmbedFieldBuilder { Name = $"{Context.Guild.GetChannel(am.Key)}", Value = $"Enabled: {am.Value.Enabled}\n" + $"Limit: {am.Value.Limit}\n" + $"Count: {am.Value.Count}\n" + "Message:\n" + $"{am.Value.Message ?? "N/A"}" }).ToList();
                var pager = new PaginatedMessage { Title = "AutoMessage Channels", Pages = list.SplitList(5).Select(x => new PaginatedMessage.Page { Fields = x }) };
                return PagedReplyAsync(pager, new ReactionList { Forward = true, Backward = true, Trash = true });
            }

            /// <summary>
            ///     Sets the amount of messages between auto-messages
            /// </summary>
            /// <param name="limit">
            ///     The limit.
            /// </param>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            /// <exception cref="Exception">
            ///     throws if the channel is not an auto-message channel
            /// </exception>
            [Command("Limit")]
            [Summary("Set number of messages between each Auto Message")]
            public Task SetLimitAsync(int limit)
            {
                var c = Service.GetCustomChannels(Context.Guild.Id);
                if (!c.AutoMessageChannels.ContainsKey(Context.Channel.Id))
                {
                    throw new Exception("Please use the toggle command to initiate an Auto Message in this channel first.");
                }

                c.AutoMessageChannels[Context.Channel.Id].Limit = limit;
                c.Save();
                return SimpleEmbedAsync($"Auto Messages will be sent every {limit} messages in this channel");
            }

            /// <summary>
            ///     Sets a message for the media channel
            /// </summary>
            /// <param name="message">
            ///     The message.
            /// </param>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            /// <exception cref="Exception">
            ///     Throws if the message is too long or channel is not enabled
            /// </exception>
            [Command("Message")]
            [Summary("Set the AutoMessage for the current channel.")]
            public Task SetMessageAsync([Remainder] string message = null)
            {
                var c = Service.GetCustomChannels(Context.Guild.Id);
                if (!c.AutoMessageChannels.ContainsKey(Context.Channel.Id))
                {
                    throw new Exception("Please use the toggle command to initiate an Auto Message in this channel first.");
                }

                if (message.Length > 512)
                {
                    throw new Exception($"Auto Message character limit exceeded. Please limit it to 512 characters. Current: {message.Length}");
                }

                c.AutoMessageChannels[Context.Channel.Id].Message = message;
                c.Save();
                return SimpleEmbedAsync("This channel's Auto Message is now:\n" + $"{message}");
            }

            /// <summary>
            ///     Toggles a media channel
            /// </summary>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            [Command("Toggle")]
            [Summary("Toggle the use of Auto-Messages in the current channel")]
            public Task ToggleAsync()
            {
                var c = Service.GetCustomChannels(Context.Guild.Id);
                if (!c.AutoMessageChannels.ContainsKey(Context.Channel.Id))
                {
                    var newChan = new ChannelService.CustomChannels.AutoMessageChannel { ChannelID = Context.Channel.Id, Enabled = true, Limit = 100, Count = 0 };
                    c.AutoMessageChannels.TryAdd(Context.Channel.Id, newChan);
                }
                else
                {
                    c.AutoMessageChannels[Context.Channel.Id].Enabled = !c.AutoMessageChannels[Context.Channel.Id].Enabled;
                }

                c.Save();
                return SimpleEmbedAsync($"AutoMessaging Enabled in {Context.Channel.Name}: {c.AutoMessageChannels[Context.Channel.Id].Enabled}");
            }
        }

        /// <summary>
        ///     The media channel setup
        /// </summary>
        [Group("Media")]
        [Summary("Media Channels automatically delete all messages that do not have a URL or attachment")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public class MediaChannel : Base
        {
            /// <summary>
            ///     Adds a media channel
            /// </summary>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            [Command("AddChannel")]
            [Summary("Add the current channel to the media channels list")]
            public Task AddAsync()
            {
                var c = Service.GetCustomChannels(Context.Guild.Id);
                c.MediaChannels.TryAdd(Context.Channel.Id, new ChannelService.CustomChannels.MediaChannel { ChannelID = Context.Channel.Id, Enabled = true, ExemptRoles = new List<ulong>() });
                c.Save();
                return SimpleEmbedAsync($"{Context.Channel.Name} is now a media channel. All messages without URLs or Attachments will be deleted");
            }

            /// <summary>
            ///     Deletes a media exempt role in the specified channel
            /// </summary>
            /// <param name="channel">
            ///     The channel.
            /// </param>
            /// <param name="role">
            ///     The exempt role.
            /// </param>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            [Command("RemoveExempt")]
            [Summary("Remove a Media Exempt role in the specified channel")]
            public async Task DelMediaExemptAsync(ITextChannel channel, IRole role)
            {
                var c = Service.GetCustomChannels(Context.Guild.Id);
                if (c.MediaChannels.ContainsKey(channel.Id))
                {
                    if (c.MediaChannels[channel.Id].ExemptRoles.Contains(role.Id))
                    {
                        c.MediaChannels[channel.Id].ExemptRoles.Remove(role.Id);
                        c.Save();
                        await SimpleEmbedAsync($"{role.Name} has been removed from the exempt roles");
                    }
                }
            }

            /// <summary>
            ///     Deletes a media exempt role
            /// </summary>
            /// <param name="role">
            ///     The role.
            /// </param>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            [Command("RemoveExempt")]
            [Summary("Remove a Media Exempt role in the current channel")]
            public Task DelMediaExemptAsync(IRole role)
            {
                return DelMediaExemptAsync(Context.Channel as ITextChannel, role);
            }

            /// <summary>
            ///     The list.
            /// </summary>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            /// <exception cref="Exception">
            ///     Throws if no channels set up
            /// </exception>
            [Command("List", RunMode = RunMode.Async)]
            [Summary("List all Media Channels in the Server")]
            public Task ListAsync()
            {
                var c = Service.GetCustomChannels(Context.Guild.Id);
                if (!c.MediaChannels.Any())
                {
                    throw new Exception("This server has no Media Channels set up.");
                }

                var list = c.MediaChannels.Where(x => Context.Guild.GetChannel(x.Key) != null && x.Value.Enabled).Select(m => new EmbedFieldBuilder { Name = $"{Context.Guild.GetChannel(m.Key)}", Value = $"Enabled: {m.Value.Enabled}\n" + "Exempt Roles: \n" + $"{string.Join("\n", m.Value.ExemptRoles.Select(x => Context.Guild.GetRole(x)).Where(x => x != null).Select(x => x.Mention))}" }).ToList();
                var pager = new PaginatedMessage { Title = "Media Channels", Pages = list.SplitList(5).Select(x => new PaginatedMessage.Page { Fields = x }) };
                return PagedReplyAsync(pager, new ReactionList { Forward = true, Backward = true, Trash = true });
            }

            /// <summary>
            ///     Adds a media exempt role in the specified channel
            /// </summary>
            /// <param name="channel">
            ///     The channel.
            /// </param>
            /// <param name="role">
            ///     The exempt role.
            /// </param>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            [Command("Exempt")]
            [Summary("Set a specific role to not be checked my media channel restrictions")]
            public Task MediaExemptAsync(ITextChannel channel, IRole role)
            {
                var c = Service.GetCustomChannels(Context.Guild.Id);
                if (!c.MediaChannels.ContainsKey(channel.Id))
                {
                    throw new Exception("channel is not a media channel");
                }

                c.MediaChannels[channel.Id].ExemptRoles.Add(role.Id);
                c.Save();
                return SimpleEmbedAsync($"{role.Name} will no longer be checked in this media channel");
            }

            /// <summary>
            ///     Adds a media exempt role in the current channel
            /// </summary>
            /// <param name="role">
            ///     The role.
            /// </param>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            [Command("Exempt")]
            [Summary("Set a specific role to not be checked my media channel restrictions")]
            public Task MediaExemptAsync(IRole role)
            {
                return MediaExemptAsync(Context.Channel as ITextChannel, role);
            }

            /// <summary>
            ///     Removes a media channel
            /// </summary>
            /// <param name="channel">
            ///     The channel.
            /// </param>
            /// <returns>
            ///     The <see cref="Task" />.
            /// </returns>
            [Command("RemoveChannel")]
            [Summary("Remove the channel from the media channels list")]
            public async Task RemoveAsync(ITextChannel channel = null)
            {
                if (channel == null)
                {
                    channel = Context.Channel as ITextChannel;
                }

                var c = Service.GetCustomChannels(Context.Guild.Id);
                if (c.MediaChannels.ContainsKey(channel.Id))
                {
                    c.MediaChannels.TryRemove(channel.Id, out _);
                    c.Save();
                    await SimpleEmbedAsync($"{channel.Name} is no longer a media channel");
                }
            }
        }
    }
}