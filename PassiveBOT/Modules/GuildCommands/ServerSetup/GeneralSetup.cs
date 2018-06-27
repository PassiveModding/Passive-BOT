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
    [Summary("General Server setup commands")]
    public class GeneralSetup : Base
    {
        /// <summary>
        /// The general setup task.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("GeneralSetup")]
        [Summary("Setup Information for the general setup module")]
        public Task GeneralSetupTaskAsync()
        {
            var settings = Context.Server.Settings;
            return SimpleEmbedAsync($"Save Guild Model: {settings.Config.SaveGuildModel}\n" + 
                                    $"Custom Prefix: `{settings.Prefix.CustomPrefix}`\n" + 
                                    $"Deny Default Prefix: {settings.Prefix.DenyDefaultPrefix}\n" + 
                                    $"Deny Mention Prefix: {settings.Prefix.DenyMentionPrefix}\n" + 
                                    $"Allow NSFW: {settings.Nsfw.Enabled}");
        }

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
        public Task SetPrefixAsync(string prefix = null)
        {
            Context.Server.Settings.Prefix.CustomPrefix = prefix;
            Context.Server.Save();
            return SimpleEmbedAsync("The bot's prefix has been updated for this server.\n" +
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
        public Task ToggleNsfwAsync()
        {
            Context.Server.Settings.Nsfw.Enabled = !Context.Server.Settings.Nsfw.Enabled;
            Context.Server.Save();
            return SimpleEmbedAsync($"Nsfw Allowed: {Context.Server.Settings.Nsfw.Enabled}");
        }

        /// <summary>
        /// The toggle mention prefix.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("DenyMentionPrefix")]
        [Summary("Toggle whether or not users can @ the bot to use a command")]
        public Task ToggleMentionPrefixAsync()
        {
            Context.Server.Settings.Prefix.DenyMentionPrefix = !Context.Server.Settings.Prefix.DenyMentionPrefix;
            Context.Server.Save();
            return SimpleEmbedAsync($"Mention Prefix Enabled = {!Context.Server.Settings.Prefix.DenyMentionPrefix}");
        }

        /// <summary>
        /// Toggles the denial of the default bot prefix
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("DenyDefaultPrefix")]
        [Summary("Toggle whether or not users can use the Default bot prefix in the server")]
        public Task ToggleDenyPrefixAsync()
        {
            Context.Server.Settings.Prefix.DenyDefaultPrefix = !Context.Server.Settings.Prefix.DenyDefaultPrefix;
            Context.Server.Save();
            return SimpleEmbedAsync($"Default Prefix Enabled = {!Context.Server.Settings.Prefix.DenyDefaultPrefix}");
        }

        /// <summary>
        /// Toggles whether to save the guild config after removing the bot from the server.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("SaveGuildConfig")]
        [Summary("Toggle whether to save the guild config even when the bot has left the server.")]
        public Task SaveGuildConfigAsync()
        {
            Context.Server.Settings.Config.SaveGuildModel = !Context.Server.Settings.Config.SaveGuildModel;
            Context.Server.Save();
            return SimpleEmbedAsync($"Save guild Model = {Context.Server.Settings.Config.SaveGuildModel}\n" + 
                                    "If set to false, all bot saved data for the server will be deleted when you remove the bot from the server.");
        }
    }
}