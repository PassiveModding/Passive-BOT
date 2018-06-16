namespace PassiveBOT.Discord.Extensions.PassiveBOT
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using global::Discord;

    using global::PassiveBOT.Discord.Context;

    /// <summary>
    /// Helps with media channels and auto-message channels.
    /// </summary>
    public class ChannelHelper
    {
        /// <summary>
        /// Updates an auto-message channel count.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static async Task<Context> DoAutoMessage(Context context)
        {
            if (context.Channel is IDMChannel)
            {
                return context;
            }

            var channel = context.Server.CustomChannel.AutoMessageChannels.FirstOrDefault(x => x.ChannelID == context.Channel.Id);
            if (channel == null)
            {
                return context;
            }

            if (!channel.Enabled)
            {
                return context;
            }

            channel.Count++;

            if (channel.Count >= channel.Limit)
            {
                await context.Channel.SendMessageAsync(string.Empty, false, new EmbedBuilder
                {
                    Title = "Auto Message",
                    Color = Color.Green,
                    Description = channel.Message
                }.Build());
                channel.Count = 0;
            }

            context.Server.Save();
            return context;
        }

        /// <summary>
        /// Checks for URLs or attachments to ensure the channel is used as a media channel.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static async Task DoMediaChannel(Context context)
        {
            var mediaChannel = context.Server.CustomChannel.MediaChannels.FirstOrDefault(x => x.Enabled && x.ChannelID == context.Channel.Id);
            if (mediaChannel != null)
            {
                if (mediaChannel.Enabled && 
                    (context.User as IGuildUser).RoleIds.All(x => !mediaChannel.ExemptRoles.Contains(x)) && 
                    !Regex.Match(context.Message.Content, @"(http|ftp|https)://([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?").Success && 
                    !context.Message.Attachments.Any())
                {
                    await context.Message.DeleteAsync();
                }
            }
        }
    }
}
