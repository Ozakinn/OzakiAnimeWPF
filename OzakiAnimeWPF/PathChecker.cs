using OzakiAnimeWPF.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OzakiAnimeWPF
{
    public class PatchCheck
    {

        DefaultPaths df = new DefaultPaths();
        SettingsJson js = new SettingsJson();

        public string Animepathchecker()
        {
            js = SettingsFile.SettingRead();

            string ex = "";

            //CHECKING OF  IP OR DEFAULTapilink is not here. its all good already.
            //do not worry we have http checker in every page
            if (js.defaultANILIST_SearchPath != df.defaultANILIST_SearchPath)
            {
                ex = "Default ANILIST Search Path is Invalid.";
            }
            else if (js.defaultANILIST_RecentPath != df.defaultANILIST_RecentPath)
            {
                ex = "Default ANILIST Recent Path is Invalid.";
            }
            else if (js.defaultANILIST_AdvSearchPath != df.defaultANILIST_AdvSearchPath)
            {
                ex = "Default ANILIST Advanced Search Path is Invalid.";
            }
            else if (js.defaultANILIST_GenrePath != df.defaultANILIST_GenrePath)
            {
                ex = "Default ANILIST Genre Path is Invalid.";
            }
            else if (js.defaultANILIST_RandomAnimePath != df.defaultANILIST_RandomAnimePath)
            {
                ex = "Default ANILIST Random Anime Path is Invalid.";
            }
            else if (js.defaultANILIST_TrendingPath != df.defaultANILIST_TrendingPath)
            {
                ex = "Default ANILIST Trending Path is Invalid.";
            }
            else if (js.defaultANILIST_PopularPath != df.defaultANILIST_PopularPath)
            {
                ex = "Default ANILIST Popular Path is Invalid.";
            }
            else if (js.defaultANILIST_AiringSchedPath != df.defaultANILIST_AiringSchedPath)
            {
                ex = "Default ANILIST Airing Schedule Path is Invalid.";
            }
            else if (js.defaultANILIST_AnimeInfoPath != df.defaultANILIST_AnimeInfoPath)
            {
                ex = "Default ANILIST Anime Info Path is Invalid.";
            }
            else if (js.defaultANILIST_EpisodeStreamLinkPath != df.defaultANILIST_EpisodeStreamLinkPath)
            {
                ex = "Default ANILIST Episode Stream Link Path is Invalid.";
            }

            return ex;
        }
    }
}
