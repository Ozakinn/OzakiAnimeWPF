using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OzakiAnimeWPF.Data
{
    //ANILIST PROPERTIES - START

    //RECENT
    public class anilistRecent
    {
        public string? currentPage { get; set; }
        public string? hasNextPage { get; set; }
        public string? totalPages { get; set; }
        public string? totalResults { get; set; }
        public Recent_Result[]? results { get; set; }
    }

    public class Recent_Result
    {
        public string? id { get; set; }
        public string? malId { get; set; }
        public Recent_Title? title { get; set; }
        public string? image { get; set; }
        public string? rating { get; set; }
        public string? color { get; set; }
        public string? episodeId { get; set; }
        public string? episodeTitle { get; set; }
        public string? episodeNumber { get; set; }
        public string[]? genres { get; set; }
        public string? type { get; set; }
    }

    public class Recent_Title
    {
        public string? romaji { get; set; }
        public string? english { get; set; }
        public string? native { get; set; }
        public string? userPreferred { get; set; }
    }



    //TRENDING
    public class anilistTrending
    {
        public string? currentPage { get; set; }
        public string? hasNextPage { get; set; }
        public Trending_Result[]? results { get; set; }
    }

    public class Trending_Result
    {
        public string? id { get; set; }
        public string? malId { get; set; }
        public Trending_Title? title { get; set; }
        public string? image { get; set; }
        public Trending_Trailer? trailer { get; set; }
        public string? description { get; set; }
        public string? status { get; set; }
        public string? cover { get; set; }
        public double? rating { get; set; }
        public string? releaseDate { get; set; }
        public string[]? genres { get; set; }
        public string? totalEpisodes { get; set; }
        public string? duration { get; set; }
        public string? type { get; set; }
    }

    public class Trending_Title
    {
        public string? romaji { get; set; }
        public string? english { get; set; }
        public string? native { get; set; }
        public string? userPreferred { get; set; }
    }

    public class Trending_Trailer
    {
        public string? id { get; set; }
        public string? site { get; set; }
        public string? thumbnail { get; set; }
    }

    //ANIME INFO

    public class anilist_Info
    {
        public string? id { get; set; }
        public Info_Title? title { get; set; }
        public string? malId { get; set; }
        public string[]? synonyms { get; set; }
        public string? isLicensed { get; set; }
        public string? isAdult { get; set; }
        public string? countryOfOrigin { get; set; }
        public string? image { get; set; }
        public string? popularity { get; set; }
        public string? color { get; set; }
        public string? cover { get; set; }
        public string? description { get; set; }
        public string? status { get; set; }
        public string? releaseDate { get; set; }
        public Info_Startdate? startDate { get; set; }
        public Info_Enddate? endDate { get; set; }
        public Info_Nextairingepisode? nextAiringEpisode { get; set; }
        public string? totalEpisodes { get; set; }
        public string? rating { get; set; }
        public string? duration { get; set; }
        public string[]? genres { get; set; }
        public string? season { get; set; }
        public string[]? studios { get; set; }
        public string? subOrDub { get; set; }
        public string? type { get; set; }
        public Info_Recommendation[]? recommendations { get; set; }
        public Info_Character[]? characters { get; set; }
        public Info_Relation[]? relations { get; set; }
        public Info_Episode[]? episodes { get; set; }
    }

    public class Info_Title
    {
        public string? romaji { get; set; }
        public string? english { get; set; }
        public string? native { get; set; }
    }

    public class Info_Startdate
    {
        public string? year { get; set; }
        public string? month { get; set; }
        public string? day { get; set; }
    }

    public class Info_Enddate
    {
        public string? year { get; set; }
        public string? month { get; set; }
        public string? day { get; set; }
    }

    public class Info_Nextairingepisode
    {
        public string? airingTime { get; set; }
        public string? timeUntilAiring { get; set; }
        public string? episode { get; set; }
    }

    public class Info_Recommendation
    {
        public string? id { get; set; }
        public string? malId { get; set; }
        public Info_Reccomendation_Title? title { get; set; }
        public string? status { get; set; }
        public string? episodes { get; set; }
        public string? image { get; set; }
        public string? cover { get; set; }
        public string? rating { get; set; }
        public string? type { get; set; }
    }

    public class Info_Reccomendation_Title
    {
        public string? romaji { get; set; }
        public string? english { get; set; }
        public string? native { get; set; }
        public string? userPreferred { get; set; }
    }

    public class Info_Character
    {
        public string? id { get; set; }
        public string? role { get; set; }
        public Info_Character_Name? name { get; set; }
        public string? image { get; set; }
        public Voiceactor[]? voiceActors { get; set; }
    }

    public class Info_Character_Name
    {
        public string? first { get; set; }
        public string? last { get; set; }
        public string? full { get; set; }
        public string? native { get; set; }
        public string? userPreferred { get; set; }
    }

    public class Voiceactor
    {
        public string? id { get; set; }
        public Info_Voiceactor_Name? name { get; set; }
        public string? image { get; set; }
    }

    public class Info_Voiceactor_Name
    {
        public string? first { get; set; }
        public string? last { get; set; }
        public string? full { get; set; }
        public string? native { get; set; }
        public string? userPreferred { get; set; }
    }

    public class Info_Relation
    {
        public string? id { get; set; }
        public string? relationType { get; set; }
        public string? malId { get; set; }
        public Info_Relation_Title? title { get; set; }
        public string? status { get; set; }
        public double? episodes { get; set; }
        public string? image { get; set; }
        public string? color { get; set; }
        public string? type { get; set; }
        public string? cover { get; set; }
        public string? rating { get; set; }
    }

    public class Info_Relation_Title
    {
        public string? romaji { get; set; }
        public string? english { get; set; }
        public string? native { get; set; }
        public string? userPreferred { get; set; }
    }

    public class Info_Episode
    {
        public string? id { get; set; }
        public string? title { get; set; }
        public string? description { get; set; }
        public string? number { get; set; }
        public string? image { get; set; }
    }

    //EPISODE STREAMING LINK

    public class anilistEpisodeStreaming
    {
        public Headers? headers { get; set; }
        public EpisodeStreaming_Source[]? sources { get; set; }
    }

    public class Headers
    {
        public string? Referer { get; set; }
    }

    public class EpisodeStreaming_Source
    {
        public string? url { get; set; }
        public string? isM3U8 { get; set; }
        public string? quality { get; set; }
    }

    //AIRING SCHEDULE

    public class anilistAiringSchedule
    {
        public int currentPage { get; set; }
        public bool hasNextPage { get; set; }
        public AiringSchedule[]? results { get; set; }
    }

    public class AiringSchedule
    {
        public string? id { get; set; }
        public string? malId { get; set; }
        public string? episode { get; set; }
        public string? airingAt { get; set; }
        public Title_Airing? title { get; set; }
        public string? country { get; set; }
        public string? image { get; set; }
        public string? description { get; set; }
        public string? cover { get; set; }
        public string[]? genres { get; set; }
        public string? color { get; set; }
        public string? rating { get; set; }
        public string? releaseDate { get; set; }
        public string? type { get; set; }
    }

    public class Title_Airing
    {
        public string? romaji { get; set; }
        public string? english { get; set; }
        public string? native { get; set; }
        public string? userPreferred { get; set; }
    }




    //ANILIST PROPERTIES - END

    //GOGOANIME PROPERTIES - START

    public class gogoanimeInfo
    {
        public string? id { get; set; }
        public string? title { get; set; }
        public string? url { get; set; }
        public string[]? genres { get; set; }
        public int totalEpisodes { get; set; }
        public string? image { get; set; }
        public string? releaseDate { get; set; }
        public string? description { get; set; }
        public string? subOrDub { get; set; }
        public string? type { get; set; }
        public string? status { get; set; }
        public string? otherName { get; set; }
        public Episode[]? episodes { get; set; }
    }

    public class Episode
    {
        public string? id { get; set; }
        public double number { get; set; }
        public string? url { get; set; }
    }

    //GOGOANIME PROPERTIES - END

    public class SettingsJson
    {
        //for DEVELOPER SETTINGS
        public string? defaultAPILink { get; set; }
        //GoGoAnime Path
        public string? defaultGOGO_TopAirPath { get; set; }
        public string? defaultGOGO_ReleasePath { get; set; }
        public string? defaultGOGO_AnimeInfoPath { get; set; }
        public string? defaultGOGO_AnimeSearchPath { get; set; }
        //Anilist Path
        public string? defaultANILIST_SearchPath { get; set; }
        public string? defaultANILIST_RecentPath { get; set; }
        public string? defaultANILIST_AdvSearchPath { get; set; }
        public string? defaultANILIST_GenrePath { get; set; }
        public string? defaultANILIST_RandomAnimePath { get; set; }
        public string? defaultANILIST_TrendingPath { get; set; }
        public string? defaultANILIST_PopularPath { get; set; }
        public string? defaultANILIST_AiringSchedPath { get; set; }
        public string? defaultANILIST_AnimeInfoPath { get; set; }
        public string? defaultANILIST_EpisodeStreamLinkPath { get; set; }
    }

    public static class PictureAnalysis
    {
        public static List<Color>? TenMostUsedColors { get; private set; }
        public static List<int>? TenMostUsedColorIncidences { get; private set; }

        public static Color MostUsedColor { get; private set; }
        public static int MostUsedColorIncidence { get; private set; }

        private static int pixelColor;

        private static Dictionary<int, int>? dctColorIncidence;

        public static async Task GetMostUsedColor(Bitmap theBitMap)
        {

            await Task.Run(() =>
            {

                TenMostUsedColors = new List<Color>();
                TenMostUsedColorIncidences = new List<int>();

                MostUsedColor = Color.Empty;
                MostUsedColorIncidence = 0;

                // does using Dictionary<int,int> here
                // really pay-off compared to using
                // Dictionary<Color, int> ?

                // would using a SortedDictionary be much slower, or ?

                dctColorIncidence = new Dictionary<int, int>();

                // this is what you want to speed up with unmanaged code
                for (int row = 0; row < theBitMap.Size.Width; row++)
                {
                    for (int col = 0; col < theBitMap.Size.Height; col++)
                    {
                        pixelColor = theBitMap.GetPixel(row, col).ToArgb();

                        if (dctColorIncidence.Keys.Contains(pixelColor))
                        {
                            dctColorIncidence[pixelColor]++;
                        }
                        else
                        {
                            dctColorIncidence.Add(pixelColor, 1);
                        }
                    }
                }

                // note that there are those who argue that a
                // .NET Generic Dictionary is never guaranteed
                // to be sorted by methods like this
                var dctSortedByValueHighToLow = dctColorIncidence.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

                // this should be replaced with some elegant Linq ?
                foreach (KeyValuePair<int, int> kvp in dctSortedByValueHighToLow.Take(10))
                {
                    TenMostUsedColors.Add(Color.FromArgb(kvp.Key));
                    TenMostUsedColorIncidences.Add(kvp.Value);
                }

                MostUsedColor = Color.FromArgb(dctSortedByValueHighToLow.First().Key);
                MostUsedColorIncidence = dctSortedByValueHighToLow.First().Value;

            });

        }

    }

}
