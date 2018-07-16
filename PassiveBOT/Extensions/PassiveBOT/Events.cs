namespace PassiveBOT.Extensions.PassiveBOT
{
    using System.Threading.Tasks;

    using Discord;
    using Discord.WebSocket;

    using global::PassiveBOT.Models;

    /// <summary>
    ///     The events.
    /// </summary>
    public class Events
    {
        /// <summary>
        ///     The user joined event
        /// </summary>
        /// <param name="guildModel">
        ///     The guild Model.
        /// </param>
        /// <param name="user">
        ///     The user.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        public static async Task UserJoinedAsync(GuildModel guildModel, SocketGuildUser user)
        {
            if (guildModel.Events.Welcome.Enabled)
            {
                var welcomeEmbed = new EmbedBuilder { Title = $"Welcome to {user.Guild.Name}, {user}", Description = $"{guildModel.Events.Welcome.Message}", Color = Color.Green };

                if (guildModel.Events.Welcome.UserCount)
                {
                    welcomeEmbed.Footer = new EmbedFooterBuilder { Text = $"Users: {user.Guild.MemberCount}" };
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
        ///     The user left event
        /// </summary>
        /// <param name="guildModel">
        ///     The guild Model.
        /// </param>
        /// <param name="user">
        ///     The user.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        public static async Task UserLeftAsync(GuildModel guildModel, SocketGuildUser user)
        {
            if (guildModel.Events.Goodbye.Enabled)
            {
                var goodbyeEmbed = new EmbedBuilder { Title = $"{user} has left the server", Description = $"{guildModel.Events.Goodbye.Message}" };

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