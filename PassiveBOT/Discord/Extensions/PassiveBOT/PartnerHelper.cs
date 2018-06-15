namespace PassiveBOT.Discord.Extensions.PassiveBOT
{
    using System.Threading.Tasks;

    using global::Discord;
    using global::Discord.WebSocket;

    using global::PassiveBOT.Models;

    /// <summary>
    /// The partner helper.
    /// </summary>
    public class PartnerHelper
    {
        /// <summary>
        /// Generates a partner message based off the input server
        /// </summary>
        /// <param name="guildObj">
        /// The guild obj.
        /// </param>
        /// <param name="guild">
        /// The guild.
        /// </param>
        /// <returns>
        /// The <see cref="EmbedBuilder"/>.
        /// </returns>
        public static EmbedBuilder GenerateMessage(GuildModel guildObj, SocketGuild guild)
        {
            var embed = new EmbedBuilder
                            {
                                Title = guild.Name,
                                Description = guildObj.Partner.Message.Content,
                                ImageUrl = guildObj.Partner.Message.ImageUrl,
                                Color = new Color(guildObj.Partner.Message.Color.R, guildObj.Partner.Message.Color.G, guildObj.Partner.Message.Color.B),
                                ThumbnailUrl = guildObj.Partner.Message.UseThumb ? guild.IconUrl : null,
                                Footer = new EmbedFooterBuilder
                                             {
                                                 Text = $"{(guildObj.Partner.Message.UserCount ? $"Users: {guild.MemberCount} || " : "")}Get PassiveBOT: {HomeModel.Load().HomeInvite}"
                                             }
                            };
            return embed;
        }

        /// <summary>
        /// Logs a partner update
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="partnerGuild">
        /// The partner guild.
        /// </param>
        /// <param name="fieldInfo">
        /// The field info.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static async Task PartnerLog(DiscordShardedClient client, GuildModel partnerGuild, EmbedFieldBuilder fieldInfo)
        {
            var home = HomeModel.Load();
            if (home.Logging.LogPartnerChanges)
            {
                if (client.GetChannel(home.Logging.PartnerLogChannel) is SocketTextChannel logChannel)
                {
                    var emb = GenerateMessage(partnerGuild, client.GetGuild(partnerGuild.ID));
                    emb.AddField(fieldInfo);
                    await logChannel.SendMessageAsync(string.Empty, false, emb.Build());
                }
            }
        }
    }
}
