namespace PassiveBOT.Extensions
{
    using Discord.WebSocket;

    /// <summary>
    /// Helps with generating discord invites
    /// </summary>
    public class InviteHelper
    {
        /// <summary>
        /// Gets a discord invite via client ID
        /// </summary>
        /// <param name="clientId">
        /// The client id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetInvite(ulong clientId)
        {
            return $"https://discordapp.com/oauth2/authorize?client_id={clientId}&scope=bot&permissions=2146958591";
        }

        /// <summary>
        /// Gets a discord bot invite via the client
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetInvite(DiscordSocketClient client)
        {
            return GetInvite(client.CurrentUser.Id);
        }

        /// <summary>
        /// Gets a discord bot invite via the client
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetInvite(DiscordShardedClient client)
        {
            return GetInvite(client.CurrentUser.Id);
        }
    }
}
