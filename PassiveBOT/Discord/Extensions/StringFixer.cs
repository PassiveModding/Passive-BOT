namespace PassiveBOT.Discord.Extensions
{
    /// <summary>
    /// String Management
    /// </summary>
    public class StringFixer
    {
        /// <summary>
        /// Shortens a string to the specified length if it is too long.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string FixLength(string message, int length = 1024)
        {
            if (message.Length > length)
            {
                message = message.Substring(0, length - 4) + "...";
            }

            return message;
        }
    }
}
