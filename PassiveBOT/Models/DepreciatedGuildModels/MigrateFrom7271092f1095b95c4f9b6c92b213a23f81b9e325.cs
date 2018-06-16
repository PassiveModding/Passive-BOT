namespace PassiveBOT.Models.DepreciatedGuildModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using global::Discord;

    using PassiveBOT.Handlers;

    using Raven.Client.ServerWide;
    using Raven.Client.ServerWide.Operations;

    /// <summary>
    /// Migrates from Old database structure
    /// </summary>
    public class MigrateFrom7271092f1095b95c4f9b6c92b213a23f81b9e325
    {
        /// <summary>
        /// The migrate.
        /// </summary>
        /// <param name="oldDatabase">
        /// The Old Database name
        /// </param>
        /// <param name="backupOldDatabase">
        /// The Name of the database where you wish to backup the old database content
        /// </param>
        /// <param name="newDatabase">
        /// The New Database name
        /// </param>
        public static void Migrate(string oldDatabase, string backupOldDatabase, string newDatabase)
        {
            if (oldDatabase == newDatabase)
            {
                if (backupOldDatabase == oldDatabase || backupOldDatabase == newDatabase)
                {
                    throw new Exception("Backup Database cannot be the same as regular database");
                }
            }

            bool do_not_delete = oldDatabase == backupOldDatabase;

            // This creates the database
            if (DatabaseHandler.Store.Maintenance.Server.Send(new GetDatabaseNamesOperation(0, 5)).All(x => x != oldDatabase))
            {
                throw new Exception("Old database must exist");
            }

            // This creates the database
            if (DatabaseHandler.Store.Maintenance.Server.Send(new GetDatabaseNamesOperation(0, 5)).All(x => x != newDatabase))
            {
                DatabaseHandler.Store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(newDatabase)));
                LogHandler.LogMessage($"Created Database => {newDatabase}");
            }

            // This creates the database
            if (DatabaseHandler.Store.Maintenance.Server.Send(new GetDatabaseNamesOperation(0, 5)).All(x => x != backupOldDatabase))
            {
                DatabaseHandler.Store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(backupOldDatabase)));
                LogHandler.LogMessage($"Created Database => {backupOldDatabase}");
            }

            using (var oldSession = DatabaseHandler.Store.OpenSession(oldDatabase))
            using (var backupSession = DatabaseHandler.Store.OpenSession(backupOldDatabase))
            using (var newSession = DatabaseHandler.Store.OpenSession(newDatabase))
            {
                var guilds = oldSession.Query<GuildModel>();
                var updated = 0;
                foreach (var guild in guilds)
                {
                    var newGuild = new Models.GuildModel
                    {
                        ID = guild.ID,
                        Settings =
                                               new Models.GuildModel.GuildSetup
                                               {
                                                   Prefix = new Models.GuildModel.GuildSetup.PrefixSetup
                                                   {
                                                       CustomPrefix
                                                                       =
                                                                       guild
                                                                           .Settings
                                                                           .Prefix
                                                                           .CustomPrefix,
                                                       DenyDefaultPrefix
                                                                       = guild
                                                                           .Settings
                                                                           .Prefix
                                                                           .DenyDefaultPrefix,
                                                       DenyMentionPrefix
                                                                       = guild
                                                                           .Settings
                                                                           .Prefix
                                                                           .DenyMentionPrefix
                                                   },
                                                   Translate =
                                                           new Models.GuildModel.GuildSetup.TranslateSetup
                                                           {
                                                               DMTranslations
                                                                       =
                                                                       guild
                                                                           .Settings
                                                                           .Translate
                                                                           .DMTranslations,
                                                               EasyTranslate
                                                                       =
                                                                       guild
                                                                           .Settings
                                                                           .Translate
                                                                           .EasyTranslate,
                                                               CustomPairs
                                                                       =
                                                                       guild
                                                                           .Settings
                                                                           .Translate
                                                                           .Custompairs
                                                                           .Select(
                                                                               x =>
                                                                                   new
                                                                                   Models
                                                                                   .GuildModel
                                                                                   .GuildSetup
                                                                                   .TranslateSetup
                                                                                   .TranslationSet
                                                                                   {
                                                                                       EmoteMatches
                                                                                               =
                                                                                               x.EmoteMatches,
                                                                                       Language
                                                                                               =
                                                                                               Enum.Parse<LanguageMap.LanguageCode>(
                                                                                                       x.Language
                                                                                                           .ToString())
                                                                                   })
                                                                           .ToList()
                                                           },
                                                   Nsfw = new Models.GuildModel.GuildSetup.NsfwSetup()
                                               },
                        Events =
                                               new Models.GuildModel.EventSetup
                                               {
                                                   Goodbye = new Models.GuildModel.EventSetup.Event
                                                   {
                                                       ChannelID
                                                                       =
                                                                       guild
                                                                           .Events
                                                                           .Goodbye
                                                                           .ChannelID,
                                                       Enabled
                                                                       =
                                                                       guild
                                                                           .Events
                                                                           .Goodbye
                                                                           .Enabled,
                                                       Message
                                                                       = guild
                                                                           .Events
                                                                           .Goodbye
                                                                           .Message,
                                                       SendDMs
                                                                       = guild
                                                                           .Events
                                                                           .Goodbye
                                                                           .SendDMs,
                                                       UserCount
                                                                       = guild
                                                                           .Events
                                                                           .Goodbye
                                                                           .UserCount
                                                   },
                                                   Welcome = new Models.GuildModel.EventSetup.Event
                                                   {
                                                       ChannelID
                                                                       =
                                                                       guild
                                                                           .Events
                                                                           .Welcome
                                                                           .ChannelID,
                                                       Enabled
                                                                       =
                                                                       guild
                                                                           .Events
                                                                           .Welcome
                                                                           .Enabled,
                                                       Message
                                                                       = guild
                                                                           .Events
                                                                           .Welcome
                                                                           .Message,
                                                       SendDMs
                                                                       = guild
                                                                           .Events
                                                                           .Welcome
                                                                           .SendDMs,
                                                       UserCount
                                                                       = guild
                                                                           .Events
                                                                           .Welcome
                                                                           .UserCount
                                                   }
                                               },
                        Levels =
                                               new Models.GuildModel.LevelSetup
                                               {
                                                   Settings = new Models.GuildModel.LevelSetup.LevelSettings
                                                   {
                                                       DMLevelUps
                                                                       =
                                                                       guild
                                                                           .Levels
                                                                           .Settings
                                                                           .DMLevelUps,
                                                       Enabled
                                                                       =
                                                                       guild
                                                                           .Levels
                                                                           .Settings
                                                                           .Enabled,
                                                       IncrementLevelRewards
                                                                       =
                                                                       guild
                                                                           .Levels
                                                                           .Settings
                                                                           .IncrementLevelRewards,
                                                       LogChannelID
                                                                       =
                                                                       guild
                                                                           .Levels
                                                                           .Settings
                                                                           .LogChannelID,
                                                       ReplyLevelUps
                                                                       = guild
                                                                           .Levels
                                                                           .Settings
                                                                           .ReplyLevelUps,
                                                       UseLogChannel
                                                                       = guild
                                                                           .Levels
                                                                           .Settings
                                                                           .UseLogChannel
                                                   },
                                                   RewardRoles =
                                                           guild.Levels
                                                               .RewardRoles
                                                               .Select(
                                                                   x =>
                                                                       new
                                                                       Models
                                                                       .GuildModel
                                                                       .LevelSetup
                                                                       .LevelReward
                                                                       {
                                                                           Requirement
                                                                                   = x
                                                                                       .Requirement,
                                                                           RoleID
                                                                                   = x
                                                                                       .RoleID
                                                                       })
                                                               .ToList(),
                                                   Users = guild
                                                           .Levels.Users
                                                           .Select(
                                                               x =>
                                                                   new
                                                                   Models
                                                                   .GuildModel
                                                                   .LevelSetup
                                                                   .LevelUser
                                                                   {
                                                                       LastUpdate
                                                                               = x
                                                                                   .LastUpdate,
                                                                       Level
                                                                               = x
                                                                                   .Level,
                                                                       UserID
                                                                               = x
                                                                                   .UserID,
                                                                       XP
                                                                               = x
                                                                                   .XP
                                                                   })
                                                           .ToList()
                                               },
                        Moderation =
                                               new Models.GuildModel.ModerationSetup
                                               {
                                                   AdminRoleIDs
                                                           = guild
                                                               .Moderation
                                                               .AdminRoleIDs,
                                                   ModRoleIDs =
                                                           guild
                                                               .Moderation
                                                               .ModRoleIDs,
                                                   SubRoleIDs =
                                                           guild
                                                               .Moderation
                                                               .SubRoleIDs
                                               },
                        Partner =
                                               new Models.GuildModel.PartnerSetup
                                               {
                                                   Message = new Models.GuildModel.PartnerSetup.PartnerMessage
                                                   {
                                                       Color
                                                                       =
                                                                       new
                                                                       Models
                                                                       .GuildModel
                                                                       .PartnerSetup
                                                                       .PartnerMessage
                                                                       .RGB
                                                                       {
                                                                           R =
                                                                                   guild
                                                                                       .Partner
                                                                                       .Message
                                                                                       .Color
                                                                                       .R,
                                                                           G =
                                                                                   guild
                                                                                       .Partner
                                                                                       .Message
                                                                                       .Color
                                                                                       .G,
                                                                           B =
                                                                                   guild
                                                                                       .Partner
                                                                                       .Message
                                                                                       .Color
                                                                                       .B
                                                                       },
                                                       Content
                                                                       =
                                                                       guild
                                                                           .Partner
                                                                           .Message
                                                                           .Content,
                                                       ImageUrl
                                                                       =
                                                                       guild
                                                                           .Partner
                                                                           .Message
                                                                           .ImageUrl,
                                                       UserCount
                                                                       = guild
                                                                           .Partner
                                                                           .Message
                                                                           .UserCount,
                                                       UseThumb
                                                                       = guild
                                                                           .Partner
                                                                           .Message
                                                                           .UseThumb
                                                   },
                                                   Settings = new Models.GuildModel.PartnerSetup.PartnerSettings
                                                   {
                                                       Banned
                                                                       =
                                                                       guild
                                                                           .Partner
                                                                           .Settings
                                                                           .Banned,
                                                       Enabled
                                                                       = guild
                                                                           .Partner
                                                                           .Settings
                                                                           .Enabled,
                                                       ChannelID
                                                                       = guild
                                                                           .Partner
                                                                           .Settings
                                                                           .ChannelID
                                                   },
                                                   Stats = new Models.GuildModel.PartnerSetup.PartnerStats
                                                   {
                                                       ServersReached
                                                                       = guild
                                                                           .Partner
                                                                           .Stats
                                                                           .ServersReached,
                                                       UsersReached
                                                                       = guild
                                                                           .Partner
                                                                           .Stats
                                                                           .UsersReached
                                                   }
                                               },
                        Tags =
                                               new Models.GuildModel.TagSetup
                                               {
                                                   Settings = new Models.GuildModel.TagSetup.TagSettings
                                                   {
                                                       AdminOnly
                                                                       = guild
                                                                           .Tags
                                                                           .Settings
                                                                           .AdminOnly,
                                                       Enabled
                                                                       = guild
                                                                           .Tags
                                                                           .Settings
                                                                           .Enabled
                                                   },
                                                   Tags = guild.Tags
                                                           .Tags.Select(
                                                               x => new Models.GuildModel.TagSetup.Tag
                                                               {
                                                                   Content
                                                                               = x
                                                                                   .Content,
                                                                   CreatorID
                                                                               = x
                                                                                   .CreatorID,
                                                                   Name
                                                                               = x
                                                                                   .Name,
                                                                   OwnerName
                                                                               = x
                                                                                   .OwnerName,
                                                                   Uses
                                                                               = x
                                                                                   .Uses
                                                               })
                                                           .ToList()
                                               },
                        CustomChannel =
                                               new Models.GuildModel.CustomChannels
                                               {
                                                   AutoMessageChannels
                                                           = guild
                                                               .AutoMessage
                                                               .AutoMessageChannels
                                                               .Select(
                                                                   x =>
                                                                       new
                                                                       Models
                                                                       .GuildModel
                                                                       .CustomChannels
                                                                       .AutoMessageChannel
                                                                       {
                                                                           ChannelID
                                                                                   =
                                                                                   x.ChannelID,
                                                                           Count
                                                                                   = x
                                                                                       .Count,
                                                                           Enabled
                                                                                   = x
                                                                                       .Enabled,
                                                                           Limit
                                                                                   = x
                                                                                       .Limit,
                                                                           Message
                                                                                   = x
                                                                                       .Message
                                                                       })
                                                               .ToList(),
                                                   MediaChannels
                                                           = guild
                                                               .CustomChannel
                                                               .MediaChannels
                                                               .Select(
                                                                   x =>
                                                                       new
                                                                       Models
                                                                       .GuildModel
                                                                       .CustomChannels
                                                                       .MediaChannel
                                                                       {
                                                                           ChannelID
                                                                                   = x
                                                                                       .ChannelID,
                                                                           Enabled
                                                                                   = x
                                                                                       .Enabled,
                                                                           ExemptRoles
                                                                                   = x
                                                                                       .ExcemptRoles
                                                                       })
                                                               .ToList()
                                               },
                        Disabled =
                                               new Models.GuildModel.CommandAccess
                                               {
                                                   CustomizedPermission = new List<Models.GuildModel.CommandAccess.CustomPermission>()
                                               }
                    };

                    if (!do_not_delete)
                    {
                        // Deletes the old config
                        try
                        {
                            oldSession.Delete(guild);
                            LogHandler.LogMessage($"Deleted Old Config => {guild.ID}");
                        }
                        catch
                        {
                            LogHandler.LogMessage($"Delete Error => {guild.ID}", LogSeverity.Warning);
                        }
                        

                        // Backs up and deletes the old guild config
                        try
                        {
                            backupSession.Store(guild, guild.ID.ToString());
                            LogHandler.LogMessage($"Backed Up Old Config => {guild.ID}");
                        }
                        catch
                        {
                            LogHandler.LogMessage($"Error Backing Up Old Config => {guild.ID}", LogSeverity.Warning);
                        }

                    }

                    // Stores the new config
                    newSession.Store(newGuild, newGuild.ID.ToString());
                    LogHandler.LogMessage($"Stored New Config => {guild.ID}");
                    newSession.SaveChanges();
                    updated++;
                    if (updated % 100 == 0)
                    {
                        try
                        {
                            oldSession.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            Console.ReadKey();
                        }

                    }
                }

                try
                {
                    if (do_not_delete)
                    {
                        oldSession.SaveChanges();
                    }
                    else
                    {
                        oldSession.SaveChanges();
                        backupSession.SaveChanges();
                    }

                    newSession.SaveChanges();
                }
                catch (Exception e)
                {
                    LogHandler.LogMessage("Error Saving Changes\n\n" + $"{e}", LogSeverity.Critical);
                    Console.ReadKey();
                }
            }
        }
    }
}
