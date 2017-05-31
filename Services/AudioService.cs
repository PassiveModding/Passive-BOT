using Discord;
using Discord.Audio;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace PassiveBOT.Services
{
    public class AudioService
    {
        private readonly ConcurrentDictionary<ulong, IAudioClient> _connectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

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
            if (path.EndsWith(".mp3"))
            {
            }
            else
            {
                path = path + ".mp3";
            }
            
            // Your task: Get a full path to the file if the value of 'path' is only a filename.
            if (!File.Exists(path))
            {
                await channel.SendMessageAsync("File does not exist.");
                return;
            }
            if (_connectedChannels.TryGetValue(guild.Id, out IAudioClient client))
            {
                var output = CreateStream(path).StandardOutput.BaseStream;
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