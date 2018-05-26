using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using PassiveBOT.Discord.Context;
using PassiveBOT.Handlers;
using PassiveBOT.Models;
using RedditSharp;
using RedditSharp.Things;

namespace PassiveBOT.Modules.Data
{
    public class Media : Base
    {
        [Command("RedditPost", RunMode = RunMode.Async)]
        [Summary("RedditPost <sub>")]
        [Remarks("Get a random post from first 25 in hot of a sub")]
        public async Task GetRPost(string subreddit)
        {
            var checkcache = RedditModels.SubReddits.FirstOrDefault(x => string.Equals(x.Title, subreddit, StringComparison.CurrentCultureIgnoreCase));
            var rnd = new Random();
            if (checkcache != null && checkcache.LastUpdate > DateTime.UtcNow - TimeSpan.FromHours(6))
            {
                if (checkcache.NSFW)
                {
                    throw new Exception("This command cannot be used on NSFW subreddits.");
                }

                var imgx = checkcache.Posts[rnd.Next(checkcache.Posts.Count)];
                await ReplyAsync($"{imgx.Title}\nhttps://reddit.com{imgx.Permalink}");
                checkcache.Hits++;
            }
            else
            {
                var r = new Reddit();
                var sub = await r.GetSubredditAsync(subreddit);
                if (sub.NSFW == true)
                {
                    throw new Exception("This Command is for non NSFW subreddits");
                }
                await ReplyAsync("Refreshing Cache");
                var num1 = await sub.GetTop(FromTime.Week, 25).Where(x => !x.NSFW).ToList();
                var post = num1[rnd.Next(24)];
                await ReplyAsync($"{post.Title}\nhttps://reddit.com{post.Permalink}");
                RedditModels.SubReddits.RemoveAll(x =>
                    string.Equals(x.Title, subreddit, StringComparison.CurrentCultureIgnoreCase));
                RedditModels.SubReddits.Add(new RedditModels.SubReddit
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
        public async Task RedditIMG(string subreddit)
        {
            if (subreddit == null) await ReplyAsync("Please give a subreddit to browse.");
            var rnd = new Random();
            var checkcache = RedditModels.SubReddits.FirstOrDefault(x => string.Equals(x.Title, subreddit, StringComparison.CurrentCultureIgnoreCase));
            if (checkcache != null && checkcache.LastUpdate > DateTime.UtcNow - TimeSpan.FromHours(6))
            {
                if (checkcache.NSFW)
                {
                    throw new Exception("This command is for non NSFW Subreddits.");
                }
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
                var sub = await r.GetSubredditAsync(subreddit);

                if (sub.NSFW == true)
                {
                    await ReplyAsync("Please use the NSFW Reddit command for NSFW Images");
                    return;
                }

                await ReplyAsync("Refreshing Cache");
                var num1 = await sub.GetTop(FromTime.Week, 150).Where(x => RedditHelper.isimage(x.Url.ToString()).isimage && !x.NSFW).ToList();
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
                RedditModels.SubReddits.RemoveAll(x =>
                    string.Equals(x.Title, subreddit, StringComparison.CurrentCultureIgnoreCase));
                RedditModels.SubReddits.Add(new RedditModels.SubReddit
                {
                    Title = subreddit,
                    LastUpdate = DateTime.UtcNow,
                    Posts = num1
                });
            }
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
        [Command("urbandictionary")]
        [Summary("urbandictionary <word>")]
        [Remarks("Search Urban Dictioanry")]
        public async Task Urban([Remainder] string word)
        {
            using (var http = new HttpClient())
            {
                var res = await http.GetStringAsync($"http://api.urbandictionary.com/v0/define?term={word}").ConfigureAwait(false);
                var resobj = JsonConvert.DeserializeObject<UrbanDictModel.UrbanDict>(res);
                if (resobj.result_type == "no_results")
                {
                    await ReplyAsync("This word has no definition");
                    return; ;
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
