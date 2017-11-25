using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Configuration;
using TwitchLib;
using System.Linq;
using PassiveBOT.Services;
using Discord.WebSocket;

namespace PassiveBOT.Commands
{
    public class Twitchcs : ModuleBase
    {
        private static TwitchAPI api;

        public Twitchcs(TwitchAPI api_)
        {
            api_.Settings.ClientId = Config.Load().twitchtoken;
            api = api_;
        }
        [Command("TwitchStatus", RunMode = RunMode.Async)]
        [Summary("TwitchStatus <username>")]
        [Remarks("Check if a twitch streamer is online")]
        public async Task Test(string username)
        {
            if (Config.Load().twitchtoken == null)
                await ReplyAsync("There is no twitch token setup by the bot owner.");
            else
                api.Settings.ClientId = Config.Load().twitchtoken;

            var id = api.Channels.v3.GetChannelByNameAsync(username).Result.Id;

            var embed = new EmbedBuilder();


            if (await api.Streams.v5.BroadcasterOnlineAsync(id))
            {
                var stream = api.Streams.v5.GetStreamByUserAsync(id).Result.Stream;
                embed.AddField($"{username} Online", $":tv:: {stream.Viewers} viewers\n" +
                                                     $":busts_in_silhouette:: {stream.Channel.Followers} followers\n" +
                                                     $":movie_camera:: {stream.Channel.Views} views\n" +
                                                     $":video_game:: {stream.Game}\n" +
                                                     $"**[View Stream]({stream.Channel.Url})**");
                embed.ThumbnailUrl = stream.Channel.Logo;
                embed.Color = Color.Green;
            }
            else
            {
                var stream = api.Channels.v5.GetChannelByIDAsync(id).Result;
                embed.AddField($"{username} Offline", $":busts_in_silhouette:: {stream.Followers} followrs\n" +
                                                      $":movie_camera:: {stream.Views} views\n" +
                                                      $"**[View Channel]({stream.Logo})**");
                embed.ThumbnailUrl = stream.Logo;
                embed.Color = Color.Red;
            }

            await ReplyAsync("", false, embed.Build());
        }


        /*[Command("TwitchUpdates")]
        [Summary("TwitchUpdates <username>")]
        [Remarks("Setup Automatic Twitch Channel Update messages in the current channel")]
        public async Task Updates(string username)
        {
            var server = GuildConfig.GetServer(Context.Guild);
            server.TwitchPostChannel = Context.Channel.Id;

            if (Config.Load().twitchtoken == null)
                await ReplyAsync("There is no twitch token setup by the bot owner.");
            else
                api.Settings.ClientId = Config.Load().twitchtoken;

            string id;
            try
            {
                id = api.Channels.v3.GetChannelByNameAsync(username).Result.Id;
            }
            catch
            {
                await ReplyAsync("User is unavailable/does not exist");
                return;
            }

            var chan = new GuildConfig.Twitch
            {
                LastCheckedStatus = false,
                Username = username
            };

            if (server.TwitchChannels.Any(x => x.Username == username))
            {
                await ReplyAsync("Channel has already been added.");
                return;
            }

            server.TwitchChannels.Add(chan);

            GuildConfig.SaveServer(server, Context.Guild);

            await ReplyAsync("Success");

            await TwitchService.Update((DiscordSocketClient)Context.Client);
        }*/
    }
}