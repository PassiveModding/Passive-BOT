using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Configuration;
using PassiveBOT.Handlers.Services.Interactive;
using PassiveBOT.Preconditions;

namespace PassiveBOT.Commands.ServerSetup
{
    [RequireAdmin]
    [RequireContext(ContextType.Guild)]
    public class Levelling : InteractiveBase
    {
        [Command("ToggleLevelling")]
        [Summary("ToggleLevelling")]
        [Remarks("Toggle Levelling system on or off")]
        public async Task ToggleSystem()
        {
            var GuildObj = GuildConfig.GetServer(Context.Guild);
            GuildObj.Levels.LevellingEnabled = !GuildObj.Levels.LevellingEnabled;
            await ReplyAsync($"Levelling System is now set to: {GuildObj.Levels.LevellingEnabled}");
            GuildConfig.SaveServer(GuildObj);
        }

        [Command("ToggleLevelUpMessages")]
        [Summary("ToggleLevelUpMessages")]
        [Remarks("Toggle LevelUp messages on or off")]
        public async Task ToggleLevelUp()
        {
            var GuildObj = GuildConfig.GetServer(Context.Guild);
            GuildObj.Levels.UseLevelMessages = !GuildObj.Levels.UseLevelMessages;
            await ReplyAsync($"Level Up Notifications are now set to: {GuildObj.Levels.UseLevelMessages}");
            GuildConfig.SaveServer(GuildObj);
        }

        [Command("SetLevelChannel")]
        [Summary("SetLevelChannel")]
        [Remarks("Set the Current channel to show all level ups")]
        public async Task SetLevelChannel()
        {
            var GuildObj = GuildConfig.GetServer(Context.Guild);
            GuildObj.Levels.LevellingChannel = Context.Channel.Id;
            await ReplyAsync($"Level Up Notifications will now be sent to: {Context.Channel.Name}");
            GuildConfig.SaveServer(GuildObj);
        }

        [Command("ToggleLevelChannel")]
        [Summary("ToggleLevelChannel")]
        [Remarks("Toggle Logging Level ups in a specific channel")]
        public async Task ToggleLevelChannel()
        {
            var GuildObj = GuildConfig.GetServer(Context.Guild);
            GuildObj.Levels.UseLevelChannel = !GuildObj.Levels.UseLevelChannel;
            await ReplyAsync(
                $"Sending Level Up notifications to a specific channel is now set to: {GuildObj.Levels.UseLevelChannel}");
            GuildConfig.SaveServer(GuildObj);
        }

        [Command("ToggleIncrementalLevelling")]
        [Summary("ToggleIncrementalLevelling")]
        [Remarks("Toggle wether to give a user only one role reward or all")]
        public async Task ToggleLevelIncrement()
        {
            var GuildObj = GuildConfig.GetServer(Context.Guild);
            GuildObj.Levels.IncrementLevelRewards = !GuildObj.Levels.IncrementLevelRewards;
            await ReplyAsync(
                $"Users will only have one level reward at a time: {GuildObj.Levels.IncrementLevelRewards}");
            GuildConfig.SaveServer(GuildObj);
        }

        [Command("AddLevel")]
        [Summary("AddLevel <@role> <Level>")]
        [Remarks("Add a role which users may receive upon getting a certain level")]
        public async Task AddLevel(IRole role, int level)
        {
            var GuildObj = GuildConfig.GetServer(Context.Guild);
            if (GuildObj.Levels.LevelRoles.Any(x => x.RoleID == role.Id))
            {
                await ReplyAsync($"This role is already a level you may remove it using the removelevel command!");
                return;
            }

            if (level <= 0)
            {
                await ReplyAsync($"Levels must be greater than zero");
                return;
            }

            GuildObj.Levels.LevelRoles.Add(new GuildConfig.levelling.Level
            {
                RoleID = role.Id,
                LevelToEnter = level
            });

            await ReplyAsync($"New Level Added: {role.Name}\n" +
                             $"Level Requirement: {level}");
            GuildConfig.SaveServer(GuildObj);
        }

        [Command("RemoveLevel")]
        [Summary("RemoveLevel <@role> <Level>")]
        [Remarks("Remove a level role")]
        public async Task Removelevel(IRole role)
        {
            var GuildObj = GuildConfig.GetServer(Context.Guild);
            if (GuildObj.Levels.LevelRoles.Any(x => x.RoleID == role.Id))
            {
                GuildObj.Levels.LevelRoles.Remove(GuildObj.Levels.LevelRoles.FirstOrDefault(x => x.RoleID == role.Id));
                await ReplyAsync($"Success Level Role has been removed.");
                GuildConfig.SaveServer(GuildObj);
            }
            else
            {
                await ReplyAsync($"ERROR: This role is not a level!");
            }
        }

        [Command("BanUserLevel")]
        [Summary("BanUserLevel <@user>")]
        [Remarks("Ban a user from using the level system")]
        public async Task AddLevel(IUser user)
        {
            var GuildObj = GuildConfig.GetServer(Context.Guild);
            var UserObj = GuildObj.Levels.Users.FirstOrDefault(x => x.userID == user.Id);
            if (UserObj != null)
            {
                UserObj.banned = !UserObj.banned;
                await ReplyAsync($"User Banned: {UserObj.banned}");
                GuildConfig.SaveServer(GuildObj);
            }
            else
            {
                await ReplyAsync($"ERROR: This user has not sent a message in the server before!");
            }
        }

        [Command("ResetLeaderboard")]
        [Summary("ResetLeaderboard")]
        [Remarks("Reset all user's levels and XP for the levelling system")]
        public async Task LeaderboardReser([Remainder] string confirm = null)
        {
            if (confirm != "su8GhbY")
            {
                await ReplyAsync("Please run the command again and use the confirmation code: `su8GhbY`\n" +
                                 "NOTE: This reset cannot be undone!");
                return;
            }

            var GuildObj = GuildConfig.GetServer(Context.Guild);
            GuildObj.Levels.Users = new List<GuildConfig.levelling.user>();
            GuildConfig.SaveServer(GuildObj);
            await ReplyAsync(
                $"All user XP and Levels have been reset (note: Role Rewards will have to be manually reset if applicable)");
        }
    }
}