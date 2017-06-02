using System;
using Discord;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord.Audio;

namespace PassiveBOT.Services
{
    public class AudioService
    {
        private readonly ConcurrentDictionary<ulong, IAudioClient> _connectedChannels =
            new ConcurrentDictionary<ulong, IAudioClient>();

        private Queue<string> Playlist { get; }  = new Queue<string>();


        public async Task JoinAudio(IGuild guild, IVoiceChannel target)
        {
            if (_connectedChannels.TryGetValue(guild.Id, out IAudioClient _))
            {
                return;
            }
            if (target.Guild.Id != guild.Id)
            {
                return;
            }

            var audioClient = await target.ConnectAsync();

            if (_connectedChannels.TryAdd(guild.Id, audioClient))
            {

            }
        }

        public async Task LeaveAudio(IGuild guild)
        {
            if (_connectedChannels.TryRemove(guild.Id, out IAudioClient client))
            {
                await client.StopAsync();
            }
        }

        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string path)
        {

            if (!path.EndsWith(".mp3"))
            {
                path = path + ".mp3";
            }

            // Your task: Get a full path to the file if the value of 'path' is only a filename.
            if (!File.Exists("music/" + path))
            {
                await channel.SendMessageAsync("File does not exist.");
                return;
            }
            if (_connectedChannels.TryGetValue(guild.Id, out IAudioClient client))
            {
                var output = CreateStream("music/" + path).StandardOutput.BaseStream;
                await channel.SendMessageAsync($"Now playing **{path}**");
                var stream = client.CreatePCMStream(AudioApplication.Music);
                await output.CopyToAsync(stream);
                await stream.FlushAsync().ConfigureAwait(false);
            }
        }

        public async Task SendSongNo(IGuild guild, IMessageChannel channel, int number)
        {

            var d = new DirectoryInfo(AppContext.BaseDirectory + "music/");
            var music = d.GetFiles("*.mp3");
            var songlist = new List<string>();
            var i = 0;
            foreach (var sng in music)
            {
                songlist.Add(sng.Name);
                i++;
            }

            var path = songlist[number];

            // Your task: Get a full path to the file if the value of 'path' is only a filename.
            if (!File.Exists($"music/{path}"))
            {
                await channel.SendMessageAsync("File does not exist.");
                return;
            }
            if (_connectedChannels.TryGetValue(guild.Id, out IAudioClient client))
            {
                var output = CreateStream("music/" + path).StandardOutput.BaseStream;
                await channel.SendMessageAsync($"Now playing **{path}**");
                var stream = client.CreatePCMStream(AudioApplication.Music);
                await output.CopyToAsync(stream);
                await stream.FlushAsync().ConfigureAwait(false);
            }
        }

        private static Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "music/ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }
    }
}