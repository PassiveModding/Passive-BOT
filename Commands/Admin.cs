using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Configuration;

namespace PassiveBOT.Commands
{
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    public class Admin : InteractiveBase
    {
        /*[Command("EmbedBuilder", RunMode = RunMode.Async)]
        [Summary("EmbedBuilder")]
        [Remarks("Create an embedded message")]
        public async Task BuildEmbed()
        {
            var embed = new EmbedBuilder();
            var embedbuilt = await ReplyAsync("", false, embed.Build());

            await ReplyAndDeleteAsync("```\n" +
                                      "Reply addon you would like to perform\n" +
                                      "[1] Set the Title\n" +
                                      "[2] Set the Description\n" +
                                      "[3] Add a Field\n" +
                                      "[4] Finish\n" +
                                      "```");
            var n1 = await NextMessageAsync();
            if (n1.Content.StartsWith("1"))
            {
                await ReplyAndDeleteAsync("In the next message you send please specify the embed title");
                var ntitle = await NextMessageAsync();
                embed.WithTitle(ntitle.Content);
                await embedbuilt.ModifyAsync(x => { x.Embed = embed.Build(); });
                await ntitle.DeleteAsync();
            }
            else if (n1.Content.StartsWith("2"))
            {
                await ReplyAndDeleteAsync("In the next message you send please specify the embed description");
                var ndesc = await NextMessageAsync();
                embed.WithDescription(ndesc.Content);
                await embedbuilt.ModifyAsync(x => { x.Embed = embed.Build(); });
                await ndesc.DeleteAsync();
            }
            else if (n1.Content.StartsWith("3"))
            {
                await ReplyAndDeleteAsync("In the next message, specify the field title");
                var nfieldtitle = await NextMessageAsync();
                await ReplyAndDeleteAsync("In the next message, specify the field description");
                var nfielddesc = await NextMessageAsync();
                embed.AddField(nfieldtitle.Content, nfielddesc.Content);
                await embedbuilt.ModifyAsync(x => { x.Embed = embed.Build(); });
                await nfielddesc.DeleteAsync();
                await nfieldtitle.DeleteAsync();
            }
            else if (n1.Content.StartsWith("4"))
            {
                return;
            }
            else
            {
                await ReplyAsync("Invalid number... exiting");
                return;
            }


            await ReplyAsync("```\n" +
                             "Reply addon you would like to perform\n" +
                             "[1] Set the Title\n" +
                             "[2] Set the Description\n" +
                             "[3] Add a Field\n" +
                             "[4] Finish\n" +
                             "```");
            var n2 = await NextMessageAsync();

            if (n2.Content.StartsWith("1"))
            {
                await ReplyAndDeleteAsync("In the next message you send please specify the embed title");
                var ntitle = await NextMessageAsync();
                embed.WithTitle(ntitle.Content);
                await embedbuilt.ModifyAsync(x => { x.Embed = embed.Build(); });
                await ntitle.DeleteAsync();
            }
            else if (n2.Content.StartsWith("2"))
            {
                await ReplyAndDeleteAsync("In the next message you send please specify the embed description");
                var ndesc = await NextMessageAsync();
                embed.WithDescription(ndesc.Content);
                await embedbuilt.ModifyAsync(x => { x.Embed = embed.Build(); });
                await ndesc.DeleteAsync();
            }
            else if (n2.Content.StartsWith("3"))
            {
                await ReplyAndDeleteAsync("In the next message, specify the field title");
                var nfieldtitle = await NextMessageAsync();
                await ReplyAndDeleteAsync("In the next message, specify the field description");
                var nfielddesc = await NextMessageAsync();
                embed.AddField(nfieldtitle.Content, nfielddesc.Content);
                await embedbuilt.ModifyAsync(x => { x.Embed = embed.Build(); });
                await nfieldtitle.DeleteAsync();
                await nfielddesc.DeleteAsync();
            }
            else if (n2.Content.StartsWith("4"))
            {
                return;
            }
            else
            {
                await ReplyAsync("Invalid number... exiting");
                return;
            }

            await ReplyAsync("```\n" +
                             "Reply addon you would like to perform\n" +
                             "[1] Set the Title\n" +
                             "[2] Set the Description\n" +
                             "[3] Add a Field\n" +
                             "[4] Finish\n" +
                             "```");
            var n3 = await NextMessageAsync();

            if (n3.Content.StartsWith("1"))
            {
                await ReplyAsync("In the next message you send please specify the embed title");
                var ntitle = await NextMessageAsync();
                embed.WithTitle(ntitle.Content);
                await embedbuilt.ModifyAsync(x => { x.Embed = embed.Build(); });
                await ntitle.DeleteAsync();
            }
            else if (n3.Content.StartsWith("2"))
            {
                await ReplyAsync("In the next message you send please specify the embed description");
                var ndesc = await NextMessageAsync();
                embed.WithDescription(ndesc.Content);
                await embedbuilt.ModifyAsync(x => { x.Embed = embed.Build(); });
                await ndesc.DeleteAsync();
            }
            else if (n3.Content.StartsWith("3"))
            {
                await ReplyAndDeleteAsync("In the next message, specify the field title");
                var nfieldtitle = await NextMessageAsync();
                await ReplyAndDeleteAsync("In the next message, specify the field description");
                var nfielddesc = await NextMessageAsync();
                embed.AddField(nfieldtitle.Content, nfielddesc.Content);
                await embedbuilt.ModifyAsync(x => { x.Embed = embed.Build(); });
                await nfielddesc.DeleteAsync();
                await nfieldtitle.DeleteAsync();
            }
            else if (n3.Content.StartsWith("4"))
            {
            }
            else
            {
                await ReplyAsync("Invalid number... exiting");
            }
        }*/

        [Command("ClearWarn")]
        [Summary("ClearWarn <@user>")]
        [Remarks("Clears warnings for the specified user")]
        public async Task ClearWarn(SocketGuildUser removeuser)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var embed = new EmbedBuilder();
            embed.WithTitle("Warns Removed");
            var config = GuildConfig.GetServer(Context.Guild);
            var list = "";
            var newconfig = new List<GuildConfig.Warns>();
            foreach (var group in config.Warnings)
                if (group.UserId == removeuser.Id)
                {
                    var moderator =
                        $"{group.Moderator}                                             ".Substring(0, 20);
                    list += $"Mod: {moderator} || Reason: {group.Reason}\n";
                    //config.Warnings.Remove(group);
                }
                else
                {
                    newconfig.Add(group);
                }
            config.Warnings = newconfig;
            embed.WithDescription(list);
            GuildConfig.SaveServer(config, Context.Guild);
            await ReplyAsync($"Warnings for the user {removeuser} have been cleared");
            try
            {
                await ReplyAsync("", false, embed.Build());
            }
            catch
            {
                //
            }
        }

