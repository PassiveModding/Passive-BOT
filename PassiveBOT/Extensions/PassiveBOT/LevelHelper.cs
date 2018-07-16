namespace PassiveBOT.Extensions.PassiveBOT
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Discord;
    using Discord.WebSocket;

    using global::PassiveBOT.Context;
    using global::PassiveBOT.Services;

    /// <summary>
    ///     The message helper.
    /// </summary>
    public class LevelHelper
    {
        public LevelHelper(LevelService service)
        {
            Service = service;
        }

        private static LevelService Service { get; set; }

        /// <summary>
        ///     Responds with the relevant level up messages
        /// </summary>
        /// <param name="context">
        ///     The context.
        /// </param>
        /// <param name="levelUser">
        ///     The level user.
        /// </param>
        /// <param name="requiredXP">
        ///     The required xp.
        /// </param>
        /// <param name="roleAdded">
        ///     The role added.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        public static async Task DoResponseAsync(Context context, LevelService.LevelSetup setup, LevelService.LevelSetup.LevelUser levelUser, int requiredXP, string roleAdded)
        {
            var embed = new EmbedBuilder { Title = $"{context.User.Username} Leveled Up!", ThumbnailUrl = context.User.GetAvatarUrl(), Description = $"Level: {levelUser.Level - 1}\n" + $"{roleAdded}" + $"XP: {requiredXP}\n" + $"Next Level At: {levelUser.Level * 50 + levelUser.Level * levelUser.Level * 25} XP", Color = Color.Blue };
            if (setup.Settings.UseLogChannel)
            {
                try
                {
                    if (context.Guild.GetChannel(setup.Settings.LogChannelID) is IMessageChannel chan)
                    {
                        await chan.SendMessageAsync(string.Empty, false, embed.Build());
                    }
                }
                catch
                {
                    // Ignored
                }
            }

            if (setup.Settings.ReplyLevelUps)
            {
                try
                {
                    await context.Channel.SendMessageAsync(string.Empty, false, embed.Build());
                }
                catch
                {
                    // Ignored
                }
            }

            if (setup.Settings.DMLevelUps)
            {
                try
                {
                    embed.Title = $"You Leveled up in {context.Guild.Name}!";
                    await context.User.SendMessageAsync(string.Empty, false, embed.Build());
                }
                catch
                {
                    // Ignored
                }
            }
        }

        /// <summary>
        ///     Returns a list of roles for the user to receive and a list of all roles available
        /// </summary>
        /// <param name="setup">
        ///     The setup.
        /// </param>
        /// <param name="levelUser">
        ///     The level user.
        /// </param>
        /// <returns>
        ///     The <see cref="KeyValuePair" />.
        /// </returns>
        public static KeyValuePair<List<LevelService.LevelSetup.LevelReward>, List<LevelService.LevelSetup.LevelReward>> GetRoles(LevelService.LevelSetup setup, LevelService.LevelSetup.LevelUser levelUser)
        {
            var rolesAvailable = setup.RewardRoles.Where(x => x.Requirement <= levelUser.Level - 1).ToList();
            var rolesToReceive = new List<LevelService.LevelSetup.LevelReward>();

            if (!rolesAvailable.Any())
            {
                return new KeyValuePair<List<LevelService.LevelSetup.LevelReward>, List<LevelService.LevelSetup.LevelReward>>(rolesToReceive, rolesAvailable);
            }

            if (setup.Settings.IncrementLevelRewards)
            {
                var max = rolesAvailable.Max(x => x.Requirement);
                rolesToReceive.Add(rolesAvailable.FirstOrDefault(x => x.Requirement == max));
            }
            else
            {
                rolesToReceive = rolesAvailable;
            }

            return new KeyValuePair<List<LevelService.LevelSetup.LevelReward>, List<LevelService.LevelSetup.LevelReward>>(rolesToReceive, rolesAvailable);
        }

        /// <summary>
        ///     Removes un-necessary roles from the user
        /// </summary>
        /// <param name="context">
        ///     The context.
        /// </param>
        /// <param name="rolesToReceive">
        ///     The roles to receive.
        /// </param>
        /// <param name="rolesAvailable">
        ///     The roles available.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        public static async Task RemoveRolesAsync(Context context, List<LevelService.LevelSetup.LevelReward> rolesToReceive, List<LevelService.LevelSetup.LevelReward> rolesAvailable)
        {
            if (rolesToReceive.Count != rolesAvailable.Count && rolesToReceive.Count == 1)
            {
                await Task.Run(
                    async () =>
                        {
                            try
                            {
                                rolesAvailable.Remove(rolesToReceive.First());
                                var toRemove = rolesAvailable.Select(x => context.Guild.GetRole(x.RoleID)).Where(x => x != null);

                                await (context.User as SocketGuildUser).RemoveRolesAsync(toRemove);
                            }
                            catch
                            {
                                // Ignored
                            }
                        });
            }
        }

        /// <summary>
        ///     Updates user XP and saves the server
        /// </summary>
        /// <param name="context">
        ///     The context.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        public async Task DoLevelsAsync(Context context)
        {
            if (context.Channel is IDMChannel)
            {
                return;
            }

            var levels = Service.GetLevelSetup(context.Guild.Id);

            if (!levels.Settings.Enabled)
            {
                return;
            }

            if (!levels.Users.ContainsKey(context.User.Id))
            {
                levels.Users.TryAdd(context.User.Id, new LevelService.LevelSetup.LevelUser(context.User.Id));
                levels.Save();
                return;
            }

            var levelUser = levels.Users[context.User.Id];
            if (levelUser.LastUpdate > DateTime.UtcNow)
            {
                return;
            }

            levelUser.XP += 10;
            levelUser.LastUpdate = DateTime.UtcNow + TimeSpan.FromMinutes(1);

            var requiredXP = (levelUser.Level * 50) + ((levelUser.Level * levelUser.Level) * 25);
            if (levelUser.XP > requiredXP)
            {
                levelUser.Level++;
                string roleAdded = null;
                if (levels.RewardRoles.Any())
                {
                    var roles = GetRoles(levels, levelUser);
                    var rolesToReceive = roles.Key;
                    var rolesAvailable = roles.Value;
                    if (rolesToReceive.Count != 0)
                    {
                        foreach (var role in rolesToReceive)
                        {
                            if (((IGuildUser)context.User).RoleIds.Contains(role.RoleID))
                            {
                                continue;
                            }

                            var socketRole = context.Guild.GetRole(role.RoleID);
                            if (socketRole != null)
                            {
                                try
                                {
                                    await (context.User as SocketGuildUser).AddRoleAsync(socketRole);
                                    roleAdded += $"Role Reward: {socketRole.Name}\n";
                                }
                                catch
                                {
                                    // Ignored
                                }
                            }
                            else
                            {
                                levels.RewardRoles.Remove(role);
                            }
                        }

                        await RemoveRolesAsync(context, rolesToReceive, rolesAvailable);
                    }
                }

                await DoResponseAsync(context, levels, levelUser, requiredXP, roleAdded);
            }

            levels.Save();
        }
    }
}