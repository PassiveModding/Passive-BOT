using System;
using System.Collections.Generic;
using System.IO;
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
            var d = new DirectoryInfo(AppContext.BaseDirectory + "music/");
            var music = d.GetFiles("*.mp3");
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
    }
}