using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Configuration;
using PassiveBOT.Handlers;
using PassiveBOT.Preconditions;
using PassiveBOT.strings;

namespace PassiveBOT.Commands.ServerSetup
{
    [CheckModerator]
    [RequireContext(ContextType.Guild)]
    public class Partner : ModuleBase
    {
        [Command("PartnerToggle")]
        [Summary("PartnerToggle")]
        [Remarks("Toggle the Partner Channel Service")]
        public async Task PToggle()
        {
            var guild = GuildConfig.GetServer(Context.Guild);
            guild.PartnerSetup.IsPartner = !guild.PartnerSetup.IsPartner;
            GuildConfig.SaveServer(guild);
            await ReplyAsync($"Partner service enabled: {guild.PartnerSetup.IsPartner}");
            if (guild.PartnerSetup.IsPartner)
            {
                if (!TimerService.AcceptedServers.Contains(Context.Guild.Id))
                    TimerService.AcceptedServers.Add(Context.Guild.Id);
            }
            else
            {
                if (TimerService.AcceptedServers.Contains(Context.Guild.Id))
                    TimerService.AcceptedServers.Remove(Context.Guild.Id);
            }

            var home = Homeserver.Load().PartnerUpdates;
            var chan = await Context.Client.GetChannelAsync(home);
            if (chan is IMessageChannel channel)
            {
                var embed = new EmbedBuilder
                {
                    Title = "Partner Toggled",
                    Description = $"{Context.Guild.Name}\n" +
                                  $"`{Context.Guild.Id}`\n" +
                                  $"Status: {guild.PartnerSetup.IsPartner}"
                };
                await channel.SendMessageAsync("", false, embed.Build());
            }
        }

        [Command("PartnerChannel")]
        [Summary("PartnerChannel")]
        [Remarks("Set the Partner Channel")]
        public async Task PChannel()
        {
            var guild = GuildConfig.GetServer(Context.Guild);
            guild.PartnerSetup.PartherChannel = Context.Channel.Id;
            GuildConfig.SaveServer(guild);
            await ReplyAsync($"Partner Channel set to {Context.Channel.Name}");

            var home = Homeserver.Load().PartnerUpdates;
            var chan = await Context.Client.GetChannelAsync(home);
            if (chan is IMessageChannel channel)
            {
                var embed = new EmbedBuilder
                {
                    Title = "Partner Channel Set",
                    Description = $"{Context.Guild.Name}\n" +
                                  $"`{Context.Guild.Id}`\n" +
                                  $"Channel: {Context.Channel.Name}"
                };
                await channel.SendMessageAsync("", false, embed.Build());
            }
        }

        [Command("PartnerImage")]
        [Summary("PartnerImage <URL>")]
        [Remarks("Set an Image for partner servers")]
        public async Task PImage(string URL = null)
        {
            var guild = GuildConfig.GetServer(Context.Guild);
            if (URL == null)
            {
                guild.PartnerSetup.ImageUrl = null;
                await ReplyAsync("Image has been removed.");
            }
            else if (RedditHelper.isimage(URL).isimage)
            {
                guild.PartnerSetup.ImageUrl = URL;
                var embed = new EmbedBuilder {ImageUrl = URL};
                await ReplyAsync("Image Set", false, embed.Build());
                var home = Homeserver.Load().PartnerUpdates;
                var chan = await Context.Client.GetChannelAsync(home);
                if (chan is IMessageChannel channel)
                {
                    var embed2 = new EmbedBuilder
                    {
                        Title = "Partner Image Set",
                        Description = $"{Context.Guild.Name}\n" +
                                      $"`{Context.Guild.Id}`\n" +
                                      $"Channel: {Context.Channel.Name}",
                        ImageUrl = URL
                    };
                    await channel.SendMessageAsync("", false, embed2.Build());
                }
            }
            else
            {
                await ReplyAsync("You have not provided an image URL.");
                return;
            }
            GuildConfig.SaveServer(guild);
        }

