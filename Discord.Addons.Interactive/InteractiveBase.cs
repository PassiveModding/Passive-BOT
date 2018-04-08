using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Discord.Addons.Interactive.Criteria;
using PassiveBOT.Discord.Addons.Interactive.Paginator;
using PassiveBOT.Discord.Addons.Interactive.Results;

namespace PassiveBOT.Discord.Addons.Interactive
{
    public class InteractiveBase : InteractiveBase<SocketCommandContext>
    {
    }

    public class InteractiveBase<T> : ModuleBase<T>
        where T : SocketCommandContext
    {
        public InteractiveService Interactive { get; set; }

        public Task<SocketMessage> NextMessageAsync(ICriterion<SocketMessage> criterion, TimeSpan? timeout = null)
        {
            return Interactive.NextMessageAsync(Context, criterion, timeout);
        }

        public Task<SocketMessage> NextMessageAsync(bool fromSourceUser = true, bool inSourceChannel = true,
            TimeSpan? timeout = null)
        {
            return Interactive.NextMessageAsync(Context, fromSourceUser, inSourceChannel, timeout);
        }

        public Task<IUserMessage> ReplyAndDeleteAsync(string content, bool isTTS = false, Embed embed = null,
            TimeSpan? timeout = null, RequestOptions options = null)
        {
            return Interactive.ReplyAndDeleteAsync(Context, content, isTTS, embed, timeout, options);
        }

        public Task<IUserMessage> PagedReplyAsync(IEnumerable<PaginatedMessage.Page> pages, bool fromSourceUser = true)
        {
            var pager = new PaginatedMessage
            {
                Pages = pages
            };
            return PagedReplyAsync(pager, fromSourceUser);
        }

        public Task<IUserMessage> PagedReplyAsync(PaginatedMessage pager, bool fromSourceUser = true)
        {
            var criterion = new Criteria<SocketReaction>();
            if (fromSourceUser)
                criterion.AddCriterion(new EnsureReactionFromSourceUserCriterion());
            return PagedReplyAsync(pager, criterion);
        }

        public Task<IUserMessage> PagedReplyAsync(PaginatedMessage pager, ICriterion<SocketReaction> criterion)
        {
            return Interactive.SendPaginatedMessageAsync(Context, pager, criterion);
        }

        public RuntimeResult Ok(string reason = null)
        {
            return new OkResult(reason);
        }
    }
}