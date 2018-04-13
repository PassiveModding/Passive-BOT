using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Configuration;
using PassiveBOT.Discord.Addons.Interactive;
using PassiveBOT.Discord.Addons.Interactive.Paginator;
using PassiveBOT.Handlers;

namespace PassiveBOT.Commands.OwnerCmds
{
    [Preconditions.BotModerator]
    public class BotModerator : InteractiveBase
    {
        [Command("PartnerList+", RunMode = RunMode.Async)]
        [Summary("PartnerList+")]
        [Remarks("Get a complete list of all partner servers")]
        public async Task PListF2()
        {
            var pages = new List<PaginatedMessage.Page>();
            foreach (var guild in Context.Client.Guilds)
            {
                try
                {
                    var guildobj = GuildConfig.GetServer(guild);
                    if (!guildobj.PartnerSetup.IsPartner) continue;

                    var pchannel = (ITextChannel)guild.GetChannel(guildobj.PartnerSetup.PartherChannel);
                    var Checking = "";
                    if (pchannel != null)
                    {
                        var ChannelOverWrites = pchannel.PermissionOverwrites;

                        foreach (var OverWrite in ChannelOverWrites)
                        {
                            try
                            {
                                var Name = "N/A";
                                if (OverWrite.TargetType == PermissionTarget.Role)
                                {
                                    var Role = guild.Roles.FirstOrDefault(x => x.Id == OverWrite.TargetId);
                                    if (Role != null)
                                    {
                                        Name = Role.Name;
                                    }
                                }
                                else
                                {
                                    var user = guild.Users.FirstOrDefault(x => x.Id == OverWrite.TargetId);
                                    if (user != null)
                                    {
                                        Name = user.Username;
                                    }
                                }

                                if (OverWrite.Permissions.ReadMessages == PermValue.Deny)
                                {
                                    Checking += $"{Name} Cannot Read Msgs.\n";
                                }

                                if (OverWrite.Permissions.ReadMessageHistory == PermValue.Deny)
                                {
                                    Checking += $"{Name} Cannot Read History.\n";
                                }

                            }
                            catch
                            {
                                //
                            }

                        }
                    }

                    var pmessage = guildobj.PartnerSetup.Message;
                    if (pmessage.Length > 1024)
                    {
                        pmessage = pmessage.Substring(0, 1024);
                    }

                    pages.Add(new PaginatedMessage.Page
                    {
                        dynamictitle = $"{guild.Name} - {guild.Id} - `{(guildobj.PartnerSetup.banned ? "BANNED" : "PUBLIC")}`",
                        description = $"__**Message:**__\n\n" +
                                      $"{(pmessage ?? "N/A")}\n\n" +
                                      $"__**Permissions:**__\n" +
                                      $"{Checking}\n\n" +
                                      $"__**Channel Info:**__\n" +
                                      $"Topic: {pchannel.Topic}\n" +
                                      $"Name: {pchannel.Name}\n" +
                                      $"Image: {guildobj.PartnerSetup.ImageUrl}\n" +
                                      $"UserCount: {guildobj.PartnerSetup.showusercount}\n\n" +
                                      $"__**Guild Info:**__\n" +
                                      $"Owner: {guild.Owner.Username}\n" +
                                      $"Owner ID: {guild.OwnerId}\n" +
                                      $"UserCount: {guild.MemberCount}\n" +
                                      $"Message Length: {guildobj.PartnerSetup.Message?.Length}\n",
                        imageurl = guildobj.PartnerSetup.ImageUrl
                    });

                }
                catch
                {
                    //
                }
            }
            var msg = new PaginatedMessage
            {
                Title = "Partner Messages",
                Pages = pages,
                Color = new Color(114, 137, 218)
            };

            await PagedReplyAsync(msg);
        }


