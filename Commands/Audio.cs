using System.Collections.Generic;
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
        [Alias("j")]
        [Summary("j")]
        [Remarks("Joins your Voice Channel")]
        public async Task JoinCmd()
        {
            await ReplyAsync($"{Context.Client.CurrentUser.Username} is joining **{Context.Channel}**");
            await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
        }

        [Command("leave", RunMode = RunMode.Async)]
        [Alias("l")]
        [Summary("l")]
        [Remarks("Leaves your Voice Channel")]
        public async Task LeaveCmd()
        {
            await _service.LeaveAudio(Context.Guild);
            await ReplyAsync($"{Context.Client.CurrentUser.Username} is Leaving **{Context.Channel}**");
        }

        [Command("play", RunMode = RunMode.Async)]
        [Alias("p")]
        [Summary("p")]
        [Remarks("Plays the requested Song")]
        public async Task PlayCmd([Remainder] string linkOrSearchTerm)
        {
            await _service.LeaveAudio(Context.Guild);
            await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
            await _service.SendAudioAsync(Context.Guild, Context.Channel, linkOrSearchTerm);
            await _service.LeaveAudio(Context.Guild);
            await ReplyAsync($"{Context.Client.CurrentUser.Username} is Leaving **{Context.Channel}**");
        }

        [Command("playlist", RunMode = RunMode.Async)]
        [Alias("pl")]
        [Summary("pl")]
        [Remarks("Plays the given playlist")]
        public async Task PlaylistCmd([Remainder] string playlistLink)
        {
            await _service.LeaveAudio(Context.Guild);
            await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);

            var ytc = new YoutubeClient();

            var playListInfo = await ytc.GetPlaylistInfoAsync(YoutubeClient.ParsePlaylistId(playlistLink));

            var idArray = playListInfo.VideoIds.ToArray();

            foreach (var id in idArray)
                await _service.SendAudioAsync(Context.Guild, Context.Channel, id);
            await _service.LeaveAudio(Context.Guild);
        }

        [Command("queue", RunMode = RunMode.Async)]
        [Alias("q")]
        [Summary("q")]
        [Remarks("Adds a song to the queue")]
        public async Task QueueSong([Remainder] string linkOrSearchTerm)
        {
            Queue.Add(linkOrSearchTerm);
            _reply = Queue.Count == 1
                ? "There is 1 song in the queue."
                : $"There are **{Queue.Count}** songs in the queue.";
            await ReplyAsync($"**{linkOrSearchTerm}** added.\n{_reply}");
        }

        [Command("list", RunMode = RunMode.Async)]
        [Alias("li")]
        [Summary("li")]
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

        [Command("clearque", RunMode = RunMode.Async)]
        [Alias("clearq")]
        [Summary("clearq")]
        [Remarks("Empties the queue")]
        public async Task ClearQue()
        {
            Queue.Clear();
            await ReplyAsync("Queue has been cleared");
        }

        [Command("playqueue", RunMode = RunMode.Async)]
        [Alias("playq")]
        [Summary("playq")]
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
            await ReplyAsync($"{Context.Client.CurrentUser.Username} is Leaving **{Context.Channel}**");
        }

        [Command("skip", RunMode = RunMode.Async)]
        [Summary("skip")]
        [Remarks("Removes the given song from the queue")]
        public async Task SkipSong([Remainder] int x = 0)
        {
            if (Queue.Count > 0 && x < Queue.Count && x > -1)
                Queue.RemoveAt(x);
            await ReplyAsync($"Removed the Song `#{x}` from the Queue");
        }
    }
}