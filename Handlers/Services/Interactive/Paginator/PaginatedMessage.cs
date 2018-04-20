using System.Collections.Generic;
using Discord;

namespace PassiveBOT.Handlers.Services.Interactive.Paginator
{
    public class PaginatedMessage
    {
        public IEnumerable<Page> Pages { get; set; }

        public string Content { get; set; } = "";
        public string Img { get; set; }
        public EmbedAuthorBuilder Author { get; set; } = null;
        public Color Color { get; set; } = Color.Default;
        public string Title { get; set; } = "";

        public PaginatedAppearanceOptions Options { get; set; } = PaginatedAppearanceOptions.Default;

        public class Page
        {
            public string description { get; set; } = "";
            public string imageurl { get; set; } = null;
            public string dynamictitle { get; set; } = null;
            public string titleURL { get; set; } = null;
        }
    }
}