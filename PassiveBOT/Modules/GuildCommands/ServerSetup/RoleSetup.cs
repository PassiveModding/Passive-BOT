namespace PassiveBOT.Modules.GuildCommands.ServerSetup
{
    using System.Threading.Tasks;

    using global::Discord;
    using global::Discord.Commands;

    using PassiveBOT.Discord.Context;
    using PassiveBOT.Discord.Preconditions;

    /// <summary>
    /// The role setup.
    /// </summary>
    [GuildOwner]
    public class RoleSetup : Base
    {
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
        public async Task Admin(IRole role)
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
        public async Task Moderator(IRole role)
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
        [Summary("Add a public role (or remove it)")]
        public async Task SetSub(IRole role)
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
