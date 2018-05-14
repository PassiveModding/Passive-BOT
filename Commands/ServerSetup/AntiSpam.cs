using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Configuration;
using PassiveBOT.Handlers.Services.Interactive;
using PassiveBOT.Preconditions;

namespace PassiveBOT.Commands.ServerSetup
{
    [RequireAdmin]
    [RequireContext(ContextType.Guild)]
    public class AntiSpam : InteractiveBase
    {
        [Command("IgnoreRole")]
        [Summary("IgnoreRole <@role>")]
        [Remarks("choose a role to ignore when using antispam")]
        public async Task IgnoreRole(IRole role)
        {
            var guild = GuildConfig.GetServer(Context.Guild);

            var ignore = guild.Antispams.IgnoreRoles.FirstOrDefault(x => x.RoleID == role.Id);
            var addrole = false;
            if (ignore == null)
            {
                ignore = new GuildConfig.antispams.IgnoreRole
                {
                    RoleID = role.Id
                };
                addrole = true;
            }
            else
            {
                ignore.AntiSpam = !ignore.AntiSpam;
            }
            if (addrole) guild.Antispams.IgnoreRoles.Add(ignore);
            GuildConfig.SaveServer(guild);
            await ReplyAsync($"Success, \n" +
                             $"{role.Name} is ignored in antispam: {ignore.AntiSpam}");
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
            var embed = new EmbedBuilder {Description = string.Join("\n", guild.Antispams.Antispam.AntiSpamSkip)};
            await ReplyAsync("", false, embed.Build());
        }
    }
}