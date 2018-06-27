namespace PassiveBOT.Modules.GuildCommands
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using global::Discord;
    using global::Discord.Commands;

    using PassiveBOT.Discord.Context;

    /// <summary>
    /// The roles module
    /// </summary>
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    [Summary("Join/Leave Roles")]
    public class Roles : Base
    {
        /// <summary>
        /// Joins a role
        /// </summary>
        /// <param name="role">
        /// The role.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Throws if role is not enabled for joining
        /// </exception>
        [Command("Sub")]
        [Alias("JoinRole")]
        [Summary("Join a public role, or leave it")]
        public async Task SubscribeAsync(IRole role = null)
        {
            if (role == null)
            {
                var roleList = Context.Guild.Roles.Where(x => Context.Server.Moderation.SubRoleIDs.Contains(x.Id));
                await ReplyAsync(new EmbedBuilder
                 {
                     Title = "Public Roles",
                     Description = string.Join("\n", roleList.Select(x => x.Name)) + "\n\nYou can join any of the roles in this list using the command:\n" +
                                   $"`{Context.Prefix}sub <@role>`"
                 });
            }

            if (Context.Server.Moderation.SubRoleIDs.Contains(role.Id))
            {
                var guildUser = Context.User as IGuildUser;
                if (guildUser.RoleIds.Contains(role.Id))
                {
                    await guildUser.RemoveRoleAsync(role);
                    await SimpleEmbedAsync($"Success, you have been removed from the role {role.Mention}");
                }
                else
                {
                    await guildUser.AddRoleAsync(role);
                    await SimpleEmbedAsync($"Success, you have been given the role {role.Mention}");
                }
            }
            else
            {
                throw new Exception("This role is not a sub role");
            }
        }
    }
}
