using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Services;

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

        [Command("play", RunMode = RunMode.Async)]
        [Summary("play IceCold")]
        [Remarks("Plays the given song")]
        public async Task PlayCmd([Remainder] string song)
        {
            if (int.TryParse(song, out int x))
            {
                if (x >= 0)
                {
                    await _service.SendSongNo(Context.Guild, Context.Channel, x);
                }

            }
            else
            {
                await _service.SendAudioAsync(Context.Guild, Context.Channel, song);
            }
        }

        [Command("list")]
        [Alias("queue")]
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
                    Console.WriteLine(url);
                    var videoId = url.Substring(url.Length - 11, 11);
                    Console.WriteLine(videoId);
                    await _service.DlAudio(Context.Guild, Context.Channel, videoId);
                }
            }
            else
            {
                await ReplyAsync(
                    "**Invalid URL format: ** please use something like `https://www.youtube.com/watch?v=tvTRZJ-4EyI`");
            }
        }

    }
}