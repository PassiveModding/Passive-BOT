using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Configuration;
using PassiveBOT.Preconditions;
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

        [Command("music queue", RunMode = RunMode.Async)]
        [Summary("music queue")]
        [Remarks("Lists all songs in the queue")]
        public async Task QueueList(int page = 0)
        {
            var list = new List<string>();
            if (Queue.ContainsKey(Context.Guild.Id))
                Queue.TryGetValue(Context.Guild.Id, out list); //gets current queue
            var songlist = new List<string>();
            if (list != null && list.Count > 0)
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

        [Command("music add playlist", RunMode = RunMode.Async)]
        [Summary("music add playlist <Playlist URL>")]
        [Remarks("Adds the given YT playlist to the Queue")]
        [CheckDj]
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
                if (list != null)
                {
                    list.Add(title);
                    await ReplyAsync($"`{i}` - **{title}** added to the queue");
                    Queue.Remove(Context.Guild.Id);
                    Queue.Add(Context.Guild.Id, list);
                }
                //ineffieient as fuck because im adding all songs one by one rather than as a group, however. it takes a long time so this is better timewise
                i++;
            }

            await PlayQueue();

            if (list != null)
                await ReplyAsync(
                    $"**{playListInfo.Title}** has been added to the end of the queue. \nQueue Length: **{list.Count}**");
        }

        [Command("music next", RunMode = RunMode.Async)]
        [Summary("music next")]
        [Remarks("Skips the current song")]
        [CheckDj]
        public async Task SkipSong()
        {
            var list = new List<string>();
            if (Queue.ContainsKey(Context.Guild.Id))
                Queue.TryGetValue(Context.Guild.Id, out list);

            //gets the queue, removes the first entry then replays the queue
            if (list != null && list.Count > 0)
            {
                list.RemoveAt(0);
                Queue.Remove(Context.Guild.Id);
                Queue.Add(Context.Guild.Id, list);
            }
            await PlayQueue();
        }

        [Command("music remove", RunMode = RunMode.Async)]
        [Summary("music remove <songnumber>")]
        [Remarks("Removes the given song from the queue")]
        [CheckDj]
        public async Task Qdel(int x)
        {
            var list = new List<string>();
            if (Queue.ContainsKey(Context.Guild.Id))
                Queue.TryGetValue(Context.Guild.Id, out list);

            //simply removes an entry from the queue then saves it
            if (list != null && list.Count > 0)
            {
                await ReplyAsync($"Removed **{list.ElementAt(x)}** from the queue");
                list.RemoveAt(x);
                Queue.Remove(Context.Guild.Id);
                Queue.Add(Context.Guild.Id, list);
            }
        }

        [Command("music clear", RunMode = RunMode.Async)]
        [Summary("music clear")]
        [Remarks("Empties the queue")]
        [CheckDj]
        public async Task ClearQue()
        {
            var list = new List<string>();
            if (Queue.ContainsKey(Context.Guild.Id))
                Queue.TryGetValue(Context.Guild.Id, out list);

            //if the queue isn't empty, clears all items from the queue

            if (list != null && list.Count > 0)
            {
                list.Clear();
                await ReplyAsync("Queue has been cleared");
                Queue.Remove(Context.Guild.Id);
                Queue.Add(Context.Guild.Id, list);
            }
        }

        [Command("music add", RunMode = RunMode.Async)]
        [Summary("music add <song name or YT URL>")]
        [Remarks("Adds a song to the queue")]
        [CheckDj]
        public async Task QueueSong([Remainder] string linkOrSearchTerm)
        {
            if (string.IsNullOrWhiteSpace(linkOrSearchTerm))
                return;

            var list = new List<string>();
            if (Queue.ContainsKey(Context.Guild.Id))
                Queue.TryGetValue(Context.Guild.Id, out list);
            if (list != null)
            {
                list.Add(linkOrSearchTerm); //adds the given item to the queue, if its a URL it will be converted to a song later on

                Queue.Remove(Context.Guild.Id);
                Queue.Add(Context.Guild.Id, list);
                if (list.Count == 1)
                    await PlayQueue();

                await ReplyAsync(
                    $"**{linkOrSearchTerm}** has been added to the end of the queue. \n" +
                    $"Queue Length: **{list.Count}**");
            }
        }

        [Command("music play", RunMode = RunMode.Async)]
        [Summary("music play <song>")]
        [Remarks("Plays the queue")]
        [CheckDj]
        public async Task PlayQueue([Remainder]string song = null)
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
                await ReplyAsync("This guilds queue is empty. Please add some songs first before playing!");
                return;
                //if the queue is empty instruct the user to add songs to it
            }

            while (list != null && list.Count > 0)
            {
                await _service.LeaveAudio(Context.Guild);
                try
                {
                    await _service.JoinAudio(Context.Guild, ((IVoiceState) Context.User).VoiceChannel);
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
            if (list != null && list.Count == 0)
                await ReplyAsync(
                    $"Queue is empty!");


            await _service.LeaveAudio(Context.Guild);
            await ReplyAsync("Leaving Audio Channel");
        }


        [Command("music list", RunMode = RunMode.Async)]
        [Summary("music list")]
        [Remarks("Lists all songs downloaded in your server")]
        public async Task SongList(int page = 0)
        {
            //gets the current guilds directory
            var dir = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}/music/");

            if (Directory.Exists(dir))
            {
                var d = new DirectoryInfo(dir);
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

        [Command("music delete")]
        [Summary("music delete <songnumber>")]
        [Remarks("deletes the given song number's file from the servers folder")]
        [CheckDj]
        public async Task DeleteTask(int song)
        {
            var dir = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}/music/");
            var d = new DirectoryInfo(dir);
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
                    File.Delete(songpath[song]);
                    await ReplyAsync($"**Deleted song: **{songname[song]}");
                }
                catch
                {
                    await ReplyAsync($"Unable to delete song number **{song}** from the songs directory");
                }
            else
                await ReplyAsync($"Unable to delete song number **{song}** from the songs directory");
        }

        [Command("music delete all", RunMode = RunMode.Async)]
        [Summary("music delete all")]
        [Remarks("Deletes all downloaded song files from the servers folder (ADMIN)")]
        [CheckModerator]
        public async Task DeleteAllTask()
        {
            var dir = Path.Combine(AppContext.BaseDirectory, $"setup/server/{Context.Guild.Id}/music/");
            var d = new DirectoryInfo(dir);
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
        [Command("music reconnect", RunMode = RunMode.Async)]
        [Summary("music reconnect")]
        [Remarks("reconnects to the voice channel")]
        [CheckDj]
        public async Task ReconnectTask()
        {
            await _service.LeaveAudio(Context.Guild);

            try
            {
                await _service.JoinAudio(Context.Guild, ((IVoiceState) Context.User).VoiceChannel);
            }
            catch
            {
                await ReplyAsync("I was unable to connect to your voice channel\n" +
                                 "Please make sure you are in a voice channel and\n" +
                                 "I have sufficient permissions");
            }
        }

        [Command("music join", RunMode = RunMode.Async)]
        [Summary("music join")]
        [Remarks("Joins your Voice Channel")]
        [CheckDj]
        public async Task JoinCmd()
        {
            await ReplyAsync("Joining Audio Channel\n" +
                             $"Add songs using - `{Load.Pre}play 'songname'`\n" +
                             $"Play the queue using - `{Load.Pre}q play`");
            await _service.JoinAudio(Context.Guild, ((IVoiceState) Context.User).VoiceChannel);
        }

        [Command("music leave", RunMode = RunMode.Async)]
        [Summary("music leave")]
        [Remarks("Leaves your Voice Channel")]
        [CheckDj]
        public async Task LeaveCmd()
        {
            await _service.LeaveAudio(Context.Guild);
            await ReplyAsync("Leaving Audio Channel");

            var list = new List<string>();
            if (Queue.ContainsKey(Context.Guild.Id))
                Queue.TryGetValue(Context.Guild.Id, out list);

            //if the queue isn't empty, clears all items from the queue

            if (list != null && list.Count > 0)
            {
                list.Clear();
                await ReplyAsync("Queue has been cleared");
                Queue.Remove(Context.Guild.Id);
                Queue.Add(Context.Guild.Id, list);
            }
        }
    }
}