        [Command("PartnerInfo+", RunMode = RunMode.Async)]
        [Summary("PartnerInfo+ <ID>")]
        [Remarks("Get Partner information via guild ID")]
        public async Task Pinfo2(ulong GuildID)
        {
            var Guild = Context.Client.Guilds.FirstOrDefault(x => x.Id == GuildID);
            if (Guild == null)
            {
                await ReplyAsync("Invalid Guild");
                return;
            }
            var guild = GuildConfig.GetServer(Guild);
            var pages = new List<PaginatedMessage.Page>();
            var PChannel = Guild.Channels.FirstOrDefault(x => x.Id == guild.PartnerSetup.PartherChannel);
            var Pchannelname = PChannel == null ? "Null" : PChannel.Name;
            pages.Add(new PaginatedMessage.Page
            {
                dynamictitle = "Info",
                description = $"{Guild.Name} - [{Guild.Id}]\n" +
                          $"Is Partner: { guild.PartnerSetup.IsPartner}\n" +
                          $"Is Banned: {guild.PartnerSetup.banned}\n" +
                          $"Partner Channel ID: {guild.PartnerSetup.PartherChannel}\n" +
                          $"Partner Channel Name: {Pchannelname}\n"
            });
            pages.Add(new PaginatedMessage.Page
            {
                dynamictitle = $"Server Message",
                description = $"---\n" +
                          $"{guild.PartnerSetup.Message}" +
                          $"\n---"
            });
            if (PChannel != null)
            {
                var ChannelOverWrites = PChannel.PermissionOverwrites;
                var Checking = "";
                foreach (var OverWrite in ChannelOverWrites)
                {
                    try
                    {
                        var Name = "N/A";
                        if (OverWrite.TargetType == PermissionTarget.Role)
                        {
                            var Role = Guild.Roles.FirstOrDefault(x => x.Id == OverWrite.TargetId);
                            if (Role != null)
                            {
                                Name = Role.Name;
                            }
                        }
                        else
                        {
                            var user = Guild.Users.FirstOrDefault(x => x.Id == OverWrite.TargetId);
                            if (user != null)
                            {
                                Name = user.Username;
                            }
                        }

                        if (OverWrite.Permissions.ReadMessages == PermValue.Deny)
                        {
                            Checking += $"{Name} Cannot Read Msgs.\n";
                        }

                        if (OverWrite.Permissions.ReadMessageHistory == PermValue.Deny)
                        {
                            Checking += $"{Name} Cannot Read History.\n";
                        }
                        
                    }
                    catch
                    {
                        //
                    }

                }

                if (Checking != "")
                {
                    pages.Add(new PaginatedMessage.Page
                    {
                        dynamictitle = "Partner Channel Perms",
                        description = $"---\n" +
                                  $"{Checking}" +
                                  $"\n---"
                    });
                }

            }
            var msg = new PaginatedMessage
            {
                Title = "Partner Messages",
                Pages = pages,
                Color = new Color(114, 137, 218)
            };

            await PagedReplyAsync(msg);
        }

        [Command("BanPartner+")]
        [Summary("BanPartner+ <guildID>")]
        [Remarks("Ban a partner from the partners program")]
        public async Task BanPartner(ulong GuildId = 0)
        {
            var guildobj = GuildConfig.GetServer(Context.Client.GetGuild(GuildId));
            guildobj.PartnerSetup.banned = true;
            GuildConfig.SaveServer(guildobj);
            await ReplyAsync("Guild Partnership Banned for:\n" +
                             $"{guildobj.PartnerSetup.Message}");
        }

        [Command("UnbanPartner+")]
        [Summary("UnbanPartner+ <guildID>")]
        [Remarks("Ban a partner from the partners program")]
        public async Task UnBanPartner(ulong GuildId = 0)
        {
            var guildobj = GuildConfig.GetServer(Context.Client.GetGuild(GuildId));
            guildobj.PartnerSetup.banned = false;
            guildobj.PartnerSetup.Message = "";
            GuildConfig.SaveServer(guildobj);
            await ReplyAsync("Guild Partnership Unbanned. Message has been reset");
        }

        [Command("GlobalGetUser+")]
        [Summary("GlobalGetUser+ <UserID>")]
        [Remarks("Get a user's info (if they share a server with the bot)")]
        public async Task GlobalGetuser(ulong UserId = 0)
        {
            if (Context.Client.Guilds.Any(x => x.Users.Any(u => u.Id == UserId)))
            {
                var guilds = Context.Client.Guilds.Where(x => x.Users.Any(u => u.Id == UserId)).Select(x => $"{x.Name} - {x.Id}");
                var user = Context.Client.GetUser(UserId);
                await ReplyAsync($"Name: {user.Username}#{user.Discriminator}\n" +
                                 $"ID: {user.Id}\n" +
                                 $"Guilds:\n" +
                                 $"{string.Join("\n", guilds)}");
            }
            else
            {
                await ReplyAsync("User Unavailable");                
            }

        }

        [Command("NoCommands+")]
        [Summary("NoCommands+ <UserID>")]
        [Remarks("Ban a user from using all bot commands")]
        public async Task GlobalBan(ulong UserId = 0)
        {
            var hs = Homeserver.Load();
            var us = Context.Client.GetUser(UserId);
            if (us == null)
            {
                await ReplyAsync("Unable to Get User, adding unknown global ban");
                hs.GlobalBans.Add(new Homeserver.globalban
                {
                    Name = "Unknown User",
                    ID = UserId
                });
            }
            else
            {
                hs.GlobalBans.Add(new Homeserver.globalban
                {
                    Name = us.Username,
                    ID = us.Id
                });                
            }
            Homeserver.SaveHome(hs);
            await ReplyAsync("User banned from PassiveBOT Commands");
        }

