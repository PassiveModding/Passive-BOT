using System.Threading.Tasks;
using Discord.Commands;
using PassiveBOT.Discord.Context;
using PassiveBOT.Discord.Preconditions;

namespace PassiveBOT.Modules.GuildSetup
{
    [Group("TagSetup")]
    [RequireAdmin]
    [RequireContext(ContextType.Guild)]
    public class Tags : Base
    {
        [Command("Toggle")]
        [Summary("Toggle")]
        [Remarks("Toggle the tagging system")]
        public async Task RawMessage()
        {
            Context.Server.Tags.Settings.Enabled = !Context.Server.Tags.Settings.Enabled;
            Context.Server.Save();
            await SimpleEmbedAsync($"Tags Enabled: {Context.Server.Tags.Settings.Enabled}");
        }

        [Command("AdminOnly")]
        [Summary("AdminOnly")]
        [Remarks("Toggle wether only admins can create tags")]
        public async Task AdminOnly()
        {
            Context.Server.Tags.Settings.AdminOnly = !Context.Server.Tags.Settings.AdminOnly;
            Context.Server.Save();
            await SimpleEmbedAsync($"Tags are admin Only: {Context.Server.Tags.Settings.AdminOnly}");
        }
    }
}