using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Configuration;
using PassiveBOT.Services;
using YoutubeExplode;

namespace PassiveBOT.Commands
{
    [RequireContext(ContextType.Guild)]
    public class Audio : ModuleBase
    {
        public static Dictionary<ulong, List<string>> Queue =
            new Dictionary<ulong, List<string>>();

        private readonly AudioService _service;
        private string _nextSong, _leftInQueue;

        public Audio(AudioService service)
        {
            _service = service;
        }

        [Command("queue", RunMode = RunMode.Async)]
        [Alias("q")]
        [Summary("q")]
        [Remarks("Lists all songs in the queue")]
        public async Task QueueList(int page = 0)
        {
            var list = new List<string>();
            if (Queue.ContainsKey(Context.Guild.Id))
                Queue.TryGetValue(Context.Guild.Id, out list); //gets current queue
            var songlist = new List<string>();
            if (list.Count > 0)
            {
                var i = 0;
                foreach (var item in list)
                {
                    songlist.Add($"`{i}` - {item}"); //adds each item in the queue with index to a list
                    i++;
                }
                if (page <= 0)
                {
                    if (i > 10)
                        await ReplyAsync(
                            $"**Page 0**\nHere are the first 10 songs in your playlist (total = {i}):\n{string.Join("\n", songlist.Take(10).ToArray())}");
                    else
                        await ReplyAsync(string.Join("\n", songlist.ToArray()));
                }
                else
                {
                    var response = string.Join("\n", songlist.Skip(page * 10).Take(10).ToArray());
                    if (response == "")
                        await ReplyAsync($"**Page {page}** of your playlist:\nEmpty");
                    await ReplyAsync($"**Page {page}** of your playlist:\n{response}");
                }
            }
            else
            {
                await ReplyAsync("The Queue is empty :(");
            }
        }


        [Command("q add", RunMode = RunMode.Async)]
        [Alias("queue add", "play")]
        [Summary("q add 'yt video'/'yt video name'")]
        [Remarks("Adds a song to the queue")]
        public async Task QueueSong([Remainder] string linkOrSearchTerm)
        {
            if (string.IsNullOrWhiteSpace(linkOrSearchTerm))
                return;

            var list = new List<string>();
            if (Queue.ContainsKey(Context.Guild.Id))
                Queue.TryGetValue(Context.Guild.Id, out list);
            list.Add(linkOrSearchTerm); //adds the given item to the queue, if its a URL it will be converted to a song later on

            Queue.Remove(Context.Guild.Id);
            Queue.Add(Context.Guild.Id, list);
            await ReplyAsync(
                $"**{linkOrSearchTerm}** has been added to the end of the queue. \n" +
                $"Queue Length: **{list.Count}**");
        }

        [Command("q pl", RunMode = RunMode.Async)]
        [Alias("q playlist", "queue playlist", "queue pl")]
        [Summary("q pl 'playlist url'")]
        [Remarks("Adds the given YT playlist to the Queue")]
        public async Task PlaylistCmd([Remainder] string playlistLink)
        {
            if (string.IsNullOrWhiteSpace(playlistLink))
                return;

            var ytc = new YoutubeClient();

            var playListInfo = await ytc.GetPlaylistInfoAsync(YoutubeClient.ParsePlaylistId(playlistLink));
            var ten = playListInfo.VideoIds.ToArray().Take(10).ToArray(); //adds first 10 songs in playlist to an array
            var list = new List<string>();
            if (Queue.ContainsKey(Context.Guild.Id))
                Queue.TryGetValue(Context.Guild.Id, out list); //outputs the current queue to a list
            await ReplyAsync($"Attempting to add the first 10 songs of **{playListInfo.Title}** to the queue!");
            var i = 0;
            foreach (var song in ten)
            {
                var videoInfo = await ytc.GetVideoInfoAsync(song);
                var title = videoInfo.Title;
                list.Add(title);
                await ReplyAsync($"`{i}` - **{title}** added to the queue");
                Queue.Remove(Context.Guild.Id);
                Queue.Add(Context.Guild.Id, list);
                //ineffieient as fuck because im adding all songs one by one rather than as a group, however. it takes a long time so this is better timewise
                i++;
            }

            await PlayQueue();

            await ReplyAsync(
                $"**{playListInfo.Title}** has been added to the end of the queue. \nQueue Length: **{list.Count}**");
        }

