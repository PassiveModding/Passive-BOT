using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Discord.Context;
using PassiveBOT.Discord.Preconditions;

namespace PassiveBOT.Modules.GuildSetup
{
    [RequireContext(ContextType.Guild)]
    [RequireAdmin]
    public class Settings : Base
    {
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
