using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Configuration;
using PassiveBOT.Handlers.Services.Interactive;
using PassiveBOT.Handlers.Services.Interactive.Paginator;
using PassiveBOT.Preconditions;
using PassiveBOT.Configuration.Objects;

namespace PassiveBOT.Commands.ServerSetup
{

    [RequireAdmin]
    [RequireContext(ContextType.Guild)]
    public class AntiSpam : InteractiveBase
    {
        [Command("ignore")]
        [Summary("ignore <selection> <@role>")]
        [Remarks("choose a role to ignore when using antispam commands")]
        public async Task IgnoreRole(string selection, IRole role = null)
        {
            var guild = GuildConfig.GetServer(Context.Guild);
            if (role == null)
            {
                await IgnoreRole();
                return;
            }

            var intselections = selection.Split(',');
            var ignore = guild.Antispams.IngoreRoles.FirstOrDefault(x => x.RoleID == role.Id);
            var addrole = false;
            if (ignore == null)
            {
                ignore = new GuildConfig.antispams.IgnoreRole
                {
                    RoleID = role.Id
                };
                addrole = true;
            }


            if (int.TryParse(intselections[0], out var zerocheck))
            {
                if (zerocheck == 0)
                {
                    guild.Antispams.IngoreRoles.Remove(ignore);
                    await ReplyAsync("Success, Role has been removed form the ignore list");
                }
                else
                {
                    foreach (var s in intselections)
                        if (int.TryParse(s, out var sint))
                        {
                            if (sint < 1 || sint > 6)
                            {
                                await ReplyAsync($"Invalid Input {s}\n" +
                                                 $"only 1-5 are accepted.");
                                return;
                            }

                            switch (sint)
                            {
                                case 1:
                                    ignore.AntiSpam = true;
                                    break;
                                case 2:
                                    ignore.Blacklist = true;
                                    break;
                                case 3:
                                    ignore.Mention = true;
                                    break;
                                case 4:
                                    ignore.Advertising = true;
                                    break;
                                case 5:
                                    ignore.Privacy = true;
                                    break;
                                case 6:
                                    ignore.Toxicity = true;
                                    break;
                            }
                        }
                        else
                        {
                            await ReplyAsync($"Invalid Input {s}");
                            return;
                        }

                    var embed = new EmbedBuilder
                    {
                        Description = $"{role.Mention}\n" +
                                      "__Ignore Antispam Detections__\n" +
                                      $"Bypass Antispam: {ignore.AntiSpam}\n" +
                                      $"Bypass Blacklist: {ignore.Blacklist}\n" +
                                      $"Bypass Mention Everyone and 5+ Role Mentions: {ignore.Mention}\n" +
                                      $"Bypass Invite Link Removal: {ignore.Advertising}\n" +
                                      $"Bypass IP Removal: {ignore.Privacy}\n" +
                                      $"Bypass Toxicity Check: {ignore.Toxicity}"
                    };
                    await ReplyAsync("", false, embed.Build());
                }

                if (addrole) guild.Antispams.IngoreRoles.Add(ignore);
                GuildConfig.SaveServer(guild);
            }
            else
            {
                await ReplyAsync("Input Error!");
            }
        }

        [Command("ignore")]
        [Summary("ignore")]
        [Remarks("ignore role setup information")]
        public async Task IgnoreRole()
        {
            await ReplyAsync("", false, new EmbedBuilder
            {
                Description =
                    $"You can select roles to ignore from all spam type checks in this module using the ignore command.\n" +
                    $"__Key__\n" +
                    $"`1` - Antispam\n" +
                    $"`2` - Blacklist\n" +
                    $"`3` - Mention\n" +
                    $"`4` - Invite\n" +
                    $"`5` - IP Addresses\n" +
                    $"`6` - Toxicity\n\n" +
                    $"__usage__\n" +
                    $"`{Config.Load().Prefix} 1 @role` - this allows the role to spam without being limited/removed\n" +
                    $"You can use commas to use multiple settings on the same role." +
                    $"`{Config.Load().Prefix} 1,2,3 @role` - this allows the role to spam, use blacklisted words and bypass mention filtering without being removed\n" +
                    $"`{Config.Load().Prefix} 0 @role` - resets the ignore config and will add all limits back to the role"
            }.Build());
        }


