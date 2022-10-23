using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzakiAnimeWPF.Pages.UserAccount
{

    public class UserAccountParse
    {
        public Favorites? Favorites { get; set; }
    }

    public class Favorites
    {
        public Anime? Anime { get; set; }
        public Manga? Manga { get; set; }
    }

    public class Anime
    {
        public Anilist[]? Anilist { get; set; }
    }

    public class Anilist
    {
        public string? AnimeId { get; set; } = "";
        public string? title { get; set; }
        public string? countryOfOrigin { get; set; }
        public string? Image { get; set; }
        public string? ReleaseDate { get; set; }
        public string? type { get; set; }
        public string? subOrdub { get; set; }
        public string? DateAdded { get; set; }
    }


    public class Manga
    {
        public Mangakakalot[]? Mangakakalot { get; set; }
    }

    public class Mangakakalot
    {
        public string? id { get; set; }
        public string? title { get; set; }
        public string? image { get; set; }
        public string? status { get; set; }
        public string? views { get; set; }
        public string[]? authors { get; set; }
    }

}
