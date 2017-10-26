using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Configuration;
using TwitchLib;

namespace PassiveBOT.Commands
{
    public class Twitchcs : ModuleBase
    {
        [Command("TwitchStatus", RunMode = RunMode.Async)]
        [Summary("TwitchStatus <username>")]
        [Remarks("Check if a twitch streamer is online")]
        public async Task Test(string username)
        {
            if (Config.Load().twitchtoken == null)
                await ReplyAsync("There is no twitch token setup by the bot owner.");
            else
                TwitchAPI.Settings.ClientId = Config.Load().twitchtoken;

            var id = TwitchAPI.Channels.v3.GetChannelByNameAsync(username).Result.Id;

            var embed = new EmbedBuilder();


            if (await TwitchAPI.Streams.v5.BroadcasterOnlineAsync(id))
            {
                var stream = TwitchAPI.Streams.v5.GetStreamByUserAsync(id).Result.Stream;
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
                var stream = TwitchAPI.Channels.v5.GetChannelByIDAsync(id).Result;
                embed.AddField($"{username} Offline", $":busts_in_silhouette:: {stream.Followers} followrs\n" +
                                                      $":movie_camera:: {stream.Views} views\n" +
                                                      $"**[View Channel]({stream.Logo})**");
                embed.ThumbnailUrl = stream.Logo;
                embed.Color = Color.Red;
            }

            await ReplyAsync("", false, embed.Build());
        }
    }
}