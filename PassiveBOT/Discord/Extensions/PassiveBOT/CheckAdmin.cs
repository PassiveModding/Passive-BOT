namespace PassiveBOT.Discord.Extensions.PassiveBOT
{
    using System.Linq;

    using global::Discord;

    /// <summary>
    /// Admin check class
    /// </summary>
    public class CheckAdmin
    {
        /// <summary>
        /// Checks if a user is an admin in the current server
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// True if they are an admin
        /// </returns>
        public static bool IsAdmin(Context.Context context)
        {
            // Note will return true if the user has an admin role (configured in the bot) OR an admin permission (configured in server settings on discord)
            return (context.User as IGuildUser).RoleIds.Any(x => context.Server.Moderation.AdminRoleIDs.Contains(x)) || (context.User as IGuildUser).GuildPermissions.Administrator;
        }
    }
}