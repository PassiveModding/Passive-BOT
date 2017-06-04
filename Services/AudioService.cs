using System;
using Discord;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.Audio;
using PassiveBOT.Configuration;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace PassiveBOT.Services
{
    public class AudioService
    {
        public readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels =
            new ConcurrentDictionary<ulong, IAudioClient>();

        private static readonly string MusicFolder = $"{AppContext.BaseDirectory}music";


        public async Task JoinAudio(IGuild guild, IVoiceChannel target)
        {
            if (ConnectedChannels.TryGetValue(guild.Id, out IAudioClient _))
            {
                return;
            }
            if (target.Guild.Id != guild.Id)
            {
                return;
            }

            var audioClient = await target.ConnectAsync();

            if (ConnectedChannels.TryAdd(guild.Id, audioClient))
            {

            }
        }

        public async Task LeaveAudio(IGuild guild)
        {
            if (ConnectedChannels.TryRemove(guild.Id, out IAudioClient client))
            {
                await client.StopAsync();
            }
        }

        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string path)
        {
            //making sure you get only the downloaded audio files
            if (!path.EndsWith(".m4a"))
            {
                path = path + ".m4a";
            }
            if (!File.Exists($"{MusicFolder}/{guild.Id}/{path}"))
            {
                await channel.SendMessageAsync("File does not exist.");
                return;
            }
            if (ConnectedChannels.TryGetValue(guild.Id, out IAudioClient client))
            {
                var output = CreateStream(path, guild).StandardOutput.BaseStream;
                await channel.SendMessageAsync($"Now playing **{path}**");
                var stream = client.CreatePCMStream(AudioApplication.Music);
                await output.CopyToAsync(stream);
                await stream.FlushAsync().ConfigureAwait(false);
            }
        }

        public async Task SendSongNo(IGuild guild, IMessageChannel channel, int number)
        {
            //builds a list of .m4a files within the guilds folder
            var d = new DirectoryInfo($"{MusicFolder}/{guild.Id}");
            var music = d.GetFiles("*.m4a");
            var songlist = music.Select(sng => sng.Name).ToList();

            //returns the chosen number
            var path = songlist[number];

            //check to see if the file exists
            if (!File.Exists($"{MusicFolder}/{guild.Id}/{path}"))
            {
                await channel.SendMessageAsync("File does not exist.");
                return;
            }
            //create the stream
            if (ConnectedChannels.TryGetValue(guild.Id, out IAudioClient client))
            {
                var output = CreateStream(path, guild).StandardOutput.BaseStream;
                await channel.SendMessageAsync($"Now playing **{path}**");
                var stream = client.CreatePCMStream(AudioApplication.Music);
                await output.CopyToAsync(stream);
                await stream.FlushAsync().ConfigureAwait(false);
            }
        }

        public async Task DlAudio(IGuild guild, IMessageChannel channel, string url)
        {
            await channel.SendMessageAsync("**Downloading**");
            var ytclient = new YoutubeClient();
            var videoInfo = await ytclient.GetVideoInfoAsync(url);
            var streamInfo = videoInfo.AudioStreams.OrderBy(s => s.AudioEncoding == AudioEncoding.Mp3).Last();
            var fileExtension = streamInfo.Container.GetFileExtension();

            var input = await ytclient.GetMediaStreamAsync(streamInfo);
            if (!Directory.Exists($"{MusicFolder}/{guild.Id}/"))
            {
                Directory.CreateDirectory($"{MusicFolder}/{guild.Id}/"); //creating the servers directory
            }
            else if (File.Exists($"{MusicFolder}/{guild.Id}/{videoInfo.Title}.{fileExtension}"))
            {
                return; //making sure we do not double up on the same file
            }

            //downloads the song
            var output = File.Create($"{MusicFolder}/{guild.Id}/{videoInfo.Title}.{fileExtension}");
            await input.CopyToAsync(output);

            //shows intructions on playing after the song is downloaded
            await channel.SendMessageAsync($"{videoInfo.Title} has been downloaded, you can play it using `{Config.Load().Prefix}play {videoInfo.Title}`");
        }

        public async Task ListMusic(IMessageChannel channel, IGuild guild)
        {
            //gets the current guilds directory
            if (Directory.Exists($"{MusicFolder}/{guild.Id}/"))
            {
                var d = new DirectoryInfo($"{MusicFolder}/{guild.Id}/");
                var music = d.GetFiles("*.m4a");
                var songlist = new List<string>();
                var i = 0;
                foreach (var sng in music)
                {
                    songlist.Add($"`{i}` - {Path.GetFileNameWithoutExtension(sng.Name)}");
                    i++;
                }

                var list = string.Join("\n", songlist.ToArray());

                await channel.SendMessageAsync(list);
            }
            else
            {
                await channel.SendMessageAsync("There are currently no songs downloaded for this guild\n" +
                                               $"you can download songs using the `{Config.Load().Prefix}get` command");
            }
        }

        private static Process CreateStream(string song, IGuild guild)
        {
            var combine = Path.Combine(MusicFolder, guild.Id.ToString());
            var complete = Path.Combine(combine, song);
            var ffmpeg = Path.Combine(AppContext.BaseDirectory, "ffmpeg.exe");
            //Console.WriteLine(complete + "---" + ffmpeg);
            return Process.Start(new ProcessStartInfo
            {
                FileName = ffmpeg,
                Arguments = $"-hide_banner -loglevel panic -i \"{complete}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }
    }
}