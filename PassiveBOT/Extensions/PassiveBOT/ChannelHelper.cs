namespace PassiveBOT.Extensions.PassiveBOT
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Discord;
    using Discord.WebSocket;

    using global::PassiveBOT.Context;
    using global::PassiveBOT.Services;

    /// <summary>
    ///     Helps with media channels and auto-message channels.
    /// </summary>
    public class ChannelHelper
    {
        public ChannelHelper(ChannelService service)
        {
            Service = service;
        }

        private static ChannelService Service { get; set; }

        /// <summary>
        ///     Updates an auto-message channel count.
        /// </summary>
        /// <param name="context">
        ///     The context.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        public async Task DoAutoMessageAsync(SocketUserMessage msg)
        {
            if (msg.Channel is IDMChannel)
            {
                return;
            }

            var gChannel = msg.Channel as ITextChannel;
            var guild = gChannel.Guild;

            var c = Service.GetCustomChannels(guild.Id);

            if (c == null)
            {
                return;
            }

            if (c.AutoMessageChannels.ContainsKey(gChannel.Id))
            {
                return;
            }

            var channel = c.AutoMessageChannels[gChannel.Id];

            if (!channel.Enabled)
            {
                return;
            }

            channel.Count++;

            if (channel.Count >= channel.Limit)
            {
                await gChannel.SendMessageAsync(string.Empty, false, new EmbedBuilder { Title = "Auto Message", Color = Color.Green, Description = channel.Message }.Build());
                channel.Count = 0;
            }

            c.Save();
        }

        /// <summary>
        /// Checks for URLs or attachments to ensure the channel is used as a media channel.
        /// </summary>
        /// <param name="msg">
        /// The msg.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task DoMediaChannelAsync(SocketUserMessage msg)
        {
            var gChannel = msg.Channel as IGuildChannel;
            var c = Service.GetCustomChannels(gChannel.Guild.Id);

            if (c == null)
            {
                return;
            }

            c.MediaChannels.TryGetValue(gChannel.Id, out var mediaChannel);
            if (mediaChannel != null)
            {
                if (mediaChannel.Enabled && (msg.Author as IGuildUser).RoleIds.All(x => !mediaChannel.ExemptRoles.Contains(x)) && !Regex.Match(msg.Content, @"(http|ftp|https)://([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?").Success && !msg.Attachments.Any())
                {
                    await msg.DeleteAsync();
                }
            }
        }
    }
}