using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Discord.Context;
using PassiveBOT.Discord.Preconditions;
using PassiveBOT.Models;

namespace PassiveBOT.Modules.GuildSetup
{
    [RequireAdmin]
    public class CustomChannels : Base
    {
        [Command("AddMediaChannel")]
        [Summary("AddMediaChannel")]
        [Remarks("Add the current channel to the media channels list")]
        public async Task Add()
        {
            Context.Server.CustomChannel.MediaChannels.Add(new GuildModel.CustomChannels.MediaChannel
            {
                ChannelID = Context.Channel.Id,
                Enabled = true,
                ExcemptRoles = new List<ulong>()
            });
            Context.Server.Save();
            await SimpleEmbedAsync($"{Context.Channel.Name} is now a media channel. All messages without URLs or Attachments will be deleted");
        }
        [Command("RemoveMediaChannel")]
        [Summary("RemoveMediaChannel <Channel>")]
        [Remarks("Remove the channel from the media channels list")]
        public async Task Remove(ITextChannel Channel = null)
        {
            if (Channel == null)
            {
                Channel = Context.Channel as ITextChannel;
            }

            var match = Context.Server.CustomChannel.MediaChannels.FirstOrDefault(x => x.ChannelID == Channel.Id);
            if (match != null)
            {
                Context.Server.CustomChannel.MediaChannels.Remove(match);
                Context.Server.Save();
                await SimpleEmbedAsync($"{Context.Channel.Name} is no longer a media channel");
            }
        }
        [Command("MediaExcempt")]
        [Summary("MediaExcempt <Channel> <Role>")]
        [Remarks("Set a specific role to not be checked my media channel restrictions")]
        public async Task MediaExcempt(ITextChannel Channel, IRole ExcemptRole)
        {
            var match = Context.Server.CustomChannel.MediaChannels.FirstOrDefault(x => x.ChannelID == Channel.Id);
            if (match != null)
            {
                match.ExcemptRoles.Add(ExcemptRole.Id);
                Context.Server.Save();
                await SimpleEmbedAsync($"{ExcemptRole.Name} will no longer be checked in this media channel");
            }
        }

        [Command("MediaExcempt")]
        [Summary("MediaExcempt <Role>")]
        [Remarks("Set a specific role to not be checked my media channel restrictions")]
        public async Task MediaExcempt(IRole ExcemptRole)
        {
            await MediaExcempt(Context.Channel as ITextChannel, ExcemptRole);
        }

        [Command("RemoveMediaExcempt")]
        [Summary("RemoveMediaExcempt <Channel> <Role>")]
        [Remarks("Remove a mediaexcempt role")]
        public async Task DelMediaExcempt(ITextChannel Channel, IRole ExcemptRole)
        {
            var match = Context.Server.CustomChannel.MediaChannels.FirstOrDefault(x => x.ChannelID == Channel.Id);
            if (match != null)
            {
                if (match.ExcemptRoles.Contains(ExcemptRole.Id))
                {
                    match.ExcemptRoles.Remove(ExcemptRole.Id);
                    Context.Server.Save();
                    await SimpleEmbedAsync($"{ExcemptRole.Name} has been removed from the excempt roles");
                }
            }
        }

        [Command("RemoveMediaExcempt")]
        [Summary("RemoveMediaExcempt <Role>")]
        [Remarks("Remove a mediaexcempt role")]
        public async Task DelMediaExcempt(IRole ExcemptRole)
        {
            await DelMediaExcempt(Context.Channel as ITextChannel, ExcemptRole);
        }
    }
}