        [Command("ClearKick")]
        [Summary("ClearKick <@user>")]
        [Remarks("Clears Kicks for the specified user")]
        public async Task ClearKick(SocketGuildUser removeuser)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var embed = new EmbedBuilder();
            embed.WithTitle("Kicks Removed");
            var config = GuildConfig.GetServer(Context.Guild);
            var list = "";
            var newconfig = new List<GuildConfig.Kicks>();
            foreach (var group in config.Kicking)
                if (group.UserId == removeuser.Id)
                {
                    var moderator =
                        $"{group.Moderator}                                             ".Substring(0, 20);
                    list += $"Mod: {moderator} || Reason: {group.Reason}\n";
                    //config.Kicking.Remove(group);
                }
                else
                {
                    newconfig.Add(group);
                }
            embed.WithDescription(list);
            config.Kicking = newconfig;
            GuildConfig.SaveServer(config, Context.Guild);
            await ReplyAsync($"Kicks for the user {removeuser} have been cleared");
            try
            {
                await ReplyAsync("", false, embed.Build());
            }
            catch
            {
                //
            }
        }

        [Command("ClearBan")]
        [Summary("ClearBan <@user>")]
        [Remarks("Clears Bans for the specified user")]
        public async Task ClearBan(SocketGuildUser removeuser)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var embed = new EmbedBuilder();
            embed.WithTitle("Bans Removed");
            var config = GuildConfig.GetServer(Context.Guild);
            var list = "";
            var newconfig = new List<GuildConfig.Bans>();
            foreach (var group in config.Banning)
                if (group.UserId == removeuser.Id)
                {
                    var moderator =
                        $"{group.Moderator}                                             ".Substring(0, 20);
                    list += $"Mod: {moderator} || Reason: {group.Reason}\n";
                    //config.Banning.Remove(group);
                }
                else
                {
                    newconfig.Add(group);
                }
            config.Banning = newconfig;
            embed.WithDescription(list);
            GuildConfig.SaveServer(config, Context.Guild);
            await ReplyAsync($"Warnings for the user {removeuser} have been cleared");
            try
            {
                await ReplyAsync("", false, embed.Build());
            }
            catch
            {
                //
            }
        }

        [Command("ClearWarn")]
        [Summary("ClearWarn <userID>")]
        [Remarks("Clears warnings for the specified user")]
        public async Task ClearWarn(ulong userId)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var embed = new EmbedBuilder();
            embed.WithTitle("Warns Removed");
            var config = GuildConfig.GetServer(Context.Guild);
            var list = "";
            var newconfig = new List<GuildConfig.Warns>();
            foreach (var group in config.Warnings)
                if (group.UserId == userId)
                {
                    var moderator =
                        $"{group.Moderator}                                             ".Substring(0, 20);
                    list += $"Mod: {moderator} || Reason: {group.Reason}\n";
                    //config.Warnings.Remove(group);
                }
                else
                {
                    newconfig.Add(group);
                }
            config.Warnings = newconfig;
            embed.WithDescription(list);
            GuildConfig.SaveServer(config, Context.Guild);
            await ReplyAsync($"Warnings for the user with the id {userId} have been cleared");
            try
            {
                await ReplyAsync("", false, embed.Build());
            }
            catch
            {
                //
            }
        }

        [Command("ClearKick")]
        [Summary("ClearKick <UserID>")]
        [Remarks("Clears Kicks for the specified user")]
        public async Task ClearKick(ulong userId)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var embed = new EmbedBuilder();
            embed.WithTitle("Kicks Removed");
            var config = GuildConfig.GetServer(Context.Guild);
            var list = "";
            var newconfig = new List<GuildConfig.Kicks>();
            foreach (var group in config.Kicking)
                if (group.UserId == userId)
                {
                    var moderator =
                        $"{group.Moderator}                                             ".Substring(0, 20);
                    list += $"Mod: {moderator} || Reason: {group.Reason}\n";
                    //config.Kicking.Remove(group);
                }
                else
                {
                    newconfig.Add(group);
                }
            embed.WithDescription(list);
            config.Kicking = newconfig;
            GuildConfig.SaveServer(config, Context.Guild);
            await ReplyAsync($"Kicks for the user with the id {userId} have been cleared");
            try
            {
                await ReplyAsync("", false, embed.Build());
            }
            catch
            {
                //
            }
        }

        [Command("ClearBan")]
        [Summary("ClearBan <UserID>")]
        [Remarks("Clears Bans for the specified user")]
        public async Task ClearBan(ulong userId)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var embed = new EmbedBuilder();
            embed.WithTitle("Bans Removed");
            var config = GuildConfig.GetServer(Context.Guild);
            var list = "";
            var newconfig = new List<GuildConfig.Bans>();
            foreach (var group in config.Banning)
                if (group.UserId == userId)
                {
                    var moderator =
                        $"{group.Moderator}                                             ".Substring(0, 20);
                    list += $"Mod: {moderator} || Reason: {group.Reason}\n";
                    //config.Banning.Remove(group);
                }
                else
                {
                    newconfig.Add(group);
                }
            config.Banning = newconfig;
            embed.WithDescription(list);
            GuildConfig.SaveServer(config, Context.Guild);
            await ReplyAsync($"Warnings for the user with the id {userId} have been cleared");
            try
            {
                await ReplyAsync("", false, embed.Build());
            }
            catch
            {
                //
            }
        }

        /*[Command("Starboard")]
        [Summary("Starboard")]
        [Remarks("Set the current channel as the starboard.")]
        public async Task Starboard([Remainder] string x = null)
        {

            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}.json");
            if (!File.Exists(file))
                GuildConfig.Setup(Context.Guild);
            var config = GuildConfig.GetServer(Context.Guild);
            if (x == null)
            {
                config.Starboard = Context.Channel.Id;
                GuildConfig.SaveServer(config, Context.Guild);
                await ReplyAsync("Starred posts will now be posted in this channel.");
            }
            else
            {
                config.Starboard = 0;
                GuildConfig.SaveServer(config, Context.Guild);
                await ReplyAsync("Starred posts will no longer be shown.");
            }
        }*/
    }
}