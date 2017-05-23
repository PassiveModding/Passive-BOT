using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace PassiveBOT.Handlers
{
    //edited from Rick
    public class EmbedHandler
    {
        private static readonly DiscordSocketClient client;

        public enum Colours
        {
            Red,
            Green,
            Blue,
            Black,
            White,
        }

        public static EmbedBuilder Embed(Colours Color)
        {
            var embed = new EmbedBuilder();
            switch (Color)
            {
                case Colours.Green:
                    embed.Color = new Color(0x32CD32);
                    break;
                case Colours.Red:
                    embed.Color = new Color(0xff0000);
                    break;

                case Colours.White:
                    embed.Color = new Color(0xFFFFFF);
                    break;

                case Colours.Black:
                    embed.Color = new Color(0x000000);
                    break;

                case Colours.Blue:
                    embed.Color = new Color(0x37FDFC);
                    break;
            }
            return embed;
        }

        public static EmbedBuilder Embed(string Author = null, string AuthPic = null, string Title = null, string Desc = null, string Image = null, string ThumbUrl = null)
        {
            return Embed(Colours.Black)
                .WithAuthor(x =>
                {
                    x.Name = Author;
                    x.IconUrl = AuthPic;
                })
                .WithTitle(Title)
                .WithDescription(Desc)
                .WithImageUrl(Image)
                .WithThumbnailUrl(ThumbUrl)
                .WithFooter(x =>
                {
                    x.WithText("PassiveBOT");
                    x.WithIconUrl((client as DiscordSocketClient).CurrentUser.GetAvatarUrl());
                });
        }
    }
}
