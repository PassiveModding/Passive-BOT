using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Services;
using PassiveBOT.Configuration;

namespace PassiveBOT.Commands
{
    public class Audio : ModuleBase<ICommandContext>
    {
        private readonly AudioService _service;
        public Audio(AudioService service)
        {
            _service = service;
        }

        [Command("join", RunMode = RunMode.Async)]
        [Summary("join")]
        [Remarks("Joins the current Audio Channel")]
        public async Task JoinCmd()
        {
            await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
        }

        [Command("leave", RunMode = RunMode.Async)]
        [Summary("leave")]
        [Remarks("Leaves the current Audio Channel")]
        public async Task LeaveCmd()
        {
            await _service.LeaveAudio(Context.Guild);
        }



        [Command("list")]
        [Summary("list")]
        [Remarks("lists all available songs")]
        public async Task ListMusic()
        {
            await _service.ListMusic(Context.Channel, Context.Guild);
        }

        [Command("get", RunMode = RunMode.Async)]
        [Summary("get 'yt url'")]
        [Remarks("requests a song for download")]
        public async Task GetSong(string url)
        {
            if (url.Contains("/"))
            {
                if (url.Contains("youtube") && url.Contains("watch"))
                {
                    //Console.WriteLine(url);
                    var videoId = url.Substring(url.Length - 11, 11);
                    //Console.WriteLine(videoId);
                    if (videoId.Contains("="))
                    {
                        await ReplyAsync(
                            "**Invalid URL format: ** please use something like `https://www.youtube.com/watch?v=tvTRZJ-4EyI`\n" +
                            "this cannot be from a playlist");
                    }
                    else
                    {
                        await _service.DlAudio(Context.Guild, Context.Channel, videoId);
                    }

                }
            }
            else
            {
                await ReplyAsync(
                    "**Invalid URL format: ** please use something like `https://www.youtube.com/watch?v=tvTRZJ-4EyI`");
            }
        }

        [Command("playlist", RunMode = RunMode.Async)]
        [Summary("playlist")]
        [Remarks("plays all saved songs for your server")]
        public async Task Playlist()
        {
            //builds a list of .m4a files within the guilds folder
            if (!Directory.Exists($"{AudioService.MusicFolder}/{Context.Guild.Id}"))
            {
                await ReplyAsync(
                    $"There are no songs saved for this server, please use the `{Load.Pre}get` command to download some");
            }
            else
            {
                var d = new DirectoryInfo($"{AudioService.MusicFolder}/{Context.Guild.Id}");
                var music = d.GetFiles("*.m4a");
                var songlist = music.Select(sng => sng.Name).ToList();
                await _service.SendPlaylist(Context.Guild, Context.Channel, songlist);
            }
        }

        [Command("queue", RunMode = RunMode.Async)]
        [Alias("q")]
        [Summary("queue")]
        [Remarks("displays the current music queue")]
        public async Task QueueTask()
        {
            await _service.Queue(Context.Channel);
        }

        [Command("queue", RunMode = RunMode.Async)]
        [Alias("q")]
        [Summary("queue")]
        [Remarks("type `.q help` for info")]
        public async Task Queue(string arg, [Remainder, Optional]int quantity)
        {
            if (arg == "add")
            {
                if (quantity >= 0)
                {
                    await _service.SendSongNo(Context.Guild, Context.Channel, quantity);
                }
            }
            else if (arg == "del")
            {
                await _service.DeQueue(Context.Channel);
            }
            else if (arg == "clear")
            {
                await _service.QueueClear(Context.Channel);
            }
            else if (arg == "help")
            {
                await ReplyAsync("**Here are the queue (q) commands:**\n\n" +
                                 $"1- `{Load.Pre}q add 'x'` adds a song from `{Load.Pre}list` to the end of the queue\n" +
                                 $"2- `{Load.Pre}q del` removes the next song from the queue without skipping the surrent one\n" +
                                 $"3- `{Load.Pre}q clear` clears all songs from the queue\n");
            }
        }

        [Command("stop", RunMode = RunMode.Async)]
        [Summary("stop")]
        [Remarks("Stops playing music")]
        public async Task Stop()
        {
            _service.Cancel();
            await ReplyAsync("Music Has been stopped");
        }

        [Command("skip", RunMode = RunMode.Async)]
        [Summary("skip")]
        [Remarks("Skips the current song")]
        public async Task Skip()
        {
            await _service.Next(Context.Guild, Context.Channel);
            await ReplyAsync("Music Has been skipped");
        }
    }
}