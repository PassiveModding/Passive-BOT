using Discord.WebSocket;
using System;
using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PassiveBOT.Handlers;

namespace PassiveBOT.Services
{
    public class PaginationService
    {
        const string FIRST = "⏮";
        const string BACK = "◀";
        const string NEXT = "▶";
        const string END = "⏭";
        const string STOP = "⏹";

        private readonly Dictionary<ulong, PaginatedMessage> _messages;
        private readonly DiscordSocketClient _client;

        public PaginationService(DiscordSocketClient client)
        {
            LogHandler.LogAsync("New Page Service");
            _messages = new Dictionary<ulong, PaginatedMessage>();
            _client = client;
            client.ReactionAdded += OnReactionAdded;
            LogHandler.LogAsync("Reaction Hooked");
        }

        public async Task<IUserMessage> SendPaginatedMessageAsync(IMessageChannel channel, PaginatedMessage paginated)
        {
            await LogHandler.LogAsync($"Sending to {channel}");

            var message = await channel.SendMessageAsync("", embed: paginated.GetEmbed());

            await message.AddReactionAsync(new Emoji(FIRST));
            await message.AddReactionAsync(new Emoji(BACK));
            await message.AddReactionAsync(new Emoji(NEXT));
            await message.AddReactionAsync(new Emoji(END));
            await message.AddReactionAsync(new Emoji(STOP));

            _messages.Add(message.Id, paginated);
            await LogHandler.LogAsync("Listening to Pages Message");

            return message;
        }

        internal async Task OnReactionAdded(Cacheable<IUserMessage, ulong> messageParam, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var message = await messageParam.GetOrDownloadAsync();
            if (message == null)
            {
                await LogHandler.LogAsync($"Dumped message (not in cache) with id {reaction.MessageId}");
                return;
            }
            if (!reaction.User.IsSpecified)
            {
                await LogHandler.LogAsync($"Dumped message (invalid user) with id {message.Id}");
                return;
            }
            if (_messages.TryGetValue(message.Id, out PaginatedMessage page))
            {
                if (reaction.UserId == _client.CurrentUser.Id) return;
                if (page.User != null && reaction.UserId != page.User.Id)
                {
                    await LogHandler.LogAsync($"ignoring reaction from user {reaction.UserId}");
                    var _ = message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                    return;
                }
                await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                await LogHandler.LogAsync($"handling reaction");
                switch (reaction.Emote.Name)
                {
                    case FIRST:
                        if (page.CurrentPage == 1) break;
                        page.CurrentPage = 1;
                        await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                        break;
                    case BACK:
                        if (page.CurrentPage == 1) break;
                        page.CurrentPage--;
                        await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                        break;
                    case NEXT:
                        if (page.CurrentPage == page.Count) break;
                        page.CurrentPage++;
                        await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                        break;
                    case END:
                        if (page.CurrentPage == page.Count) break;
                        page.CurrentPage = page.Count;
                        await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                        break;
                    case STOP:
                        await message.DeleteAsync();
                        _messages.Remove(message.Id);
                        return;
                    default:
                        break;
                }
            }
        }
    }

    public class PaginatedMessage
    {
        public PaginatedMessage(IReadOnlyCollection<string> pages, string title = "", Color? embedColor = null, IUser user = null)
        {
            Pages = pages;
            Title = title;
            EmbedColor = embedColor ?? Color.Default;
            User = user;
            CurrentPage = 1;
        }

        internal Embed GetEmbed()
        {
            return new EmbedBuilder()
                .WithColor(EmbedColor)
                .WithTitle(Title)
                .WithDescription(Pages.ElementAtOrDefault(CurrentPage - 1) ?? "")
                .WithFooter(footer =>
                {
                    footer.Text = $"Page {CurrentPage}/{Count}";
                })
                .Build();
        }

        internal string Title { get; }
        internal Color EmbedColor { get; } 
        internal IReadOnlyCollection<string> Pages { get; }
        internal IUser User { get; }
        internal int CurrentPage { get; set; }
        internal int Count => Pages.Count;
    }
}
