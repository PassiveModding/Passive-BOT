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
        /// Responds with the relevant level up messages
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="setup">
        /// The setup.
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
        public static async Task DoResponseAsync(SocketUserMessage msg, LevelService.LevelSetup setup, LevelService.LevelSetup.LevelUser levelUser, int requiredXP, string roleAdded)
        {
            var gChannel = msg.Channel as ITextChannel;
            var guild = gChannel.Guild as SocketGuild;
            var gUser = msg.Author as SocketGuildUser;

            var embed = new EmbedBuilder { Title = $"{gUser.Username} Leveled Up!", ThumbnailUrl = gUser.GetAvatarUrl(), Description = $"Level: {levelUser.Level - 1}\n" + $"{roleAdded}" + $"XP: {requiredXP}\n" + $"Next Level At: {levelUser.Level * 50 + levelUser.Level * levelUser.Level * 25} XP", Color = Color.Blue };
            if (setup.Settings.UseLogChannel)
            {
                try
                {
                    if (guild.GetChannel(setup.Settings.LogChannelID) is IMessageChannel chan)
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
                    await gChannel.SendMessageAsync(string.Empty, false, embed.Build());
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
                    embed.Title = $"You Leveled up in {guild.Name}!";
                    await gUser.SendMessageAsync(string.Empty, false, embed.Build());
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
        /// Removes un-necessary roles from the user
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="guild">
        /// The guild.
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
        public static async Task RemoveRolesAsync(SocketGuildUser user, SocketGuild guild, List<LevelService.LevelSetup.LevelReward> rolesToReceive, List<LevelService.LevelSetup.LevelReward> rolesAvailable)
        {
            if (rolesToReceive.Count != rolesAvailable.Count && rolesToReceive.Count == 1)
            {
                await Task.Run(
                    async () =>
                        {
                            try
                            {
                                rolesAvailable.Remove(rolesToReceive.First());
                                var toRemove = rolesAvailable.Select(x => guild.GetRole(x.RoleID)).Where(x => x != null);

                                await user.RemoveRolesAsync(toRemove);
                            }
                            catch
                            {
                                // Ignored
                            }
                        });
            }
        }

        /// <summary>
        /// Updates user XP and saves the server
        /// </summary>
        /// <param name="msg">
        /// The msg.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task DoLevelsAsync(SocketUserMessage msg)
        {
            if (msg.Channel is IDMChannel)
            {
                return;
            }

            var gChannel = msg.Channel as IGuildChannel;
            var guild = gChannel.Guild as SocketGuild;
            var gUser = msg.Author as SocketGuildUser;

            var levels = Service.GetLevelSetup(guild.Id);

            if (levels == null)
            {
                return;
            }

            if (!levels.Settings.Enabled)
            {
                return;
            }

            if (!levels.Users.ContainsKey(msg.Author.Id))
            {
                levels.Users.TryAdd(msg.Author.Id, new LevelService.LevelSetup.LevelUser(msg.Author.Id));
                levels.Save();
                return;
            }

            var levelUser = levels.Users[msg.Author.Id];
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
                            if (((IGuildUser)msg.Author).RoleIds.Contains(role.RoleID))
                            {
                                continue;
                            }

                            var socketRole = guild.GetRole(role.RoleID);
                            if (socketRole != null)
                            {
                                try
                                {
                                    await gUser.AddRoleAsync(socketRole);
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

                        await RemoveRolesAsync(gUser, guild, rolesToReceive, rolesAvailable);
                    }
                }

                await DoResponseAsync(msg, levels, levelUser, requiredXP, roleAdded);
            }

            levels.Save();
        }
    }
}