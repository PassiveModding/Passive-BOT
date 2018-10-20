namespace PassiveBOT.Models
{
    /// <summary>
    ///     The config model.
    /// </summary>
    public class ConfigModel
    {
        /// <summary>
        ///     Gets or sets a value indicating whether to log command usages.
        /// </summary>
        public bool LogCommandUsages { get; set; } = true;

        /// <summary>
        ///     Gets or sets a value indicating whether to log user messages.
        /// </summary>
        public bool LogUserMessages { get; set; } = false;

        /// <summary>
        ///     Gets or sets the bot PrefixSetup
        /// </summary>
        public string Prefix { get; set; } = "+";

        /// <summary>
        ///     Gets or sets the amount of shards for the bot
        /// </summary>
        public int Shards { get; set; } = 1;

        /// <summary>
        ///     Gets or sets the store URL for translation Upgrades
        /// </summary>
        public string TranslateStoreUrl { get; set; } = null;

        /// <summary>
        ///     Returns the translation store message
        /// </summary>
        /// <returns></returns>
        public string GetTranslateUrl()
        {
            if (string.IsNullOrEmpty(TranslateStoreUrl))
            {
                return null;
            }

            return $"You may upgrade your translation limits by purchasing a token from {TranslateStoreUrl} and using the command `{Prefix}translate redeem <token>`";
        }

        /// <summary>
        ///     Gets or sets the token.
        /// </summary>
        public string Token { get; set; } = "Token";
    }
}