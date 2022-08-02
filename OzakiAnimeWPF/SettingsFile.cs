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

        public static void DefaultDevSettingSave(bool forceReset)
        {
            // Version 2 API
            //https://ozakinime.herokuapp.com/anime/gogoanime/top-airing

            //Version 1 API
            //https://ozakianime.herokuapp.com

            string defaultAPILink = "http://152.70.87.238:7777";
            string defaultTopAirPath = "/anime/gogoanime/top-airing";
            string defaultReleasePath = "/anime/gogoanime/recent-episodes";
            string defaultAnimeInfoPath = "/anime/gogoanime/info";

            var TempPath = Path.GetTempPath();

            if (!File.Exists(TempPath + "OzakiAnimeConfig.txt") || forceReset == true)
            {

                string[] SettingsContent = new string[]
                {
                defaultAPILink,         //0
                defaultTopAirPath,      //1
                defaultReleasePath,     //2
                defaultAnimeInfoPath,   //3
                };

                using (StreamWriter sw = new StreamWriter(TempPath + "OzakiAnimeConfig.txt"))
                {

                    foreach (string s in SettingsContent)
                    {
                        sw.WriteLine(s);
                    }
                }
            }
        }

        public static List<string> SettingRead()
        {
            var TempPath = Path.GetTempPath();
            List<string> data = new List<string>();

            string line;
            using (StreamReader sr = new StreamReader(TempPath + "OzakiAnimeConfig.txt"))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    data.Add(line);
                }
            }

            return data;
            //MessageBox.Show(String.Join("\n", data));
        }


        public void SaveCustomDevSetting(string customApi, string customtopairPath, string customreleasePath, string customanimeinfoPath)
        {
            var TempPath = Path.GetTempPath();


            string[] SettingsContent = new string[]
            {
            customApi,              //0
            customtopairPath,       //1
            customreleasePath,      //2
            customanimeinfoPath     //3
            };

            using (StreamWriter sw = new StreamWriter(TempPath + "OzakiAnimeConfig.txt"))
            {

                foreach (string s in SettingsContent)
                {
                    sw.WriteLine(s);
                }
            }
            
        }

    }
}
