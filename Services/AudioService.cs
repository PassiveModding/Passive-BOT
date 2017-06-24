using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using YoutubeExplode;
using YoutubeExplode.Models;

namespace PassiveBOT.Services
{
    public class AudioService
    {
        private readonly ConcurrentDictionary<ulong, IAudioClient> _connectedChannels =
            new ConcurrentDictionary<ulong, IAudioClient>();

        public async Task JoinAudio(IGuild guild, IVoiceChannel target)
        {
            if (_connectedChannels.TryGetValue(guild.Id, out IAudioClient _))
                return;

            if (target.Guild.Id != guild.Id)
                return;

            var audioClient = await target.ConnectAsync();

            if (_connectedChannels.TryAdd(guild.Id, audioClient))
            {
            }
        }

        public async Task LeaveAudio(IGuild guild)
        {
            if (_connectedChannels.TryRemove(guild.Id, out IAudioClient client))
                await client.StopAsync();
        }


        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string userInput)
        {
            var ytc = new YoutubeClient();

            if (userInput.ToLower().Contains("youtube.com"))
            {
                userInput = YoutubeClient.ParseVideoId(userInput);
            }
            else
            {
                var searchList = await ytc.SearchAsync(userInput);
                userInput = searchList.First();
            }

            var videoInfo = await ytc.GetVideoInfoAsync(userInput);

            var asi = videoInfo.AudioStreams.OrderBy(x => x.Bitrate).Last();

            var title = videoInfo.Title;

            var rgx = new Regex("[^a-zA-Z0-9 -]");
            title = rgx.Replace(title, "");

            var path =
                $"{AppContext.BaseDirectory}/setup/server/{guild.Id}/music/{title}.{asi.Container.GetFileExtension()}";
            if (!Directory.Exists($"{AppContext.BaseDirectory}/setup/server/{guild.Id}/music/"))
                Directory.CreateDirectory($"{AppContext.BaseDirectory}/setup/server/{guild.Id}/music/");

            if (!File.Exists(path))
            {
                await channel.SendMessageAsync($"Attempting to download **{title}**!");
                using (var input = await ytc.GetMediaStreamAsync(asi))
                using (var Out = File.Create(path))
                {
                    await input.CopyToAsync(Out);
                }
            }

            if (_connectedChannels.TryGetValue(guild.Id, out IAudioClient audioClient))
            {
                await channel.SendMessageAsync($"Now Playing: **{title}**");
                var discordStream = audioClient.CreatePCMStream(AudioApplication.Music);
                await CreateStream(path).StandardOutput.BaseStream.CopyToAsync(discordStream);
                await discordStream.FlushAsync();
            }
        }

        private static Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }
    }
}