        [Command("PartnerUserCount")]
        [Summary("PartnerUserCount")]
        [Remarks("Toggle wether or not to show your server's user count in partner message")]
        public async Task PUserCount()
        {
            var guild = GuildConfig.GetServer(Context.Guild);
            guild.PartnerSetup.showusercount = !guild.PartnerSetup.showusercount;
            await ReplyAsync($"Will show UserCount in pertner messages: {guild.PartnerSetup.showusercount}");
            var home = Homeserver.Load().PartnerUpdates;
            var chan = await Context.Client.GetChannelAsync(home);
            if (chan is IMessageChannel channel)
            {
                var embed2 = new EmbedBuilder
                {
                    Title = "Partner UserCount Toggled",
                    Description = $"{Context.Guild.Name}\n" +
                                    $"`{Context.Guild.Id}`\n" +
                                    $"Channel: {Context.Channel.Name}\n" +
                                  $"ShowUserCount: {guild.PartnerSetup.showusercount}"
                };
                await channel.SendMessageAsync("", false, embed2.Build());
            }

            GuildConfig.SaveServer(guild);
        }

        [Command("PartnerHelp")]
        [Summary("PartnerHelp")]
        [Remarks("See the PartnerHelp tutorial")]
        public async Task PHelp()
        {
            var embed = new EmbedBuilder
            {
                Title = "Partner Help Tutorial",
                Description = $"```\r\n" +
                              $"What is the partner program?\r\n" +
                              $"```\r\n" +
                              $"-This is a part of PassiveBOT, different servers may sign up for this by enabling the partner program with the command\r\n" +
                              $"\r\n" +
                              $"```\r\n" +
                              $"Full Tutorial\r\n" +
                              $"```\r\n" +
                              $"`1.` Type `.p invite` to get an invite link to the bot, add it to your server with all permissions\r\n" +
                              $"`2.` Type `.p PartnerToggle` to enable to partner system in your server\r\n" +
                              $"`3.` Type `.p PartnerChannel` in your desired channel to set which channel other Partner messages will be sent to\r\n" +
                              $"`4.` Type `.p PartnerMessage <message>` to set the message you want to be sent to other servers, make sure to include your `discord.gg` or `discord.me` invite link for the server so people can join.\r\n" +
                              $"**[OPTIONAL]**\n" +
                              $"`.p PartnerImage <image URL>` - set an image for your partner messages\n" +
                              $"`.p PartnerUserCount` - toggle wether or not to show your server's user count\n" +
                              $"\r\n" +
                              $"That's it, your message will be sent to a server every hour and your server will receive a message every hour!!\r\n" +
                              $"ENJOY!!!\n\n" +
                              $"```\n" +
                              $"**RULES**\n" +
                              $"```\n" +
                              $"It is important to note the following rules for the Partner Program:\r\n" +
                              $"-The partner channel must be visible to all roles in your server\r\n" +
                              $"-Profanity and other 18+ type content is not allowed in partner messages (you may tell people you have NSFW Content but that\'s all)\r\n" +
                              $"-The text must not contain hard to read characters or that which would be considered spammy. Ie. Zalgo text. Symbols and emojis are fine though.\r\n" +
                              $"-Using multiple servers/messages to promote a single server (ie. All with an invite to one server OR having invites to multiple different servers) is not allowed\r\n" +
                              $"`Any attempt to bypass these rules or breaking these rules may result in your server being Permanently Banned from the partner program`"
            };
            await ReplyAsync("", false, embed.Build());
        }

        [Command("PartnerReport")]
        [Summary("PartnerReport")]
        [Remarks("Report a partner message")]
        public async Task PReport([Remainder] string message = null)
        {
            if (message == null)
                await ReplyAsync(
                    "Please provide some information about the Partner message you are reporting, and we will do our best to remove it");
            else
                try
                {
                    var s = Homeserver.Load().Suggestion;
                    var c = await Context.Client.GetChannelAsync(s);
                    var embed = new EmbedBuilder();
                    embed.AddField($"Partner Message Report from {Context.User.Username}", message);
                    embed.WithFooter(x => { x.Text = $"{Context.Message.CreatedAt} || {Context.Guild.Name}"; });
                    embed.Color = Color.Blue;
                    await ((ITextChannel) c).SendMessageAsync("", false, embed.Build());
                    await ReplyAsync("Report Sent!!");
                }
                catch
                {
                    await ReplyAsync("The bots owner has not yet configured the Reports channel");
                }
        }

