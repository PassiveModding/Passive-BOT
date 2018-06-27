namespace PassiveBOT.Discord.Extensions.PassiveBOT
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    /// <summary>
    /// The nsfw helper.
    /// </summary>
    public class NsfwHelper
    {
        /// <summary>
        /// The nsfw type.
        /// </summary>
        public enum NsfwType
        {
            Rule34,
            Yandere,
            Gelbooru,
            Konachan,
            Danbooru,
            Cureninja
        }
                
        /// <summary>
        /// The hentai async.
        /// </summary>
        /// <param name="random">
        /// The random.
        /// </param>
        /// <param name="nsfwType">
        /// The nsfw type.
        /// </param>
        /// <param name="tags">
        /// The tags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static async Task<string> HentaiAsync(Random random, NsfwType nsfwType,
            List<string> tags)
        {
            string url = null;
            string result;
            tags = !tags.Any() ? new[] { "boobs", "tits", "ass", "sexy", "neko" }.ToList() : tags;
            switch (nsfwType)
            {
                case NsfwType.Danbooru:
                    url = $"http://danbooru.donmai.us/posts?page={random.Next(0, 15)}{string.Join("+", tags.Select(x => x.Replace(" ", "_")))}";
                    break;
                case NsfwType.Gelbooru:
                    url = $"http://gelbooru.com/index.php?page=dapi&s=post&q=index&limit=100&tags={string.Join("+", tags.Select(x => x.Replace(" ", "_")))}";
                    break;
                case NsfwType.Rule34:
                    url = $"http://rule34.xxx/index.php?page=dapi&s=post&q=index&limit=100&tags={string.Join("+", tags.Select(x => x.Replace(" ", "_")))}";
                    break;
                case NsfwType.Cureninja:
                    url = $"https://cure.ninja/booru/api/json?f=a&o=r&s=1&q={string.Join("+", tags.Select(x => x.Replace(" ", "_")))}";
                    break;
                case NsfwType.Konachan:
                    url = $"http://konachan.com/post?page={random.Next(0, 5)}&tags={string.Join("+", tags.Select(x => x.Replace(" ", "_")))}";
                    break;
                case NsfwType.Yandere:
                    url = $"https://yande.re/post.xml?limit=25&page={random.Next(0, 15)}&tags={string.Join("+", tags.Select(x => x.Replace(" ", "_")))}";
                    break;
            }

            var matches = await GetMatchesAsync(nsfwType, url);

            switch (nsfwType)
            {
                case NsfwType.Danbooru:
                    result = $"http://danbooru.donmai.us/{matches[random.Next(matches.Count)].Groups[1].Value}";
                    break;
                case NsfwType.Konachan:
                case NsfwType.Gelbooru:
                case NsfwType.Yandere:
                case NsfwType.Rule34:
                    result = $"{matches[random.Next(matches.Count)].Groups[1].Value}";
                    break;
                case NsfwType.Cureninja:
                    result = matches[random.Next(matches.Count)].Groups[1].Value.Replace("\\/", "/");
                    break;
                default:
                    return null;
            }

            result = result.EndsWith("/") ? result.Substring(0, result.Length - 1) : result;
            return result;
        }

        /// <summary>
        /// Gets matches from the URL
        /// </summary>
        /// <param name="nsfwType">
        /// The nsfw type.
        /// </param>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static async Task<MatchCollection> GetMatchesAsync(NsfwType nsfwType, string url)
        {
            using (var client = new HttpClient())
            {
                MatchCollection matches;
                var get = await client.GetStringAsync(url).ConfigureAwait(false);
                switch (nsfwType)
                {
                    case NsfwType.Danbooru:
                        matches = Regex.Matches(get, "data-large-file-url=\"(.*)\"");
                        break;
                    case NsfwType.Yandere:
                    case NsfwType.Gelbooru:
                    case NsfwType.Rule34:
                        matches = Regex.Matches(get, "file_url=\"(.*?)\" ");
                        break;
                    case NsfwType.Cureninja:
                        matches = Regex.Matches(get, "\"url\":\"(.*?)\"");
                        break;
                    case NsfwType.Konachan:
                        matches = Regex.Matches(get, "<a class=\"directlink smallimg\" href=\"(.*?)\"");
                        break;
                    default:
                        matches = Regex.Matches(get, "\"url\":\"(.*?)\"");
                        break;
                }

                if (!matches.Any())
                {
                    return null;
                }

                return matches;
            }
        }
    }
}
