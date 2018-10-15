namespace PassiveBOT.Modules.GuildCommands.ServerSetup
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Addons.PrefixService;
    using Discord.Commands;
    using Discord.WebSocket;

    using PassiveBOT.Context;
    using PassiveBOT.Extensions;
    using PassiveBOT.Extensions.PassiveBOT;
    using PassiveBOT.Preconditions;
    using PassiveBOT.Services;

    /// <summary>
    ///     The partner module.
    /// </summary>
    [Group("Partner")]
    [Summary("PassiveBOT Partner program, sends your server's message to another guild periodically, also receives one periodically")]
    [RequireAdmin]
    [RequireContext(ContextType.Guild)]
    public class Partner : Base
    {
        public Partner(PrefixService prefixService, PartnerService partnerService, PartnerHelper partnerHelper)
        {
            PrefixService = prefixService;
            PartnerService = partnerService;
            PartnerHelp = partnerHelper;
        }

        private PartnerService PartnerService { get; }

        private PartnerHelper PartnerHelp { get; }

        private PrefixService PrefixService { get; }

        /// <summary>
        ///     Sets the partner message color
        /// </summary>
        /// <param name="color">
        ///     The color.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("Color")]
        [Summary("Set the embed color for the partner message")]
        public Task ColorAsync(string color)
        {
            var color_response = ColorManagement.GetColor(color);

            var p = PartnerService.GetPartnerInfo(Context.Guild.Id, true);
            p.Message.Color = new PartnerService.PartnerInfo.PartnerMessage.RGB { R = color_response.R, G = color_response.G, B = color_response.B };
            p.Save();
            return ReplyAsync(PartnerHelp.GenerateMessage(p, Context.Guild));
        }

        /// <summary>
        ///     The image url.
        /// </summary>
        /// <param name="imageUrl">
        ///     The imageUrl.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("ImageUrl")]
        [Summary("Set an optional image url for the partner message")]
        public async Task ImageURLAsync(string imageUrl = null)
        {
            var p = PartnerService.GetPartnerInfo(Context.Guild.Id, true);
            if (!string.IsNullOrEmpty(imageUrl) && !Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                throw new Exception("Url must be a well-formed URI");
            }

            p.Message.ImageUrl = imageUrl;
            p.Save();
            var partnerEmbed = PartnerHelp.GenerateMessage(p, Context.Guild);
            await ReplyAsync(partnerEmbed);

            await PartnerHelp.PartnerLogAsync(Context.Client, p, new EmbedFieldBuilder { Name = "Partner Image Updated", Value = $"Guild: {Context.Guild.Name} [{Context.Guild.Id}]\n" + $"Owner: {Context.Guild.Owner.Username}\n" + $"Users: {Context.Guild.MemberCount}" });
        }

        /// <summary>
        ///     Displays partner message info
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("Info", RunMode = RunMode.Async)]
        [Summary("Show partner info and stats")]
        public async Task InfoAsync()
        {
            var p = PartnerService.GetPartnerInfo(Context.Guild.Id, true);
            await SimpleEmbedAsync("**Stats**\n" + $"Users Reached: {p.Stats.UsersReached}\n" + $"Servers Reached: {p.Stats.ServersReached}\n" + "**Settings**\n" + $"Enabled: {p.Settings.Enabled}\n" + $"Channel: {Context.Guild.GetChannel(p.Settings.ChannelId)?.Name ?? "N/A"}\n" + "**Config**\n" + $"Color (RGB): [{p.Message.Color.R}, {p.Message.Color.G}, {p.Message.Color.B}]\n" + $"Using Server Thumbnail: {p.Message.UseThumb}\n" + $"Showing UserCount: {p.Message.UserCount}\n" + $"Image URL: {p.Message.ImageUrl ?? "N/A"}\n" + $"Message: (Refer to Partner Message Embed, for raw do `{PrefixService.GetPrefix(Context.Guild.Id)}partner RawMessage`)\n" + "**Partner Message Embed**\n" + "(See Next Message)");
            await ReplyAsync(PartnerHelp.GenerateMessage(p, Context.Guild));
        }

        /// <summary>
        ///     The raw message.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("RawMessage")]
        [Summary("Show raw partner message with formatting")]
        public Task RawMessageAsync()
        {
            var p = PartnerService.GetPartnerInfo(Context.Guild.Id, true);
            return SimpleEmbedAsync(Format.Sanitize(p.Message.Content));
        }

        /// <summary>
        ///     Sets the partner channel
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        /// <exception cref="Exception">
        ///     throws if channel does not have enough users
        /// </exception>
        [Command("SetChannel")]
        [Summary("Set the current channel as partner channel")]
        public async Task SetChannelAsync()
        {
            var p = PartnerService.GetPartnerInfo(Context.Guild.Id, true);
            if ((decimal)(Context.Channel as SocketTextChannel).Users.Count / Context.Guild.Users.Count * 100 < 90)
            {
                throw new Exception("Partner messages will not be shared as this channel has less than 90% visibility in the server,\n" + "You can fix this by ensuring that all roles have permissions to view messages and message history in the channel settings");
            }

            if ((Context.Channel as SocketTextChannel).IsNsfw)
            {
                throw new Exception("Partner channel cannot be set in a NSFW channel");
            }

            p.Settings.ChannelId = Context.Channel.Id;
            p.Save();
            await SimpleEmbedAsync($"Partner Updates will now be sent in {Context.Channel.Name}");

            await PartnerHelp.PartnerLogAsync(Context.Client, p, new EmbedFieldBuilder { Name = "Partner Channel Updated", Value = $"Guild: {Context.Guild.Name} [{Context.Guild.Id}]\n" + $"Owner: {Context.Guild.Owner.Username}\n" + $"Users: {Context.Guild.MemberCount}" });
        }

        /// <summary>
        ///     Sets the partner message
        /// </summary>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        /// <exception cref="Exception">
        ///     throws if the message does not comply with the rules.
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
            
            if (message.ToLower().Contains("discord.gg") || message.ToLower().Contains("discordapp.com") || message.ToLower().Contains("discord.me") || Regex.Match(message, @"(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?(d+i+s+c+o+r+d+|a+p+p)+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$").Success)
            {
                throw new Exception("No need to include an invite to the bot in your message. PassiveBOT will auto-generate one");

                /*
                var invites = Regex.Matches(message, @"(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?(d+i+s+c+o+r+d+|a+p+p)+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$").OfType<Match>().ToList();
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
                    throw new Exception("Please ensure the message passes all checks:\n" + "1.Only invites from this server are allowed in the partner message!\n" + "2.Ensure that the invite link you are using is set to never expire\n" + "3.Ensure that it does not have a use limit.\n" + "4.If your server uses 2FA please disable it while running the command then re-enable it after\n" + "If you are using an invite for your server and you are seeing this message, please generate a new invite for your server\n\n" + $"If you believe this is an error, please contact the support server: {HomeModel.Load().HomeInvite}");
                }
                */
            }

            var p = PartnerService.GetPartnerInfo(Context.Guild.Id, true);
            p.Message.Content = message;
            p.Save();

            var generateMessage = PartnerHelp.GenerateMessage(p, Context.Guild);
            await ReplyAsync(generateMessage);

            await PartnerHelp.PartnerLogAsync(Context.Client, p, new EmbedFieldBuilder { Name = "Partner Message Updated", Value = $"Guild: {Context.Guild.Name} [{Context.Guild.Id}]\n" + $"Owner: {Context.Guild.Owner.Username}\n" + $"Users: {Context.Guild.MemberCount}" });
        }

        /// <summary>
        ///     Toggles the partner Thumbnail
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("Thumbnail")]
        [Summary("Toggle the Thumbnail of the server in the partner message")]
        public Task ThumbnailAsync()
        {
            var p = PartnerService.GetPartnerInfo(Context.Guild.Id, true);
            p.Message.UseThumb = !p.Message.UseThumb;
            p.Save();
            return ReplyAsync(PartnerHelp.GenerateMessage(p, Context.Guild));
        }

        [Command("RegenerateInvite")]
        [Summary("Regenerate your server's invite for partner messages")]
        public Task RegenerateAsync()
        {
            var p = PartnerService.GetPartnerInfo(Context.Guild.Id, true);
            p.Message.Invite = Context.Guild.GetTextChannel(p.Settings.ChannelId)?.CreateInviteAsync(null).Result?.Url;
            p.Save();
            return ReplyAsync(PartnerHelp.GenerateMessage(p, Context.Guild));
        }

        /// <summary>
        ///     The toggle.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("Toggle")]
        [Summary("Toggle the Program in the server")]
        public async Task ToggleAsync()
        {
            var p = PartnerService.GetPartnerInfo(Context.Guild.Id, true);
            p.Settings.Enabled = !p.Settings.Enabled;
            p.Save();

            await PartnerHelp.PartnerLogAsync(Context.Client, p, new EmbedFieldBuilder { Name = "Partner Toggled", Value = $"Enabled: {p.Settings.Enabled}" });
            await SimpleEmbedAsync($"Partner Program Enabled: {p.Settings.Enabled}");
        }

        /// <summary>
        ///     Toggles userCount in partner message
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("UserCount")]
        [Summary("Toggle the User Count in the footer of the partner message")]
        public Task UserCountAsync()
        {
            var p = PartnerService.GetPartnerInfo(Context.Guild.Id, true);
            p.Message.UserCount = !p.Message.UserCount;
            p.Save();
            return ReplyAsync(PartnerHelp.GenerateMessage(p, Context.Guild));
        }
    }
}