using Discord;
using Discord.WebSocket;
using PassiveBOT.Models;

namespace PassiveBOT.Discord.Extensions
{
    public class GeneratePartnerMessage
    {

        public static EmbedBuilder GenerateMessage(GuildModel GuildObj, SocketGuild guild)
        {
            var embed = new EmbedBuilder
            {
                Title = guild.Name,
                Description = GuildObj.Partner.Message.Content,
                ImageUrl = GuildObj.Partner.Message.ImageUrl,
                Color = new Color(GuildObj.Partner.Message.Color.R, GuildObj.Partner.Message.Color.G, GuildObj.Partner.Message.Color.B),
                ThumbnailUrl = GuildObj.Partner.Message.UseThumb ? guild.IconUrl : null,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"{(GuildObj.Partner.Message.UserCount ? $"Users: {guild.MemberCount}" : "")} || Get PassiveBOT: {ConfigModel.Load().SupportServer}"
                }
            };
            return embed;
        }
    }
}