        [Command("SetMuted")]
        [Summary("SetMuted <@role>")]
        [Remarks("Set the Mute Role For your server NOTE: Will try to reset all permissions for that role!")]
        public async Task SetMute(SocketRole muteRole)
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);

            jsonObj.RoleConfigurations.MutedRole = muteRole.Id;
            string perms;
            var channels = "";
            try
            {
                var unverifiedPerms =
                    new OverwritePermissions(sendMessages: PermValue.Deny, addReactions: PermValue.Deny);
                foreach (var channel in Context.Guild.TextChannels)
                    try
                    {
                        await channel.AddPermissionOverwriteAsync(muteRole, unverifiedPerms);
                        channels += $"`#{channel.Name}` Perms Modified\n";
                    }
                    catch
                    {
                        channels += $"`#{channel.Name}` Perms Not Modified\n";
                    }

                perms = "Role Can No longer Send Messages, or Add Reactions";
            }
            catch
            {
                perms = "Role Unable to be modified, ask an administrator to do this manually.";
            }


            GuildConfig.SaveServer(jsonObj);


            await ReplyAsync($"Muted Role has been set as {muteRole.Mention}\n" +
                             $"{perms}\n" +
                             $"{channels}");
        }

        [Command("NoToxicity")]
        [Summary("NoToxicity <threshhold amount>")]
        [Remarks("Toggle the auto-removal of Toxic Messages")]
        public async Task Toxicity(int threshhold = 90)
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.Antispams.Toxicity.UsePerspective = !jsonObj.Antispams.Toxicity.UsePerspective;
            jsonObj.Antispams.Toxicity.ToxicityThreshHold = threshhold;
            GuildConfig.SaveServer(jsonObj);

            await ReplyAsync($"Toxicity Removal: {jsonObj.Antispams.Toxicity.UsePerspective}\n" +
                             $"Threshhold: {jsonObj.Antispams.Toxicity.ToxicityThreshHold}% (will delete messages detected as this amount toxic!)\n\n" +
                             $"NOTE: In using Toxicity detection, the time it takes for commands to run will be increased slightly as this requires a web-request per message.\n" +
                             $"Consider disabling Toxicity Detection if you experience such issues.");
        }

        [Command("NoIPs")]
        [Summary("NoIps")]
        [Remarks("Toggle the auto-removal of IP addresses")]
        public async Task NoIP()
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.Antispams.Privacy.RemoveIPs = !jsonObj.Antispams.Privacy.RemoveIPs;
            GuildConfig.SaveServer(jsonObj);

            await ReplyAsync($"IPs Removal: {jsonObj.Antispams.Privacy.RemoveIPs}");
        }

        [Command("NoInvite")]
        [Summary("NoInvite <true/false>")]
        [Remarks("disables/enables the sending of invites in a server from regular members")]
        public async Task NoInvite()
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.Antispams.Advertising.Invite = !jsonObj.Antispams.Advertising.Invite;
            GuildConfig.SaveServer(jsonObj);

            if (jsonObj.Antispams.Advertising.Invite)
                await ReplyAsync("Invite links will now be deleted!");
            else
                await ReplyAsync("Invite links are now allowed to be sent");
        }


        [Command("NoInviteMessage")]
        [Summary("NoInviteMessage <message>")]
        [Remarks("set the no invites message")]
        public async Task NoinviteMSG([Remainder] string noinvmessage = null)
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.Antispams.Advertising.NoInviteMessage = noinvmessage;
            GuildConfig.SaveServer(jsonObj);

            await ReplyAsync("The No Invites message is now:\n" +
                             $"{jsonObj.Antispams.Advertising.NoInviteMessage ?? "Default"}");
        }


        [Command("NoSpam")]
        [Summary("NoSpam")]
        [Remarks("Toggle wether or not to disable spam in the server")]
        public async Task SpamToggle()
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.Antispams.Antispam.NoSpam = !jsonObj.Antispams.Antispam.NoSpam;
            GuildConfig.SaveServer(jsonObj);

            await ReplyAsync($"NoSpam: {jsonObj.Antispams.Antispam.NoSpam}");
        }

        [Command("SkipAntiSpam")]
        [Summary("SkipAntiSpam <message>")]
        [Remarks("Skip antispam on messages starting with the given message (useful for gambling commands)")]
        public async Task SkipAntiSpam([Remainder] string message = null)
        {
            if (message == null)
            {
                await ReplyAsync("Please provide a message that will be skipped.");
                return;
            }

            var guild = GuildConfig.GetServer(Context.Guild);

            if (guild.Antispams.Antispam.AntiSpamSkip.Any(x =>
                string.Equals(x, message, StringComparison.CurrentCultureIgnoreCase)))
            {
                await ReplyAsync($"`{message}` is already included in the SkipAntiSpam list");
                return;
            }

            guild.Antispams.Antispam.AntiSpamSkip.Add(message);

            GuildConfig.SaveServer(guild);
            await ReplyAsync("Complete.");
        }

        [Command("RemSkipAntiSpam")]
        [Summary("RemSkipAntiSpam <message>")]
        [Remarks("Remove a message from anti spam skipper")]
        public async Task RemSkipAntiSpam([Remainder] string message = null)
        {
            if (message == null)
            {
                await ReplyAsync("Please provide a message that will be removed.");
                return;
            }

            var guild = GuildConfig.GetServer(Context.Guild);

            if (!guild.Antispams.Antispam.AntiSpamSkip.Any(x =>
                string.Equals(x, message, StringComparison.CurrentCultureIgnoreCase)))
            {
                await ReplyAsync($"`{message}` is already not included in the SkipAntiSpam list");
                return;
            }

            guild.Antispams.Antispam.AntiSpamSkip.Remove(message);

            GuildConfig.SaveServer(guild);
            await ReplyAsync("Complete.");
        }

        [Command("ClearSkipAntiSpam")]
        [Summary("ClearSkipAntiSpam")]
        [Remarks("Clear the SkipAntiSpam List")]
        public async Task ClearAntiSpam()
        {
            var guild = GuildConfig.GetServer(Context.Guild);
            guild.Antispams.Antispam.AntiSpamSkip = new List<string>();

            GuildConfig.SaveServer(guild);
            await ReplyAsync("Complete.");
        }

        [Command("SkipAntiSpamList")]
        [Summary("SkipAntiSpamList")]
        [Remarks("List of messages antispam will skip")]
        public async Task SkipAntiSpam()
        {
            var guild = GuildConfig.GetServer(Context.Guild);
            var embed = new EmbedBuilder { Description = string.Join("\n", guild.Antispams.Antispam.AntiSpamSkip) };
            await ReplyAsync("", false, embed.Build());
        }

        [Command("NoMassMention")]
        [Summary("NoMassMention")]
        [Remarks("Stops users from tagging more than 5 users or roles in a single message")]
        public async Task NoMassMention()
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.Antispams.Mention.RemoveMassMention = !jsonObj.Antispams.Mention.RemoveMassMention;
            GuildConfig.SaveServer(jsonObj);

            if (jsonObj.Antispams.Mention.RemoveMassMention)
                await ReplyAsync("Mass Mentions will now be deleted!");
            else
                await ReplyAsync("Mass Mentions are now allowed to be sent");
        }

        [Command("NoMention")]
        [Summary("NoMention")]
        [Remarks("disables/enables the use of @ everyone and @ here in a server from regular members")]
        public async Task NoMention()
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.Antispams.Mention.MentionAll = !jsonObj.Antispams.Mention.MentionAll;
            GuildConfig.SaveServer(jsonObj);

            if (jsonObj.Antispams.Mention.MentionAll)
                await ReplyAsync("Everyone and Here mentions will be deleted");
            else
                await ReplyAsync("Everyone and Here mentions will no longer be deleted");
        }

        [Command("NoMentionMessage")]
        [Summary("NoMentionMessage <meggage>")]
        [Remarks("set the no mention message")]
        public async Task NoMentionMSG([Remainder] string noMentionmsg = null)
        {
            var jsonObj = GuildConfig.GetServer(Context.Guild);
            jsonObj.Antispams.Mention.MentionAllMessage = noMentionmsg;
            GuildConfig.SaveServer(jsonObj);

            await ReplyAsync("The blacklist message is now:\n" +
                             $"{jsonObj.Antispams.Mention.MentionAllMessage ?? "Default"}");
        }
    }
}