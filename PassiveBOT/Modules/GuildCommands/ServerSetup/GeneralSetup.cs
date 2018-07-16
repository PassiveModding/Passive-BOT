namespace PassiveBOT.Modules.GuildCommands.ServerSetup
{
    using System.Threading.Tasks;

    using Discord.Addons.PrefixService;
    using Discord.Commands;

    using PassiveBOT.Context;
    using PassiveBOT.Preconditions;

    /// <summary>
    ///     General Server Setup
    /// </summary>
    [GuildOwner]
    [Summary("General Server setup commands")]
    public class GeneralSetup : Base
    {
        public GeneralSetup(PrefixService prefixService)
        {
            PrefixService = prefixService;
        }

        private PrefixService PrefixService { get; }

        /// <summary>
        ///     The general setup task.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("GeneralSetup", RunMode = RunMode.Async)]
        [Summary("Setup Information for the general setup module")]
        public Task GeneralSetupTaskAsync()
        {
            var settings = Context.Server.Settings;
            var pre = PrefixService.GetPrefix(Context.Guild.Id);

            return SimpleEmbedAsync($"Save Guild Model: {settings.Config.SaveGuildModel}\n" + $"Prefix: `{pre}`\n" + $"Allow NSFW: {settings.Nsfw.Enabled}");
        }

        /// <summary>
        ///     Toggles whether to save the guild config after removing the bot from the server.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("SaveGuildConfig")]
        [Summary("Toggle whether to save the guild config even when the bot has left the server.")]
        public Task SaveGuildConfigAsync()
        {
            Context.Server.Settings.Config.SaveGuildModel = !Context.Server.Settings.Config.SaveGuildModel;
            Context.Server.Save();
            return SimpleEmbedAsync($"Save guild Model = {Context.Server.Settings.Config.SaveGuildModel}\n" + "If set to false, all bot saved data for the server will be deleted when you remove the bot from the server.");
        }

        /// <summary>
        ///     Set a custom prefix
        /// </summary>
        /// <param name="prefix">
        ///     The prefix.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("SetPrefix")]
        [Summary("Set a custom prefix for the bot")]
        [Remarks("Will reset the prefix if no value provided\nAlso, use \"prefix \" to use spaces in the prefix")]
        public Task SetPrefixAsync(string prefix = null)
        {
            var result = PrefixService.SetPrefix(Context.Guild.Id, prefix);
            if (result == PrefixService.PrefixSetResult.success)
            {
                return SimpleEmbedAsync("The bot's prefix has been updated for this server.\n" + "Command usage is now as follows:\n" + $"`{prefix}help`");
            }

            return SimpleEmbedAsync("The bot's prefix has been updated for this server.\n" + "Command usage is now as follows:\n" + $"`{PrefixService.GetPrefix(Context.Guild.Id)}help`");
        }

        /// <summary>
        ///     Toggles the NsfwAllowed Precondition.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("ToggleNsfw")]
        [Summary("Toggles the use of Nsfw Commands AT ALL in the server")]
        public Task ToggleNsfwAsync()
        {
            Context.Server.Settings.Nsfw.Enabled = !Context.Server.Settings.Nsfw.Enabled;
            Context.Server.Save();
            return SimpleEmbedAsync($"Nsfw Allowed: {Context.Server.Settings.Nsfw.Enabled}");
        }
    }
}