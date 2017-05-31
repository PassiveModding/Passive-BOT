using System;
using System.IO;
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
        public async Task JoinCmd()
        {
            await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
        }

        [Command("leave", RunMode = RunMode.Async)]
        public async Task LeaveCmd()
        {
            await _service.LeaveAudio(Context.Guild);
        }
    
        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayCmd([Remainder] string song)
        {
            await _service.SendAudioAsync(Context.Guild, Context.Channel, "music/" + song);
        }

        [Command("list")]
        public async Task ListMusic()
        {
            var filepath = AppContext.BaseDirectory;
            var d = new DirectoryInfo(filepath + "music/");
            var songs = d.GetFiles("*.mp3").Select(file => file.Name).ToList();
            songs.Sort();

            var list = string.Join("\n", songs.ToArray());

            await ReplyAsync(list);
        }
    }
}