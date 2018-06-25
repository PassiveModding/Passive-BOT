namespace PassiveBOT.Modules.GuildCommands.ServerSetup
{
    using System.Threading.Tasks;

    using global::Discord.Commands;

    using Microsoft.Extensions.DependencyInjection;

    using PassiveBOT.Discord.Context;
    using PassiveBOT.Discord.Preconditions;
    using PassiveBOT.Models;

    /// <summary>
    /// General Server Setup
    /// </summary>
    [GuildOwner]
    public class GeneralSetup : Base
    {
        /// <summary>
        /// Set a custom prefix
        /// </summary>
        /// <param name="prefix">
        /// The prefix.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("SetPrefix")]
        [Summary("Set a custom prefix for the bot")]
        [Remarks("Will reset the prefix if no value provided\nAlso, use \"prefix \" to use spaces in the prefix")]
        public async Task SetPrefix(string prefix = null)
        {
            Context.Server.Settings.Prefix.CustomPrefix = prefix;
            Context.Server.Save();
            await SimpleEmbedAsync("The bot's prefix has been updated for this server.\n" +
                                   "Command usage is now as follows:\n" +
                                   $"`{prefix ?? Context.Provider.GetRequiredService<ConfigModel>().Prefix}help`");
        }

        /// <summary>
        /// Toggles the NsfwAllowed Precondition.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("ToggleNsfw")]
        [Summary("Toggles the use of Nsfw Commands AT ALL in the server")]
        public async Task ToggleNsfw()
        {
            Context.Server.Settings.Nsfw.Enabled = !Context.Server.Settings.Nsfw.Enabled;
            Context.Server.Save();
            await SimpleEmbedAsync($"Nsfw Allowed: {Context.Server.Settings.Nsfw.Enabled}");
        }

        /// <summary>
        /// The toggle mention prefix.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("DenyMentionPrefix")]
        [Summary("Toggle whether or not users can @ the bot to use a command")]
        public async Task ToggleMentionPrefix()
        {
            Context.Server.Settings.Prefix.DenyMentionPrefix = !Context.Server.Settings.Prefix.DenyMentionPrefix;
            Context.Server.Save();
            await SimpleEmbedAsync($"Mention Prefix Enabled = {!Context.Server.Settings.Prefix.DenyMentionPrefix}");
        }

        /// <summary>
        /// Toggles the denial of the default bot prefix
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("DenyDefaultPrefix")]
        [Summary("Toggle whether or not users can use the Default bot prefix in the server")]
        public async Task ToggleDenyPrefix()
        {
            Context.Server.Settings.Prefix.DenyDefaultPrefix = !Context.Server.Settings.Prefix.DenyDefaultPrefix;
            Context.Server.Save();
            await SimpleEmbedAsync($"Default Prefix Enabled = {!Context.Server.Settings.Prefix.DenyDefaultPrefix}");
        }

        /// <summary>
        /// Toggles whether to save the guild config after removing the bot from the server.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("SaveGuildConfig")]
        [Summary("Toggle whether to save the guild config even when the bot has left the server.")]
        public async Task SaveGuildConfig()
        {
            Context.Server.Settings.Config.SaveGuildModel = !Context.Server.Settings.Config.SaveGuildModel;
            Context.Server.Save();
            await SimpleEmbedAsync($"Save guild Model = {Context.Server.Settings.Config.SaveGuildModel}\n" + 
                                   $"If set to false, all bot saved data for the server will be deleted when you remove the bot from the server.");
        }
    }
}