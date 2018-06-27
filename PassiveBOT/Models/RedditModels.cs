namespace PassiveBOT.Models
{
    using System;
    using System.Collections.Generic;

    using RedditSharp.Things;

    /// <summary>
    /// The reddit models.
    /// </summary>
    public class RedditModels
    {
        /// <summary>
        /// Gets or sets sub reddits.
        /// </summary>
        public static List<SubReddit> SubReddits { get; set; } = new List<SubReddit>();

        /// <summary>
        /// The subreddit module
        /// </summary>
        public class SubReddit
        {
            /// <summary>
            /// Gets or sets the title.
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// Gets or sets the posts.
            /// </summary>
            public List<Post> Posts { get; set; }

            /// <summary>
            /// Gets or sets the last update.
            /// </summary>
            public DateTime LastUpdate { get; set; }

            /// <summary>
            /// Gets or sets the hits.
            /// </summary>
            public int Hits { get; set; } = 0;

            /// <summary>
            /// Gets or sets a value indicating whether nsfw.
            /// </summary>
            public bool NSFW { get; set; } = false;
        }
    }
}