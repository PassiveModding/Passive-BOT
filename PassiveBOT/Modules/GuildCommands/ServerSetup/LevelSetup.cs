namespace PassiveBOT.Modules.GuildCommands.ServerSetup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Commands;

    using PassiveBOT.Context;
    using PassiveBOT.Preconditions;
    using PassiveBOT.Services;

    /// <summary>
    /// The level setup commands
    /// </summary>
    [RequireAdmin]
    [RequireContext(ContextType.Guild)]
    [Group("Leveling")]
    [Summary("Users can gain levels, special roles and xp based on activity in the server.")]
    public class LevelSetup : Base
    {
        private static LevelService Service { get; set; }

        public LevelSetup(LevelService service)
        {
            Service = service;
        }

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
            var leveling = Service.GetLevelSetup(Context.Guild.Id);
            return SimpleEmbedAsync($"Enabled: {leveling.Settings.Enabled}\n" + 
                                    $"Incremental Rewards: {leveling.Settings.IncrementLevelRewards}\n" + 
                                    "**Messaging**\n" + 
                                    $"Reply In Channel: {leveling.Settings.ReplyLevelUps}\n" + 
                                    $"DM Level Ups: {leveling.Settings.DMLevelUps}\n" + 
                                    $"Using Log Channel: {(leveling.Settings.UseLogChannel ? $"{Context.Guild.GetChannel(leveling.Settings.LogChannelID)?.Name}" : "false")}\n" + 
                                    "**Users**\n" + 
                                    $"Level User Count: {leveling.Users.Count}\n" + 
                                    $"Total Levels: {leveling.Users.Sum(x => x.Value.Level)}\n" + 
                                    $"Total XP: {leveling.Users.Sum(x => x.Value.XP)}\n" + 
                                    $"Highest Level & XP: {leveling.Users.Max(x => x.Value.Level)} || {leveling.Users.Max(x => x.Value.XP)}\n" + 
                                    "**Reward Roles**\n" + 
                                    $"{string.Join("\n", leveling.RewardRoles.OrderByDescending(x => x.Requirement).Where(x => Context.Guild.GetRole(x.RoleID) != null).Select(x => $"{x.Requirement} - {Context.Guild.GetRole(x.RoleID).Mention}"))}");
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
            var l = Service.GetLevelSetup(Context.Guild.Id);
            l.Settings.Enabled = !l.Settings.Enabled;
            l.Save();
            return SimpleEmbedAsync($"Leveling System Enabled: {l.Settings.Enabled}");
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
            var l = Service.GetLevelSetup(Context.Guild.Id);
            l.Settings.ReplyLevelUps = !l.Settings.ReplyLevelUps;
            l.Save();
            return SimpleEmbedAsync($"Level Up Notifications Enabled: {l.Settings.ReplyLevelUps}");
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
            var l = Service.GetLevelSetup(Context.Guild.Id);
            l.Settings.DMLevelUps = !l.Settings.DMLevelUps;
            l.Save();
            return SimpleEmbedAsync($"Users will receive a direct message upon leveling up: {l.Settings.DMLevelUps}");
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
            var l = Service.GetLevelSetup(Context.Guild.Id);
            l.Settings.LogChannelID = Context.Channel.Id;
            l.Save();
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
            var l = Service.GetLevelSetup(Context.Guild.Id);
            l.Settings.UseLogChannel = !l.Settings.UseLogChannel;
            l.Save();
            return SimpleEmbedAsync($"Log Level up Messages: {l.Settings.UseLogChannel}");
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
            var l = Service.GetLevelSetup(Context.Guild.Id);
            l.Settings.IncrementLevelRewards = !l.Settings.IncrementLevelRewards;
            l.Save();
            return SimpleEmbedAsync($"Users will only have one level reward at a time: {l.Settings.IncrementLevelRewards}");
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
            var l = Service.GetLevelSetup(Context.Guild.Id);
            if (l.RewardRoles.Any(x => x.RoleID == role.Id))
            {
                throw new Exception("This role is already a level you may remove it using the removeLevel command!");
            }

            if (level <= 0)
            {
                throw new Exception("Levels must be greater than zero");
            }

            if (l.RewardRoles.Any(x => x.Requirement == level))
            {
                throw new Exception("Users can only gain one role per level reward.");
            }

            l.RewardRoles.Add(new LevelService.LevelSetup.LevelReward
            {
                RoleID = role.Id,
                Requirement = level
            });
            l.Save();
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
            var l = Service.GetLevelSetup(Context.Guild.Id);
            if (l.RewardRoles.Any(x => x.RoleID == role.Id))
            {
                l.RewardRoles.Remove(l.RewardRoles.FirstOrDefault(x => x.RoleID == role.Id));
                l.Save();
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
            var l = Service.GetLevelSetup(Context.Guild.Id);
            if (l.RewardRoles.Any(x => x.RoleID == roleId))
            {
                l.RewardRoles.Remove(l.RewardRoles.FirstOrDefault(x => x.RoleID == roleId));
                l.Save();
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

            var l = Service.GetLevelSetup(Context.Guild.Id);
            l.Users = new Dictionary<ulong, LevelService.LevelSetup.LevelUser>();
            l.Save();
            await SimpleEmbedAsync("All user XP and Levels have been reset (note: Role Rewards will have to be manually reset if applicable)");
        }
    }
}