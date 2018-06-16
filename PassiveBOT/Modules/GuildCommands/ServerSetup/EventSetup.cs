namespace PassiveBOT.Modules.GuildCommands.ServerSetup
{
    using System.Threading.Tasks;

    using global::Discord.Commands;

    using PassiveBOT.Discord.Context;
    using PassiveBOT.Discord.Preconditions;

    /// <summary>
    /// The event setup.
    /// </summary>
    [Group]
    [RequireAdmin]
    [RequireContext(ContextType.Guild)]
    public class EventSetup : Base
    {
        /// <summary>
        /// The welcome event setup
        /// </summary>
        [Group("Welcome")]
        public class Welcome : Base
        {
            /// <summary>
            /// The welcome info.
            /// </summary>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [Command("Info")]
            [Summary("Display welcome setup info")]
            public async Task WelcomeInfo()
            {
                await SimpleEmbedAsync("**Welcome Event**\n" +
                                       $"`{Context.Prefix}Welcome Info` - This Message\n" +
                                       $"`{Context.Prefix}Welcome Toggle` - Toggle the welcome event\n" +
                                       $"`{Context.Prefix}Welcome SetChannel` - Set the channel where welcome events will be sent\n" +
                                       $"`{Context.Prefix}Welcome Message <Message>` - Set the Welcome Message\n\n" +
                                       "**Welcome Settings**\n" +
                                       $"Enabled: {Context.Server.Events.Welcome.Enabled}\n" +
                                       $"Channel: {Context.Guild.GetChannel(Context.Server.Events.Welcome.ChannelID)?.Name ?? "N/A"}\n" +
                                       "Message:\n" +
                                       $"{Context.Server.Events.Welcome.Message ?? "N/A"}");
            }

            /// <summary>
            /// Toggles the welcome event
            /// </summary>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [Command("Toggle")]
            [Summary("Toggle the welcome event in the server")]
            public async Task Toggle()
            {
                Context.Server.Events.Welcome.Enabled = !Context.Server.Events.Welcome.Enabled;
                Context.Server.Save();
                await SimpleEmbedAsync($"Welcome Event: {Context.Server.Events.Welcome.Enabled}");
            }

            /// <summary>
            /// Toggles whether to DM welcome messages
            /// </summary>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [Command("Direct")]
            [Summary("Direct direct messaging of welcome messages")]
            public async Task DmTask()
            {
                Context.Server.Events.Welcome.SendDMs = !Context.Server.Events.Welcome.SendDMs;
                Context.Server.Save();
                await SimpleEmbedAsync($"Users will be private messaged the welcome message upon joining the server: {Context.Server.Events.Welcome.SendDMs}");
            }

            /// <summary>
            /// Sets the welcome message channel
            /// </summary>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [Command("SetChannel")]
            [Summary("Set the channel welcome events will be sent to")]
            public async Task SetChannel()
            {
                Context.Server.Events.Welcome.ChannelID = Context.Channel.Id;
                Context.Server.Save();
                await SimpleEmbedAsync($"Success, Welcome messages will now be sent in the channel: {Context.Channel.Name}");
            }

            /// <summary>
            /// Toggles whether to show user count in welcome messages
            /// </summary>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [Command("UserCount")]
            [Summary("Toggle user count in welcome messages")]
            public async Task UserCount()
            {
                Context.Server.Events.Welcome.UserCount = !Context.Server.Events.Welcome.UserCount;
                Context.Server.Save();
                await SimpleEmbedAsync("**Success**\n" +
                                       $"User count will be shown in the footer of Welcome messages: {Context.Server.Events.Welcome.UserCount}");
            }

            /// <summary>
            /// Sets the welcome message
            /// </summary>
            /// <param name="message">
            /// The message.
            /// </param>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [Command("Message")]
            [Summary("Set the welcome message")]
            public async Task SetMessage([Remainder] string message)
            {
                Context.Server.Events.Welcome.Message = message;
                Context.Server.Save();
                await SimpleEmbedAsync("**Success the welcome message is now:**\n" +
                                       $"{message}");
            }
        }

        /// <summary>
        /// The goodbye event setup
        /// </summary>
        [Group("Goodbye")]
        public class Goodbye : Base
        {
            /// <summary>
            /// The goodbye event info
            /// </summary>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [Command("Info")]
            [Summary("Display Goodbye setup info")]
            public async Task GoodbyeEvent()
            {
                await SimpleEmbedAsync("**Goodbye Event**\n" +
                                       $"`{Context.Prefix}Goodbye Info` - This Message\n" +
                                       $"`{Context.Prefix}Goodbye Toggle` - Toggle the Goodbye event\n" +
                                       $"`{Context.Prefix}Goodbye SetChannel` - Set the channel where Goodbye events will be sent\n" +
                                       $"`{Context.Prefix}Goodbye Message <Message>` - Set the Goodbye Message\n\n" +
                                       "**Goodbye Settings**\n" +
                                       $"Enabled: {Context.Server.Events.Goodbye.Enabled}\n" +
                                       $"Channel: {Context.Guild.GetChannel(Context.Server.Events.Goodbye.ChannelID)?.Name ?? "N/A"}\n" +
                                       "Message:\n" +
                                       $"{Context.Server.Events.Goodbye.Message ?? "N/A"}");
            }

            /// <summary>
            /// Toggles the goodbye event
            /// </summary>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [Command("Toggle")]
            [Summary("Toggle the Goodbye event in the server")]
            public async Task Toggle()
            {
                Context.Server.Events.Goodbye.Enabled = !Context.Server.Events.Goodbye.Enabled;
                Context.Server.Save();
                await SimpleEmbedAsync($"Goodbye Event: {Context.Server.Events.Goodbye.Enabled}");
            }

            /// <summary>
            /// Sets the goodbye event channel
            /// </summary>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [Command("SetChannel")]
            [Summary("Set the channel Goodbye events will be sent to")]
            public async Task SetChannel()
            {
                Context.Server.Events.Goodbye.ChannelID = Context.Channel.Id;
                Context.Server.Save();
                await SimpleEmbedAsync($"Success, Goodbye messages will now be sent in the channel: {Context.Channel.Name}");
            }

            /// <summary>
            /// Sets the goodbye message
            /// </summary>
            /// <param name="message">
            /// The message.
            /// </param>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [Command("Goodbye")]
            [Summary("Set the Goodbye message")]
            public async Task SetMessage([Remainder] string message)
            {
                Context.Server.Events.Goodbye.Message = message;
                Context.Server.Save();
                await SimpleEmbedAsync("**Success the Goodbye message is now:**\n" +
                                       $"{message}");
            }
        }
    }
}