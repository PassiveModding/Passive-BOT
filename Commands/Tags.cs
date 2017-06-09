using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace PassiveBOT.Commands
{
    [RequireContext(ContextType.Guild)]
    public class Tags : ModuleBase
    {
        [Command("tag add")]
        [Summary("tag add")]
        [Remarks("adds a tag to the servers files")]
        public async Task Tagadd(string tagname, [Remainder] string tagmessage)
        {
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, $"tags/{Context.Guild.Id}/")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, $"tags/{Context.Guild.Id}/"));

            var tagfile = AppContext.BaseDirectory + $"tags/{Context.Guild.Id}/{tagname}.txt";
            if (File.Exists(tagfile))
            {
                await ReplyAsync("This tag already exists!");
            }
            else
            {
                File.AppendAllText(tagfile,
                    $"{Context.User.Id}\n{tagmessage}" + Environment.NewLine);
                await ReplyAsync($"**Tag Name:** {tagname}\n **Tag Response:** {tagmessage}");
            }
        }

        [Command("tag del")]
        [Summary("Removes a tag from the servers files")]
        public async Task Tagdel(string tagname)
        {
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, $"tags/{Context.Guild.Id}/")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, $"tags/{Context.Guild.Id}/"));

            var tagfile = AppContext.BaseDirectory + $"tags/{Context.Guild.Id}/{tagname}.txt";

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

        [Command("tag")]
        [Summary("tag")]
        [Remarks("adds a tag to the servers files")]
        public async Task Tag(string tagname = null)
        {
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, $"tags/{Context.Guild.Id}/")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, $"tags/{Context.Guild.Id}/"));

            var tagfolder = AppContext.BaseDirectory + $"tags/{Context.Guild.Id}/";
            var tagfile = tagfolder + $"{tagname}.txt";
            if (tagname == null)
            {
                var d = new DirectoryInfo(tagfolder).GetFiles("*.*");
                var newlist = string.Join(", ",
                    d.Select(file => Path.GetFileNameWithoutExtension(file.Name)).ToArray());
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