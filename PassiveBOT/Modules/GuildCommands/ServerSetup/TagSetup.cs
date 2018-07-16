namespace PassiveBOT.Modules.GuildCommands.ServerSetup
{
    using System.Threading.Tasks;

    using Discord.Commands;

    using PassiveBOT.Context;
    using PassiveBOT.Preconditions;
    using PassiveBOT.Services;

    /// <summary>
    ///     The tags.
    /// </summary>
    [Group("TagSetup")]
    [Summary("Tag Setup commands")]
    [RequireAdmin]
    [RequireContext(ContextType.Guild)]
    public class Tags : Base
    {
        public Tags(TagService service)
        {
            Service = service;
        }

        private TagService Service { get; }

        /// <summary>
        ///     The tag setup task.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("TagSetup")]
        [Summary("Setup information for the TagSetup Module")]
        public Task TagSetupTaskAsync()
        {
            var t = Service.GetTagSetup(Context.Guild.Id);
            return SimpleEmbedAsync($"Tagging Enabled: {t.Enabled}");
        }

        /// <summary>
        ///     Toggles the tag system
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("Toggle")]
        [Summary("Toggle the tagging system")]
        public Task TagToggleAsync()
        {
            var t = Service.GetTagSetup(Context.Guild.Id);
            t.Enabled = !t.Enabled;
            t.Save();
            return SimpleEmbedAsync($"Tags Enabled: {t.Enabled}");
        }
    }
}