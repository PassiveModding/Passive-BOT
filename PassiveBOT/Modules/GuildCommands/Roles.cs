namespace PassiveBOT.Modules.GuildCommands
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Addons.PrefixService;
    using Discord.Commands;
    using Discord.WebSocket;

    using PassiveBOT.Context;
    using PassiveBOT.Services;

    /// <summary>
    ///     The roles module
    /// </summary>
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    [Summary("Join/Leave Roles")]
    public class Roles : Base
    {
        public Roles(PrefixService prefixService, WaitService wait)
        {
            PrefixService = prefixService;
            WaitService = wait;
        }

        private WaitService WaitService { get; }
        private PrefixService PrefixService { get; }

        /// <summary>
        ///     Joins a role
        /// </summary>
        /// <param name="role">
        ///     The role.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        /// <exception cref="Exception">
        ///     Throws if role is not enabled for joining
        /// </exception>
        [Command("Sub", RunMode = RunMode.Async)]
        [Alias("JoinRole")]
        [Summary("Join a public role, or leave it")]
        public async Task SubscribeAsync(IRole role = null)
        {
            if (role == null)
            {
                var roleList = Context.Guild.Roles.Where(x => Context.Server.Moderation.SubRoleIDs.Contains(x.Id));
                await ReplyAsync(new EmbedBuilder { Title = "Public Roles", Description = string.Join("\n", roleList.Select(x => x.Name)) + "\n\nYou can join any of the roles in this list using the command:\n" + $"`{PrefixService.GetPrefix(Context.Guild.Id)}sub <@role>`" });
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

        [Command("TempRole")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Give a user a role for the given amount of time.")]
        public async Task SetSubAsync(IRole role, SocketGuildUser user, TimeSpan time)
        {
            try
            {
                if (role.Position >= Context.Guild.GetUser(Context.Client.CurrentUser.Id).Hierarchy)
                {
                    await SimpleEmbedAsync("Role level is higher than the bot and cannot be applied to another user.");
                    return;
                }

                await user.AddRoleAsync(role);
                var roleModel = WaitService.AddTempRole(Context.Guild.Id, user.Id, role.Id, time);
                await SimpleEmbedAsync($"{user.Mention} will have the role {role.Mention} until: {roleModel.ExpiresOn.ToShortDateString()} {roleModel.ExpiresOn.ToShortTimeString()}");
            }
            catch (Exception e)
            {
                await SimpleEmbedAsync("There was an error adding the role to the user");
                Console.WriteLine(e.ToString());
            }
        }
    }
}