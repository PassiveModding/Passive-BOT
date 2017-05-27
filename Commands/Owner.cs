using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ImageSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace PassiveBOT.Commands
{
    [RequireOwner]
    public class Owner : ModuleBase
    {
        public DiscordSocketClient client;

        [Command("die"), Summary("die"), Remarks("Kills the bot (owner only)")]
        public async Task Die()
        {
            await ReplyAsync("Bye Bye :heart:");
            await client.StopAsync();
            Environment.Exit(1);
        }

        [Command("Username"), Summary("username 'name'"), Remarks("Sets the bots username")]
        public async Task UsernameAsync([Remainder] string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be empty");
            var client = Context.Client as DiscordSocketClient;
            await Context.Client.CurrentUser.ModifyAsync(x => x.Username = value).ConfigureAwait(false);
            await ReplyAsync("Bot Username updated").ConfigureAwait(false);
        }

        [Command("nopre"), Summary("nopre"), Remarks("toggles prefixless commands in the current server")]
        [RequireContext(ContextType.Guild)]
        public async Task Nopre()
        {
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "moderation/prefix/")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "moderation/prefix/"));

            var lines = File.ReadAllLines(AppContext.BaseDirectory + $"moderation/prefix/nopre.txt");
            List<string> result = lines.ToList();
            if (result.Contains(Context.Guild.Id.ToString()))
            {
                var oldLines = File.ReadAllLines($"{AppContext.BaseDirectory + $"moderation/prefix/nopre.txt"}");
                var newLines = oldLines.Where(line => !line.Contains(Context.Guild.Id.ToString()));
                File.WriteAllLines($"{AppContext.BaseDirectory + $"moderation/prefix/nopre.txt"}", newLines);
                await ReplyAsync($"{Context.Guild} has been removed from the noprefix list (secret commands and prefixless commands are now enabled)");
            }
            else
            {
                File.AppendAllText($"{AppContext.BaseDirectory + $"moderation/prefix/nopre.txt"}", $"{Context.Guild.Id}" + Environment.NewLine);
                await ReplyAsync($"{Context.Guild} has been added to the noprefix list (secret commands and prefixless commands are now disabled)");
            }
        }

        [Command("kicks-"), Summary("kicks"), Remarks("Users kicked by passivebot")]
        [RequireContext(ContextType.Guild)]
        public async Task Kicks()
        {
            var kicks = File.ReadAllText(AppContext.BaseDirectory + $"moderation/kick/{Context.Guild.Id}.txt");
            await ReplyAsync("```\n" + kicks + "\n```");
        }
        [Command("warns-"), Summary("warns"), Remarks("Users warned by passivebot")]
        [RequireContext(ContextType.Guild)]
        public async Task Warns()
        {
            var warns = File.ReadAllText(AppContext.BaseDirectory + $"moderation/warn/{Context.Guild.Id}.txt");
            await ReplyAsync("```\n" + warns + "\n```");
        }
        [Command("bans-"), Summary("bans"), Remarks("Users banned by passivebot")]
        [RequireContext(ContextType.Guild)]
        public async Task Bans()
        {
            var bans = File.ReadAllText(AppContext.BaseDirectory + $"moderation/ban/{Context.Guild.Id}.txt");
            await ReplyAsync("```\n" + bans + "\n```");
        }

        //from RICK
        [Command("ServerList"), Summary("Normal Command"), Remarks("Get's a list of all guilds the bot is in."), Alias("Sl")]
        public async Task ServerListAsync()
        {
            var client = Context.Client as DiscordSocketClient;
            var Info = await client.GetApplicationInfoAsync();

            var String = new StringBuilder();
            foreach (SocketGuild guild in client.Guilds)
            {
                string List = $"{guild.Name} || **Owner:** {guild.Owner.Username}\n";
                String.AppendLine(List);
            }
            await (await Info.Owner.CreateDMChannelAsync()).SendMessageAsync(String.ToString());
        }
        [Command("Info"), Summary("Normal Command"), Remarks("Shows application info")]
        public async Task InfoAsync()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            var AppInfo = Process.GetCurrentProcess();
            var S = new StringBuilder();
            var Is64BitStr = Environment.Is64BitProcess ? "Yes" : "No";
            var Is64Bit = IntPtr.Size == 8 ? "Yes" : "No";
            var IsOS64 = Environment.Is64BitOperatingSystem ? "Yes" : "No";
            var isMono = Environment.Is64BitOperatingSystem ? "Yes" : "No";
            string _description = $"{Format.Bold("Info")}\n" +
                                $"- Author: {application.Owner.Username} (ID {application.Owner.Id})\n" +
                                $"- Library: Discord.Net ({DiscordConfig.Version})\n" +
                                $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}\n" +
                                $"- Uptime: {(DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss")}\n\n" +

                                $"{Format.Bold("Stats")}\n" +
                                $"- Heap Size: {Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString()} MB\n" +
                                $"- Guilds: {(Context.Client as DiscordSocketClient).Guilds.Count}\n" +
                                $"- Channels: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count)}\n" +
                                $"- Users: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Users.Count)}\n\n" +

                                $"{Format.Bold("Full dump of all diagnostic information about this instance.")}\n" +
                                $"- PID: {AppInfo.Id}\n" +
                                $"- Is 64-bit: {Is64BitStr}\n" +
                                $"- Is 64-bit: {Is64Bit}\n" +
                                $"- Thread count: {AppInfo.Threads.Count}\n" +
                                $"- Total processor time: {AppInfo.TotalProcessorTime:c}\n" +
                                $"- User processor time: {AppInfo.UserProcessorTime:c}\n" +
                                $"- Privileged processor time: {AppInfo.PrivilegedProcessorTime:c}\n" +
                                $"- Handle count: {AppInfo.HandleCount:#,##0}\n" +
                                $"- Working set: {AppInfo.WorkingSet64.ToString()}\n" +
                                $"- Virtual memory size: {AppInfo.VirtualMemorySize64.ToString()}\n" +
                                $"- Paged memory size: {AppInfo.PagedMemorySize64.ToString()}\n\n" +

                                $"{Format.Bold("OS and .Net")}" +
                                $"- OS platform: {Environment.OSVersion.Platform.ToString()}\n" +
                                $"- OS version: {Environment.OSVersion.Version} ({Environment.OSVersion.VersionString})\n" +
                                $"- OS is 64-bit: {IsOS64}\n" +
                                $"- .NET is Mono: {isMono}\n";
            var embed = new EmbedBuilder()
                .WithTitle("PassiveBOT info")
                .WithDescription(_description);

            
            await ReplyAsync("",false, embed.Build());
        }

        //Works but I dont reccomend using as it can be annoying to many people
        /*
        [Command("Broadcast"), Summary("broadcast 'message'"), Remarks("Sends a message to ALL severs that the bot is connected to."), Alias("Yell", "Shout")]
        public async Task AsyncBroadcast([Remainder] string msg)
        {
            var glds = (Context.Client as DiscordSocketClient).Guilds;
            var defaultchan = glds.Select(g => g.GetChannel(g.Id)).Cast<ITextChannel>();
            await Task.WhenAll(defaultchan.Select(c => c.SendMessageAsync(msg)));
        }*/

        //basically gives owner of the bot access to all permissions the bot has, kinda cheaty so its not included
        /*
        [Command("kick-"), Summary("kick '@badperson' 'for not being cool'"), Remarks("Kicks the specified user (requires Kick Permissions)")]
        [RequireContext(ContextType.Guild)]
        public async Task Kickuser(SocketGuildUser user, [Remainder, Optional] string reason)
        {
            if (user.GuildPermissions.ManageRoles == true)
            {
                await ReplyAsync($"**ERROR: **you cannot kick a a user with manage roles permission");
                return;
            }
            else if (reason == null)
            {
                await ReplyAsync("**ERROR: **Please specify a reason for Kicking the user");
                return;
            }
            else
            {
                await ReplyAsync($"{user.Mention} you have been kicked for `{reason}`:bangbang: ");
                var dm = await user.CreateDMChannelAsync();
                await dm.SendMessageAsync($"{user.Mention} you have been kicked from {Context.Guild} for `{reason}`");
                await user.KickAsync();

                var warnpath = Path.Combine(AppContext.BaseDirectory, $"moderation/kick/{Context.Guild.Id}.txt");
                if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "moderation/kick/")))
                    Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "moderation/kick/"));

                File.AppendAllText(AppContext.BaseDirectory + $"moderation/kick/{Context.Guild.Id}.txt", $"User: {user} || Moderator: {Context.User} || Reason: {reason}" + Environment.NewLine);
            }
        }
        [Command("warn-"), Summary("warn '@naughtykiddo' 'for being a noob'"), Remarks("warns the specified user")]
        [RequireContext(ContextType.Guild)]
        public async Task NewWarnuser(SocketGuildUser user, [Remainder, Optional] string reason)
        {
            if (user.GuildPermissions.ManageRoles == true)
            {
                await ReplyAsync($"**ERROR: **you cannot warn a a user with manage roles permission");
                return;
            }
            else if (reason == null)
            {
                await ReplyAsync("**ERROR: **Please Specify a reason for warning the user");
                return;
            }
            else
            {
                await ReplyAsync($"{user.Mention} you have been warned for `{reason}`");
                var dm = await user.CreateDMChannelAsync();
                await dm.SendMessageAsync($"{user.Mention} you have been warned for `{reason}` in {Context.Guild}");


                var warnpath = Path.Combine(AppContext.BaseDirectory, $"moderation/warn/{Context.Guild.Id}.txt");
                if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "moderation/warn/")))
                    Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "moderation/warn/"));

                File.AppendAllText(AppContext.BaseDirectory + $"moderation/warn/{Context.Guild.Id}.txt", $"User: {user} || Moderator: {Context.User} || Reason: {reason}" + Environment.NewLine);

            }

        }
        [Command("ban-"), Summary("ban 'badfag' 'for sucking'"), Remarks("bans the specified user (requires Ban Permissions)")]
        [RequireContext(ContextType.Guild)]
        public async Task Banuser(SocketGuildUser user, [Remainder, Optional] string reason)
        {

            if (user.GuildPermissions.ManageRoles == true)
            {
                await ReplyAsync($"**ERROR: **you cannot ban a user with manage roles permission");
                return;
            }
            else if (reason == null)
            {
                await ReplyAsync("**ERROR: ** Please specify a reason for banning the user!");
                return;
            }
            else
            {
                await ReplyAsync($"{user.Mention} you have been banned for `{reason}`:bangbang: ");
                var dm = await user.CreateDMChannelAsync();
                await dm.SendMessageAsync($"{user.Mention} you have been banned from {Context.Guild} for `{reason}`");
                await Context.Guild.AddBanAsync(user);

                var warnpath = Path.Combine(AppContext.BaseDirectory, $"moderation/ban/{Context.Guild.Id}.txt");
                if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "moderation/ban/")))
                    Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "moderation/ban/"));


                File.AppendAllText(AppContext.BaseDirectory + $"moderation/ban/{Context.Guild.Id}.txt", $"User: {user} || Moderator: {Context.User} || Reason: {reason}" + Environment.NewLine);

            }
        }
        
        [Command("mute-"), Summary("mute '@loudboy'"), Remarks("Mutes the specified player")]
        public async Task Mute(string user, [Remainder, Optional] string reason)
        {

            if (Utilities.GetMutedRole((SocketGuild)Context.Guild) == null)
            {
                await ReplyAsync("This server does not contain the role 'Muted' type `.mutehelp` for more info");
            }
            else if (reason == null)
            {
                await ReplyAsync("**ERROR: **Please specify a reason type `.mutehelp` for more info");
            }
            else if (Context.Message.MentionedUserIds.Count != 0 && Context.Guild.GetUserAsync(Context.Message.MentionedUserIds.FirstOrDefault()) != null)
            {
                IGuildUser target = await Context.Guild.GetUserAsync(Context.Message.MentionedUserIds.FirstOrDefault());
                IRole muted = Utilities.GetMutedRole((SocketGuild)Context.Guild);
                if (target.RoleIds.Contains(muted.Id))
                {
                    await ReplyAsync("**ERROR: ** The user is already muted");
                }
                else
                {
                    await target.AddRoleAsync(muted);
                    await ReplyAsync(target.Username + " is muted.");
                }
            }
            else if (await Utilities.GetUser(Context.Guild, user) != null)
            {
                IGuildUser target = await Utilities.GetUser(Context.Guild, user);
                IRole muted = Utilities.GetMutedRole((SocketGuild)Context.Guild);
                if (target.RoleIds.Contains(muted.Id))
                {
                    await ReplyAsync("**ERROR: ** The user is already muted");
                }
                else
                {
                    await target.AddRoleAsync(muted);
                    await ReplyAsync(target.Username + " is muted.");
                }
            }
            else
            {
                await ReplyAsync("Who the fuck is " + user + "?");
            }
        }
        [Command("unmute-"), Summary("unmute 'quiteboy'"), Remarks("unmutes the specified user")]
        public async Task Unmute(string user)
        {

            if (Utilities.GetMutedRole((SocketGuild)Context.Guild) == null)
                await ReplyAsync("This server does not contain the role 'Muted' type `.mutehelp` for more info");
            else if (Context.Message.MentionedUserIds.Count != 0 && Context.Guild.GetUserAsync(Context.Message.MentionedUserIds.FirstOrDefault()) != null)
            {
                IGuildUser target = await Context.Guild.GetUserAsync(Context.Message.MentionedUserIds.FirstOrDefault());
                IRole muted = Utilities.GetMutedRole((SocketGuild)Context.Guild);
                if (target.RoleIds.Contains(muted.Id))
                {
                    await target.RemoveRoleAsync(muted);
                    await ReplyAsync(target.Username + " is unmuted.");
                }
                else
                {
                    await ReplyAsync("**ERROR: ** The user wasn't muted in the first place");
                }
            }
            else if (await Utilities.GetUser(Context.Guild, user) != null)
            {
                IGuildUser target = await Utilities.GetUser(Context.Guild, user);
                IRole muted = Utilities.GetMutedRole((SocketGuild)Context.Guild);
                if (target.RoleIds.Contains(muted.Id))
                {
                    await target.RemoveRoleAsync(muted);
                    await ReplyAsync(target.Username + " is unmuted");
                }
                else
                {
                    await ReplyAsync("**ERROR: ** The user wasn't muted in the first place");
                }
            }
            else
            {
                await ReplyAsync("Who the fuck is " + user + "?");
            }
            }*/
    }
}