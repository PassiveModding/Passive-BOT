using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace PassiveBOT.Commands
{
    [RequireContext(ContextType.Guild)]
    [Group("tag")]
    public class Tags : ModuleBase
    {
        [Command("add")]
        [Summary("tag add <name> <message>")]
        [Remarks("adds a tag to the servers files")]
        public async Task Tagadd(string tagname, [Remainder] string tagmessage)
        {
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}/tags/")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory,
                    $"setup/server/{Context.Guild.Id}/tags/"));

            var tagfile = AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/tags/{tagname}.txt";
            if (File.Exists(tagfile))
            {
                await ReplyAsync("This tag already exists!");
            }
            else
            {
                File.AppendAllText(tagfile,
                    $"{Context.User.Id}\n{tagmessage}" + Environment.NewLine);
                await ReplyAsync($"**Tag Name:** {tagname}\n**Tag Response:** {tagmessage}");
            }
        }

        [Command("del")]
        [Summary("tag  del <name>")]
        [Remarks("Removes a tag from the servers files")]
        public async Task Tagdel(string tagname)
        {
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}/tags/")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory,
                    $"setup/server/{Context.Guild.Id}/tags/"));

            var tagfile = AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/tags/{tagname}.txt";

            if (File.Exists(tagfile))
            {
                var user = Context.User as SocketGuildUser;
                if (Context.User.Id.ToString() == File.ReadLines(tagfile).First() || user.GuildPermissions.Administrator
                ) //only allows admins or the tags creator to delete it
                {
                    File.Delete(tagfile);
                    await ReplyAsync($"The Tag {tagname} has been deleted");
                }
                else
                {
                    await ReplyAsync("Only an administrator or the tags creator can delete this");
                }
            }
            else
            {
                await ReplyAsync("This tag already exists!");
            }
        }

        [Command]
        [Summary("tag [optional]<tagname>")]
        [Remarks("lists all tags for the server or sends a tag")]
        public async Task Tag(string tagname = null)
        {
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}/tags/")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory,
                    $"setup/server/{Context.Guild.Id}/tags/"));

            var tagfolder = AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/tags/";
            var tagfile = tagfolder + $"{tagname}.txt";
            if (tagname == null)
            {
                var d = new DirectoryInfo(tagfolder).GetFiles("*.*");
                var newlist = string.Join(", ",
                    d.Select(file => Path.GetFileNameWithoutExtension(file.Name)).ToArray());
                if (newlist == "")
                    await ReplyAsync("There are currently no tags for your server, you can add some using `.tag add`");
                else
                    await ReplyAsync($"Here are the tags for this server: \n{newlist}");
            }
            else
            {
                if (File.Exists(tagfile))
                {
                    //var userId = File.ReadLines(tagfile).First(); not required for this command
                    var taglinesList = File.ReadAllLines(tagfile).Skip(1).ToList();
                    var newlist = string.Join("\n", Enumerable.ToArray(taglinesList));
                    await ReplyAsync($"{newlist}");
                }
                else
                {
                    await ReplyAsync("This tag does not exist");
                }
            }
        }
    }
}