using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Discord.Context;

namespace PassiveBOT.Modules.Data
{
    [RequireContext(ContextType.Guild)]
    public class SubRole : Base
    {
        [Command("Sub")]
        [Summary("Sub <@Role>")]
        [Alias("joinrole")]
        [Remarks("Join a public role, or leave it")]
        public async Task SetSub(IRole SubRole)
        {
            if (Context.Server.Moderation.SubRoleIDs.Contains(SubRole.Id))
            {
                var guser = Context.User as IGuildUser;
                if (guser.RoleIds.Contains(SubRole.Id))
                {
                    await guser.RemoveRoleAsync(SubRole);
                    await SimpleEmbedAsync($"Success, you have been removed from the role {SubRole.Mention}");
                }
                else
                {
                    await guser.AddRoleAsync(SubRole);
                    await SimpleEmbedAsync($"Success, you have been given the role {SubRole.Mention}");
                }
            }
            else
            {
                throw new Exception("This Role is not publically joinable");
            }
        }

        [Command("Sub")]
        [Summary("Sub")]
        [Alias("joinrole")]
        [Remarks("subrole info")]
        public async Task SetSub()
        {
            var rolelist = Context.Guild.Roles.Where(x => Context.Server.Moderation.SubRoleIDs.Contains(x.Id));
            await ReplyAsync(new EmbedBuilder
            {
                Title = "Public Roles",
                Description = string.Join("\n", rolelist.Select(x => x.Name)) + "\n\nYou can join any of the roles in this list using the command:\n" +
                              $"`{Context.Prefix}sub <@role>`"
            });
        }
    }
}