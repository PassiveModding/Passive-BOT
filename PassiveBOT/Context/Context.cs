namespace PassiveBOT.Context
{
    using System;

    using Discord.Commands;
    using Discord.WebSocket;

    using Microsoft.Extensions.DependencyInjection;

    using PassiveBOT.Handlers;
    using PassiveBOT.Models;

    /// <summary>
    ///     The context.
    /// </summary>
    public class Context : ShardedCommandContext
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Context" /> class.
        /// </summary>
        /// <param name="client">
        ///     The client param.
        /// </param>
        /// <param name="message">
        ///     The Message param.
        /// </param>
        /// <param name="serviceProvider">
        ///     The service provider.
        /// </param>
        public Context(DiscordShardedClient client, SocketUserMessage message, IServiceProvider serviceProvider)
            : base(client, message)
        {
            // These are our custom additions to the context, giving access to the server object and all server objects through Context.
            Server = serviceProvider.GetRequiredService<DatabaseHandler>().Execute<GuildModel>(DatabaseHandler.Operation.LOAD, null, Guild.Id);
            Provider = serviceProvider;
        }

        /// <summary>
        ///     Gets the provider.
        /// </summary>
        public IServiceProvider Provider { get; }

        /// <summary>
        ///     Gets the server.
        /// </summary>
        public GuildModel Server { get; }
    }
}