using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Discord.Context;
using PassiveBOT.Discord.Extensions;
using PassiveBOT.Discord.Preconditions;
using PassiveBOT.Models;

namespace PassiveBOT.Modules.GuildSetup
{
    [Group("Partner")]
    [RequireAdmin]
    public class Partner : Base
    {
        [Command("Info")]
        [Summary("Info")]
        [Remarks("Show partner info and stats")]
        public async Task Info()
        {
            await SimpleEmbedAsync("**Stats**\n" +
                                   $"Users Reached: {Context.Server.Partner.Stats.UsersReached}\n" +
                                   $"Servers Reached: {Context.Server.Partner.Stats.ServersReached}\n" +
                                   "**Settings**\n" +
                                   $"Enabled: {Context.Server.Partner.Settings.Enabled}\n" +
                                   $"Channel: {Context.Socket.Guild.GetChannel(Context.Server.Partner.Settings.ChannelID)?.Name ?? "N/A"}\n" +
                                   $"**Config**\n" +
                                   $"Color (RGB): [{Context.Server.Partner.Message.Color.R}, {Context.Server.Partner.Message.Color.G}, {Context.Server.Partner.Message.Color.B}]\n" +
                                   $"Using Server Thumbnail: {Context.Server.Partner.Message.UseThumb}\n" +
                                   $"Showing Usercount: {Context.Server.Partner.Message.UserCount}\n" +
                                   $"Image URL: {Context.Server.Partner.Message.ImageUrl ?? "N/A"}\n" +
                                   $"Message: (Refer to Partner Message Embed, for raw do `{Context.Prefix}partner rawmessage`)\n" +
                                   "**Partner Message Embed**\n" +
                                   "(See Next Message)");
            await SendEmbedAsync(GeneratePartnerMessage.GenerateMessage(Context.Server, Context.Socket.Guild));
        }

        [Command("RawMessage")]
        [Summary("RawMessage")]
        [Remarks("Show raw partner message with formatting")]
        public async Task RawMessage()
        {
            await SimpleEmbedAsync(Format.Sanitize(Context.Server.Partner.Message.Content));
        }

        [Command("Toggle")]
        [Summary("Toggle")]
        [Remarks("Toggle the Program in the server")]
        public async Task Toggle()
        {
            Context.Server.Partner.Settings.Enabled = !Context.Server.Partner.Settings.Enabled;
            Context.Server.Save();
            await SimpleEmbedAsync($"Partner Program Enabled: {Context.Server.Partner.Settings.Enabled}");
        }

        [Command("SetChannel")]
        [Summary("SetChannel")]
        [Remarks("Set the partner channel")]
        public async Task SetChannel()
        {
            Context.Server.Partner.Settings.ChannelID = Context.Channel.Id;
            Context.Server.Save();
            await SimpleEmbedAsync($"Partner Updates will now be sent in {Context.Channel.Name}");
        }

        [Command("Message")]
        [Summary("Message")]
        [Remarks("Set the partner message for this server")]
        public async Task SetChannel([Remainder]string message)
        {
            if (message.Length > 1000)
            {
                throw new Exception($"Partner Message must be shorter than 1000 characters. Given: {message.Length}");
            }
            Context.Server.Partner.Message.Content = message;
            Context.Server.Save();
            await SendEmbedAsync(GeneratePartnerMessage.GenerateMessage(Context.Server, Context.Socket.Guild));
        }

        [Command("UserCount")]
        [Summary("UserCount")]
        [Remarks("Toggle the usercount in the footer of the partner message")]
        public async Task usercount()
        {
            Context.Server.Partner.Message.UserCount = !Context.Server.Partner.Message.UserCount;
            Context.Server.Save();
            await SendEmbedAsync(GeneratePartnerMessage.GenerateMessage(Context.Server, Context.Socket.Guild));
        }

        [Command("ImageUrl")]
        [Summary("ImageUrl")]
        [Remarks("Set an optional image url for the partner message")]
        public async Task ImageURL(string imageurl = null)
        {
            Context.Server.Partner.Message.ImageUrl = imageurl;
            Context.Server.Save();
            await SendEmbedAsync(GeneratePartnerMessage.GenerateMessage(Context.Server, Context.Socket.Guild));
        }

        [Command("Thumbnail")]
        [Summary("Thumbnail")]
        [Remarks("Toggle the thumbnail of the server in the partner message")]
        public async Task thumbnail()
        {
            Context.Server.Partner.Message.UseThumb = !Context.Server.Partner.Message.UseThumb;
            Context.Server.Save();
            await SendEmbedAsync(GeneratePartnerMessage.GenerateMessage(Context.Server, Context.Socket.Guild));
        }

        [Command("SetColor")]
        [Summary("SetColor")]
        [Remarks("Set the embed color for the partner message")]
        public async Task Color(string color)
        {
            var color_response = HexToColor.GetCol(color);

            Context.Server.Partner.Message.Color = color_response;
            Context.Server.Save();
            await SendEmbedAsync(GeneratePartnerMessage.GenerateMessage(Context.Server, Context.Socket.Guild));
        }
    }
}
