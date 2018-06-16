namespace PassiveBOT.Discord.Extensions.PassiveBOT
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using global::Discord;
    using global::Discord.WebSocket;

    using global::PassiveBOT.Discord.Context;
    using global::PassiveBOT.Models;

    /// <summary>
    /// The message helper.
    /// </summary>
    public class LevelHelper
    {
        /// <summary>
        /// Updates user XP and saves the server
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static async Task<Context> DoLevels(Context context)
        {
            if (context.Channel is IDMChannel)
            {
                return context;
            }

            if (!context.Server.Levels.Settings.Enabled)
            {
                return context;
            }

            var levelUser = context.Server.Levels.Users.FirstOrDefault(x => x.UserID == context.User.Id);
            if (levelUser == null)
            {
                return await InitializeUser(context);
            }

            if (levelUser.LastUpdate > DateTime.UtcNow)
            {
                return context;
            }

            levelUser.XP += 10;
            levelUser.LastUpdate = DateTime.UtcNow + TimeSpan.FromMinutes(1);

            var requiredXP = (levelUser.Level * 50) + ((levelUser.Level * levelUser.Level) * 25);
            if (levelUser.XP > requiredXP)
            {
                levelUser.Level++;
                string roleAdded = null;
                if (context.Server.Levels.RewardRoles.Any())
                {
                    var roles = GetRoles(context, levelUser);
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
                                context.Server.Levels.RewardRoles.Remove(role);
                            }
                        }

                        await RemoveRoles(context, rolesToReceive, rolesAvailable);
                    }
                }

                await DoResponse(context, levelUser, requiredXP, roleAdded);
            }

            context.Server.Save();
            return context;
        }

        /// <summary>
        /// Returns a list of roles for the user to receive and a list of all roles available
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="levelUser">
        /// The level user.
        /// </param>
        /// <returns>
        /// The <see cref="KeyValuePair"/>.
        /// </returns>
        public static KeyValuePair<List<GuildModel.LevelSetup.LevelReward>, List<GuildModel.LevelSetup.LevelReward>> GetRoles(Context context, GuildModel.LevelSetup.LevelUser levelUser)
        {
            var rolesAvailable = context.Server.Levels.RewardRoles.Where(x => x.Requirement <= levelUser.Level - 1).ToList();
            var rolesToReceive = new List<GuildModel.LevelSetup.LevelReward>();

            if (!rolesAvailable.Any())
            {
                return new KeyValuePair<List<GuildModel.LevelSetup.LevelReward>, List<GuildModel.LevelSetup.LevelReward>>(rolesToReceive, rolesAvailable);
            }

            if (context.Server.Levels.Settings.IncrementLevelRewards)
            {
                var max = rolesAvailable.Max(x => x.Requirement);
                rolesToReceive.Add(rolesAvailable.FirstOrDefault(x => x.Requirement == max));
            }
            else
            {
                rolesToReceive = rolesAvailable;
            }

            return new KeyValuePair<List<GuildModel.LevelSetup.LevelReward>, List<GuildModel.LevelSetup.LevelReward>>(rolesToReceive, rolesAvailable);
        }

        /// <summary>
        /// Removes un-necessary roles from the user
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="rolesToReceive">
        /// The roles to receive.
        /// </param>
        /// <param name="rolesAvailable">
        /// The roles available.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static async Task RemoveRoles(Context context, List<GuildModel.LevelSetup.LevelReward> rolesToReceive, List<GuildModel.LevelSetup.LevelReward> rolesAvailable)
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
        /// Initializes a user in the leveling system
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static Task<Context> InitializeUser(Context context)
        {
            context.Server.Levels.Users.Add(new GuildModel.LevelSetup.LevelUser
            {
                Level = 1,
                UserID = context.User.Id,
                XP = 0,
                LastUpdate = DateTime.UtcNow + TimeSpan.FromMinutes(1)
            });

            context.Server.Save();
            return Task.FromResult(context);
        }

        /// <summary>
        /// Responds with the relevant level up messages
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="levelUser">
        /// The level user.
        /// </param>
        /// <param name="requiredXP">
        /// The required xp.
        /// </param>
        /// <param name="roleAdded">
        /// The role added.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static async Task DoResponse(Context context, GuildModel.LevelSetup.LevelUser levelUser, int requiredXP, string roleAdded)
        {
            var embed = new EmbedBuilder
            {
                Title = $"{context.User.Username} Leveled Up!",
                ThumbnailUrl = context.User.GetAvatarUrl(),
                Description = $"Level: {levelUser.Level - 1}\n" +
                              $"{roleAdded}" +
                              $"XP: {requiredXP}\n" +
                              $"Next Level At: {levelUser.Level * 50 + levelUser.Level * levelUser.Level * 25} XP",
                Color = Color.Blue
            };
            if (context.Server.Levels.Settings.UseLogChannel)
            {
                try
                {
                    if (context.Guild.GetChannel(context.Server.Levels.Settings.LogChannelID) is IMessageChannel chan)
                    {
                        await chan.SendMessageAsync(string.Empty, false, embed.Build());
                    }
                }
                catch
                {
                    // Ignored
                }
            }

            if (context.Server.Levels.Settings.ReplyLevelUps)
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

            if (context.Server.Levels.Settings.DMLevelUps)
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
    }
}
