// ReSharper disable StringLiteralTypo
namespace PassiveBOT.Modules.GuildCommands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Addons.Interactive;
    using Discord.Commands;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json.Linq;

    using PassiveBOT.Context;
    using PassiveBOT.Extensions.PassiveBOT;
    using PassiveBOT.Preconditions;
    using PassiveBOT.Models;

    using RedditSharp;
    using RedditSharp.Things;

    /// <summary>
    /// The NSFW Commands
    /// </summary>
    [NsfwAllowed]
    [RequireNsfw]
    [Summary("🔞Pornographic imagery")]
    public class NSFW : Base
    {
        /// <summary>
        /// The reddit nsfw.
        /// </summary>
        /// <param name="subreddit">
        /// The subreddit.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("RedditNSFW", RunMode = RunMode.Async)]
        [Summary("Get a random post from first 150 in hot of a sub")]
        public async Task RedditNSFWAsync(string subreddit = null)
        {
            if (subreddit == null)
            {
                await ReplyAsync("Please give a subreddit to browse.");
                return;
            }

            var rnd = new Random();
            List<Post> posts;
            var checkCache = RedditModels.SubReddits.FirstOrDefault(x =>
                string.Equals(x.Title, subreddit, StringComparison.CurrentCultureIgnoreCase));
            if (checkCache != null && checkCache.LastUpdate > DateTime.UtcNow - TimeSpan.FromHours(6))
            {
                posts = checkCache.Posts;
                checkCache.Hits++;
            }
            else
            {
                var r = new Reddit();
                var sub = await r.GetSubredditAsync(subreddit);

                await ReplyAsync("Refreshing Cache");
                posts = await sub.GetTop(FromTime.Week, 150).Where(x => RedditHelper.IsImage(x.Url.ToString()).IsImage).ToList();
                RedditModels.SubReddits.RemoveAll(x =>
                    string.Equals(x.Title, subreddit, StringComparison.CurrentCultureIgnoreCase));
                RedditModels.SubReddits.Add(new RedditModels.SubReddit
                {
                    Title = subreddit,
                    LastUpdate = DateTime.UtcNow,
                    Posts = posts,
                    NSFW = true
                });
            }

            var img = posts[rnd.Next(posts.Count - 1)];
            var obj = RedditHelper.IsImage(img.Url.ToString());
            var embed = new EmbedBuilder
            {
                Title = img.Title,
                Url = $"https://reddit.com{img.Permalink}",
                Footer = new EmbedFooterBuilder
                {
                    Text = obj.Extension
                }
            };
            await ReplyAsync(obj.Url);
            await ReplyAsync(string.Empty, false, embed.Build());
        }

        /// <summary>
        /// The browse reddit nsfw.
        /// </summary>
        /// <param name="subreddit">
        /// The subreddit.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("BrowseRedditNSFW", RunMode = RunMode.Async)]
        [Summary("Get a random post from first 150 in hot of a sub")]
        public async Task BRedditNSFWAsync(string subreddit = null)
        {
            if (subreddit == null)
            {
                await ReplyAsync("Please give a subreddit to browse.");
                return;
            }

            var subObject = RedditModels.SubReddits.FirstOrDefault(x =>
                string.Equals(x.Title, subreddit, StringComparison.CurrentCultureIgnoreCase));
            List<Post> posts;
            if (subObject != null && subObject.LastUpdate > DateTime.UtcNow - TimeSpan.FromHours(6))
            {
                // just post
                posts = subObject.Posts;
                subObject.Hits++;
            }
            else
            {
                // get images then post
                var r = new Reddit();
                var sub = await r.GetSubredditAsync(subreddit);
                await ReplyAsync("Refreshing Cache");
                posts = await sub.GetTop(FromTime.Week, 150).Where(x => RedditHelper.IsImage(x.Url.ToString()).IsImage).ToList();
                RedditModels.SubReddits.RemoveAll(x =>
                    string.Equals(x.Title, subreddit, StringComparison.CurrentCultureIgnoreCase));
                RedditModels.SubReddits.Add(new RedditModels.SubReddit
                {
                    Title = subreddit,
                    LastUpdate = DateTime.UtcNow,
                    Posts = posts,
                    Hits = 0,
                    NSFW = true
                });
            }

            // post 
            var pages = new List<PaginatedMessage.Page>();
            foreach (var image in posts.OrderByDescending(x => new Random().Next()))
            {
                var imageObject = RedditHelper.IsImage(image.Url.ToString());
                if (imageObject.IsImage && !imageObject.Url.Contains("gfy"))
                {
                    pages.Add(new PaginatedMessage.Page { ImageUrl = imageObject.Url, Description = $"{imageObject.Extension} || [Link](https://reddit.com{image.Permalink})", Title = image.Title });
                }
            }

            var msg = new PaginatedMessage
            {
                Title = $"{subreddit} Images",
                Pages = pages,
                Color = new Color(114, 137, 218)
            };

            await PagedReplyAsync(msg, new ReactionList
                                           {
                                               Forward = true,
                                               Backward = true,
                                               Trash = true
                                           });
        }

        /// <summary>
        /// The boobs async.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("tits", RunMode = RunMode.Async)]
        [Alias("boobs", "rack")]
        [Summary("Fetches some sexy titties")]
        public async Task BoobsAsync()
        {
            JToken obj;
            var rnd = new Random().Next(0, 10229);
            using (var http = new HttpClient())
            {
                obj = JArray.Parse(await http.GetStringAsync($"http://api.oboobs.ru/boobs/{rnd}"))[0];
            }

            var builder = new EmbedBuilder
            {
                ImageUrl = $"http://media.oboobs.ru/{obj["preview"]}",
                Description = $"Tits Database Size: 10229\n Image Number: {rnd}",
                Title = "Tits",
                Url = $"http://adult.passivenation.com/18217229/http://media.oboobs.ru/{obj["preview"]}"
            };

            await ReplyAsync(string.Empty, false, builder.Build());
        }

        /// <summary>
        /// The bums async.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("Ass", RunMode = RunMode.Async)]
        [Summary("Sexy Ass!")]
        public async Task BumsAsync()
        {
            JToken obj;
            var rnd = new Random().Next(0, 4222);
            using (var http = new HttpClient())
            {
                obj = JArray.Parse(await http.GetStringAsync($"http://api.obutts.ru/butts/{rnd}"))[0];
            }

            var builder = new EmbedBuilder
            {
                ImageUrl = $"http://media.obutts.ru/{obj["preview"]}",
                Description = $"Ass Database Size: 4222\n Image Number: {rnd}",
                Title = "Ass",
                Url = $"http://adult.passivenation.com/18217229/http://media.obutts.ru/{obj["preview"]}"
            };
            await ReplyAsync(string.Empty, false, builder.Build());
        }

        /// <summary>
        /// The porn.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("nsfw", RunMode = RunMode.Async)]
        [Alias("nude", "porn")]
        [Summary("Sexy Stuff!")]
        public Task PornAsync()
        {
            return RedditNSFWAsync("nsfw");
        }

        /// <summary>
        /// The pussy.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("pussy", RunMode = RunMode.Async)]
        [Summary(";)")]
        public Task PussyAsync()
        {
            var rnd = new Random();
            var subs = new[]
            {
                "grool",
                "creampies",
                "creampie",
                "creampiegifs",
                "pussyjobs",
                "pussyslip",
                "upskirt",
                "pussy",
                "rearpussy",
                "simps",
                "vagina",
                "moundofvenus"
            };
            return BRedditNSFWAsync(subs[rnd.Next(subs.Length - 1)]);
        }

        /// <summary>
        /// The NsfwGif.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("NsfwGif", RunMode = RunMode.Async)]
        [Summary("Gifs")]
        public Task NsfwGifAsync()
        {
            var rnd = new Random();
            var subs = new[]
            {
                "nsfwgif",
                "booty_gifs",
                "boobgifs",
                "creampiegifs",
                "pussyjobs",
                "gifsgonewild",
                "nsfw_gif",
                "nsfw_gifs",
                "porn_gifs",
                "adultgifs"
            };
            return BRedditNSFWAsync(subs[rnd.Next(subs.Length - 1)]);
        }

        /// <summary>
        /// The r 34.
        /// </summary>
        /// <param name="tags">
        /// The tags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("Rule34", RunMode = RunMode.Async)]
        [Alias("R34")]
        [Summary("Search Rule34 Porn using tags")]
        public async Task R34Async(params string[] tags)
        {
            var result = await NsfwHelper.HentaiAsync(Context.Provider.GetRequiredService<Random>(), NsfwHelper.NsfwType.Rule34, tags.ToList());
            if (result == null)
            {
                await ReplyAsync("No Results.");
            }
            else
            {
                var embed = new EmbedBuilder
                {
                    ImageUrl = result,
                    Title = "View On Site [R34]",
                                    Url = $"http://adult.passivenation.com/18217229/{result}",
                                    Footer = new EmbedFooterBuilder { Text = string.Join(", ", tags) }
                                };
                await ReplyAsync(string.Empty, false, embed.Build());
            }
        }

        /// <summary>
        /// The yandere.
        /// </summary>
        /// <param name="tags">
        /// The tags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("Yandere", RunMode = RunMode.Async)]
        [Summary("Search Yandere Porn using tags")]
        public async Task YandereAsync(params string[] tags)
        {
            var result = await NsfwHelper.HentaiAsync(Context.Provider.GetRequiredService<Random>(), NsfwHelper.NsfwType.Yandere, tags.ToList());
            if (result == null)
            {
                await ReplyAsync("No Results.");
            }
            else
            {
                var embed = new EmbedBuilder
                {
                    ImageUrl = result,
                    Title = "View On Site [Yandere]",
                    Url = $"http://adult.passivenation.com/18217229/{result}",
                    Footer = new EmbedFooterBuilder()
                };
                embed.Footer.Text = string.Join(", ", tags);
                await ReplyAsync(string.Empty, false, embed.Build());
            }
        }

        /// <summary>
        /// The gelbooru.
        /// </summary>
        /// <param name="tags">
        /// The tags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("Gelbooru", RunMode = RunMode.Async)]
        [Summary("Search Gelbooru Porn using tags")]
        public async Task GelbooruAsync(params string[] tags)
        {
            var result = await NsfwHelper.HentaiAsync(Context.Provider.GetRequiredService<Random>(), NsfwHelper.NsfwType.Gelbooru, tags.ToList());
            if (result == null)
            {
                await ReplyAsync("No Results.");
            }
            else
            {
                var embed = new EmbedBuilder
                {
                    ImageUrl = result,
                    Title = "View On Site [Gelbooru]",
                                    Url = $"http://adult.passivenation.com/18217229/{result}",
                                    Footer = new EmbedFooterBuilder { Text = string.Join(", ", tags) }
                                };
                await ReplyAsync(string.Empty, false, embed.Build());
            }
        }

        /// <summary>
        /// The cureninja.
        /// </summary>
        /// <param name="tags">
        /// The tags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("Cureninja", RunMode = RunMode.Async)]
        [Summary("Search Cureninja Porn using tags")]
        public async Task CureninjaAsync(params string[] tags)
        {
            var result = await NsfwHelper.HentaiAsync(Context.Provider.GetRequiredService<Random>(), NsfwHelper.NsfwType.Cureninja, tags.ToList());
            if (result == null)
            {
                await ReplyAsync("No Results.");
            }
            else
            {
                var embed = new EmbedBuilder
                {
                    ImageUrl = result,
                    Title = "View On Site [Cureninja]",
                                    Url = $"http://adult.passivenation.com/18217229/{result}",
                                    Footer = new EmbedFooterBuilder { Text = string.Join(", ", tags) }
                                };
                await ReplyAsync(string.Empty, false, embed.Build());
            }
        }

        /// <summary>
        /// The konachan.
        /// </summary>
        /// <param name="tags">
        /// The tags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("Konachan", RunMode = RunMode.Async)]
        [Summary("Search Konachan Porn using tags")]
        public async Task KonachanAsync(params string[] tags)
        {
            var result = await NsfwHelper.HentaiAsync(Context.Provider.GetRequiredService<Random>(), NsfwHelper.NsfwType.Konachan, tags.ToList());
            if (result == null)
            {
                await ReplyAsync("No Results.");
            }
            else
            {
                var embed = new EmbedBuilder
                {
                    ImageUrl = result,
                    Title = "View On Site [Konachan]",
                                    Url = $"http://adult.passivenation.com/18217229/{result}",
                                    Footer = new EmbedFooterBuilder { Text = string.Join(", ", tags) }
                                };
                await ReplyAsync(string.Empty, false, embed.Build());
            }
        }
    }
}