using System.Threading.Tasks;
using Discord.Commands;
using PassiveBOT.Discord.Context;
using PassiveBOT.Discord.Preconditions;

namespace PassiveBOT.Modules.GuildSetup
{
    [Group]
    [RequireAdmin]
    [RequireContext(ContextType.Guild)]
    public class Events : Base
    {
        [Group("Welcome")]
        public class welcome : Base
        {
            [Command("Info")]
            [Summary("Info")]
            [Remarks("Display welcome setup info")]
            public async Task Welcome()
            {
                await SimpleEmbedAsync("**Welcome Event**\n" +
                                       $"`{Context.Prefix}Welcome Info` - This Message\n" +
                                       $"`{Context.Prefix}Welcome Toggle` - Toggle the welcome event\n" +
                                       $"`{Context.Prefix}Welcome SetChannel` - Set the channel where welcome events will be sent\n" +
                                       $"`{Context.Prefix}Welcome Message <Message>` - Set the Welcome Message\n\n" +
                                       "**Welcome Settings**\n" +
                                       $"Enabled: {Context.Server.Events.Welcome.Enabled}\n" +
                                       $"Channel: {Context.Socket.Guild.GetChannel(Context.Server.Events.Welcome.ChannelID)?.Name ?? "N/A"}\n" +
                                       "Message:\n" +
                                       $"{Context.Server.Events.Welcome.Message ?? "N/A"}");
            }

            [Command("Toggle")]
            [Summary("Toggle")]
            [Remarks("Toggle the welcome event in the server")]
            public async Task Toggle()
            {
                Context.Server.Events.Welcome.Enabled = !Context.Server.Events.Welcome.Enabled;
                Context.Server.Save();
                await SimpleEmbedAsync($"Welcome Event: {Context.Server.Events.Welcome.Enabled}");
            }

            [Command("Direct")]
            [Summary("Direct")]
            [Remarks("Direct direct messaging of welcome messages")]
            public async Task DmTask()
            {
                Context.Server.Events.Welcome.SendDMs = !Context.Server.Events.Welcome.SendDMs;
                Context.Server.Save();
                await SimpleEmbedAsync($"Users will be DM's the welcome message upon joining the server: {Context.Server.Events.Welcome.SendDMs}");
            }

            [Command("SetChannel")]
            [Summary("SetChannel")]
            [Remarks("Set the channel welcome events will be sent to")]
            public async Task SetChannel()
            {
                Context.Server.Events.Welcome.ChannelID = Context.Channel.Id;
                Context.Server.Save();
                await SimpleEmbedAsync($"Success, Welcome messages will now be sent in the channel: {Context.Channel.Name}");
            }

            [Command("UserCount")]
            [Summary("UserCount")]
            [Remarks("Toggle usercount in welcome messages")]
            public async Task UserCount()
            {
                Context.Server.Events.Welcome.UserCount = !Context.Server.Events.Welcome.UserCount;
                Context.Server.Save();
                await SimpleEmbedAsync("**Success**\n" +
                                       $"Usercount will be shown in the footer of Welcome messages: {Context.Server.Events.Welcome.UserCount}");
            }

            [Command("Message")]
            [Summary("Message")]
            [Remarks("Set the welcome message")]
            public async Task SetChannel([Remainder] string message)
            {
                Context.Server.Events.Welcome.Message = message;
                Context.Server.Save();
                await SimpleEmbedAsync("**Success the welcome message is now:**\n" +
                                       $"{message}");
            }
        }

        [Group("Goodbye")]
        public class Goodbye : Base
        {
            [Command("Info")]
            [Summary("Info")]
            [Remarks("Display Goodbye setup info")]
            public async Task GoodbyeE()
            {
                await SimpleEmbedAsync("**Goodbye Event**\n" +
                                       $"`{Context.Prefix}Goodbye Info` - This Message\n" +
                                       $"`{Context.Prefix}Goodbye Toggle` - Toggle the Goodbye event\n" +
                                       $"`{Context.Prefix}Goodbye SetChannel` - Set the channel where Goodbye events will be sent\n" +
                                       $"`{Context.Prefix}Goodbye Message <Message>` - Set the Goodbye Message\n\n" +
                                       "**Goodbye Settings**\n" +
                                       $"Enabled: {Context.Server.Events.Goodbye.Enabled}\n" +
                                       $"Channel: {Context.Socket.Guild.GetChannel(Context.Server.Events.Goodbye.ChannelID)?.Name ?? "N/A"}\n" +
                                       "Message:\n" +
                                       $"{Context.Server.Events.Goodbye.Message ?? "N/A"}");
            }

            [Command("Toggle")]
            [Summary("Toggle")]
            [Remarks("Toggle the Goodbye event in the server")]
            public async Task Toggle()
            {
                Context.Server.Events.Goodbye.Enabled = !Context.Server.Events.Goodbye.Enabled;
                Context.Server.Save();
                await SimpleEmbedAsync($"Goodbye Event: {Context.Server.Events.Goodbye.Enabled}");
            }

            [Command("SetChannel")]
            [Summary("SetChannel")]
            [Remarks("Set the channel Goodbye events will be sent to")]
            public async Task SetChannel()
            {
                Context.Server.Events.Goodbye.ChannelID = Context.Channel.Id;
                Context.Server.Save();
                await SimpleEmbedAsync($"Success, Goodbye messages will now be sent in the channel: {Context.Channel.Name}");
            }

            [Command("Goodbye")]
            [Summary("Goodbye")]
            [Remarks("Set the Goodbye message")]
            public async Task SetChannel([Remainder] string message)
            {
                Context.Server.Events.Goodbye.Message = message;
                Context.Server.Save();
                await SimpleEmbedAsync("**Success the Goodbye message is now:**\n" +
                                       $"{message}");
            }
        }
    }
}