using Newtonsoft.Json;
using OzakiAnimeWPF.Data;
using SixLabors.ImageSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shell;

namespace OzakiAnimeWPF.Pages.UserAccount
{
    public class AccountFavorites
    {
        public static void AccountSave(bool forceReset)
        {
            UserAccountParse userAccountParse = new UserAccountParse();

            var useraccount = new UserAccountParse()
            {
                Favorites = new Favorites()
                {
                    Anime = new Anime()
                    {
                        Anilist = new Anilist[]
                        {

                        }
                    }
                }
            };

            //convert to json format then save
            string jsonSettingsString = JsonConvert.SerializeObject(useraccount);
            string savepath = "Account.json";

            if (!File.Exists(savepath) || forceReset == true)
            {
                //MessageBox.Show(jsonSettingsString);
                File.WriteAllText(savepath, jsonSettingsString);
            }
        }

        public static bool Anime_Anilist_CheckFavorite(string id)
        {
            UserAccountParse account;
            bool isFavorite = false;

            var jsonData = File.ReadAllText("Account.json");
            account = JsonConvert.DeserializeObject<UserAccountParse>(jsonData);


            // MULTIPLE CHECK JUST TO ENSURE
            // level 1 CHECK
            if (account.Favorites is not null)
            {
                // level 2 CHECK
                if (account.Favorites.Anime is not null)
                {
                    // level 3 CHECK
                    if (account.Favorites.Anime.Anilist is not null)
                    {
                        // NOW CHECKS IF ALREADY FAVORITE
                        for (int i = 0; i < account.Favorites.Anime.Anilist.Length; i++)
                        {
                            if (account.Favorites.Anime.Anilist[i].AnimeId == id)
                            {
                                isFavorite = true;
                            }
                        }
                    }
                }
            }

            return isFavorite;
        }

        public static void Anime_Anilist_AddFavorite(string id, string title, string country,
            string image, string releasedate, string type, string subordub)
        {
            UserAccountParse account;

            var jsonData = File.ReadAllText("Account.json");
            account = JsonConvert.DeserializeObject<UserAccountParse>(jsonData);

            if (account.Favorites.Anime.Anilist.Length == 0)
            {
                //System.Windows.MessageBox.Show("here");
                Anilist[] anilist = new Anilist[1];
                anilist[0] = new Anilist
                {
                    AnimeId = id,
                    title = title,
                    countryOfOrigin = country,
                    Image = image,
                    ReleaseDate = releasedate,
                    type = type,
                    subOrdub = subordub,
                    DateAdded = DateTime.Now.ToString("dddd, dd MMMM yyyy hh:mm tt")
                };



                var useraccount = new UserAccountParse()
                {
                    Favorites = new Favorites()
                    {
                        Anime = new Anime()
                        {
                            Anilist = anilist
                        }
                    }
                };



                string fileName = "Account.json";
                string jsonString = JsonConvert.SerializeObject(useraccount);
                File.WriteAllText(fileName, jsonString);
            }
            else
            {
                //System.Windows.MessageBox.Show("here else");
                Anilist[] anilist;
                int length = account.Favorites.Anime.Anilist.Length;
                anilist = account.Favorites.Anime.Anilist;

                bool isSimilar = false;
                //System.Windows.MessageBox.Show((length-1).ToString());

                foreach (var ids in anilist)
                {
                    //System.Windows.MessageBox.Show(anilist[i].AnimeId.ToString());
                    if (ids.AnimeId == id)
                    {
                        isSimilar = true;
                        //anilist[i].AnimeId = id;
                    }
                }

                if (isSimilar == false)
                {

                    Array.Resize(ref anilist, length+1);
                    anilist[length] = new Anilist
                    {
                        AnimeId = id,
                        title = title,
                        countryOfOrigin = country,
                        Image = image,
                        ReleaseDate = releasedate,
                        type = type,
                        subOrdub = subordub,
                        DateAdded = DateTime.Now.ToString("dddd, dd MMMM yyyy hh:mm tt")
                    };
                }

                var useraccount = new UserAccountParse()
                {
                    Favorites = new Favorites()
                    {
                        Anime = new Anime()
                        {
                            Anilist = anilist
                        }
                    }
                };


                string fileName = "Account.json";
                string jsonString = JsonConvert.SerializeObject(useraccount);
                File.WriteAllText(fileName, jsonString);

            }

        }

        public static void Anime_Anilist_RemoveFavorite(string id)
        {
            UserAccountParse account;

            var jsonData = File.ReadAllText("Account.json");
            account = JsonConvert.DeserializeObject<UserAccountParse>(jsonData);

            Anilist[] anilist;
            int length = account.Favorites.Anime.Anilist.Length;
            anilist = account.Favorites.Anime.Anilist;
            int indexPos = 0;

            var anilist_List = anilist.ToList();

            for (int i = 0; i <= length - 1; i++)
            {
                if (anilist[i].AnimeId == id)
                {
                    indexPos = i;
                    //System.Windows.MessageBox.Show(anilist[i].title +" | "+ indexPos.ToString());
                }
            }

            anilist_List.RemoveAt(indexPos);
            anilist = anilist_List.ToArray();

            var useraccount = new UserAccountParse()
            {
                Favorites = new Favorites()
                {
                    Anime = new Anime()
                    {
                        Anilist = anilist
                    }
                }
            };

            string fileName = "Account.json";
            string jsonString = JsonConvert.SerializeObject(useraccount);
            File.WriteAllText(fileName, jsonString);
        }
    }
}
