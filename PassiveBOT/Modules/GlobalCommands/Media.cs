namespace PassiveBOT.Modules.GlobalCommands
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Commands;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json;

    using PassiveBOT.Context;
    using PassiveBOT.Extensions;
    using PassiveBOT.Extensions.PassiveBOT;
    using PassiveBOT.Modules.GlobalCommands.MediaModels;
    using PassiveBOT.Preconditions;

    using RedditSharp;
    using RedditSharp.Things;

    /// <summary>
    ///     The media.
    /// </summary>
    public class Images : Base
    {
        /// <summary>
        ///     Gets a random dog image
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("dog")]
        [Summary("Gets a random dog image from random.dog")]
        public async Task DogAsync()
        {
            var woof = "http://random.dog/" + await SearchHelper.GetResponseStringAsync("https://random.dog/woof").ConfigureAwait(false);
            var embed = new EmbedBuilder().WithImageUrl(woof).WithTitle("Woof").WithUrl(woof);
            await ReplyAsync(embed.Build());
        }

        /// <summary>
        ///     gets random post from specified sub
        /// </summary>
        /// <param name="subreddit">
        ///     The subreddit.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        /// <exception cref="Exception">
        ///     throws if it is nsfw
        /// </exception>
        [Command("RedditPost", RunMode = RunMode.Async)]
        [Summary("Get a random post from first 25 in hot of a sub")]
        public async Task GetPostAsync(string subreddit)
        {
            var checkCache = RedditModels.SubReddits.FirstOrDefault(x => string.Equals(x.Title, subreddit, StringComparison.CurrentCultureIgnoreCase));
            var rnd = new Random();
            if (checkCache != null && checkCache.LastUpdate > DateTime.UtcNow - TimeSpan.FromHours(6))
            {
                if (checkCache.NSFW)
                {
                    throw new Exception("This command cannot be used on NSFW subreddits.");
                }

                var image = checkCache.Posts[rnd.Next(checkCache.Posts.Count)];
                await ReplyAsync($"{image.Title}\nhttps://reddit.com{image.Permalink}");
                checkCache.Hits++;
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
                RedditModels.SubReddits.RemoveAll(x => string.Equals(x.Title, subreddit, StringComparison.CurrentCultureIgnoreCase));
                RedditModels.SubReddits.Add(new RedditModels.SubReddit { Title = subreddit, LastUpdate = DateTime.UtcNow, Posts = num1 });
            }
        }

        /// <summary>
        ///     The reddit img.
        /// </summary>
        /// <param name="subreddit">
        ///     The subreddit.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        /// <exception cref="Exception">
        ///     throws if nsfw or no sub
        /// </exception>
        [Command("RedditImage", RunMode = RunMode.Async)]
        [Alias("rimg", "rimage")]
        [Summary("Get a random post from first 25 in hot of a sub")]
        public async Task RedditIMGAsync(string subreddit)
        {
            if (subreddit == null)
            {
                throw new Exception("Please give a subreddit to browse.");
            }

            var cache = RedditModels.SubReddits.FirstOrDefault(x => string.Equals(x.Title, subreddit, StringComparison.CurrentCultureIgnoreCase));
            if (cache != null && cache.LastUpdate > DateTime.UtcNow - TimeSpan.FromHours(6))
            {
                if (cache.NSFW)
                {
                    throw new Exception("This command is for non NSFW Subreddits.");
                }

                var image = cache.Posts[Context.Provider.GetRequiredService<Random>().Next(cache.Posts.Count)];
                var isImage = RedditHelper.IsImage(image.Url.ToString());
                var embed = new EmbedBuilder { Title = image.Title, Url = $"https://reddit.com{image.Permalink}", Footer = new EmbedFooterBuilder { Text = isImage.Extension } };
                await ReplyAsync(isImage.Url);
                await ReplyAsync(embed.Build());
                cache.Hits++;
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
                var num1 = await sub.GetTop(FromTime.Week, 150).Where(x => RedditHelper.IsImage(x.Url.ToString()).IsImage && !x.NSFW).ToList();
                var img = num1[Context.Provider.GetRequiredService<Random>().Next(num1.Count)];
                var obj = RedditHelper.IsImage(img.Url.ToString());
                var embed = new EmbedBuilder { Title = img.Title, Url = $"https://reddit.com{img.Permalink}", Footer = new EmbedFooterBuilder { Text = obj.Extension } };
                await ReplyAsync(obj.Url);
                await ReplyAsync(embed.Build());
                RedditModels.SubReddits.RemoveAll(x => string.Equals(x.Title, subreddit, StringComparison.CurrentCultureIgnoreCase));
                RedditModels.SubReddits.Add(new RedditModels.SubReddit { Title = subreddit, LastUpdate = DateTime.UtcNow, Posts = num1 });
            }
        }

        /// <summary>
        ///     The urban dictionary command
        /// </summary>
        /// <param name="word">
        ///     The word.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [RequireNsfw]
        [NsfwAllowed]
        [Command("urbanDictionary")]
        [Summary("Search Urban Dictionary")]
        public async Task UrbanAsync([Remainder] string word)
        {
            using (var http = new HttpClient())
            {
                var res = await http.GetStringAsync($"http://api.urbandictionary.com/v0/define?term={word}").ConfigureAwait(false);
                var model = JsonConvert.DeserializeObject<UrbanDictModel>(res);
                if (model.result_type == "no_results")
                {
                    await ReplyAsync("This word has no definition");
                    return;
                }

                var mostVoted = model.list.OrderByDescending(x => x.thumbs_up).First();
                if (mostVoted.definition.Length > 1024)
                {
                    mostVoted.definition = mostVoted.definition.Substring(0, 1020) + "...";
                }

                if (mostVoted.example.Length > 1024)
                {
                    mostVoted.example = mostVoted.example.Substring(0, 1020) + "...";
                }

                var emb = new EmbedBuilder { Title = mostVoted.word, Color = Color.LightOrange }.AddField("Definition", $"{mostVoted.definition}", true).AddField("Example", $"{mostVoted.example}", true).AddField("Votes", $"^ [{mostVoted.thumbs_up}] v [{mostVoted.thumbs_down}]");
                await ReplyAsync(emb.Build());
            }
        }

        /// <summary>
        ///     The xkcd.
        /// </summary>
        /// <param name="number">
        ///     The number.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("xkcd", RunMode = RunMode.Async)]
        [Summary("Get a random xkcd post")]
        public async Task XkcdAsync(string number = null)
        {
            var random = new Random();
            using (var http = new HttpClient())
            {
                string res;
                if (number == "latest")
                {
                    res = await http.GetStringAsync("https://xkcd.com/info.0.json").ConfigureAwait(false);
                }
                else if (int.TryParse(number, out var result))
                {
                    res = await http.GetStringAsync($"https://xkcd.com/{result}/info.0.json").ConfigureAwait(false);
                }
                else
                {
                    res = await http.GetStringAsync($"https://xkcd.com/{random.Next(1, 1921)}/info.0.json").ConfigureAwait(false);
                }

                var comic = JsonConvert.DeserializeObject<XkcdComic>(res);
                var embed = new EmbedBuilder().WithColor(Color.Blue).WithImageUrl(comic.ImageLink).WithTitle($"{comic.Title}").WithUrl($"https://xkcd.com/{comic.Num}").AddField("Comic Number", $"#{comic.Num}", true).AddField("Date", $"{comic.Month}/{comic.Year}", true);
                var sent = await ReplyAsync(embed.Build());

                await Task.Delay(10000).ConfigureAwait(false);

                await sent.ModifyAsync(m => m.Embed = embed.AddField(efb => efb.WithName("Alt").WithValue(comic.Alt.ToString()).WithIsInline(false)).Build());
            }
        }
    }
}