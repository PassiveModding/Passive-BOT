using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using PassiveBOT.Configuration.Objects;
using PassiveBOT.Handlers;
using PassiveBOT.Handlers.Services;
using PassiveBOT.preconditions;
using PassiveBOT.strings;
using RedditSharp;

namespace PassiveBOT.Commands.Media
{
    [Ratelimit(1, 5, Measure.Seconds)]
    public class Media : ModuleBase
    {
        [Command("GetRedditPost", RunMode = RunMode.Async)]
        [Summary("GetRedditPost <sub>")]
        [Remarks("Get a random post from first 25 in hot of a sub")]
        public async Task RedditTask(string subreddit = null)
        {
            if (subreddit == null) await ReplyAsync("Please give a subreddit to browse.");

            var rnd = new Random();
            var checkcache = CommandHandler.SubReddits.FirstOrDefault(x =>
                string.Equals(x.Title, subreddit, StringComparison.CurrentCultureIgnoreCase));
            if (checkcache != null && checkcache.LastUpdate > DateTime.UtcNow - TimeSpan.FromHours(6))
            {
                var imgx = checkcache.Posts[rnd.Next(checkcache.Posts.Count)];
                await ReplyAsync($"{imgx.Title}\nhttps://reddit.com{imgx.Permalink}");
                checkcache.Hits++;
            }
            else
            {
                var r = new Reddit();
                var sub = r.GetSubreddit(subreddit);
                await ReplyAsync("Refreshing Cache");
                var num1 = sub.Hot.GetListing(25).ToList();
                var post = num1[rnd.Next(24)];
                await ReplyAsync($"{post.Title}\nhttps://reddit.com{post.Permalink}");
                CommandHandler.SubReddits.RemoveAll(x =>
                    string.Equals(x.Title, subreddit, StringComparison.CurrentCultureIgnoreCase));
                CommandHandler.SubReddits.Add(new CommandHandler.SubReddit
                {
                    Title = subreddit,
                    LastUpdate = DateTime.UtcNow,
                    Posts = num1
                });
            }
        }

