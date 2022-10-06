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
    public class SettingsFile
    {
        SettingsJson settignsjson;
        public static void DefaultDevSettingSave(bool forceReset)
        {
            // Version 2 API
            //https://ozakinime.herokuapp.com/anime/gogoanime/top-airing

            //Version 1 API
            //https://ozakianime.herokuapp.com

            string defaultAPILink = "http://168.138.43.201:7777";
            string defaultTopAirPath = "/anime/gogoanime/top-airing";
            string defaultReleasePath = "/anime/gogoanime/recent-episodes";
            string defaultAnimeInfoPath = "/anime/gogoanime/info";
            string filename = "OzakiAnimeConfig.json";

            var TempPath = Path.GetTempPath();


            string savepath = TempPath + filename;

            SettingsJson setjson = new SettingsJson();

            setjson.defaultAPILink = defaultAPILink;
            setjson.defaultTopAirPath = defaultTopAirPath;
            setjson.defaultReleasePath = defaultReleasePath;
            setjson.defaultAnimeInfoPath = defaultAnimeInfoPath;

            string jsonSettingsString = JsonConvert.SerializeObject(setjson);

            if (!File.Exists(TempPath + filename) || forceReset == true)
            {

                File.WriteAllText(savepath, jsonSettingsString);
            }
        }

        public static SettingsJson SettingRead()
        {
            var TempPath = Path.GetTempPath();
            string filename = "OzakiAnimeConfig.json";
            string filepath = TempPath + filename;

            DefaultDevSettingSave(false);

            var jsonString = File.ReadAllText(filepath);
            SettingsJson settignsjson = System.Text.Json.JsonSerializer.Deserialize<SettingsJson>(jsonString);

            return settignsjson;
        }


        public void SaveCustomDevSetting(string customApi, string customtopairPath, string customreleasePath, string customanimeinfoPath)
        {
            var TempPath = Path.GetTempPath();
            string filename = "OzakiAnimeConfig.json";
            string savepath = TempPath + filename;
            SettingsJson setjson = new SettingsJson();

            setjson.defaultAPILink = customApi;
            setjson.defaultTopAirPath = customtopairPath;
            setjson.defaultReleasePath = customreleasePath;
            setjson.defaultAnimeInfoPath = customanimeinfoPath;

            string jsonSettingsString = JsonConvert.SerializeObject(setjson);

            File.WriteAllText(savepath, jsonSettingsString);

        }

    }
}
