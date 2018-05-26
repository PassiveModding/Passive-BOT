using System;
using System.Globalization;
using Discord;

namespace PassiveBOT.Discord.Extensions
{
    public class HexToColor
    {
        public static Color GetCol(string Color)
        {
            Color = Color.Replace("#", "");
            if (Color.Length != 6)
            {
                throw new Exception("Color Length must be 6 characters (not including the # out the front), ie. #FFFFFF");
            }

            try
            {
                var rgb = System.Drawing.Color.FromArgb(int.Parse(Color, NumberStyles.AllowHexSpecifier));
                var discordcolor = new Color(rgb.R, rgb.G, rgb.B);
                return discordcolor;
            }
            catch
            {
                throw new Exception("Invalid Color Conversion Please ensure you input a valid hex color, ie. #FFFFFF");
            }
        }
    }
}
