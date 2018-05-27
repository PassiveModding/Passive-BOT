using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using PassiveBOT.Discord.Context;
using PassiveBOT.Handlers;
using PassiveBOT.Models;

namespace PassiveBOT.Modules.BotConfig
{
    [RequireOwner]
    [RequireContext(ContextType.Guild)]
    public class Setup : Base
    {
        [Command("SetHomeServer")]
        [Summary("SetHomeServer")]
        [Remarks("Set home server ID")]
        public async Task SetHS()
        {
            var hs = HomeModel.Load();
            hs.ID = Context.Guild.Id;
            hs.Save();
            await SimpleEmbedAsync($"Homeserver Saved!");
        }

        [Command("TogglePartnerLog")]
        [Summary("TogglePartnerLog")]
        [Remarks("Toggle partner logging")]
        public async Task Toggle()
        {
            var hs = HomeModel.Load();
            hs.Logging.LogPartnerChanges = !hs.Logging.LogPartnerChanges;
            hs.Save();
            await SimpleEmbedAsync($"Log Partner Events: {hs.Logging.LogPartnerChanges}");
        }

        [Command("SetPartnerLogChannel")]
        [Summary("SetPartnerLogChannel")]
        [Remarks("Set channel to log partner changed")]
        public async Task SerParther()
        {
            var hs = HomeModel.Load();
            hs.Logging.PartnerLogChannel = Context.Channel.Id;
            hs.Save();
            await SimpleEmbedAsync($"Partner events will be logged in: {Context.Channel.Name}");
        }

        [Command("Migrate")]
        [Summary("Migrate")]
        [Remarks("Migrates all data from the old guild object over to rewrite of bot\nNOTE: Will delete any current configs.")]
        public async Task Migrate()
        {
            if (Directory.Exists(Path.Combine(AppContext.BaseDirectory, "setup/server/")))
            {
                var added = 0;
                foreach (var guild in Context.Socket.Client.Guilds)
                {
                    if (!(GuildModel_Depreciated.GetServer(guild) is GuildModel_Depreciated DepModel)) continue;
                    var newmodel = new GuildModel
                    {
                        AutoMessage = new GuildModel.autoMessage
                        {
                            AutoMessageChannels = DepModel.AutoMessage.Select(x => new GuildModel.autoMessage.amChannel
                            {
                                ChannelID = x.channelID,
                                Count = 0,
                                Enabled = x.enabled,
                                Limit = x.sendlimit,
                                Message = x.automessage
                            }).ToList()
                        },
                        Events = new GuildModel.events
                        {
                            Welcome = new GuildModel.events._event
                            {
                                ChannelID = DepModel.WelcomeChannel,
                                Enabled = DepModel.WelcomeEvent,
                                Message = DepModel.WelcomeMessage
                            },
                            Goodbye = new GuildModel.events._event
                            {
                                ChannelID = DepModel.GoodByeChannel,
                                Enabled = DepModel.GoodbyeEvent,
                                Message = DepModel.GoodbyeMessage
                            }
                        },
                        ID = DepModel.GuildId,
                        Moderation = new GuildModel.moderation
                        {
                            AdminRoleIDs = DepModel.RoleConfigurations.AdminRoleList,
                            ModRoleIDs = DepModel.RoleConfigurations.ModeratorRoleList,
                            SubRoleIDs = DepModel.RoleConfigurations.SubRoleList
                        },
                        Levels = new GuildModel.levelling
                        {
                            RewardRoles = DepModel.Levels.LevelRoles.Select(x => new GuildModel.levelling.levelreward
                            {
                                RoleID = x.RoleID,
                                Requirement = x.LevelToEnter
                            }).ToList(),
                            Settings = new GuildModel.levelling.lsettings
                            {
                                Enabled = DepModel.Levels.LevellingEnabled,
                                LogChannelID = DepModel.Levels.LevellingChannel,
                                ReplyLevelUps = DepModel.Levels.UseLevelMessages,
                                UseLogChannel = DepModel.Levels.UseLevelChannel
                            },
                            Users = DepModel.Levels.Users.Select(x => new GuildModel.levelling.luser
                            {
                                UserID = x.userID,
                                Level = x.level,
                                XP = x.xp
                            }).ToList()
                        },
                        Partner = new GuildModel.partner
                        {
                            Message = new GuildModel.partner.message
                            {
                                Content = DepModel.PartnerSetup.Message,
                                ImageUrl = DepModel.PartnerSetup.ImageUrl,
                                UserCount = DepModel.PartnerSetup.showusercount
                            },
                            Settings = new GuildModel.partner.psettings
                            {
                                Banned = DepModel.PartnerSetup.banned,
                                ChannelID = DepModel.PartnerSetup.PartherChannel,
                                Enabled = DepModel.PartnerSetup.IsPartner
                            },
                            Stats = new GuildModel.partner.pstats
                            {
                                ServersReached = 0,
                                UsersReached = 0
                            }
                        },
                        Settings = new GuildModel.gsettings
                        {
                            Prefix = new GuildModel.gsettings.prefix
                            {
                                CustomPrefix = DepModel.Prefix
                            }
                        },
                        Tags = new GuildModel.tags
                        {
                            Tags = DepModel.Dict.Select(x => new GuildModel.tags.tag
                            {
                                Content = x.Content,
                                Name = x.Tagname,
                                CreatorID = x.Creator,
                                OwnerName = guild.GetUser(x.Creator)?.Username ?? $"[{x.Creator}]",
                                Uses = x.uses
                            }).ToList()
                        }
                    };
                    DatabaseHandler.RemoveGuild(guild.Id);
                    DatabaseHandler.InsertGuildObject(newmodel);
                    added++;
                }

                await SimpleEmbedAsync($"Guilds Added: {added}");
            }
            else
            {
                await SimpleEmbedAsync($"No Previous Guild Objects found.");
            }
        }
    }
}