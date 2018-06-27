namespace PassiveBOT.Modules.GuildCommands.ServerSetup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using global::Discord;
    using global::Discord.Commands;

    using PassiveBOT.Discord.Context;
    using PassiveBOT.Discord.Preconditions;
    using PassiveBOT.Models;

    /// <summary>
    /// The level setup commands
    /// </summary>
    [RequireAdmin]
    [RequireContext(ContextType.Guild)]
    [Group("Leveling")]
    [Summary("Users can gain levels, special roles and xp based on activity in the server.")]
    public class LevelSetup : Base
    {
        /// <summary>
        /// The level setup task.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("LevelSetup")]
        [Summary("Setup information for the leveling module")]
        public Task LevelSetupTaskAsync()
        {
            var leveling = Context.Server.Levels;
            return SimpleEmbedAsync($"Enabled: {leveling.Settings.Enabled}\n" + 
                                    $"Incremental Rewards: {leveling.Settings.IncrementLevelRewards}\n" + 
                                    "**Messaging**\n" + 
                                    $"Reply In Channel: {leveling.Settings.ReplyLevelUps}\n" + 
                                    $"DM Level Ups: {leveling.Settings.DMLevelUps}\n" + 
                                    $"Using Log Channel: {(leveling.Settings.UseLogChannel ? $"{Context.Guild.GetChannel(leveling.Settings.LogChannelID)?.Name}" : "false")}\n" + 
                                    "**Users**\n" + 
                                    $"Level User Count: {leveling.Users.Count}\n" + 
                                    $"Total Levels: {leveling.Users.Sum(x => x.Level)}\n" + 
                                    $"Total XP: {leveling.Users.Sum(x => x.XP)}\n" + 
                                    $"Highest Level & XP: {leveling.Users.Max(x => x.Level)} || {leveling.Users.Max(x => x.XP)}\n" + 
                                    "**Reward Roles**\n" + 
                                    $"{string.Join("\n", Context.Server.Levels.RewardRoles.OrderByDescending(x => x.Requirement).Where(x => Context.Guild.GetRole(x.RoleID) != null).Select(x => $"{x.Requirement} - {Context.Guild.GetRole(x.RoleID).Mention}"))}");
        }

        /// <summary>
        /// The toggle system.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("Toggle")]
        [Summary("Toggle Leveling system on or off")]
        public Task ToggleSystemAsync()
        {
            Context.Server.Levels.Settings.Enabled = !Context.Server.Levels.Settings.Enabled;
            Context.Server.Save();
            return SimpleEmbedAsync($"Leveling System Enabled: {Context.Server.Levels.Settings.Enabled}");
        }

        /// <summary>
        /// The toggle level up.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("ToggleMessages")]
        [Summary("Toggle LevelUp messages on or off")]
        public Task ToggleLevelUpAsync()
        {
            Context.Server.Levels.Settings.ReplyLevelUps = !Context.Server.Levels.Settings.ReplyLevelUps;
            Context.Server.Save();
            return SimpleEmbedAsync($"Level Up Notifications Enabled: {Context.Server.Levels.Settings.ReplyLevelUps}");
        }

        /// <summary>
        /// The toggle dm replies.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("ToggleDMs")]
        [Summary("Toggle Direct Messaging of Level Up Messages")]
        public Task ToggleDMRepliesAsync()
        {
            Context.Server.Levels.Settings.DMLevelUps = !Context.Server.Levels.Settings.DMLevelUps;
            Context.Server.Save();
            return SimpleEmbedAsync($"Users will receive a direct message upon leveling up: {Context.Server.Levels.Settings.DMLevelUps}");
        }

        /// <summary>
        /// The set level channel.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("SetChannel")]
        [Summary("Set the Current channel to show all level ups")]
        public Task SetLevelChannelAsync()
        {
            Context.Server.Levels.Settings.LogChannelID = Context.Channel.Id;
            Context.Server.Save();
            return SimpleEmbedAsync($"Level Up Notifications will now be sent to: {Context.Channel.Name}");
        }

        /// <summary>
        /// The toggle level channel.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("ToggleChannel")]
        [Summary("Toggle Logging Level ups in a specific channel")]
        public Task ToggleLevelChannelAsync()
        {
            Context.Server.Levels.Settings.UseLogChannel = !Context.Server.Levels.Settings.UseLogChannel;
            Context.Server.Save();
            return SimpleEmbedAsync($"Log Level up Messages: {Context.Server.Levels.Settings.UseLogChannel}");
        }

        /// <summary>
        /// The toggle level increment.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("ToggleIncrementalLeveling")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Toggle whether to give a user only one role reward or all")]
        public Task ToggleLevelIncrementAsync()
        {
            Context.Server.Levels.Settings.IncrementLevelRewards = !Context.Server.Levels.Settings.IncrementLevelRewards;
            Context.Server.Save();
            return SimpleEmbedAsync($"Users will only have one level reward at a time: {Context.Server.Levels.Settings.IncrementLevelRewards}");
        }

        /// <summary>
        /// The add level.
        /// </summary>
        /// <param name="role">
        /// The role.
        /// </param>
        /// <param name="level">
        /// The level.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("AddLevel")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Add a role which users may receive upon getting a certain level")]
        public Task AddLevelAsync(IRole role, int level)
        {
            if (Context.Server.Levels.RewardRoles.Any(x => x.RoleID == role.Id))
            {
                throw new Exception("This role is already a level you may remove it using the removeLevel command!");
            }

            if (level <= 0)
            {
                throw new Exception("Levels must be greater than zero");
            }

            if (Context.Server.Levels.RewardRoles.Any(x => x.Requirement == level))
            {
                throw new Exception("Users can only gain one role per level reward.");
            }

            Context.Server.Levels.RewardRoles.Add(new GuildModel.LevelSetup.LevelReward
            {
                RoleID = role.Id,
                Requirement = level
            });
            Context.Server.Save();
            return SimpleEmbedAsync($"New Level Added: {role.Name}\n" +
                                    $"Level Requirement: {level}");
            }

        /// <summary>
        /// The Remove Level Command.
        /// </summary>
        /// <param name="role">
        /// The role.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("RemoveLevel")]
        [Summary("Remove a level role")]
        public async Task RemoveLevelAsync(IRole role)
        {
            if (Context.Server.Levels.RewardRoles.Any(x => x.RoleID == role.Id))
            {
                Context.Server.Levels.RewardRoles.Remove(Context.Server.Levels.RewardRoles.FirstOrDefault(x => x.RoleID == role.Id));
                Context.Server.Save();
                await SimpleEmbedAsync("Success Level Role has been removed.");
            }
            else
            {
                throw new Exception("This role is not a level!");
            }
        }

        /// <summary>
        /// The remove level.
        /// </summary>
        /// <param name="roleId">
        /// The role ID
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("RemoveLevel")]
        [Summary("Remove a level role via ID")]
        public async Task RemoveLevelAsync(ulong roleId)
        {
            if (Context.Server.Levels.RewardRoles.Any(x => x.RoleID == roleId))
            {
                Context.Server.Levels.RewardRoles.Remove(Context.Server.Levels.RewardRoles.FirstOrDefault(x => x.RoleID == roleId));
                Context.Server.Save();
                await SimpleEmbedAsync("Success Level Role has been removed.");
            }
            else
            {
                throw new Exception("This role is not a level!");
            }
        }

        /// <summary>
        /// The LeaderBoard reset.
        /// </summary>
        /// <param name="confirm">
        /// The confirm.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("ResetLeaderBoard")]
        [Summary("Reset all user's levels and XP for the leveling system")]
        public async Task LeaderBoardResetAsync([Remainder] string confirm = null)
        {
            if (confirm != "su8GhbY")
            {
                await SimpleEmbedAsync("Please run the command again and use the confirmation code: `su8GhbY`\n" +
                                 "NOTE: This reset cannot be undone!");
                return;
            }

            Context.Server.Levels.Users = new List<GuildModel.LevelSetup.LevelUser>();
            Context.Server.Save();
            await SimpleEmbedAsync("All user XP and Levels have been reset (note: Role Rewards will have to be manually reset if applicable)");
        }
    }
}