using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PassiveBOT.Handlers
{
    public enum RequestHttpMethod
    {
        Get,
        Post
    }

    public static class RedditHelper
    {
        public class isimg
        {
            public string url { get; set; }
            public bool isimage { get; set; }
            public string extension { get; set; }
        }

        public static isimg isimage(string urli)
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
    }


    public static class SearchHelper
    {
        public static async Task<Stream> GetResponseStreamAsync(string url,
            IEnumerable<KeyValuePair<string, string>> headers = null, RequestHttpMethod method = RequestHttpMethod.Get)
        {
            var cl = new HttpClient();
            cl.DefaultRequestHeaders.Clear();
            switch (method)
            {
                case RequestHttpMethod.Get:
                    if (headers == null) return await cl.GetStreamAsync(url).ConfigureAwait(false);
                    foreach (var header in headers)
                        cl.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                    return await cl.GetStreamAsync(url).ConfigureAwait(false);
                case RequestHttpMethod.Post:
                    FormUrlEncodedContent formContent = null;
                    if (headers != null)
                        formContent = new FormUrlEncodedContent(headers);
                    var message = await cl.PostAsync(url, formContent).ConfigureAwait(false);
                    return await message.Content.ReadAsStreamAsync().ConfigureAwait(false);
                default:
                    throw new NotImplementedException("That type of request is unsupported.");
            }
        }

        public static async Task<string> GetResponseStringAsync(string url,
            IEnumerable<KeyValuePair<string, string>> headers = null,
            RequestHttpMethod method = RequestHttpMethod.Get)
        {
            using (var streamReader =
                new StreamReader(await GetResponseStreamAsync(url, headers, method).ConfigureAwait(false)))
            {
                return await streamReader.ReadToEndAsync().ConfigureAwait(false);
            }
        }
    }
}