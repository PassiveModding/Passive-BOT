using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using PassiveBOT.Configuration;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace PassiveBOT.Services
{
    public class AudioService
    {
        internal static readonly string MusicFolder = $"{AppContext.BaseDirectory}music";

        public readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels =
            new ConcurrentDictionary<ulong, IAudioClient>();

        private CancellationTokenSource _cancel = new CancellationTokenSource();
        private Process _ffmpeg;
        private bool _next;

        public Queue<string> Playlist { get; } = new Queue<string>();


        public async Task JoinAudio(IGuild guild, IVoiceChannel target)
        {
            if (ConnectedChannels.TryGetValue(guild.Id, out IAudioClient _))
                return;
            if (target.Guild.Id != guild.Id)
                return;

            var audioClient = await target.ConnectAsync();

            if (ConnectedChannels.TryAdd(guild.Id, audioClient))
            {
            }
        }

        public async Task Queue(IMessageChannel channel)
        {
            if (Playlist.Count != 0)
            {
                var list = new StringBuilder();
                var i = 0;
                foreach (var song in Playlist)
                {
                    list.AppendLine($"`{i}` - {song}");
                    i++;
                }
                await channel.SendMessageAsync(list.ToString());
            }
            else
            {
                await channel.SendMessageAsync(
                    $"There are currently no songs in the queue, type `{Load.Pre}help` for info");
                //making sure the user knows if there are no songs
            }
        }

        public async Task DeQueue(IMessageChannel channel)
        {
            if (Playlist.Count == 0)
            {
                await channel.SendMessageAsync("-_- the queue is already empty");
            }
            else
            {
                await channel.SendMessageAsync($"**{Playlist.Peek()}** has been removed from the queue");
                Playlist.Dequeue();
            }
        }

        public async Task QueueClear(IMessageChannel channel, string message)
        {
            await channel.SendMessageAsync(message);
            Playlist.Clear();
        }

        public async Task SendSongNo(IGuild guild, IMessageChannel channel, int number)
        {
            //builds a list of .m4a files within the guilds folder
            var d = new DirectoryInfo($"{MusicFolder}/{guild.Id}");
            var music = d.GetFiles("*.m4a");
            var songlist = music.Select(sng => sng.Name).ToList();

            //returns the chosen number
            var path = songlist[number];
            var song = new List<string> {path};
            //check to see if the file exists
            if (!File.Exists($"{MusicFolder}/{guild.Id}/{path}"))
            {
                await channel.SendMessageAsync("File does not exist.");
            }
            else
            {
                if (Playlist.Count == 0)
                    await SendPlaylist(guild, channel, song); //if the playlist is emply play straight away
                else
                    Playlist.Enqueue(path); //if the playlist is already playing add to the end of the queue

                await channel.SendMessageAsync(
                    $"Added **{path}** to the queue, you can qiew the queue using `{Load.Pre}queue`");
            }
        }

        public async Task SendPlaylist(IGuild guild, IMessageChannel channel, List<string> list)
        {
            ConnectedChannels.TryGetValue(guild.Id, out IAudioClient client);

            foreach (var song in list)
                Playlist.Enqueue(song); //adds all songs in the supplied list to the queue

            while (Playlist.Count > 0)
            {
                var path = Playlist.Dequeue();
                var combine = Path.Combine(MusicFolder, guild.Id.ToString());
                var complete = Path.Combine(combine, path); //making sure the path is correct
                await channel.SendMessageAsync($"Now Playing **{path}**!!");
                using (_ffmpeg = Process.Start(new ProcessStartInfo
                {
                    FileName =
                        Path.Combine(AppContext.BaseDirectory,
                            "ffmpeg.exe"), //sets the ffmpeg.exe directory to be the same as the bot
                    Arguments = $"-hide_banner -loglevel panic -i \"{complete}\" -ac 2 -f s16le -ar 48000 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = false
                }))
                {
                    var stream = client.CreatePCMStream(AudioApplication.Music);
                    try
                    {
                        await _ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream, 81920, _cancel.Token)
                            .ConfigureAwait(false);
                    }
                    catch (TaskCanceledException)
                    {
                        if (!_next)
                        {
                            Playlist.Dequeue();
                            break;
                        }
                        else
                        {
                            _next = false;
                        }
                    }
                    finally
                    {
                        await stream.FlushAsync().ConfigureAwait(false);
                    }
                }
            }
        }


        public async Task Cancel(IGuild guild)
        {
            Playlist.Clear();
            _cancel.Cancel();
            _cancel.Dispose();
            _cancel = new CancellationTokenSource();
            _ffmpeg.Kill();
            if (ConnectedChannels.TryRemove(guild.Id, out IAudioClient client))
                await client.StopAsync();
        }

        public async Task Next(IGuild guild, IMessageChannel channel)
        {
            //removes the next song from the queue
            Playlist.Dequeue();
            //adds the song to a list so it is not cleared when cancelled
            var list = Playlist.ToList();
            _cancel.Cancel();
            _cancel.Dispose();
            _cancel = new CancellationTokenSource();
            _ffmpeg.Kill();
            //sends the playlist back to the player to continue (not efficient I know but im shit)
            await SendPlaylist(guild, channel, list);
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
                Directory.CreateDirectory($"{MusicFolder}/{guild.Id}/"); //creating the servers directory
            else if (File.Exists($"{MusicFolder}/{guild.Id}/{videoInfo.Title}.{fileExtension}"))
                return; //making sure we do not double up on the same file

            //downloads the song
            var output = File.Create($"{MusicFolder}/{guild.Id}/{videoInfo.Title}.{fileExtension}");
            await input.CopyToAsync(output);

            //shows intructions on playing after the song is downloaded
            await channel.SendMessageAsync(
                $"{videoInfo.Title} has been downloaded, you can play it using `{Config.Load().Prefix}play {videoInfo.Title}`");
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
    }
}