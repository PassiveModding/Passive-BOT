namespace PassiveBOT.Modules.GuildCommands.ServerSetup
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using global::Discord;
    using global::Discord.Commands;
    using global::Discord.WebSocket;

    using PassiveBOT.Discord.Context;
    using PassiveBOT.Discord.Extensions;
    using PassiveBOT.Discord.Extensions.PassiveBOT;
    using PassiveBOT.Discord.Preconditions;
    using PassiveBOT.Models;

    /// <summary>
    /// The partner module.
    /// </summary>
    [Group("Partner")]
    [Summary("PassiveBOT Partner program, sends your server's message to another guild periodically, also receives one periodically")]
    [RequireAdmin]
    [RequireContext(ContextType.Guild)]
    public class Partner : Base
    {
        /// <summary>
        /// Displays partner message info
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("Info")]
        [Summary("Show partner info and stats")]
        public async Task InfoAsync()
        {
            await SimpleEmbedAsync("**Stats**\n" +
                                   $"Users Reached: {Context.Server.Partner.Stats.UsersReached}\n" +
                                   $"Servers Reached: {Context.Server.Partner.Stats.ServersReached}\n" +
                                   "**Settings**\n" +
                                   $"Enabled: {Context.Server.Partner.Settings.Enabled}\n" +
                                   $"Channel: {Context.Guild.GetChannel(Context.Server.Partner.Settings.ChannelID)?.Name ?? "N/A"}\n" +
                                   "**Config**\n" +
                                   $"Color (RGB): [{Context.Server.Partner.Message.Color.R}, {Context.Server.Partner.Message.Color.G}, {Context.Server.Partner.Message.Color.B}]\n" +
                                   $"Using Server Thumbnail: {Context.Server.Partner.Message.UseThumb}\n" +
                                   $"Showing UserCount: {Context.Server.Partner.Message.UserCount}\n" +
                                   $"Image URL: {Context.Server.Partner.Message.ImageUrl ?? "N/A"}\n" +
                                   $"Message: (Refer to Partner Message Embed, for raw do `{Context.Prefix}partner RawMessage`)\n" +
                                   "**Partner Message Embed**\n" +
                                   "(See Next Message)");
            await ReplyAsync(PartnerHelper.GenerateMessage(Context.Server, Context.Guild));
        }

        /// <summary>
        /// The raw message.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("RawMessage")]
        [Summary("Show raw partner message with formatting")]
        public Task RawMessageAsync()
        {
            return SimpleEmbedAsync(Format.Sanitize(Context.Server.Partner.Message.Content));
        }

        /// <summary>
        /// The toggle.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("Toggle")]
        [Summary("Toggle the Program in the server")]
        public async Task ToggleAsync()
        {
            Context.Server.Partner.Settings.Enabled = !Context.Server.Partner.Settings.Enabled;
            Context.Server.Save();

            await PartnerHelper.PartnerLogAsync(Context.Client, Context.Server, new EmbedFieldBuilder { Name = "Partner Toggled", Value = $"Enabled: {Context.Server.Partner.Settings.Enabled}" });
            await SimpleEmbedAsync($"Partner Program Enabled: {Context.Server.Partner.Settings.Enabled}");
        }

        /// <summary>
        /// Sets the partner channel
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// throws if channel does not have enough users
        /// </exception>
        [Command("SetChannel")]
        [Summary("Set the current channel as partner channel")]
        public async Task SetChannelAsync()
        {
            if ((decimal)(Context.Channel as SocketTextChannel).Users.Count / Context.Guild.Users.Count * 100 < 90)
            {
                throw new Exception("Partner messages will not be shared as this channel has less than 90% visibility in the server,\n" + "You can fix this by ensuring that all roles have permissions to view messages and message history in the channel settings");
            }

            Context.Server.Partner.Settings.ChannelID = Context.Channel.Id;
            Context.Server.Save();
            await SimpleEmbedAsync($"Partner Updates will now be sent in {Context.Channel.Name}");

            await PartnerHelper.PartnerLogAsync(Context.Client, Context.Server, new EmbedFieldBuilder { Name = "Partner Channel Updated", Value = $"Guild: {Context.Guild.Name} [{Context.Guild.Id}]\n" + $"Owner: {Context.Guild.Owner.Username}\n" + $"Users: {Context.Guild.MemberCount}" });
        }

        /// <summary>
        /// Sets the partner message
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// throws if the message does not comply with the rules.
        /// </exception>
        [Command("Message")]
        [Summary("Set the partner message for this server")]
        public async Task SetMessageAsync([Remainder] string message)
        {
            if (message.Length > 1000)
            {
                throw new Exception($"Partner Message must be shorter than 1000 characters. Given: {message.Length}");
            }

            if (Context.Message.MentionedRoles.Any() || Context.Message.MentionedUsers.Any() || Context.Message.MentionedChannels.Any() || Context.Message.Content.Contains("@everyone") || Context.Message.Content.Contains("@here"))
            {
                throw new Exception("Partner Message cannot contain role or user mentions as they cannot be referenced from external guilds");
            }

            if (Profanity.ContainsProfanity(message))
            {
                throw new Exception("Partner Message cannot contain profanity");
            }

            if (!message.ToLower().Contains("discord.gg") && !message.ToLower().Contains("discordapp.com") && !message.ToLower().Contains("discord.me"))
            {
                throw new Exception("You should include an invite link to your server in the Partner Message too\n" +
                                    $"If you believe this is an error, please contact the support server: {HomeModel.Load().HomeInvite}\n" +
                                    "NOTE: If you use 2 Factor Authentication for your server (User Must have a verified phone number on their Discord account)\n" +
                                    "Please disable this during setup, you may re-enable after the message has been set.");
            }

            if (Regex.Match(message, @"(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?(d+i+s+c+o+r+d+|a+p+p)+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$").Success)
            {
                var invites = Regex.Matches(message, @"(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?(d+i+s+c+o+r+d+|a+p+p)+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$").ToList();
                var inviteMetadata = await Context.Guild.GetInvitesAsync();
                var mismatch = false;
                foreach (var invite in invites)
                {
                    var match = inviteMetadata.Where(x => x.MaxAge == null).FirstOrDefault(x => invite.ToString().ToLower().Contains(x.Code.ToLower()));
                    if (match == null)
                    {
                        mismatch = true;
                    }
                }

                if (mismatch)
                {
                    throw new Exception("Please ensure the message passes all checks:\n" + 
                                        "1.Only invites from this server are allowed in the partner message!\n" +
                                        "2.Ensure that the invite link you are using is set to never expire\n" +
                                        "3.Ensure that it does not have a use limit.\n" + 
                                        "4.If your server uses 2FA please disable it while running the command then re-enable it after\n" + 
                                        "If you are using an invite for your server and you are seeing this message, please generate a new invite for your server\n\n" +
                                        $"If you believe this is an error, please contact the support server: {HomeModel.Load().HomeInvite}");
                }
            }

            Context.Server.Partner.Message.Content = message;
            Context.Server.Save();
            var generateMessage = PartnerHelper.GenerateMessage(Context.Server, Context.Guild);
            await ReplyAsync(generateMessage);

            await PartnerHelper.PartnerLogAsync(Context.Client, Context.Server, new EmbedFieldBuilder { Name = "Partner Message Updated", Value = $"Guild: {Context.Guild.Name} [{Context.Guild.Id}]\n" + $"Owner: {Context.Guild.Owner.Username}\n" + $"Users: {Context.Guild.MemberCount}" });
        }

        /// <summary>
        /// Toggles userCount in partner message
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("UserCount")]
        [Summary("Toggle the User Count in the footer of the partner message")]
        public Task UserCountAsync()
        {
            Context.Server.Partner.Message.UserCount = !Context.Server.Partner.Message.UserCount;
            Context.Server.Save();
            return ReplyAsync(PartnerHelper.GenerateMessage(Context.Server, Context.Guild));
        }

        /// <summary>
        /// The image url.
        /// </summary>
        /// <param name="imageUrl">
        /// The imageUrl.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("ImageUrl")]
        [Summary("Set an optional image url for the partner message")]
        public async Task ImageURLAsync(string imageUrl = null)
        {
            if (!string.IsNullOrEmpty(imageUrl) && !Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                throw new Exception("Url must be a well-formed URI");
            }

            Context.Server.Partner.Message.ImageUrl = imageUrl;
            Context.Server.Save();
            var partnerEmbed = PartnerHelper.GenerateMessage(Context.Server, Context.Guild);
            await ReplyAsync(partnerEmbed);

            await PartnerHelper.PartnerLogAsync(Context.Client, Context.Server, new EmbedFieldBuilder { Name = "Partner Image Updated", Value = $"Guild: {Context.Guild.Name} [{Context.Guild.Id}]\n" + $"Owner: {Context.Guild.Owner.Username}\n" + $"Users: {Context.Guild.MemberCount}" });
        }

        /// <summary>
        /// Toggles the partner Thumbnail
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("Thumbnail")]
        [Summary("Toggle the Thumbnail of the server in the partner message")]
        public Task ThumbnailAsync()
        {
            Context.Server.Partner.Message.UseThumb = !Context.Server.Partner.Message.UseThumb;
            Context.Server.Save();
            return ReplyAsync(PartnerHelper.GenerateMessage(Context.Server, Context.Guild));
        }

        /// <summary>
        /// Sets the partner message color
        /// </summary>
        /// <param name="color">
        /// The color.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("Color")]
        [Summary("Set the embed color for the partner message")]
        public Task ColorAsync(string color)
        {
            var color_response = ColorManagement.GetColor(color);

            Context.Server.Partner.Message.Color = new GuildModel.PartnerSetup.PartnerMessage.RGB
            {
                R = color_response.R,
                G = color_response.G,
                B = color_response.B
            };
            Context.Server.Save();
            return ReplyAsync(PartnerHelper.GenerateMessage(Context.Server, Context.Guild));
        }
    }
}