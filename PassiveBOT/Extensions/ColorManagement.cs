namespace PassiveBOT.Extensions
{
    using System;
    using System.Globalization;

    using Discord;

    /// <summary>
    /// TColor Helpers
    /// </summary>
    public class ColorManagement
    {
        /// <summary>
        /// The get col.
        /// </summary>
        /// <param name="color">
        /// The color.
        /// </param>
        /// <returns>
        /// The <see cref="Color"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Throws if unable to convert.
        /// </exception>
        public static Color GetColor(string color)
        {
            color = color.Replace("#", string.Empty);
            if (color.Length != 6)
            {
                throw new Exception("color Length must be 6 characters (not including the # out the front), ie. #FFFFFF");
            }

            try
            {
                var rgb = System.Drawing.Color.FromArgb(int.Parse(color, NumberStyles.AllowHexSpecifier));
                var discordColor = new Color(rgb.R, rgb.G, rgb.B);
                return discordColor;
            }
            catch
            {
                throw new Exception("Invalid color Conversion Please ensure you input a valid hex color, ie. #FFFFFF");
            }
        }
    }
}