using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using PassiveBOT.Configuration;

namespace PassiveBOT.Commands
{
    [RequireContext(ContextType.Guild)]
    [Group("tag")]
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
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}/config.json");
            var jsononb = JsonConvert.DeserializeObject<GuildConfig>(File.ReadAllText(file));

            try
            {
                var d = GuildConfig.Load(Context.Guild.Id).Dict;
                foreach (var tagging in d)
                    if (tagging.Tagname == tagname)
                    {
                        await ReplyAsync(
                            $"**{tagname}** is already a tag in this server, if you want to edit it, please delete it first, then add the new tag");
                        return;
                    }
                d.Add(tg);
                jsononb.Dict = d;
                var output = JsonConvert.SerializeObject(jsononb, Formatting.Indented);
                File.WriteAllText(file, output);
                await ReplyAsync("Tags List Updated");
            }
            catch
            {
                var d = new List<Tagging> {tg};
                jsononb.Dict = d;
                var output = JsonConvert.SerializeObject(jsononb, Formatting.Indented);
                File.WriteAllText(file, output);
            }
        }

        [Command("del")]
        [Summary("tag  del <name>")]
        [Remarks("Removes a tag from the server")]
        public async Task Tagdel(string tagname)
        {
            var file = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}/config.json");
            var jsononb = JsonConvert.DeserializeObject<GuildConfig>(File.ReadAllText(file));

            try
            {
                var dict = GuildConfig.Load(Context.Guild.Id).Dict;

                foreach (var tagging in dict)
                    if (tagging.Tagname.ToLower() == tagname.ToLower())
                    {
                        if ((Context.User as SocketGuildUser).GuildPermissions.Administrator)
                        {
                            dict.Remove(tagging);
                            await ReplyAsync("Tag Deleted using Admin Permissions");
                            jsononb.Dict = dict;
                            var output = JsonConvert.SerializeObject(jsononb, Formatting.Indented);
                            File.WriteAllText(file, output);
                            return;
                        }
                        if (tagging.Creator == Context.User.Id)
                        {
                            dict.Remove(tagging);
                            await ReplyAsync("Tag Deleted By Owner");
                            jsononb.Dict = dict;
                            var output = JsonConvert.SerializeObject(jsononb, Formatting.Indented);
                            File.WriteAllText(file, output);
                            return;
                        }

                        await ReplyAsync("You do not own this tag");
                        return;
                    }
                await ReplyAsync($"No Tags found with the name: {tagname}");
            }
            catch
            {
                await ReplyAsync("No Tags To Delete");
            }
        }

        [Command]
        [Summary("tag [optional]<tagname>")]
        [Remarks("lists all tags for the server or sends a tag")]
        public async Task Tag(string tagname = null)
        {
            if (tagname == null)
                try
                {
                    var dict = GuildConfig.Load(Context.Guild.Id).Dict;
                    var list = "";
                    foreach (var tagging in dict)
                        list += $"{tagging.Tagname}, ";

                    var res = list.Substring(0, list.Length - 2);
                    await ReplyAsync($"**Tags:** {res}");
                }
                catch
                {
                    await ReplyAsync("This server has no tags added");
                }
            else
                try
                {
                    var dict = GuildConfig.Load(Context.Guild.Id).Dict;
                    var embed = new EmbedBuilder();
                    foreach (var tagging in dict)
                        if (tagging.Tagname.ToLower() == tagname.ToLower())
                        {
                            string ownername;
                            try
                            {
                                var own = await Context.Guild.GetUserAsync(tagging.Creator);
                                ownername = own.Username;
                            }
                            catch
                            {
                                ownername = "Owner Left";
                            }

                            embed.AddField(tagging.Tagname, tagging.Content);
                            embed.WithFooter(x =>
                            {
                                x.Text =
                                    $"Tag Owner: {ownername} || Command Invokee: {Context.User.Username}";
                            });
                            await ReplyAsync("", false, embed.Build());
                            return;
                        }
                    await ReplyAsync($"No tag with the name **{tagname}** exists.");
                }
                catch
                {
                    await ReplyAsync("This server has no tags :(");
                }
        }

        public class Tagging
        {
            public string Tagname { get; set; }
            public string Content { get; set; }
            public ulong Creator { get; set; }
        }
    }
}