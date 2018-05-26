using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RedditSharp.Things;

namespace PassiveBOT.Models
{
    public class RedditModels
    {

        public static List<SubReddit> SubReddits = new List<SubReddit>();
        public class SubReddit
        {
            public string Title { get; set; }
            public List<Post> Posts { get; set; }
            public DateTime LastUpdate { get; set; }
            public int Hits { get; set; } = 0;
        }
    }

    public static class RedditHelper
    {
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

            if (urli.EndsWith(".gifv")) urli = urli.Replace(".gifv", ".gif");
            return new isimg
            {
                extension = imgextensions.Find(ex => urli.ToLower().Contains(ex)),
                isimage = true,
                url = urli
            };
        }

        public class isimg
        {
            public string url { get; set; }
            public bool isimage { get; set; }
            public string extension { get; set; }
        }
    }

}
