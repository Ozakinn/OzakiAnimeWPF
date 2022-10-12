using Newtonsoft.Json;
using OzakiAnimeWPF.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OzakiAnimeWPF
{
    public class DefaultPaths
    {
        public string filename = "OzakiAnimeConfig.json";
        public string defaultAPILink = "http://168.138.43.201:7777";

        //gogoanime routes
        public string defaultGOGO_TopAirPath = "/anime/gogoanime/top-airing";
        public string defaultGOGO_ReleasePath = "/anime/gogoanime/recent-episodes";
        public string defaultGOGO_AnimeInfoPath = "/anime/gogoanime/info";
        public string defaultGOGO_AnimeSearchPath = "/anime/gogoanime";

        //anilist routes
        public string defaultANILIST_SearchPath = "/meta/anilist/";
        public string defaultANILIST_RecentPath = "/meta/anilist/recent-episodes";
        public string defaultANILIST_AdvSearchPath = "/meta/anilist/advanced-search";
        public string defaultANILIST_GenrePath = "/meta/anilist/genre";
        public string defaultANILIST_RandomAnimePath = "/meta/anilist/random-anime";
        public string defaultANILIST_TrendingPath = "/meta/anilist/trending";
        public string defaultANILIST_PopularPath = "/meta/anilist/popular";
        public string defaultANILIST_AiringSchedPath = "/meta/anilist/airing-schedule";
        public string defaultANILIST_AnimeInfoPath = "/meta/anilist/info/";
        public string defaultANILIST_EpisodeStreamLinkPath = "/meta/anilist/watch/";
    }

    public class SettingsFile
    {

        public static SettingsJson DefaultPathValues(bool custom, string customApi)
        {
            DefaultPaths defaultPaths = new DefaultPaths();
            SettingsJson setjson = new SettingsJson();

            if (custom == false)
            {
                //api link
                setjson.defaultAPILink = defaultPaths.defaultAPILink;

                //gogoanime
                setjson.defaultGOGO_TopAirPath = defaultPaths.defaultGOGO_TopAirPath;
                setjson.defaultGOGO_ReleasePath = defaultPaths.defaultGOGO_ReleasePath;
                setjson.defaultGOGO_AnimeInfoPath = defaultPaths.defaultGOGO_AnimeInfoPath;
                setjson.defaultGOGO_AnimeSearchPath = defaultPaths.defaultGOGO_AnimeSearchPath;

                //anilist
                setjson.defaultANILIST_SearchPath = defaultPaths.defaultANILIST_SearchPath;
                setjson.defaultANILIST_RecentPath = defaultPaths.defaultANILIST_RecentPath;
                setjson.defaultANILIST_AdvSearchPath = defaultPaths.defaultANILIST_AdvSearchPath;
                setjson.defaultANILIST_GenrePath = defaultPaths.defaultANILIST_GenrePath;
                setjson.defaultANILIST_RandomAnimePath = defaultPaths.defaultANILIST_RandomAnimePath;
                setjson.defaultANILIST_TrendingPath = defaultPaths.defaultANILIST_TrendingPath;
                setjson.defaultANILIST_PopularPath = defaultPaths.defaultANILIST_PopularPath;
                setjson.defaultANILIST_AiringSchedPath = defaultPaths.defaultANILIST_AiringSchedPath;
                setjson.defaultANILIST_AnimeInfoPath = defaultPaths.defaultANILIST_AnimeInfoPath;
                setjson.defaultANILIST_EpisodeStreamLinkPath = defaultPaths.defaultANILIST_EpisodeStreamLinkPath;


                return setjson;
            }
            else
            {
                //api link
                setjson.defaultAPILink = customApi;

                //gogoanime
                setjson.defaultGOGO_TopAirPath = defaultPaths.defaultGOGO_TopAirPath;
                setjson.defaultGOGO_ReleasePath = defaultPaths.defaultGOGO_ReleasePath;
                setjson.defaultGOGO_AnimeInfoPath = defaultPaths.defaultGOGO_AnimeInfoPath;
                setjson.defaultGOGO_AnimeSearchPath = defaultPaths.defaultGOGO_AnimeSearchPath;

                //anilist
                setjson.defaultANILIST_SearchPath = defaultPaths.defaultANILIST_SearchPath;
                setjson.defaultANILIST_RecentPath = defaultPaths.defaultANILIST_RecentPath;
                setjson.defaultANILIST_AdvSearchPath = defaultPaths.defaultANILIST_AdvSearchPath;
                setjson.defaultANILIST_GenrePath = defaultPaths.defaultANILIST_GenrePath;
                setjson.defaultANILIST_RandomAnimePath = defaultPaths.defaultANILIST_RandomAnimePath;
                setjson.defaultANILIST_TrendingPath = defaultPaths.defaultANILIST_TrendingPath;
                setjson.defaultANILIST_PopularPath = defaultPaths.defaultANILIST_PopularPath;
                setjson.defaultANILIST_AiringSchedPath = defaultPaths.defaultANILIST_AiringSchedPath;
                setjson.defaultANILIST_AnimeInfoPath = defaultPaths.defaultANILIST_AnimeInfoPath;
                setjson.defaultANILIST_EpisodeStreamLinkPath = defaultPaths.defaultANILIST_EpisodeStreamLinkPath;


                return setjson;
            }
        }

        public static void DefaultDevSettingSave(bool forceReset)
        {

            // Version 2 API
            //https://ozakinime.herokuapp.com/anime/gogoanime/top-airing

            //Version 1 API
            //https://ozakianime.herokuapp.com

            var TempPath = Path.GetTempPath();

            DefaultPaths defaultPaths = new DefaultPaths();
            string savepath = TempPath + defaultPaths.filename;

            SettingsJson setjson = DefaultPathValues(false, string.Empty);

            //convert to json format then save
            string jsonSettingsString = JsonConvert.SerializeObject(setjson);

            if (!File.Exists(savepath) || forceReset == true)
            {
                MessageBox.Show(jsonSettingsString);
                File.WriteAllText(savepath, jsonSettingsString);
            }
        }

        public static SettingsJson SettingRead()
        {
            DefaultPaths defaultPaths = new DefaultPaths();
            var TempPath = Path.GetTempPath();
            string filepath = TempPath + defaultPaths.filename;

            DefaultDevSettingSave(false);

            var jsonString = File.ReadAllText(filepath);
            SettingsJson settingsjson = System.Text.Json.JsonSerializer.Deserialize<SettingsJson>(jsonString);

            return settingsjson;
        }


        public void SaveCustomSetting(string customApi)
        {
            DefaultPaths defaultPaths = new DefaultPaths();
            var TempPath = Path.GetTempPath();
            string savepath = TempPath + defaultPaths.filename;

            SettingsJson setjson = DefaultPathValues(true, customApi);

            setjson.defaultAPILink = customApi;
            setjson.defaultGOGO_TopAirPath = defaultPaths.defaultGOGO_TopAirPath;
            setjson.defaultGOGO_ReleasePath = defaultPaths.defaultGOGO_ReleasePath;
            setjson.defaultGOGO_AnimeInfoPath = defaultPaths.defaultGOGO_AnimeInfoPath;

            string jsonSettingsString = JsonConvert.SerializeObject(setjson);

            File.WriteAllText(savepath, jsonSettingsString);

        }

    }
}
