using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
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
                                   "**Config**\n" +
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
            if ((decimal) ((SocketTextChannel) Context.Socket.Channel).Users.Count / Context.Socket.Guild.Users.Count * 100 < 90)
            {
                throw new Exception("Partner messages will not be shared as this channel has less than 90% visibility in the server,\n" +
                                    "You can fix this by ensuring that all roles have permissions to view messages and message history in the channel settings");
            }

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

            if (Context.Message.MentionedRoleIds.Any() || Context.Message.MentionedUserIds.Any() || Context.Message.MentionedChannelIds.Any() || Context.Message.Content.Contains("@everyone") || Context.Message.Content.Contains("@here"))
            {
                throw new Exception("Partner Message cannot contain role or user mentions as they cannot be referenced from external guilds");
            }

            if (CheckProfanity.ContainsProfanity(message))
            {
                throw new Exception("Partner Message cannot contain profanity");
            }

            if (!message.ToLower().Contains("discord.gg") && !message.ToLower().Contains("discordapp.com") && !message.ToLower().Contains("discord.me"))
            {
                throw new Exception("You should include an invite link to your server in the Partner Message too\n" +
                                 $"If you believe this is an error, please contact the support server: {ConfigModel.Load().SupportServer}\n" +
                                 "NOTE: If you use 2 Factor Authentication for your server (User Must have a verified phone number on their Discord account)\n" +
                                 "Please disable this during setup, you may re-enable after the message has been set.");
            }

            if (Regex.Match(message, @"(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?(d+i+s+c+o+r+d+|a+p+p)+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$").Success)
            {
                var invites = Regex.Matches(message, @"(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?(d+i+s+c+o+r+d+|a+p+p)+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$").ToList();
                var officialinvites = ((SocketGuild)Context.Guild).GetInvitesAsync().Result;
                var mismatch = false;
                foreach (var invite in invites)
                {
                    var match = officialinvites.Where(x => x.MaxAge == null).FirstOrDefault(x => invite.ToString().ToLower().Contains(x.Code.ToLower()));
                    if (match == null)
                    {
                        mismatch = true;
                    }
                }

                if (mismatch)
                {
                    throw new Exception("Only invites from this server are allowed in the partner message!\n" +
                                     "NOTE: please ensure that the invite link you are using is set to never expire\n" +
                                     "If you are using an invite for your server and you are seeing this message, please generate a new invite for your server\n\n" +
                                     $"If you believe this is an error, please contact the support server: {ConfigModel.Load().SupportServer}");
                }
            }

            Context.Server.Partner.Message.Content = message;
            Context.Server.Save();
            var partnerembed = GeneratePartnerMessage.GenerateMessage(Context.Server, Context.Socket.Guild);
            await SendEmbedAsync(partnerembed);
            var HS = HomeModel.Load();
            if (HS.Logging.LogPartnerChanges && await Context.Client.GetChannelAsync(HS.Logging.PartnerLogChannel) is IMessageChannel channel)
            {
                await channel.SendMessageAsync("", false, partnerembed.AddField("Partner Message Updated", $"Guild: {Context.Guild.Name} [{Context.Guild.Id}]\n" +
                                                                                                                                $"Owner: {Context.Socket.Guild.Owner.Username}\n" +
                                                                                                                                $"Users: {Context.Socket.Guild.MemberCount}")
                                                                                                                                .Build());
            }
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

        [Command("Color")]
        [Summary("Color")]
        [Remarks("Set the embed color for the partner message")]
        public async Task Color(string color)
        {
            var color_response = HexToColor.GetCol(color);

            Context.Server.Partner.Message.Color = new GuildModel.partner.message.rgb
            {
                R = color_response.R,
                G = color_response.G,
                B = color_response.B
            };
            Context.Server.Save();
            await SendEmbedAsync(GeneratePartnerMessage.GenerateMessage(Context.Server, Context.Socket.Guild));
        }
    }
}
