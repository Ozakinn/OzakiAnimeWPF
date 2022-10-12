using System;
using System.Collections.Generic;
using System.Text;

namespace OzakiAnimeWPF.Data
{

    public class AnilistAdvancedSearch
    {
        public string? currentPage { get; set; }
        public string? hasNextPage { get; set; }
        public string? totalPages { get; set; }
        public string? totalResults { get; set; }
        public Result[]? results { get; set; }
    }

    public class Result
    {
        public string? id { get; set; }
        public string? malId { get; set; }
        public Title? title { get; set; }
        public string? status { get; set; }
        public string? image { get; set; }
        public string? cover { get; set; }
        public string? popularity { get; set; }
        public string? totalEpisodes { get; set; }
        public string? description { get; set; }
        public string[]? genres { get; set; }
        public string? rating { get; set; }
        public string? color { get; set; }
        public string? type { get; set; }
        public string? releaseDate { get; set; }
    }

    public class Title
    {
        public string? romaji { get; set; }
        public string? english { get; set; }
        public string? native { get; set; }
        public string? userPreferred { get; set; }
    }

}
