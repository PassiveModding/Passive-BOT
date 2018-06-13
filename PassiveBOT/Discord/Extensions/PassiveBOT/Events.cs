namespace PassiveBOT.Discord.Extensions.PassiveBOT
{
    using System;
    using System.Threading.Tasks;

    using global::Discord;
    using global::Discord.WebSocket;

    using global::PassiveBOT.Handlers;
    using global::PassiveBOT.Models;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// The events.
    /// </summary>
    public class Events
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Events"/> class.
        /// </summary>
        /// <param name="database">
        /// The database.
        /// </param>
        private Events(IServiceProvider database)
        {
            Database = database;
        }

        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        public static IServiceProvider Database { get; set; }

        /// <summary>
        /// The user joined event
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static async Task UserJoined(GuildModel guildModel, SocketGuildUser user)
        {
            if (guildModel.Events.Welcome.Enabled)
            {
                var welcomeEmbed = new EmbedBuilder
                {
                    Title = $"Welcome to {user.Guild.Name}, {user}",
                    Description = $"{guildModel.Events.Welcome.Message}",
                    Color = Color.Green
                };

                if (guildModel.Events.Welcome.UserCount)
                {
                    welcomeEmbed.Footer = new EmbedFooterBuilder
                    {
                        Text = $"Users: {user.Guild.MemberCount}"
                    };
                }

                if (guildModel.Events.Welcome.SendDMs)
                {
                    try
                    {
                        await user.SendMessageAsync(user.Mention, false, welcomeEmbed.Build());
                    }
                    catch
                    {
                        // ignored
                    }
                }

                if (user.Guild.GetChannel(guildModel.Events.Welcome.ChannelID) is ITextChannel WChannel)
                {
                    try
                    {
                        await WChannel.SendMessageAsync(user.Mention, false, welcomeEmbed.Build());
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        /// <summary>
        /// The user left event
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static async Task UserLeft(GuildModel guildModel, SocketGuildUser user)
        {
            if (guildModel.Events.Goodbye.Enabled)
            {
                var goodbyeEmbed = new EmbedBuilder
                {
                    Title = $"{user} has left the server",
                    Description = $"{guildModel.Events.Goodbye.Message}"
                };

                if (user.Guild.GetChannel(guildModel.Events.Goodbye.ChannelID) is ITextChannel GChannel)
                {
                    try
                    {
                        await GChannel.SendMessageAsync(user.Mention, false, goodbyeEmbed.Build());
                    }
                    catch
                    {
                        // Ignored
                    }
                }
            }
        }
    }
}