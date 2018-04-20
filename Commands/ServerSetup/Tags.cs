using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Configuration;

namespace PassiveBOT.Commands.ServerSetup
{
    [RequireContext(ContextType.Guild)]
    [Group("Tag")]
    [Alias("Tags")]
    public class Tags : ModuleBase
    {
        [Command("add")]
        [Summary("tag add <name> <message>")]
        [Remarks("adds a tag to the server")]
        public async Task Tagadd(string tagname, [Remainder] string tagmessage)
        {
            var tg = new Tagging
            {
                Tagname = tagname,
                Content = tagmessage,
                Creator = Context.User.Id
            };
            var server = GuildConfig.GetServer(Context.Guild);
            if (server.Dict.Any(x => string.Equals(x.Tagname, tagname, StringComparison.CurrentCultureIgnoreCase)))
            {
                await ReplyAsync(
                    $"**{tagname}** is already a tag in this server, if you want to edit it, please delete it first, then add the new tag");
                return;
            }

            server.Dict.Add(tg);
            GuildConfig.SaveServer(server);
            await ReplyAsync("Tags List Updated");
        }

        [Command("del")]
        [Summary("tag  del <name>")]
        [Remarks("Removes a tag from the server")]
        public async Task Tagdel(string tagname)
        {
            var ServerConfig = GuildConfig.GetServer(Context.Guild);
            if (ServerConfig.Dict.Count > 0)
            {
                foreach (var tagging in ServerConfig.Dict)
                    if (string.Equals(tagging.Tagname, tagname, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (((SocketGuildUser) Context.User).GuildPermissions.Administrator)
                        {
                            ServerConfig.Dict.Remove(tagging);
                            await ReplyAsync("Tag Deleted using Admin Permissions");
                        }
                        else if (tagging.Creator == Context.User.Id)
                        {
                            ServerConfig.Dict.Remove(tagging);
                            await ReplyAsync("Tag Deleted By Owner");
                        }
                        else
                        {
                            await ReplyAsync("You do not own this tag");
                        }

                        GuildConfig.SaveServer(ServerConfig);
                        return;
                    }

                return;
            }

            await ReplyAsync($"No Tags found with the name: {tagname}");
        }

        [Command]
        [Summary("tag [optional]<tagname>")]
        [Remarks("lists all tags for the server or sends a tag")]
        public async Task Tag(string tagname = null)
        {
            if (tagname == null)
            {
                var tags = GuildConfig.GetServer(Context.Guild).Dict;
                if (tags.Count > 0)
                {
                    var taglist = string.Join(", ", tags.Select(x => x.Tagname));
                    await ReplyAsync($"**Tags:** {taglist}");
                }
                else
                {
                    await ReplyAsync("This server has no tags yet.");
                }
            }
            else
            {
                var server = GuildConfig.GetServer(Context.Guild);
                var embed = new EmbedBuilder();
                if (server.Dict.Count > 0)
                {
                    var tag = server.Dict.FirstOrDefault(x =>
                        string.Equals(x.Tagname, tagname, StringComparison.CurrentCultureIgnoreCase));
                    if (tag == null)
                    {
                        await ReplyAsync($"No tag with the name **{tagname}** exists.");
                    }
                    else
                    {
                        string ownername;
                        try
                        {
                            var own = await Context.Guild.GetUserAsync(tag.Creator);
                            ownername = own.Username;
                        }
                        catch
                        {
                            ownername = "Owner Left";
                        }

                        embed.AddField(tag.Tagname, tag.Content);
                        embed.WithFooter(x =>
                        {
                            x.Text =
                                $"Tag Owner: {ownername} || Uses: {tag.uses} || Command Invokee: {Context.User.Username}";
                        });
                        tag.uses++;
                        GuildConfig.SaveServer(server);
                        await ReplyAsync("", false, embed.Build());
                    }
                }
            }
        }

        public class Tagging
        {
            public string Tagname { get; set; }
            public string Content { get; set; }
            public ulong Creator { get; set; }
            public int uses { get; set; }
        }
    }
}