namespace PassiveBOT.Modules.GuildCommands.ServerSetup
{
    using System.Threading.Tasks;

    using Discord.Addons.PrefixService;
    using Discord.Commands;

    using PassiveBOT.Context;
    using PassiveBOT.Preconditions;

    /// <summary>
    /// The event setup.
    /// </summary>
    [Group]
    [RequireAdmin]
    [RequireContext(ContextType.Guild)]
    public class EventSetup : Base
    {
        private static PrefixService PrefixService { get; set; }

        public EventSetup(PrefixService prefixService)
        {
            PrefixService = prefixService;
        }


        /// <summary>
        /// The welcome event setup
        /// </summary>
        [Group("Welcome")]
        [Summary("Messages that will be sent whenever a user joins the server.")]
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
            public Task WelcomeInfoAsync()
            {
                var pre = PrefixService.GetPrefix(Context.Guild.Id);
                return SimpleEmbedAsync("**Welcome Event**\n" +
                                        $"`{pre}Welcome Info` - This Message\n" +
                                        $"`{pre}Welcome Toggle` - Toggle the welcome event\n" +
                                        $"`{pre}Welcome SetChannel` - Set the channel where welcome events will be sent\n" +
                                        $"`{pre}Welcome Message <Message>` - Set the Welcome Message\n\n" +
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
            public Task ToggleAsync()
            {
                Context.Server.Events.Welcome.Enabled = !Context.Server.Events.Welcome.Enabled;
                Context.Server.Save();
                return SimpleEmbedAsync($"Welcome Event: {Context.Server.Events.Welcome.Enabled}");
            }

            /// <summary>
            /// Toggles whether to DM welcome messages
            /// </summary>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [Command("Direct")]
            [Summary("Direct direct messaging of welcome messages")]
            public Task DmTaskAsync()
            {
                Context.Server.Events.Welcome.SendDMs = !Context.Server.Events.Welcome.SendDMs;
                Context.Server.Save();
                return SimpleEmbedAsync($"Users will be private messaged the welcome message upon joining the server: {Context.Server.Events.Welcome.SendDMs}");
            }

            /// <summary>
            /// Sets the welcome message channel
            /// </summary>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [Command("SetChannel")]
            [Summary("Set the channel welcome events will be sent to")]
            public Task SetChannelAsync()
            {
                Context.Server.Events.Welcome.ChannelID = Context.Channel.Id;
                Context.Server.Save();
                return SimpleEmbedAsync($"Success, Welcome messages will now be sent in the channel: {Context.Channel.Name}");
            }

            /// <summary>
            /// Toggles whether to show user count in welcome messages
            /// </summary>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [Command("UserCount")]
            [Summary("Toggle user count in welcome messages")]
            public Task UserCountAsync()
            {
                Context.Server.Events.Welcome.UserCount = !Context.Server.Events.Welcome.UserCount;
                Context.Server.Save();
                return SimpleEmbedAsync("**Success**\n" +
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
            public Task SetMessageAsync([Remainder] string message)
            {
                Context.Server.Events.Welcome.Message = message;
                Context.Server.Save();
                return SimpleEmbedAsync($"**Success the welcome message is now:**\n{message}");
            }
        }

        /// <summary>
        /// The goodbye event setup
        /// </summary>
        [Group("Goodbye")]
        [Summary("Messages that will be sent whenever a user leaves the server.")]
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
            public Task GoodbyeEventAsync()
            {
                var pre = PrefixService.GetPrefix(Context.Guild.Id);
                return SimpleEmbedAsync("**Goodbye Event**\n" +
                                        $"`{pre}Goodbye Info` - This Message\n" +
                                        $"`{pre}Goodbye Toggle` - Toggle the Goodbye event\n" +
                                        $"`{pre}Goodbye SetChannel` - Set the channel where Goodbye events will be sent\n" +
                                        $"`{pre}Goodbye Message <Message>` - Set the Goodbye Message\n\n" +
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
            public Task ToggleAsync()
            {
                Context.Server.Events.Goodbye.Enabled = !Context.Server.Events.Goodbye.Enabled;
                Context.Server.Save();
                return SimpleEmbedAsync($"Goodbye Event: {Context.Server.Events.Goodbye.Enabled}");
            }

            /// <summary>
            /// Sets the goodbye event channel
            /// </summary>
            /// <returns>
            /// The <see cref="Task"/>.
            /// </returns>
            [Command("SetChannel")]
            [Summary("Set the channel Goodbye events will be sent to")]
            public Task SetChannelAsync()
            {
                Context.Server.Events.Goodbye.ChannelID = Context.Channel.Id;
                Context.Server.Save();
                return SimpleEmbedAsync($"Success, Goodbye messages will now be sent in the channel: {Context.Channel.Name}");
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
            public Task SetMessageAsync([Remainder] string message)
            {
                Context.Server.Events.Goodbye.Message = message;
                Context.Server.Save();
                return SimpleEmbedAsync("**Success the Goodbye message is now:**\n" +
                                        $"{message}");
            }
        }
    }
}