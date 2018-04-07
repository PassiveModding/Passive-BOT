using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using PassiveBOT.Handlers;
using PassiveBOT.preconditions;

namespace PassiveBOT.Commands
{
    [Ratelimit(1, 3, Measure.Seconds)]
    public class Reddit : ModuleBase
    {
        [Command("GetRedditPost")]
        [Summary("GetRedditPost <sub>")]
        [Remarks("Get a random post from first 25 in hot of a sub")]
        public async Task RedditTask(string subreddit = null)
        {
            if (subreddit == null)
            {
                await ReplyAsync("Please give a subreddit to browse.");
            }

            var r = new RedditSharp.Reddit();
            var sub = r.GetSubreddit(subreddit);
            var rnd = new Random().Next(24);
            var num1 = sub.Hot.GetListing(25).ToList()[rnd];
            await ReplyAsync($"{num1.Title}\nhttps://reddit.com{num1.Permalink}");
        }

        [Command("RedditImage", RunMode = RunMode.Async)]
        [Summary("RedditImage <sub>")]
        [Alias("rimg", "rimage")]
        [Remarks("Get a random post from first 25 in hot of a sub")]
        public async Task RedditIMG(string subreddit = null)
        {
            if (subreddit == null)
            {
                await ReplyAsync("Please give a subreddit to browse.");
            }
            var rnd = new Random();
            var checkcache = CommandHandler.SubReddits.FirstOrDefault(x => String.Equals(x.title, subreddit, StringComparison.CurrentCultureIgnoreCase));
            if (checkcache != null)
            {
                if (checkcache.LastUpdate > DateTime.UtcNow - TimeSpan.FromHours(1))
                {
                    var imgx = checkcache.Posts[rnd.Next(checkcache.Posts.Count)];
                    var objx = isimage(imgx.Url.ToString());
                    var embedx = new EmbedBuilder
                    {
                        Title = imgx.Title,
                        Url = $"https://reddit.com{imgx.Permalink}",
                        Footer = new EmbedFooterBuilder
                        {
                            Text = objx.extension
                        }
                    };
                    await ReplyAsync(objx.url); await ReplyAsync("", false, embedx.Build());
                }
            }
            else
            {
                var r = new RedditSharp.Reddit();
                var sub = r.GetSubreddit(subreddit);
            
                if (sub.NSFW)
                {
                    await ReplyAsync("Please use the NSFW Reddit command for NSFW Images");
                    return;
                }

                var num1 = sub.Hot.GetListing(250).Where(x => isimage(x.Url.ToString()).isimage && !x.NSFW).ToList();
                var img = num1[rnd.Next(num1.Count)];
                var obj = isimage(img.Url.ToString());
                var embed = new EmbedBuilder
                {
                    Title = img.Title,
                    Url = $"https://reddit.com{img.Permalink}",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = obj.extension
                    }
                };
                await ReplyAsync(obj.url); await ReplyAsync("", false, embed.Build());

                CommandHandler.SubReddits.Add(new CommandHandler.SubReddit
                {
                    title = subreddit,
                    LastUpdate = DateTime.UtcNow,
                    Posts = num1
                });
            }

        }

        public class isimg
        {
            public string url { get; set; }
            public bool isimage { get; set; }
            public string extension { get; set; }
        }

        public isimg isimage(string urli)
        {
            var imgextensions = new List<string>
            {
                ".jpg",
                ".gif",
                ".webm",
                ".png",
                "gfycat",
                ".mp4"
            };

            if (!imgextensions.Any(ex => urli.ToLower().Contains(ex)))
                return new isimg
                {
                    extension = null,
                    isimage = false,
                    url = urli
                };

            var urli1 = urli;
            if (imgextensions.Find(ex => urli1.ToLower().Contains(ex)) == "gfycat")
            {
                urli = $"{urli.ToLower().Replace("gfycat.com", "zippy.gfycat.com")}.gif";
            }

            if (urli.EndsWith(".gifv"))
            {
                urli = urli.Replace(".gifv", ".gif");
            }
            return new isimg
            {
                extension = imgextensions.Find(ex => urli.ToLower().Contains(ex)),
                isimage = true,
                url = urli
            };
        }

        [Command("RedditNSFW", RunMode = RunMode.Async)]
        [Summary("RedditNSFW <sub>")]
        [Remarks("Get a random post from first 25 in hot of a sub")]
        public async Task RedditNSFW(string subreddit = null)
        {
            if (subreddit == null)
            {
                await ReplyAsync("Please give a subreddit to browse.");
            }
            if (subreddit == null)
            {
                await ReplyAsync("Please give a subreddit to browse.");
            }
            var rnd = new Random();
            var checkcache = CommandHandler.SubReddits.FirstOrDefault(x => String.Equals(x.title, subreddit, StringComparison.CurrentCultureIgnoreCase));
            if (checkcache != null)
            {
                if (checkcache.LastUpdate > DateTime.UtcNow - TimeSpan.FromHours(1))
                {
                    var imgx = checkcache.Posts[rnd.Next(checkcache.Posts.Count)];
                    var objx = isimage(imgx.Url.ToString());
                    var embedx = new EmbedBuilder
                    {
                        Title = imgx.Title,
                        Url = $"https://reddit.com{imgx.Permalink}",
                        Footer = new EmbedFooterBuilder
                        {
                            Text = objx.extension
                        }
                    };
                    await ReplyAsync(objx.url); await ReplyAsync("", false, embedx.Build());
                }
            }
            else
            {
                var r = new RedditSharp.Reddit();
                var sub = r.GetSubreddit(subreddit);

                if (sub.NSFW)
                {
                    await ReplyAsync("Please use the NSFW Reddit command for NSFW Images");
                    return;
                }

                var num1 = sub.Hot.GetListing(250).Where(x => isimage(x.Url.ToString()).isimage).ToList();
                var img = num1[rnd.Next(num1.Count)];
                var obj = isimage(img.Url.ToString());
                var embed = new EmbedBuilder
                {
                    Title = img.Title,
                    Url = $"https://reddit.com{img.Permalink}",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = obj.extension
                    }
                };
                await ReplyAsync(obj.url); await ReplyAsync("", false, embed.Build());

                CommandHandler.SubReddits.Add(new CommandHandler.SubReddit
                {
                    title = subreddit,
                    LastUpdate = DateTime.UtcNow,
                    Posts = num1
                });
            }
        }
    }
}
