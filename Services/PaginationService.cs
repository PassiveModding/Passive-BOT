using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PassiveBOT.Handlers;

namespace PassiveBOT.Services
{
    public class PaginationService
    {
        private const string First = "⏮";
        private const string Back = "◀";
        private const string Next = "▶";
        private const string End = "⏭";
        private const string Stop = "⏹";
        private readonly DiscordSocketClient _client;

        private readonly Dictionary<ulong, PaginatedMessage> _messages;

        public PaginationService(DiscordSocketClient client)
        {
            ColourLog.ColourInfo("Paginator       | New Service             |");
            _messages = new Dictionary<ulong, PaginatedMessage>();
            _client = client;
            client.ReactionAdded += OnReactionAdded;
            ColourLog.ColourInfo("Paginator       | Reaction Hooked         |");
        }

        public async Task<IUserMessage> SendPaginatedMessageAsync(IMessageChannel channel, PaginatedMessage paginated)
        {
            await ColourLog.ColourInfo($"Paginator       | Message Sent            | Channel: {channel}");

            var message = await channel.SendMessageAsync("", embed: paginated.GetEmbed());

            await message.AddReactionAsync(new Emoji(First));
            await message.AddReactionAsync(new Emoji(Back));
            await message.AddReactionAsync(new Emoji(Next));
            await message.AddReactionAsync(new Emoji(End));
            await message.AddReactionAsync(new Emoji(Stop));

            _messages.Add(message.Id, paginated);
            await ColourLog.ColourInfo($"Paginator       | Message Listening       | Channel: {channel}");

            return message;
        }

        internal async Task OnReactionAdded(Cacheable<IUserMessage, ulong> messageParam, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            var message = await messageParam.GetOrDownloadAsync();
            if (message == null)
            {
                await ColourLog.ColourInfo($"Paginator       | Message Dumped          | User: {reaction.MessageId}");
                return;
            }
            if (!reaction.User.IsSpecified)
            {
                await ColourLog.ColourInfo($"Paginator       | Reaction Invalid        | User: {message.Id}");
                return;
            }
            if (_messages.TryGetValue(message.Id, out PaginatedMessage page))
            {
                if (reaction.UserId == _client.CurrentUser.Id) return;
                if (page.User != null && reaction.UserId != page.User.Id)
                {
                    await ColourLog.ColourInfo($"Paginator       | Reaction Ignored        | User: {reaction.UserId}");
                    var _ = message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                    return;
                }
                await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                await ColourLog.ColourInfo(
                    $"Paginator       | Reaction Handled        | Message: {reaction.MessageId}");
                switch (reaction.Emote.Name)
                {
                    case First:
                        if (page.CurrentPage == 1) break;
                        page.CurrentPage = 1;
                        await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                        break;
                    case Back:
                        if (page.CurrentPage == 1) break;
                        page.CurrentPage--;
                        await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                        break;
                    case Next:
                        if (page.CurrentPage == page.Count) break;
                        page.CurrentPage++;
                        await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                        break;
                    case End:
                        if (page.CurrentPage == page.Count) break;
                        page.CurrentPage = page.Count;
                        await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                        break;
                    case Stop:
                        await message.DeleteAsync();
                        _messages.Remove(message.Id);
                        return;
                }
            }
        }
    }

    public class PaginatedMessage
    {
        public PaginatedMessage(IReadOnlyCollection<string> pages, string title = "", Color? embedColor = null,
            IUser user = null)
        {
            Pages = pages;
            Title = title;
            EmbedColor = embedColor ?? Color.Default;
            User = user;
            CurrentPage = 1;
        }

        internal string Title { get; }
        internal Color EmbedColor { get; }
        internal IReadOnlyCollection<string> Pages { get; }
        internal IUser User { get; }
        internal int CurrentPage { get; set; }
        internal int Count => Pages.Count;

        internal Embed GetEmbed()
        {
            return new EmbedBuilder()
                .WithColor(EmbedColor)
                .WithTitle(Title)
                .WithDescription(Pages.ElementAtOrDefault(CurrentPage - 1) ?? "")
                .WithFooter(footer => { footer.Text = $"Page {CurrentPage}/{Count}"; })
                .Build();
        }
    }
}