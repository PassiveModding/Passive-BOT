using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Configuration;
using PassiveBOT.Services;
using YoutubeExplode;

namespace PassiveBOT.Commands
{
    public class Audio : ModuleBase<ICommandContext>
    {
        private static readonly List<string> Queue = new List<string>();

        private readonly AudioService _service;
        private string _reply, _nextSong, _leftInQueue;

        public Audio(AudioService service)
        {
            _service = service;
        }

        [Command("join", RunMode = RunMode.Async)]
        [Summary("join")]
        [Remarks("Joins your Voice Channel")]
        public async Task JoinCmd()
        {
            await ReplyAsync("Joining Audio Channel");
            await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
        }

        [Command("leave", RunMode = RunMode.Async)]
        [Summary("loin")]
        [Remarks("Leaves your Voice Channel")]
        public async Task LeaveCmd()
        {
            await _service.LeaveAudio(Context.Guild);
            await ReplyAsync("Leaving Audio Channel");
        }

        [Command("songs", RunMode = RunMode.Async)]
        [Summary("songs")]
        [Remarks("Lists all songs downloaded in your server")]
        public async Task SongList()
        {
            //gets the current guilds directory
            if (Directory.Exists($"{AppContext.BaseDirectory}/music/{Context.Guild.Id}/"))
            {
                var d = new DirectoryInfo($"{AppContext.BaseDirectory}/music/{Context.Guild.Id}/");
                var music = d.GetFiles("*.*");
                var songlist = new List<string>();
                var i = 0;
                foreach (var sng in music)
                {
                    songlist.Add($"`{i}` - {Path.GetFileNameWithoutExtension(sng.Name)}");
                    i++;
                }

                var list = string.Join("\n", songlist.ToArray());

                await ReplyAsync(list);
            }
            else
            {
                await ReplyAsync("There are currently no songs downloaded for this guild\n" +
                                 $"you can download songs using the `{Load.Pre}play` command");
            }
        }

        [Command("queue", RunMode = RunMode.Async)]
        [Alias("q")]
        [Summary("q")]
        [Remarks("Lists all songs in the queue")]
        public async Task QueueList()
        {
            if (Queue.Count > 0)
            {
                var songList = new StringBuilder();
                var i = 0;
                foreach (var song in Queue)
                {
                    songList.AppendLine($"`{i}` - {song}");
                    i++;
                }
                await ReplyAsync(songList.ToString());
            }
            else
            {
                await ReplyAsync("There are No songs in the queue");
            }
        }

        [Command("q play", RunMode = RunMode.Async)]
        [Alias("queue play")]
        [Summary("q play 'YT video'/'YT video name'")]
        [Remarks("Plays the queue")]
        public async Task PlayQueue()
        {
            while (Queue.Count > 0)
            {
                await _service.LeaveAudio(Context.Guild);
                await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);

                _nextSong = Queue.Count != 1 ? $", next song: **{Queue.ElementAt(1)}**" : "";
                _leftInQueue = Queue.Count == 1
                    ? "There is 1 song in the queue."
                    : $"There are {Queue.Count} songs in the queue.";
                await ReplyAsync($"Now Playing: **{Queue.First()}** {_nextSong}.\n{_leftInQueue}");

                await _service.SendAudioAsync(Context.Guild, Context.Channel, Queue.First());
                Queue.RemoveAt(0);
            }

            await ReplyAsync($"Sorry, the queue is empty, {Load.Pre}queue (or {Load.Pre}q) to add more!");

            await _service.LeaveAudio(Context.Guild);
            await ReplyAsync("Leaving Audio Channel");
        }

        [Command("q all", RunMode = RunMode.Async)]
        [Alias("queue all")]
        [Summary("pa")]
        [Remarks("Plays all downloaded songs")]
        public async Task Pall()
        {
            await _service.LeaveAudio(Context.Guild);
            await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);

            if (Directory.Exists($"{AppContext.BaseDirectory}/music/{Context.Guild.Id}/"))
            {
                var d = new DirectoryInfo($"{AppContext.BaseDirectory}/music/{Context.Guild.Id}/");
                var music = d.GetFiles("*.*");
                foreach (var sng in music)
                {
                    Queue.Add(Path.GetFileNameWithoutExtension(sng.Name));
                }
                await PlayQueue();
            }
            else
            {
                await ReplyAsync("There are no songs downloaded in this server yet");
            }
        }

        [Command("q pl", RunMode = RunMode.Async)]
        [Alias("q playlist", "queue playlist", "queue pl")]
        [Summary("q pl 'playlist url'")]
        [Remarks("Adds the given YT playlist to the Queue")]
        public async Task PlaylistCmd([Remainder] string playlistLink)
        {
            await _service.LeaveAudio(Context.Guild);
            await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);

            var ytc = new YoutubeClient();

            var playListInfo = await ytc.GetPlaylistInfoAsync(YoutubeClient.ParsePlaylistId(playlistLink));
            var idArray = playListInfo.VideoIds.ToArray();
            var twenty = idArray.Take(10).ToArray();
            foreach (var song in twenty)
            {
                var videoInfo = await ytc.GetVideoInfoAsync(song);
                var title = videoInfo.Title;
                Queue.Add(title);
            }
            await PlayQueue();
        }

        [Command("q add", RunMode = RunMode.Async)]
        [Alias("queue add")]
        [Summary("q add 'yt video'/'yt video name'")]
        [Remarks("Adds a song to the queue")]
        public async Task QueueSong([Remainder] string linkOrSearchTerm)
        {
            Queue.Add(linkOrSearchTerm);
            _reply = Queue.Count == 1
                ? "There is 1 song in the queue."
                : $"There are **{Queue.Count}** songs in the queue.";
            await ReplyAsync($"**{linkOrSearchTerm}** added.\n{_reply}");
        }

        [Command("q skip", RunMode = RunMode.Async)]
        [Alias("queue skip")]
        [Summary("q skip")]
        [Remarks("Skips the current song")]
        public async Task SkipSong()
        {
            if (Queue.Count > 0)
                Queue.RemoveAt(0);
            await PlayQueue();
        }

        [Command("q del", RunMode = RunMode.Async)]
        [Alias("queue del", "q delete", "queue delete")]
        [Summary("q del 'x'")]
        [Remarks("Removes the given song from the queue")]
        public async Task Qdel(int x)
        {
            await ReplyAsync($"Removed **{Queue.ElementAt(x)}** from the queue");
            if (Queue.Count > 0)
                Queue.RemoveAt(x);

        }

        [Command("q clear", RunMode = RunMode.Async)]
        [Alias("queue clear")]
        [Summary("q clear")]
        [Remarks("Empties the queue")]
        public async Task ClearQue()
        {
            Queue.Clear();
            await ReplyAsync("Queue has been cleared");
        }
    }
}