        [Command("PartnerMessage")]
        [Summary("PartnerMessage <message>")]
        [Remarks("Set your Servers PertnerMessage")]
        public async Task PMessage([Remainder] string input = null)
        {
            if (input == null)
            {
                await ReplyAsync("Please input a message");
                return;
            }

            if (input.Length > 1024)
            {
                await ReplyAsync($"Message is too long. Please limit it to 1024 characters or less. (Current = {input.Length})");
                return;
            }

            if (NsfwStr.Profanity.Any(x =>
                ProfanityFilter.doreplacements(ProfanityFilter.RemoveDiacritics(input.ToLower())).ToLower()
                    .Contains(x.ToLower())))
            {
                await ReplyAsync("Profanity Detected, unable to set message!");
                return;
            }

            if (Context.Message.MentionedRoleIds.Any() || Context.Message.MentionedUserIds.Any() ||
                Context.Message.MentionedChannelIds.Any() || Context.Message.Content.Contains("@everyone")
                || Context.Message.Content.Contains("@here"))
            {
                await ReplyAsync("There is no need to mention roles, users or channels in the partner " +
                                 "program as it shares to other servers which may not have access" +
                                 "to them!");
                return;
            }

            if (!input.Contains("discord.gg") && !input.Contains("discord.me"))
            {
                await ReplyAsync("You should include an invite link to your server in the Partner Message too!");
                return;
            }

            if (input.Contains("discord.gg"))
            {
                var invites = input.Split(' ').Where(x => x.Contains("discord.gg")).ToList();
                var officialinvites = ((SocketGuild) Context.Guild).GetInvitesAsync().Result;
                bool mismatch = false;
                foreach (var invite in invites)
                {
                    var match = officialinvites.FirstOrDefault(x => x.Url == invite);
                    if (match == null)
                    {
                        mismatch = true;
                    }
                }

                if (mismatch)
                {
                    await ReplyAsync("Only invites from this server are allowed in the partner message!\n" +
                                     "Also please ensure that the invite link you are using is set to never expire\n\n" +
                                     "If you are using an invite for your server and you are seeing this message, please generate a new invite for your server");
                    return;
                }
            }

            var guild = GuildConfig.GetServer(Context.Guild);
            guild.PartnerSetup.Message = input;
            GuildConfig.SaveServer(guild);
            var embed = new EmbedBuilder
            {
                Title = Context.Guild.Name,
                Description = input,
                ThumbnailUrl = Context.Guild.IconUrl,
                Color = Color.Green
            };

            await ReplyAsync("Success, here is your Partner Message:", false, embed.Build());

            var home = Homeserver.Load().PartnerUpdates;
            var chan = await Context.Client.GetChannelAsync(home);
            if (chan is IMessageChannel channel)
            {
                var embed2 = new EmbedBuilder
                {
                    Title = "Partner Msg. Updated",
                    Description = $"{Context.Guild.Name}\n" +
                                  $"`{Context.Guild.Id}`\n" +
                                  $"{guild.PartnerSetup.Message}",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"{((SocketGuild)Context.Guild).Owner.Username}#{((SocketGuild)Context.Guild).Owner.Discriminator}"
                    }
                };
                await channel.SendMessageAsync("", false, embed2.Build());
            }
        }

        [Command("PartnerPreview")]
        [Summary("PartnerPreview")]
        [Remarks("Preview how your server's partner message will appear to other servers.")]
        public async Task PPreview()
        {
            var guild = GuildConfig.GetServer(Context.Guild);
            var embed = new EmbedBuilder
            {
                Title = Context.Guild.Name,
                Description = guild.PartnerSetup.Message,
                ThumbnailUrl = Context.Guild.IconUrl,
                ImageUrl = guild.PartnerSetup.ImageUrl,
                Color = Color.Green,
                Footer = new EmbedFooterBuilder
                {
                    Text = (guild.PartnerSetup.showusercount ? $"User Count: {((SocketGuild)Context.Guild).MemberCount}" : null)
                }
            };
            await ReplyAsync("", false, embed.Build());
        }

        [Command("PartnerInfo")]
        [Summary("PartnerInfo")]
        [Remarks("See Partner Setup Info")]
        public async Task PInfo()
        {
            var embed = new EmbedBuilder();
            var guild = GuildConfig.GetServer(Context.Guild);
            embed.Description =
                $"Channel: {Context.Client.GetChannelAsync(guild.PartnerSetup.PartherChannel).Result?.Name}\n" +
                $"Enabled: {guild.PartnerSetup.IsPartner}\n" +
                $"Banned: {guild.PartnerSetup.banned}\n" +
                $"Image URL: {guild.PartnerSetup.ImageUrl}\n" +
                $"Show User Count: {guild.PartnerSetup.showusercount}\n" +
                $"Message:\n{guild.PartnerSetup.Message}";
            embed.Color = Color.Blue;
            await ReplyAsync("", false, embed.Build());
        }
    }
}