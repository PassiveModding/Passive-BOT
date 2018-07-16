namespace PassiveBOT.Models.Migration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Discord;

    using PassiveBOT.Extensions.PassiveBOT;
    using PassiveBOT.Handlers;
    using PassiveBOT.Services;

    /*
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    public class GuildModelToServices
    {
        private ChannelService _ChannelService { get; }

        private TagService _TagService { get; }

        private PartnerService _PartnerService { get; }

        private LevelService _LevelService { get; }

        public GuildModelToServices(ChannelService cServ, TagService tServ, PartnerService pServ, LevelService lServ)
        {
            _ChannelService = cServ;
            _TagService = tServ;
            _PartnerService = pServ;
            _LevelService = lServ;
        }

        public void SplitModelAsync(GuildModel model)
        {
            try
            {
                var channels = new ChannelService.CustomChannels(model.ID)
                {
                    MediaChannels = model.CustomChannel.MediaChannels.Select(x => new ChannelService.CustomChannels.MediaChannel
                    {
                        ChannelID = x.ChannelID,
                        Enabled = x.Enabled,
                        ExemptRoles = x.ExemptRoles
                    }).ToConcurrentDictionary(x => x.ChannelID),
                    AutoMessageChannels = model.CustomChannel.AutoMessageChannels.Select(x => new ChannelService.CustomChannels.AutoMessageChannel
                    {
                        ChannelID = x.ChannelID,
                        Enabled = x.Enabled,
                        Count = x.Count,
                        Limit = x.Limit,
                        Message = x.Message
                    }).ToConcurrentDictionary(x => x.ChannelID)
                };
                _ChannelService.OverWrite(channels);

                var levels = new LevelService.LevelSetup(model.ID)
                {
                    RewardRoles = model.Levels.RewardRoles.Select(r => new LevelService.LevelSetup.LevelReward
                    {
                        Requirement = r.Requirement,
                        RoleID = r.RoleID
                    }).ToList(),
                    Settings = new LevelService.LevelSetup.LevelSettings
                    {
                        DMLevelUps = model.Levels.Settings.DMLevelUps,
                        Enabled = model.Levels.Settings.Enabled,
                        IncrementLevelRewards = model.Levels.Settings.IncrementLevelRewards,
                        LogChannelID = model.Levels.Settings.LogChannelID,
                        ReplyLevelUps = model.Levels.Settings.ReplyLevelUps,
                        UseLogChannel = model.Levels.Settings.UseLogChannel
                    },
                    Users = model.Levels.Users.Select(u => new LevelService.LevelSetup.LevelUser(u.UserID)
                    {
                        LastUpdate = u.LastUpdate,
                        Level = u.Level,
                        XP = u.XP
                    }).ToConcurrentDictionary(u => u.UserID)
                };

                _LevelService.OverWrite(levels);

                var tags = new TagService.TagSetup(model.ID)
                {
                    Enabled = model.Tags.Settings.Enabled,
                    Tags = model.Tags.Tags.Select(t => new TagService.TagSetup.Tag
                    {
                        Content = t.Content,
                        Creator = t.OwnerName,
                        CreatorId = t.CreatorID,
                        Name = t.Name,
                        Uses = t.Uses
                    }).ToConcurrentDictionary(t => t.Name.ToLower())
                };

                _TagService.OverWrite(tags);

                var partners = new PartnerService.PartnerInfo(model.ID) { Message = new PartnerService.PartnerInfo.PartnerMessage { Color = new PartnerService.PartnerInfo.PartnerMessage.RGB { B = model.Partner.Message.Color.B, G = model.Partner.Message.Color.G, R = model.Partner.Message.Color.R }, Content = model.Partner.Message.Content, ImageUrl = model.Partner.Message.ImageUrl, UserCount = model.Partner.Message.UserCount, UseThumb = model.Partner.Message.UseThumb }, Settings = new PartnerService.PartnerInfo.PartnerSettings { Banned = model.Partner.Settings.Banned, Enabled = model.Partner.Settings.Enabled, ChannelId = model.Partner.Settings.ChannelID }, Stats = new PartnerService.PartnerInfo.PartnerStats { ServersReached = model.Partner.Stats.ServersReached, UsersReached = model.Partner.Stats.UsersReached } };
                _PartnerService.OverWrite(partners);

                LogHandler.LogMessage($"Migrated {model.ID} Successfully");
            }
            catch
            {
                LogHandler.LogMessage($"Failed to Migrate {model.ID} Successfully", LogSeverity.Error);
            }

        }
    }*/
}
