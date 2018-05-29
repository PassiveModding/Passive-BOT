using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Discord.Context;
using PassiveBOT.Discord.Context.Interactive.Paginator;
using PassiveBOT.Discord.Extensions;
using PassiveBOT.Discord.Preconditions;
using PassiveBOT.Models;

namespace PassiveBOT.Modules.GuildSetup
{
    [RequireAdmin]
    [RequireContext(ContextType.Guild)]
    [Group("AutoMessage")]
    public class AutoMessage : Base
    {
        [Command("Toggle", RunMode = RunMode.Async)]
        [Summary("Toggle")]
        [Remarks("Toggle the use of automessages in the current channel")]
        public async Task Toggle()
        {
            var AMChannel = Context.Server.AutoMessage.AutoMessageChannels.FirstOrDefault(x => x.ChannelID == Context.Channel.Id);
            if (AMChannel == null)
            {
                Context.Server.AutoMessage.AutoMessageChannels.Add(new GuildModel.autoMessage.amChannel
                {
                    ChannelID = Context.Channel.Id,
                    Enabled = true,
                    Limit = 100,
                    Count = 0
                });
                AMChannel = Context.Server.AutoMessage.AutoMessageChannels.FirstOrDefault(x => x.ChannelID == Context.Channel.Id);
            }
            else
            {
                AMChannel.Enabled = !AMChannel.Enabled;
            }

            await SimpleEmbedAsync($"AutoMessaging Enabled in {Context.Channel.Name}: {AMChannel.Enabled}");
            Context.Server.Save();
        }

        [Command("Message", RunMode = RunMode.Async)]
        [Summary("Message <Message>")]
        [Remarks("Set the AutoMessage for the current channel.")]
        public async Task SetMessage([Remainder] string message = null)
        {
            var AMChannel = Context.Server.AutoMessage.AutoMessageChannels.FirstOrDefault(x => x.ChannelID == Context.Channel.Id);
            if (AMChannel == null)
            {
                throw new Exception("Please use the toggle command to initiate an automessage in this channel first.");
            }

            if (message.Length > 512)
            {
                throw new Exception($"Automessage character limit exceeded. Please limit it to 512 characters. Current: {message.Length}");
            }

            AMChannel.Message = message;
            await SimpleEmbedAsync("This channel's automessage is now:\n" +
                                   $"{message}");
            Context.Server.Save();
        }

        [Command("Limit", RunMode = RunMode.Async)]
        [Summary("Limit <Limit>")]
        [Remarks("Set number of messages between each automessage")]
        public async Task SetLimit(int limit)
        {
            var AMChannel = Context.Server.AutoMessage.AutoMessageChannels.FirstOrDefault(x => x.ChannelID == Context.Channel.Id);
            if (AMChannel == null)
            {
                throw new Exception("Please use the toggle command to initiate an automessage in this channel first.");
            }

            AMChannel.Limit = limit;
            await SimpleEmbedAsync($"Automessages will be sent every {limit} messages in this channel");
            Context.Server.Save();
        }

        [Command("List", RunMode = RunMode.Async)]
        [Summary("List")]
        [Remarks("List all automessages in the Server")]
        public async Task List()
        {
            if (!Context.Server.AutoMessage.AutoMessageChannels.Any())
            {
                throw new Exception("This server has no automessages set up.");
            }

            var msglist = Context.Server.AutoMessage.AutoMessageChannels.Where(x => Context.Socket.Guild.GetChannel(x.ChannelID) != null && x.Message != null && x.Enabled).Select(am => new EmbedFieldBuilder
            {
                Name = $"{Context.Socket.Guild.GetChannel(am.ChannelID)}",
                Value = $"Enabled: {am.Enabled}\n" +
                        $"Limit: {am.Limit}\n" +
                        $"Count: {am.Count}\n" +
                        $"Message:\n" +
                        $"{am.Message ?? "N/A"}"
            }).ToList();
            var pager = new PaginatedMessage
            {
                Title = $"AutoMessage Channels",
                Pages = TextManagement.splitList(msglist, 5).Select(x => new PaginatedMessage.Page
                {
                    Fields = x
                })
            };
            await PagedReplyAsync(pager);
        }
    }
}