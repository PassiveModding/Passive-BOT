using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Discord.Context;
using PassiveBOT.Discord.Preconditions;
using PassiveBOT.Models;

namespace PassiveBOT.Modules.GuildSetup
{
    [RequireAdmin]
    [RequireContext(ContextType.Guild)]
    [Group("Levelling")]
    public class Levelling : Base
    {
        [Command("Toggle")]
        [Summary("Toggle")]
        [Remarks("Toggle Levelling system on or off")]
        public async Task ToggleSystem()
        {
            Context.Server.Levels.Settings.Enabled = !Context.Server.Levels.Settings.Enabled;
            await ReplyAsync($"Levelling System Enabled: {Context.Server.Levels.Settings.Enabled}");
            Context.Server.Save();
        }

        [Command("ToggleMessages")]
        [Summary("ToggleMessages")]
        [Remarks("Toggle LevelUp messages on or off")]
        public async Task ToggleLevelUp()
        {
            Context.Server.Levels.Settings.ReplyLevelUps = !Context.Server.Levels.Settings.ReplyLevelUps;
            await ReplyAsync($"Level Up Notifications Enabled: {Context.Server.Levels.Settings.ReplyLevelUps}");
            Context.Server.Save();
        }

        [Command("ToggleDMs")]
        [Summary("ToggleDMs")]
        [Remarks("Toggle Direct Messaging of Level Up Messages")]
        public async Task ToggleDMReplies()
        {
            Context.Server.Levels.Settings.DMLevelUps = !Context.Server.Levels.Settings.DMLevelUps;
            await ReplyAsync($"Users will recieve a direct message upon levelling up: {Context.Server.Levels.Settings.DMLevelUps}");
            Context.Server.Save();
        }

        [Command("SetChannel")]
        [Summary("SetChannel")]
        [Remarks("Set the Current channel to show all level ups")]
        public async Task SetLevelChannel()
        {
            Context.Server.Levels.Settings.LogChannelID = Context.Channel.Id;
            await ReplyAsync($"Level Up Notifications will now be sent to: {Context.Channel.Name}");
            Context.Server.Save();
        }

        [Command("ToggleChannel")]
        [Summary("ToggleChannel")]
        [Remarks("Toggle Logging Level ups in a specific channel")]
        public async Task ToggleLevelChannel()
        {
            Context.Server.Levels.Settings.UseLogChannel = !Context.Server.Levels.Settings.UseLogChannel;
            await ReplyAsync($"Log Level up Messages: {Context.Server.Levels.Settings.UseLogChannel}");
            Context.Server.Save();
        }

        [Command("ToggleIncrementalLevelling")]
        [Summary("ToggleIncrementalLevelling")]
        [Remarks("Toggle wether to give a user only one role reward or all")]
        public async Task ToggleLevelIncrement()
        {
            Context.Server.Levels.Settings.IncrementLevelRewards = !Context.Server.Levels.Settings.IncrementLevelRewards;
            await ReplyAsync(
                $"Users will only have one level reward at a time: {Context.Server.Levels.Settings.IncrementLevelRewards}");
            Context.Server.Save();
        }

        [Command("AddLevel")]
        [Summary("AddLevel <@role> <Level>")]
        [Remarks("Add a role which users may receive upon getting a certain level")]
        public async Task AddLevel(IRole role, int level)
        {
            if (Context.Server.Levels.RewardRoles.Any(x => x.RoleID == role.Id))
            {
                await ReplyAsync("This role is already a level you may remove it using the removelevel command!");
                return;
            }

            if (level <= 0)
            {
                await ReplyAsync("Levels must be greater than zero");
                return;
            }

            Context.Server.Levels.RewardRoles.Add(new GuildModel.levelling.levelreward
            {
                RoleID = role.Id,
                Requirement = level
            });

            await ReplyAsync($"New Level Added: {role.Name}\n" +
                             $"Level Requirement: {level}");
            Context.Server.Save();
        }

        [Command("RemoveLevel")]
        [Summary("RemoveLevel <@role> <Level>")]
        [Remarks("Remove a level role")]
        public async Task Removelevel(IRole role)
        {
            if (Context.Server.Levels.RewardRoles.Any(x => x.RoleID == role.Id))
            {
                Context.Server.Levels.RewardRoles.Remove(Context.Server.Levels.RewardRoles.FirstOrDefault(x => x.RoleID == role.Id));
                await ReplyAsync("Success Level Role has been removed.");
                Context.Server.Save();
            }
            else
            {
                await ReplyAsync("ERROR: This role is not a level!");
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

            Context.Server.Levels.Users = new List<GuildModel.levelling.luser>();
            Context.Server.Save();
            await ReplyAsync("All user XP and Levels have been reset (note: Role Rewards will have to be manually reset if applicable)");
        }
    }
}