        [Command("RedditImage", RunMode = RunMode.Async)]
        [Summary("RedditImage <sub>")]
        [Alias("rimg", "rimage")]
        [Remarks("Get a random post from first 25 in hot of a sub")]
        public async Task RedditIMG(string subreddit = null)
        {
            if (subreddit == null) await ReplyAsync("Please give a subreddit to browse.");
            var rnd = new Random();
            var checkcache = CommandHandler.SubReddits.FirstOrDefault(x =>
                string.Equals(x.Title, subreddit, StringComparison.CurrentCultureIgnoreCase));
            if (checkcache != null && checkcache.LastUpdate > DateTime.UtcNow - TimeSpan.FromHours(6))
            {
                var imgx = checkcache.Posts[rnd.Next(checkcache.Posts.Count)];
                var objx = RedditHelper.isimage(imgx.Url.ToString());
                var embedx = new EmbedBuilder
                {
                    Title = imgx.Title,
                    Url = $"https://reddit.com{imgx.Permalink}",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = objx.extension
                    }
                };
                await ReplyAsync(objx.url);
                await ReplyAsync("", false, embedx.Build());
                checkcache.Hits++;
            }
            else
            {
                var r = new Reddit();
                var sub = r.GetSubreddit(subreddit);

                if (sub.NSFW)
                {
                    await ReplyAsync("Please use the NSFW Reddit command for NSFW Images");
                    return;
                }

                await ReplyAsync("Refreshing Cache");
                var num1 = sub.Hot.GetListing(150).Where(x => RedditHelper.isimage(x.Url.ToString()).isimage && !x.NSFW)
                    .ToList();
                var img = num1[rnd.Next(num1.Count)];
                var obj = RedditHelper.isimage(img.Url.ToString());
                var embed = new EmbedBuilder
                {
                    Title = img.Title,
                    Url = $"https://reddit.com{img.Permalink}",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = obj.extension
                    }
                };
                await ReplyAsync(obj.url);
                await ReplyAsync("", false, embed.Build());
                CommandHandler.SubReddits.RemoveAll(x =>
                    string.Equals(x.Title, subreddit, StringComparison.CurrentCultureIgnoreCase));
                CommandHandler.SubReddits.Add(new CommandHandler.SubReddit
                {
                    Title = subreddit,
                    LastUpdate = DateTime.UtcNow,
                    Posts = num1
                });
            }
        }


        /*[Command("youtube", RunMode = RunMode.Async)]
        [Summary("youtube <search>")]
        [Alias("yt")]
        [Remarks("Gives the first youtube result from the given search terms")]
        public async Task Youtube(string search)
        {
            var ytc = new YoutubeClient();
            await ReplyAsync($"Trying to get the top three results for the search: **{search}**\n" +
                             "Please be patient... <3");
            var searchList = await ytc.SearchAsync(search);
            var results = searchList.Take(3);
            var embed = new EmbedBuilder();
            foreach (var item in results)
            {
                var video = await ytc.GetVideoInfoAsync(item);
                embed.AddField(video.Title, $"https://www.youtube.com/watch?v={video.Id}");
            }
            await ReplyAsync("", false, embed.Build());
        }*/

        [Command("meme")]
        [Summary("meme")]
        [Alias("memes")]
        [Remarks("Dankness ( ͡° ͜ʖ ͡°)")]
        public async Task Meme()
        {
            var str = MemeStr.Meme;
            var rnd = new Random();
            var result = rnd.Next(0, str.Length);

            var builder = new EmbedBuilder()
                .WithImageUrl(str[result])
                .WithFooter(x =>
                {
                    x.WithText($"PassiveBOT | {result}/{str.Length}");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, builder.Build());
        }

        [Command("xkcd", RunMode = RunMode.Async)]
        [Summary("xkcd <latest/number>")]
        [Remarks("Get a rancom xkcd post")]
        public async Task xkcd(string number = null)
        {
            var random = new Random();
            using (var http = new HttpClient())
            {
                string res;
                if (number == "latest")
                    res = await http.GetStringAsync($"https://xkcd.com/info.0.json").ConfigureAwait(false);
                else if (int.TryParse(number, out var result))
                    res = await http.GetStringAsync($"https://xkcd.com/{result}/info.0.json").ConfigureAwait(false);
                else
                    res = await http.GetStringAsync($"https://xkcd.com/{random.Next(1, 1921)}/info.0.json")
                        .ConfigureAwait(false);

                var comic = JsonConvert.DeserializeObject<XkcdComic>(res);
                var embed = new EmbedBuilder().WithColor(Color.Blue)
                    .WithImageUrl(comic.ImageLink)
                    .WithTitle($"{comic.Title}")
                    .WithUrl($"https://xkcd.com/{comic.Num}")
                    .AddField("Comic Number", $"#{comic.Num}", true)
                    .AddField("Date", $"{comic.Month}/{comic.Year}", true);
                var sent = await ReplyAsync("", false, embed.Build());

                await Task.Delay(10000).ConfigureAwait(false);

                await sent.ModifyAsync(m =>
                    m.Embed = embed.AddField(efb =>
                        efb.WithName("Alt").WithValue(comic.Alt.ToString()).WithIsInline(false)).Build());
            }
        }

        [Command("dog")]
        [Summary("dog")]
        [Remarks("Gets a random dog image from random.dog")]
        public async Task Doggo2()
        {
            var woof = "http://random.dog/" + await SearchHelper.GetResponseStringAsync("https://random.dog/woof")
                           .ConfigureAwait(false);
            var embed = new EmbedBuilder()
                .WithImageUrl(woof)
                .WithTitle("Woof")
                .WithUrl(woof)
                .WithFooter(x =>
                {
                    x.WithText("PassiveBOT");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });
            await ReplyAsync("", false, embed.Build());
        }

        /*
        [Command("cat")]
        [Summary("Gets a random dog image from random.cat")]
        public async Task Kitty2()
        {
            var xDoc = JsonConvert.DeserializeXNode(await new HttpClient().GetStringAsync("http://aws.random.cat/meow"),
                "root");
            var embed = new EmbedBuilder()
                .WithImageUrl(xDoc.Element("root")?.Element("file")?.Value)
                .WithTitle("Meow")
                .WithUrl(xDoc.Element("root")?.Element("file")?.Value)
                .WithFooter(x =>
                {
                    x.WithText("PassiveBOT");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, embed.Build());
        }
        */
        [Command("salt")]
        [Summary("salt <@user>")]
        [Alias("salty")]
        [Remarks("For salty people")]
        public async Task Salt([Optional] IUser user)
        {
            if (user == null)
            {
                var str = MemeStr.Salt;
                var rnd = new Random();
                var result = rnd.Next(0, str.Length);

                var builder = new EmbedBuilder()
                    .WithImageUrl(str[result])
                    .WithFooter(x =>
                    {
                        x.WithText($"PassiveBOT | {result}/{str.Length}");
                        x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                    });

                await ReplyAsync("", false, builder.Build());
            }
            else
            {
                var str = MemeStr.Salt;
                var rnd = new Random();
                var result = rnd.Next(0, str.Length);

                var builder = new EmbedBuilder()
                    .WithTitle($"{user.Username} is a salty ass kid")
                    .WithImageUrl(str[result])
                    .WithFooter(x =>
                    {
                        x.WithText($"PassiveBOT | {result}/{str.Length}");
                        x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                    });

                await ReplyAsync("", false, builder.Build());
            }
        }

        [Command("leet")]
        [Summary("leet")]
        [Alias("mlg", "1337", "dank", "doritos", "illuminati", "memz", "pro")]
        [Remarks("MLG  Dankness")]
        public async Task Leet()
        {
            var str = MemeStr.Mlg;
            var rnd = new Random();
            var result = rnd.Next(0, str.Length);

            var builder = new EmbedBuilder()
                .WithImageUrl(str[result])
                .WithFooter(x =>
                {
                    x.WithText($"PassiveBOT | {result}/{str.Length}");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, builder.Build());
        }

        [Command("spoonfed")]
        [Summary("spoonfed")]
        [Remarks("for those who just ask for code")]
        public async Task Spoonfed()
        {
            var str = MemeStr.Spoon;
            var rnd = new Random();
            var result = rnd.Next(0, str.Length);

            var builder = new EmbedBuilder()
                .WithImageUrl(str[result])
                .WithFooter(x =>
                {
                    x.WithText($"PassiveBOT | {result}/{str.Length}");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, builder.Build());
        }

        [Command("derp")]
        [Summary("derp <@user>")]
        [Remarks("For special people (note: may be offensive to some)")]
        public async Task Derp([Remainder] [Optional] IUser user)
        {
            var title = user == null
                ? "You suffer from an extreme case of autism"
                : $"{user.Username} suffers from extreme cases of autism";

            var str = MemeStr.Autism;
            var rnd = new Random();
            var result = rnd.Next(0, str.Length);

            var builder = new EmbedBuilder()
                .WithTitle($"{title}")
                .WithImageUrl(str[result])
                .WithFooter(x =>
                {
                    x.WithText($"PassiveBOT | {result}/{str.Length}");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, builder.Build());
        }

        [Command("urbandictionary")]
        [Summary("urbandictionary <word>")]
        [Remarks("Search Urban Dictioanry")]
        public async Task Urban([Remainder] string word)
        {
            using (var http = new HttpClient())
            {
                var res = await http.GetStringAsync($"http://api.urbandictionary.com/v0/define?term={word}").ConfigureAwait(false);
                var resobj = JsonConvert.DeserializeObject<UrbanDict>(res);
                if (resobj.result_type == "no_results")
                {
                    await ReplyAsync("This word has no definition");
                    return;;
                }

                var topres = resobj.list.OrderByDescending(x => x.thumbs_up).First();
                if (topres.definition.Length > 1024)
                {
                    topres.definition = topres.definition.Substring(0, 1020) + "...";
                }
                if (topres.example.Length > 1024)
                {
                    topres.example = topres.example.Substring(0, 1020) + "...";
                }
                var emb = new EmbedBuilder
                {
                    Title = topres.word,
                    Color = Color.LightOrange
                }.AddField("Definition", $"{topres.definition}", true)
                    .AddField("Example", $"{topres.example}", true)
                    .AddField("Votes", $"^ [{topres.thumbs_up}] v [{topres.thumbs_down}]");
                await ReplyAsync("", false, emb.Build());
            }
        }

        public class XkcdComic
        {
            public int Num { get; set; }
            public string Month { get; set; }
            public string Year { get; set; }

            [JsonProperty("safe_title")]
            public string Title { get; set; }

            [JsonProperty("img")]
            public string ImageLink { get; set; }

            public string Alt { get; set; }
        }
    }
}