        [Command("q all", RunMode = RunMode.Async)]
        [Alias("queue all")]
        [Summary("q all")]
        [Remarks("Plays all downloaded songs")]
        public async Task Pall()
        {
            var list = new List<string>();
            if (Queue.ContainsKey(Context.Guild.Id))
                Queue.TryGetValue(Context.Guild.Id, out list);

            //similar to the .songs command, this gets the current guilds list, adds all downloaded songs to it and plays
            if (Directory.Exists($"{AppContext.BaseDirectory}/setup/server/{Context.Guild.Id}/music/"))
            {
                var d = new DirectoryInfo($"{AppContext.BaseDirectory}/setup/server/{Context.Guild.Id}/music/");
                var music = d.GetFiles("*.*");
                list.AddRange(music.Select(sng => Path.GetFileNameWithoutExtension(sng.Name)));
                Queue.Remove(Context.Guild.Id);
                Queue.Add(Context.Guild.Id, list);
                await PlayQueue();
            }
            else
            {
                await ReplyAsync("There are no songs downloaded in this server yet");
            }
        }

        [Command("q skip", RunMode = RunMode.Async)]
        [Alias("queue skip")]
        [Summary("q skip")]
        [Remarks("Skips the current song")]
        public async Task SkipSong()
        {
            var list = new List<string>();
            if (Queue.ContainsKey(Context.Guild.Id))
                Queue.TryGetValue(Context.Guild.Id, out list);

            //gets the queue, removes the first entry then replays the queue
            if (list.Count > 0)
            {
                list.RemoveAt(0);
                Queue.Remove(Context.Guild.Id);
                Queue.Add(Context.Guild.Id, list);
            }
            await PlayQueue();
        }

        [Command("q del", RunMode = RunMode.Async)]
        [Alias("queue del", "q delete", "queue delete")]
        [Summary("q del 'x'")]
        [Remarks("Removes the given song from the queue")]
        public async Task Qdel(int x)
        {
            var list = new List<string>();
            if (Queue.ContainsKey(Context.Guild.Id))
                Queue.TryGetValue(Context.Guild.Id, out list);

            //simply removes an entry from the queue then saves it
            if (list.Count > 0)
            {
                await ReplyAsync($"Removed **{list.ElementAt(x)}** from the queue");
                list.RemoveAt(x);
                Queue.Remove(Context.Guild.Id);
                Queue.Add(Context.Guild.Id, list);
            }
        }

        [Command("q clear", RunMode = RunMode.Async)]
        [Alias("queue clear")]
        [Summary("q clear")]
        [Remarks("Empties the queue")]
        public async Task ClearQue()
        {
            var list = new List<string>();
            if (Queue.ContainsKey(Context.Guild.Id))
                Queue.TryGetValue(Context.Guild.Id, out list);

            //if the queue isn't empty, clears all items from the queue

            if (list.Count > 0)
            {
                list.Clear();
                await ReplyAsync("Queue has been cleared");
                Queue.Remove(Context.Guild.Id);
                Queue.Add(Context.Guild.Id, list);
            }
        }

        [Command("q play", RunMode = RunMode.Async)]
        [Alias("queue play")]
        [Summary("q play")]
        [Remarks("Plays the queue")]
        public async Task PlayQueue(string song = null)
        {
            if (string.IsNullOrWhiteSpace(song))
                song = null;
            if (song != null)
                await QueueSong(song);

            List<string> list;
            if (Queue.ContainsKey(Context.Guild.Id))
            {
                Queue.TryGetValue(Context.Guild.Id, out list); //gets the guilds queue
            }
            else
            {
                await ReplyAsync("This guilds queue is empty. Please add some songs first before playing!\n" +
                                 $"`{Load.Pre}q add` - adds a song to the queue\n" +
                                 $"`{Load.Pre}q pl 'playlist URL'` - adds the first ten songs form a youtube playlist to the queue\n" +
                                 $"`{Load.Pre}q all` - adds all songs previously downloaded in your server to the queue");
                return;
                //if the queue is empty instruct the user to add songs to it
            }

            while (list.Count > 0)
            {
                await _service.LeaveAudio(Context.Guild);
                try
                {
                    await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
                }
                catch
                {
                    await ReplyAsync("The playlists DJ is no longer in the channel");
                    break;
                }


                _nextSong = list.Count != 1 ? $", next song: **{list.ElementAt(1)}**" : "";
                _leftInQueue = list.Count == 1
                    ? "There is 1 song in the queue."
                    : $"There are {list.Count} songs in the queue.";
                await ReplyAsync($"Now Playing: **{list.First()}** {_nextSong}.\n{_leftInQueue}");

                await _service.SendAudioAsync(Context.Guild, Context.Channel, list.First());
                list.RemoveAt(0);
                Queue.Remove(Context.Guild.Id);
                Queue.Add(Context.Guild.Id, list);
                Queue.TryGetValue(Context.Guild.Id, out list);
                //at end of song, removes the current song from the list and plays the next
            }
            if (list.Count == 0)
                await ReplyAsync(
                    $"Sorry, the queue is empty, `{Load.Pre}play 'song'` (or `{Load.Pre}q add 'song'`) to add more!");


            await _service.LeaveAudio(Context.Guild);
            await ReplyAsync("Leaving Audio Channel");
        }


