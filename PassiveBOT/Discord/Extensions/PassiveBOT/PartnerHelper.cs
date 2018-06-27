namespace PassiveBOT.Discord.Extensions.PassiveBOT
{
    using System;
    using System.Threading.Tasks;

    using global::Discord;
    using global::Discord.WebSocket;

    using global::PassiveBOT.Handlers;
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
            var image = guildObj.Partner.Message.ImageUrl;
            if (image != null)
            {
                if (!Uri.IsWellFormedUriString(image, UriKind.Absolute))
                {
                    image = null;
                    guildObj.Partner.Message.ImageUrl = null;
                    guildObj.Save();
                }
            }

            try
            {
                var embed = new EmbedBuilder();
                embed.Title = guild.Name;
                embed.Description = guildObj.Partner.Message.Content;
                embed.ImageUrl = image;
                embed.Color = new Color(guildObj.Partner.Message.Color.R, guildObj.Partner.Message.Color.G, guildObj.Partner.Message.Color.B);
                embed.ThumbnailUrl = guildObj.Partner.Message.UseThumb ? guild.IconUrl : null;
                embed.Footer = new EmbedFooterBuilder
                                   {
                                       Text = $"{(guildObj.Partner.Message.UserCount ? $"Users: {guild.MemberCount} || " : "")}Get PassiveBOT: {HomeModel.Load().HomeInvite}",
                                       IconUrl = guild.IconUrl
                                   };
                return embed;
            }
            catch (Exception e)
            {
                LogHandler.LogMessage($"Partner Embed Build Error for {guild.Name} [{guild.Id}]\n" + $"{e}");
                return new EmbedBuilder
                           {
                               Description = $"Partner Embed Build Error for {guild.Name} [{guild.Id}]\n" + 
                                             $"{e.Message}"
                           };
            }
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
        public static async Task PartnerLogAsync(DiscordShardedClient client, GuildModel partnerGuild, EmbedFieldBuilder fieldInfo)
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
