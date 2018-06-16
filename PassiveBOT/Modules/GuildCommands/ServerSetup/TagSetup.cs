namespace PassiveBOT.Modules.GuildCommands.ServerSetup
{
    using System.Threading.Tasks;

    using global::Discord.Commands;

    using PassiveBOT.Discord.Context;
    using PassiveBOT.Discord.Preconditions;

    /// <summary>
    /// The tags.
    /// </summary>
    [Group("TagSetup")]
    [RequireAdmin]
    [RequireContext(ContextType.Guild)]
    public class Tags : Base
    {
        /// <summary>
        /// Toggles the tag system
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("Toggle")]
        [Summary("Toggle the tagging system")]
        public async Task TagToggle()
        {
            Context.Server.Tags.Settings.Enabled = !Context.Server.Tags.Settings.Enabled;
            Context.Server.Save();
            await SimpleEmbedAsync($"Tags Enabled: {Context.Server.Tags.Settings.Enabled}");
        }

        /// <summary>
        /// Toggles admin only settings for tag creation
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("AdminOnly")]
        [Summary("Toggles admin only settings for tag creation")]
        [Remarks("Does not take parameters, just toggles")]
        public async Task AdminOnly()
        {
            Context.Server.Tags.Settings.AdminOnly = !Context.Server.Tags.Settings.AdminOnly;
            Context.Server.Save();
            await SimpleEmbedAsync($"Tags are admin Only: {Context.Server.Tags.Settings.AdminOnly}");
        }
    }
}