        [Command("songs", RunMode = RunMode.Async)]
        [Summary("songs")]
        [Remarks("Lists all songs downloaded in your server")]
        public async Task SongList(int page = 0)
        {
            //gets the current guilds directory
            if (Directory.Exists($"{AppContext.BaseDirectory}/setup/server/{Context.Guild.Id}/music/"))
            {
                var d = new DirectoryInfo($"{AppContext.BaseDirectory}/setup/server/{Context.Guild.Id}/music/");
                var music = d.GetFiles("*.*");
                var songlist = new List<string>();
                var i = 0;
                foreach (var sng in music)
                {
                    songlist.Add($"`{i}` - {Path.GetFileNameWithoutExtension(sng.Name)}");
                    i++;
                }
                var list = string.Join("\n", songlist.Take(10).ToArray());
                if (i > 10)
                    if (page <= 0)
                    {
                        await ReplyAsync(
                            $"**Page 0**\nHere are the first 10 songs saved in your server (total = {i})\n" +
                            $"{list}");
                    }
                    else
                    {
                        list = string.Join("\n", songlist.Skip(page * 10).Take(10).ToArray());
                        if (list == "")
                            await ReplyAsync($"**Page {page}**\n" +
                                             "This page is empty");
                        else
                            await ReplyAsync($"**Page {page}**\n" +
                                             $"{list}");
                    }
                else
                    await ReplyAsync(list);
            }
            else
            {
                await ReplyAsync("There are currently no songs downloaded for this guild\n" +
                                 $"you can download songs using the `{Load.Pre}play` command");
            }
        }

        [Command("delete")]
        [Summary("delete 'songnumber'")]
        [Remarks("deletes the given song number's file from the servers folder")]
        public async Task DeleteTask(int song)
        {
            var d = new DirectoryInfo($"{AppContext.BaseDirectory}/setup/server/{Context.Guild.Id}/music/");
            var music = d.GetFiles("*.*");
            var songpath = new List<string>();
            var songname = new List<string>();
            var i = 0;
            foreach (var sng in music)
            {
                songpath.Add($"{sng.FullName}");
                songname.Add($"{Path.GetFileNameWithoutExtension(sng.Name)}");
                i++;
            }
            if (song <= i)
                try
                {
                    await ReplyAsync($"**Deleted song: **{songname[song]}");
                    File.Delete(songpath[song]);
                }
                catch
                {
                    await ReplyAsync($"Unable to delete song number **{song}** from the songs directory");
                }
            else
                await ReplyAsync($"Unable to delete song number **{song}** from the songs directory");
        }

        [Command("delete all")]
        [Summary("delete all")]
        [Remarks("Deletes all downloaded song files from the servers folder (ADMIN)")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DeleteAllTask()
        {
            var d = new DirectoryInfo($"{AppContext.BaseDirectory}/setup/server/{Context.Guild.Id}/music/");
            var music = d.GetFiles("*.*");
            var i = 0;
            foreach (var sng in music)
            {
                File.Delete(sng.FullName);
                i++;
            }

            await ReplyAsync($"{Context.User} deleted all downloaded songs (total = {i}) from this server's folder\n" +
                             $"you can download more using `{Load.Pre}play 'songname or YT URL'`\n" +
                             $"for a rundown on commands type `{Load.Pre}help`");
        }

        //connection commands
        [Command("reconnect", RunMode = RunMode.Async)]
        [Summary("reconnect")]
        [Remarks("reconnects to the voice channel")]
        public async Task ReconnectTask()
        {
            await _service.LeaveAudio(Context.Guild);

            try
            {
                await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
            }
            catch
            {
                await ReplyAsync("I was unable to connect to your voice channel\n" +
                                 "Please make sure you are in a voice channel and\n" +
                                 "I have sufficient permissions");
            }
        }

        [Command("join", RunMode = RunMode.Async)]
        [Summary("join")]
        [Remarks("Joins your Voice Channel")]
        public async Task JoinCmd()
        {
            await ReplyAsync("Joining Audio Channel\n" +
                             $"Add songs using - `{Load.Pre}play 'songname'`\n" +
                             $"Play the queue using - `{Load.Pre}q play`");
            await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
        }

        [Command("leave", RunMode = RunMode.Async)]
        [Summary("leave")]
        [Remarks("Leaves your Voice Channel")]
        public async Task LeaveCmd()
        {
            await _service.LeaveAudio(Context.Guild);
            await ReplyAsync("Leaving Audio Channel");
        }
    }
}