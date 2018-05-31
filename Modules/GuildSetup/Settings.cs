using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Discord.Context;
using PassiveBOT.Discord.Preconditions;
using PassiveBOT.Handlers;

namespace PassiveBOT.Modules.GuildSetup
{
    [RequireContext(ContextType.Guild)]
    [RequireAdmin]
    public class Settings : Base
    {
        [Command("SetPrefix")]
        [Summary("SetPrefix <prefix>")]
        [Remarks("Set a custom prefix for the bot")]
        public async Task SetPrefix([Remainder] string prefix = null)
        {
            Context.Server.Settings.Prefix.CustomPrefix = prefix;
            Context.Server.Save();
            await SimpleEmbedAsync("The bot's prefix has been updated for this server.\n" +
                                   "Command usage is now as follows:\n" +
                                   $"`{prefix ?? CommandHandler.Config.Prefix}help`");
        }

        [Command("DenyMentionPrefix")]
        [Summary("DenyMentionPrefix")]
        [Remarks("Toggle wether or not users can @ the bot to use a command")]
        public async Task ToggleMentionPrefix()
        {
            Context.Server.Settings.Prefix.DenyMentionPrefix = !Context.Server.Settings.Prefix.DenyMentionPrefix;
            Context.Server.Save();
            await SimpleEmbedAsync($"Mention Prefix Enabled = {!Context.Server.Settings.Prefix.DenyMentionPrefix}");
        }

        [Command("DenyDefaultPrefix")]
        [Summary("DenyDefaultPrefix")]
        [Remarks("Toggle wether or not users can use the Defaut bot prefix in the server")]
        public async Task ToggleDenyPrefix()
        {
            Context.Server.Settings.Prefix.DenyDefaultPrefix = !Context.Server.Settings.Prefix.DenyDefaultPrefix;
            Context.Server.Save();
            await SimpleEmbedAsync($"Default Prefix Enabled = {!Context.Server.Settings.Prefix.DenyDefaultPrefix}");
        }

        [Command("SetAdmin")]
        [Summary("SetAdmin <@Role>")]
        [Remarks("Add an admin role in the server (or remove it)")]
        public async Task Admin(IRole AdminRole)
        {
            if (Context.Server.Moderation.AdminRoleIDs.Contains(AdminRole.Id))
            {
                Context.Server.Moderation.AdminRoleIDs.Remove(AdminRole.Id);
                await SimpleEmbedAsync($"{AdminRole.Mention} has been removed from the admin role list.");
            }
            else
            {
                Context.Server.Moderation.AdminRoleIDs.Add(AdminRole.Id);
                await SimpleEmbedAsync($"{AdminRole.Mention} has been added to the admin role list.");
            }

            Context.Server.Save();
        }

        [Command("SetMod")]
        [Summary("SetMod <@Role>")]
        [Remarks("Add a moderator role in the server (or remove it)")]
        public async Task Moderator(IRole ModRole)
        {
            if (Context.Server.Moderation.ModRoleIDs.Contains(ModRole.Id))
            {
                Context.Server.Moderation.ModRoleIDs.Remove(ModRole.Id);
                await SimpleEmbedAsync($"{ModRole.Mention} has been removed from the admin role list.");
            }
            else
            {
                Context.Server.Moderation.ModRoleIDs.Add(ModRole.Id);
                await SimpleEmbedAsync($"{ModRole.Mention} has been added to the admin role list.");
            }

            Context.Server.Save();
        }

        [Command("SetSub")]
        [Summary("SetSub <@Role>")]
        [Remarks("Add a publically joinable role (or remove it)")]
        public async Task SetSub(IRole SubRole)
        {
            if (Context.Server.Moderation.SubRoleIDs.Contains(SubRole.Id))
            {
                Context.Server.Moderation.SubRoleIDs.Remove(SubRole.Id);
                await SimpleEmbedAsync($"{SubRole.Mention} has been removed from the sub role list.");
            }
            else
            {
                Context.Server.Moderation.SubRoleIDs.Add(SubRole.Id);
                await SimpleEmbedAsync($"{SubRole.Mention} has been added to the sub role list.");
            }

            Context.Server.Save();
        }
    }
}