namespace PassiveBOT.Modules.GuildCommands.ServerSetup
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Addons.Interactive;
    using Discord.Commands;

    using PassiveBOT.Context;
    using PassiveBOT.Preconditions;

    /// <summary>
    /// The role setup.
    /// </summary>
    [GuildOwner]
    [Summary("Role setup commands")]
    [RequireContext(ContextType.Guild)]
    public class RoleSetup : Base
    {
        /// <summary>
        /// The role setup task.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("RoleSetup")]
        [Summary("Displays admin, mod and sub roles")]
        public Task RoleSetupTaskAsync()
        {
            return PagedReplyAsync(new PaginatedMessage
                                       {
                                           Pages = new List<PaginatedMessage.Page>
                                                       {
                                                           new PaginatedMessage.Page
                                                               {
                                                                   Description = $"{string.Join("\n", Context.Server.Moderation.AdminRoleIDs.Select(x => Context.Guild.GetRole(x)?.Mention).Where(x => x != null))}",
                                                                   Title = "Administrator roles"
                                                               },
                                                           new PaginatedMessage.Page
                                                               {
                                                                   Description = $"{string.Join("\n", Context.Server.Moderation.ModRoleIDs.Select(x => Context.Guild.GetRole(x)?.Mention).Where(x => x != null))}",
                                                                   Title = "Moderator roles"
                                                               },
                                                           new PaginatedMessage.Page
                                                               {
                                                                   Description = $"{string.Join("\n", Context.Server.Moderation.SubRoleIDs.Select(x => Context.Guild.GetRole(x)?.Mention).Where(x => x != null))}",
                                                                   Title = "Sub roles"
                                                               }
                                                       },
                                           Color = Color.DarkRed
                                       }, new ReactionList
                                              {
                                                  Forward = true,
                                                  Backward = true,
                                                  Trash = true
                                              });
        }

        /// <summary>
        /// Adds or removes an admin role
        /// </summary>
        /// <param name="role">
        /// The admin role.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("SetAdmin")]
        [Summary("Add an admin role in the server (or remove it)")]
        public async Task AdminAsync(IRole role)
        {
            if (Context.Server.Moderation.AdminRoleIDs.Contains(role.Id))
            {
                Context.Server.Moderation.AdminRoleIDs.Remove(role.Id);
                await SimpleEmbedAsync($"{role.Mention} has been removed from the admin role list.");
            }
            else
            {
                Context.Server.Moderation.AdminRoleIDs.Add(role.Id);
                await SimpleEmbedAsync($"{role.Mention} has been added to the admin role list.");
            }

            Context.Server.Save();
        }

        /// <summary>
        /// Adds or removes a moderator role
        /// </summary>
        /// <param name="role">
        /// The mod role.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("SetMod")]
        [Summary("Add a moderator role in the server (or remove it)")]
        public async Task ModeratorAsync(IRole role)
        {
            if (Context.Server.Moderation.ModRoleIDs.Contains(role.Id))
            {
                Context.Server.Moderation.ModRoleIDs.Remove(role.Id);
                await SimpleEmbedAsync($"{role.Mention} has been removed from the admin role list.");
            }
            else
            {
                Context.Server.Moderation.ModRoleIDs.Add(role.Id);
                await SimpleEmbedAsync($"{role.Mention} has been added to the admin role list.");
            }

            Context.Server.Save();
        }

        /// <summary>
        /// Sets or removes a public role
        /// </summary>
        /// <param name="role">
        /// The sub role.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("SetSub")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Add a public role (or remove it)")]
        public async Task SetSubAsync(IRole role)
        {
            if (Context.Server.Moderation.SubRoleIDs.Contains(role.Id))
            {
                Context.Server.Moderation.SubRoleIDs.Remove(role.Id);
                await SimpleEmbedAsync($"{role.Mention} has been removed from the sub role list.");
            }
            else
            {
                Context.Server.Moderation.SubRoleIDs.Add(role.Id);
                await SimpleEmbedAsync($"{role.Mention} has been added to the sub role list.");
            }

            Context.Server.Save();
        }
    }
}
