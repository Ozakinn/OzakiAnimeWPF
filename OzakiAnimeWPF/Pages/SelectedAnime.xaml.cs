using OzakiAnimeWPF.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace OzakiAnimeWPF.Pages
{
    /// <summary>
    /// Interaction logic for SelectedAnime.xaml
    /// </summary>
    public partial class SelectedAnime : UiPage
    {
        string topair_id;
        string topair_Img;

        //Settings API config
        List<string> settingsAPI = new List<string>();

        SettingsJson jsonSetting;

        //URL checker
        bool checkUrl;

        // Cancel Async
        CancellationTokenSource source = new CancellationTokenSource();

        public animeInfo selectedanime;

        public SelectedAnime(string topairid, string topair_img)
        {

            topair_id = topairid;
            topair_Img = topair_img;

            jsonSetting = new SettingsJson();
            jsonSetting = SettingsFile.SettingRead();

            InitializeComponent();
        }

        private async void UiPage_Loaded(object sender, RoutedEventArgs e)
        {

            PageLoad.Visibility = Visibility.Visible;
            CoverPhoto.Visibility = Visibility.Hidden;

            await SelectedAnime_Load();

            CoverPhoto.Visibility = Visibility.Visible;
            PageLoad.Visibility = Visibility.Hidden;


        }

        public async Task SelectedAnime_Load()
        {
            await Task.WhenAll(fetch_SelectedAnime());
        }

        public async Task fetch_SelectedAnime()
        {
            checkUrl = await UrlIsValid(jsonSetting.defaultAPILink);
            if (checkUrl == true)
            {

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                var client = new HttpClient();
                var selected_anime = await client.GetStreamAsync(jsonSetting.defaultAPILink + jsonSetting.defaultAnimeInfoPath + "/"+ topair_id);
                selectedanime = System.Text.Json.JsonSerializer.Deserialize<animeInfo>(selected_anime);


                //await getColor();

                BitmapImage bitmapimg = new BitmapImage();
                bitmapimg.BeginInit();
                bitmapimg.UriSource = new Uri(selectedanime.image, UriKind.RelativeOrAbsolute);
                bitmapimg.EndInit();

                //stream iamge to bitmap then get top colors
                //var request = new HttpClient();
                //var response = await request.GetAsync(topair_Img);
                //System.IO.Stream responseStream = response;
                //System.Drawing.Bitmap bitmap2 = new System.Drawing.Bitmap(responseStream);
                //var img = ImageUtilities.ResizeImage(test, 1080,1920);
                //var final_img = ConvertBitmapToBitmapImage.Convert(img);

                CoverPhotoImage.ImageSource = bitmapimg;

                string status = selectedanime.status;
                string title = selectedanime.title;

                if (status == "Ongoing")
                {
                    animeStatus.Foreground = Brushes.Gold;
                    animeStatus.Text = "In progress";
                }
                else
                {
                    animeStatus.Foreground = Brushes.LimeGreen;
                    animeStatus.Text = status;
                }

                animeTitle.Text = title;
                animeTitle.ToolTip = title;
                animeOtherName.Text = selectedanime.otherName;
                animeDescription.Text = selectedanime.description;

                string[] otherInfo = new string[4];
                otherInfo[0] = selectedanime.subOrDub.ToUpper();
                otherInfo[1] = selectedanime.releaseDate;
                otherInfo[2] = selectedanime.totalEpisodes.ToString() +" Episodes Now";
                otherInfo[3] = selectedanime.type;

                foreach (string info in otherInfo)
                {
                    var infoBadge = new Wpf.Ui.Controls.Badge();

                    infoBadge.FontWeight = FontWeights.DemiBold;
                    infoBadge.Appearance = Wpf.Ui.Common.ControlAppearance.Light;
                    infoBadge.Margin = new Thickness(0, 5, 2, 0);
                    infoBadge.Content = info;

                    animeOtherInfoBadge.Children.Add(infoBadge);
                }

                foreach (string genre in selectedanime.genres)
                {
                    var genreButton = new Wpf.Ui.Controls.Button();

                    genreButton.FontWeight = FontWeights.Bold;
                    genreButton.Appearance = Wpf.Ui.Common.ControlAppearance.Secondary;
                    genreButton.Margin = new Thickness(0,5,5,0);
                    genreButton.Content = genre;

                    panelGenreButton.Children.Add(genreButton);
                }


                string url = "https://manifest.prod.boltdns.net/manifest/v1/hls/v4/clear/6312875934001/0518f466-d199-459c-bbb8-967975b9505a/6s/master.m3u8?fastly_token=NjJlMDIyYzFfNGI5YTc4YWZiZmU4NDJkNjAzZWY0MjZlMGY3OTI2NjJlMjcwYjI2ZjQzN2FkZGEyYzlmNTEzMDNmN2M0NzY4NA%3D%3D";
                //Data.VideoThumnail.Program();
                //await VideoThumnail.Main(url);
            }
        }

        public SolidColorBrush badgeColor(string Genre)
        {

            if (Genre == "Action")
            {
                Color newColor = Color.FromArgb(255, 250, 255, 0);
                SolidColorBrush nt = new SolidColorBrush(newColor);
                return nt;
            }
            else if (Genre == "Comedy")
            {
                Color newColor = Color.FromArgb(255, 250, 255, 0);
                SolidColorBrush nt = new SolidColorBrush(newColor);
                return nt;
            }
            else
            {
                return Brushes.Gray;
            }
        }


        //GET Most used Color of the Image
        public async Task getColor()
        {
            //stream iamge to bitmap then get top colors
            var request = new HttpClient();
            var response = await request.GetAsync(topair_Img);

            //System.IO.Stream responseStream = response;
            //System.Drawing.Bitmap bitmap2 = new System.Drawing.Bitmap(responseStream);

            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var memStream = new System.IO.MemoryStream();
                await stream.CopyToAsync(memStream);
                memStream.Position = 0;
                await PictureAnalysis.GetMostUsedColor(new System.Drawing.Bitmap(memStream));
            }

            //get color
            System.Drawing.Color color = PictureAnalysis.TenMostUsedColors[5];
            Color newColor = Color.FromArgb(color.A, color.R, color.G, color.B);
            //SolidColorBrush nt = new SolidColorBrush(newColor);

            //CoverPhotoColor.Color = newColor;
        }

        //URL Checker if API is active or down
        public async Task<bool> UrlIsValid(string url)
        {
            try
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    //PageLoad.Visibility = Visibility.Hidden;
                    return true;
                }
                else
                {
                    PageLoad.Visibility = Visibility.Visible;

                    //System.Windows.MessageBox.Show("error url");
                    //error path
                    //example: retry, check error content, log error, throw exception

                    //Console.WriteLine($"Request failed. Error status code: {(int)response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                PageLoad.Visibility = Visibility.Visible;

                /*
                var messageBox = new Wpf.Ui.Controls.MessageBox();

                messageBox.ButtonLeftName = "Hello World";
                messageBox.ButtonRightName = "Just close me";
                
                */
                //System.Windows.MessageBox.Show("error url");

                return false;
            }
        }

    }
}
