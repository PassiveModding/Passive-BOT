using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PassiveBOT.Handlers;
using PassiveBOT.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace PassiveBOT.Context
{
    public abstract class Base : ModuleBase<Context>
    {

        /// <summary>
        ///     Reply in the server. This is a shortcut for context.channel.sendmessageasync
        /// </summary>
        public async Task<IUserMessage> ReplyAsync(string Message, Embed Embed = null)
        {
            await Context.Channel.TriggerTypingAsync();
            return await base.ReplyAsync(Message, false, Embed);
        }

        /// <summary>
        ///     Reply in the server and then delete after the provided delay.
        /// </summary>
        public async Task<IUserMessage> ReplyAndDeleteAsync(string Message, TimeSpan? Timeout = null)
        {
            Timeout = Timeout ?? TimeSpan.FromSeconds(5);
            var Msg = await ReplyAsync(Message).ConfigureAwait(false);
            _ = Task.Delay(Timeout.Value).ContinueWith(_ => Msg.DeleteAsync().ConfigureAwait(false)).ConfigureAwait(false);
            return Msg;
        }

        /// <summary>
        ///     Shorthand for  replying with just an embed
        /// </summary>
        public async Task<IUserMessage> SendEmbedAsync(EmbedBuilder embed)
        {
            return await base.ReplyAsync("", false, embed.Build());
        }
        public async Task<IUserMessage> SendEmbedAsync(Embed embed)
        {
            return await base.ReplyAsync("", false, embed);
        }
    }

    public class Context : ICommandContext
    {
        public Context(IDiscordClient ClientParam, IUserMessage MessageParam, IServiceProvider ServiceProvider)
        {
            Client = ClientParam;
            Message = MessageParam;
            User = MessageParam.Author;
            Channel = MessageParam.Channel;
            Guild = MessageParam.Channel is IDMChannel ? null : (MessageParam.Channel as IGuildChannel).Guild;

            //This is a shorthand conversion for our context, giving access to socket context stuff without the need to cast within out commands
            Socket = new SocketContext
            {
                Guild = Guild as SocketGuild,
                User = User as SocketUser,
                Client = Client as DiscordSocketClient,
                Message = Message as SocketUserMessage,
                Channel = Channel as ISocketMessageChannel
            };

            //These are our custom additions to the context, giving access to the server object and all server objects through Context.
            Server = Channel is IDMChannel ? null : DatabaseHandler.GetGuild(Guild.Id);
            Session = ServiceProvider.GetRequiredService<IDocumentStore>().OpenSession();
        }

        public GuildModel Server { get; }
        public IDocumentSession Session { get; }
        public SocketContext Socket { get; }
        public IUser User { get; }
        public IGuild Guild { get; }
        public IDiscordClient Client { get; }
        public IUserMessage Message { get; }
        public IMessageChannel Channel { get; }

        public class SocketContext
        {
            public SocketUser User { get; set; }
            public SocketGuild Guild { get; set; }
            public DiscordSocketClient Client { get; set; }
            public SocketUserMessage Message { get; set; }
            public ISocketMessageChannel Channel { get; set; }
        }
    }
    
}