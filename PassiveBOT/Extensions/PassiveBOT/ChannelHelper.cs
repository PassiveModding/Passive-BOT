namespace PassiveBOT.Extensions.PassiveBOT
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Discord;

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
        public async Task<Context> DoAutoMessageAsync(Context context)
        {
            if (context.Channel is IDMChannel)
            {
                return context;
            }

            var c = Service.GetCustomChannels(context.Guild.Id);
            if (c.AutoMessageChannels.ContainsKey(context.Channel.Id))
            {
                return context;
            }

            var channel = c.AutoMessageChannels[context.Channel.Id];

            if (!channel.Enabled)
            {
                return context;
            }

            channel.Count++;

            if (channel.Count >= channel.Limit)
            {
                await context.Channel.SendMessageAsync(string.Empty, false, new EmbedBuilder { Title = "Auto Message", Color = Color.Green, Description = channel.Message }.Build());
                channel.Count = 0;
            }

            c.Save();
            return context;
        }

        /// <summary>
        ///     Checks for URLs or attachments to ensure the channel is used as a media channel.
        /// </summary>
        /// <param name="context">
        ///     The context.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        public async Task DoMediaChannelAsync(Context context)
        {
            Service.GetCustomChannels(context.Guild.Id).MediaChannels.TryGetValue(context.Channel.Id, out var mediaChannel);
            if (mediaChannel != null)
            {
                if (mediaChannel.Enabled && (context.User as IGuildUser).RoleIds.All(x => !mediaChannel.ExemptRoles.Contains(x)) && !Regex.Match(context.Message.Content, @"(http|ftp|https)://([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?").Success && !context.Message.Attachments.Any())
                {
                    await context.Message.DeleteAsync();
                }
            }
        }
    }
}