namespace PassiveBOT.Extensions.PassiveBOT
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     The reddit helper.
    /// </summary>
    public static class RedditHelper
    {
        /// <summary>
        ///     The IsImage.
        /// </summary>
        /// <param name="url">
        ///     The url.
        /// </param>
        /// <returns>
        ///     The <see cref="IsImg" />.
        /// </returns>
        public static IsImg IsImage(string url)
        {
            var imgextensions = new List<string> { ".jpg", ".gif", ".webm", ".png", "gfycat", ".mp4" };

            if (!imgextensions.Any(ex => url.ToLower().Contains(ex)))
            {
                return new IsImg { Extension = null, IsImage = false, Url = url };
            }

            var url1 = url;
            if (imgextensions.Find(ex => url1.ToLower().Contains(ex)) == "gfycat")
            {
                url = $"{url.ToLower().Replace("gfycat.com", "zippy.gfycat.com")}.gif";
            }

            if (url.EndsWith(".gifv"))
            {
                url = url.Replace(".gifv", ".gif");
            }

            return new IsImg { Extension = imgextensions.Find(ex => url.ToLower().Contains(ex)), IsImage = true, Url = url };
        }

        /// <summary>
        ///     Image Info.
        /// </summary>
        public class IsImg
        {
            /// <summary>
            ///     Gets or sets the extension.
            /// </summary>
            public string Extension { get; set; }

            /// <summary>
            ///     Gets or sets a value indicating whether it is an image.
            /// </summary>
            public bool IsImage { get; set; }

            /// <summary>
            ///     Gets or sets the url.
            /// </summary>
            public string Url { get; set; }
        }
    }
}