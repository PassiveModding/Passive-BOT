namespace PassiveBOT.Extensions.PassiveBOT
{
    using System;
    using System.Threading.Tasks;

    using Discord;
    using Discord.WebSocket;

    using global::PassiveBOT.Handlers;
    using global::PassiveBOT.Models;
    using global::PassiveBOT.Services;

    /// <summary>
    ///     The partner helper.
    /// </summary>
    public class PartnerHelper
    {
        public PartnerHelper(HomeService homeService)
        {
            _HomeService = homeService;
        }

        private static HomeService _HomeService { get; set; }

        /// <summary>
        ///     Generates a partner message based off the input server
        /// </summary>
        /// <param name="guildObj">
        ///     The guild obj.
        /// </param>
        /// <param name="guild">
        ///     The guild.
        /// </param>
        /// <returns>
        ///     The <see cref="EmbedBuilder" />.
        /// </returns>
        public static EmbedBuilder GenerateMessage(PartnerService.PartnerInfo guildObj, SocketGuild guild)
        {
            var image = guildObj.Message.ImageUrl;
            if (image != null)
            {
                if (!Uri.IsWellFormedUriString(image, UriKind.Absolute))
                {
                    image = null;
                    guildObj.Message.ImageUrl = null;
                    guildObj.Save();
                }
            }

            try
            {
                var embed = new EmbedBuilder();
                embed.Title = guild.Name;
                embed.Description = guildObj.Message.Content;
                embed.ImageUrl = image;
                embed.Color = new Color(guildObj.Message.Color.R, guildObj.Message.Color.G, guildObj.Message.Color.B);
                if (guildObj.Message.Invite == null)
                {
                    guildObj.Message.Invite = guild.GetTextChannel(guildObj.Settings.ChannelId)?.CreateInviteAsync(null).Result?.Url;
                    guildObj.Save();
                }
                embed.AddField("Invite", $"{guildObj.Message.Invite ?? "N/A"}");
                embed.ThumbnailUrl = guildObj.Message.UseThumb ? guild.IconUrl : null;
                embed.Footer = new EmbedFooterBuilder { Text = $"{(guildObj.Message.UserCount ? $"Users: {guild.MemberCount} || " : string.Empty)}Get PassiveBOT: {_HomeService.CurrentHomeModel.HomeInvite}", IconUrl = guild.IconUrl };
                return embed;
            }
            catch (Exception e)
            {
                LogHandler.LogMessage($"Partner Embed Build Error for {guild.Name} [{guild.Id}]\n" + $"{e}");
                return new EmbedBuilder { Description = $"Partner Embed Build Error for {guild.Name} [{guild.Id}]\n" + $"{e.Message}" };
            }
        }

        /// <summary>
        ///     Logs a partner update
        /// </summary>
        /// <param name="client">
        ///     The client.
        /// </param>
        /// <param name="partnerGuild">
        ///     The partner guild.
        /// </param>
        /// <param name="fieldInfo">
        ///     The field info.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        public static async Task PartnerLogAsync(DiscordShardedClient client, PartnerService.PartnerInfo partnerGuild, EmbedFieldBuilder fieldInfo)
        {
            var home = _HomeService.CurrentHomeModel;
            if (home.Logging.LogPartnerChanges)
            {
                if (client.GetChannel(home.Logging.PartnerLogChannel) is SocketTextChannel logChannel)
                {
                    var emb = GenerateMessage(partnerGuild, client.GetGuild(partnerGuild.GuildId));
                    emb.AddField(fieldInfo);
                    await logChannel.SendMessageAsync(string.Empty, false, emb.Build());
                }
            }
        }
    }
}