        [Command("YesCommands+")]
        [Summary("YesCommands+ <UserID>")]
        [Remarks("UnBan a user from using all bot commands")]
        public async Task GlobalUnBan(ulong UserId = 0)
        {
            var hs = Homeserver.Load();
            hs.GlobalBans.Remove(hs.GlobalBans.FirstOrDefault(x => x.ID == UserId));
            Homeserver.SaveHome(hs);
            await ReplyAsync("User UnBanned from PassiveBOT Commands");
        }

        [Command("NoCommandsList+")]
        [Summary("NoCommandsList+")]
        [Remarks("View all global bans")]
        public async Task GlobalBans()
        {
            var hs = Homeserver.Load();
            try
            {
                var embed = new EmbedBuilder().WithDescription(string.Join("\n", hs.GlobalBans.Select(x => $"{x.Name} || {x.ID}")));
                embed.Title = "Global Bans";
                await ReplyAsync("", false, embed.Build());
            }
            catch
            {
                await ReplyAsync("No Global Bans");
            }

        }

        [Command("ViewRedditCache+")]
        [Summary("ViewRedditCache+")]
        [Remarks("Get a list of cached subreddits")]
        public async Task RCache()
        {
            var pages = new List<string>();
            var curpage = "";
            foreach (var sub in CommandHandler.SubReddits)
            {
                curpage += $"__{sub.title}__\n" +
                           $"Cached Posts: {sub.Posts.Count}\n" +
                           $"NSFW Posts: {sub.Posts.Count(x => x.NSFW)}\n" +
                           $"Last Refresh: {sub.LastUpdate}\n" +
                           $"Hits Since Refresh: {sub.Hits}\n";
                if (curpage.Length > 400)
                {
                    pages.Add(curpage);
                    curpage = "";
                }
            }
            pages.Add(curpage);
            var msg = new PaginatedMessage
            {
                Title = "Reddit Cache",
                Pages = pages.Select(x => new PaginatedMessage.Page
                {
                    description = x
                }),
                Color = new Color(114, 137, 218)
            };

            await PagedReplyAsync(msg);
        }

        [Command("ClearRedditCache+")]
        [Summary("ClearRedditCache+")]
        [Remarks("Clear all cached subreddits")]
        public async Task CCache()
        {
            CommandHandler.SubReddits = new List<CommandHandler.SubReddit>();
            await ReplyAsync("SubReddit Cache Cleared");
        }

        [Command("GetCMDUses+")]
        [Summary("GetCMDUses+")]
        [Remarks("Get a list of all commands that have been used since restart")]
        public async Task CUses()
        {
            var pages = new List<string>();
            var curpage = "";
            foreach (var cmd in CommandHandler.CommandUses.OrderByDescending(x => x.Uses))
            {
                curpage += $"{cmd.Name} - {cmd.Uses}\n";
                if (curpage.Length > 400)
                {
                    pages.Add(curpage);
                    curpage = "";
                }
            }
            pages.Add(curpage);
            var msg = new PaginatedMessage
            {
                Title = "Command Uses",
                Pages = pages.Select(x => new PaginatedMessage.Page
                {
                    description = x
                }),
                Color = new Color(114, 137, 218)
            };

            await PagedReplyAsync(msg);
        }

        [Command("GetInvite+")]
        [Summary("GetInvite+ <guild ID>")]
        [Remarks("Create an invite to the specified server")]
        public async Task GetInvite(ulong id)
        {
            if (id <= 0)
                await ReplyAsync("Please enter a valid Guild ID");

            foreach (var guild in Context.Client.Guilds)
                if (guild.Id == id)
                    foreach (var channel in guild.Channels)
                        try
                        {
                            var inv = channel.CreateInviteAsync().Result.Url;
                            await ReplyAsync(inv);
                            return;
                        }
                        catch
                        {
                            //
                        }

            await ReplyAsync("No Invites able to be created.");
        }

        [Command("GetAlias+")]
        [Summary("GetAlias+ <@User>")]
        [Remarks("Get user aliases")]
        public async Task GetAlias(IGuildUser user)
        {
            var HS = Homeserver.Load();
            var Alias = HS.Aliases.FirstOrDefault(x => x.UserID == user.Id);
            if (Alias == null)
            {
                await ReplyAsync("No Logged aliases for this user");
            }
            else
            {
                var pages = new List<PaginatedMessage.Page>();
                foreach (var guild in Alias.Guilds)
                {
                    pages.Add(new PaginatedMessage.Page
                    {
                        dynamictitle = guild.GuildName,
                        description = string.Join("\n", guild.GuildAliases.OrderByDescending(x => x.DateChanged).Select(x => x.Name))
                    });
                }
                var paginated = new PaginatedMessage
                {
                    Title = $"{user.Username}'s Nickname History",
                    Pages = pages,
                    Color = Color.Blue
                };
                await PagedReplyAsync(paginated);
            }
        }
    }
}