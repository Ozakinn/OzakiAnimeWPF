using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;
using OzakiAnimeWPF.Data;
using System.Windows.Media.Animation;
using System.Diagnostics;

namespace OzakiAnimeWPF.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    /// 

    public partial class Home : UiPage
    {

        //Settings API config
        List<string> settingsAPI = new List<string>();
        string defaultAPILink;
        string defaultTopAirPath;
        string defaultReleasePath;
        string defaultAnimeInfoPath;

        //Top Airing Anime
        List<string> topair_id = new List<string>();
        List<string> topair_title = new List<string>();
        List<string> topair_img = new List<string>();
        List<string> topair_url = new List<string>();
        List<string> topair_genre = new List<string>();
        List<string> topair_releasedate = new List<string>();
        List<string> topair_subordub = new List<string>();
        string[] topair_array_id = { };
        string[] topair_array_title = { };
        string[] topair_array_img = { };
        string[] topair_array_url = { };
        string[] topair_array_genre = { };
        string[] topair_array_releasedate = { };
        string[] topair_array_subordub = { };

        //New Release / Recent Release
        List<string> recent_id = new List<string>();
        List<string> recent_episodeid = new List<string>();
        List<string> recent_epNum = new List<string>();
        List<string> recent_title = new List<string>();
        List<string> recent_img = new List<string>();
        List<string> recent_url = new List<string>();
        List<string> recent_subordub = new List<string>();
        List<string> recent_releasedate = new List<string>();
        string[] recent_array_id = { };
        string[] recent_array_episodeid = { };
        string[] recent_array_epNum = { };
        string[] recent_array_title = { };
        string[] recent_array_img = { };
        string[] recent_array_url = { };
        string[] recent_array_subordub = { };
        string[] recent_array_releasedate = { };

        private MainWindow mainForm;

        Point scrollMousePoint = new Point();
        double hOff = 1;

        bool checkUrl;

        CancellationTokenSource source = new CancellationTokenSource();

        int marginScroll = 0;

        public Home(MainWindow main)
        {

            mainForm = main;
            InitializeComponent();

            settingsAPI = SettingsFile.SettingRead();
            Settings_API();

        }

        private async void UiPage_Loaded(object sender, RoutedEventArgs e)
        {

            TopAirStack.Visibility = Visibility.Hidden;
            RecentStack.Visibility = Visibility.Hidden;
            HomeLoader.Visibility = Visibility.Visible;

            try
            {
                await Home_Load();
            }
            catch (Exception ex)
            {
                mainForm.RootDialog.Title = "Error: Loading Home";
                mainForm.RootDialog.Content = ex.Message;
                mainForm.RootDialog.Show();
                TopAirStack.Visibility = Visibility.Hidden;
                RecentStack.Visibility = Visibility.Hidden;
                HomeLoader.Visibility = Visibility.Visible;
            }

            TopAirStack.Visibility = Visibility.Visible;
            RecentStack.Visibility = Visibility.Visible;
            HomeLoader.Visibility = Visibility.Hidden;


            //gridRow1.MinHeight = gridRow1.ActualHeight + 20;
            //gridRow2.MinHeight = gridRow2.ActualHeight + 20;

        }

        public async Task Home_Load()
        {
            if (!source.IsCancellationRequested)
            {
                var sw = new Stopwatch();
                sw.Start();
                await Task.WhenAll(fetchTopAir(),fetchRecent());
                //System.Windows.MessageBox.Show("done async");
                sw.Stop();

                var elapsed = sw.ElapsedMilliseconds;
                //System.Windows.MessageBox.Show("took: " + elapsed + "ms");
            }
            else
            {
                //System.Windows.MessageBox.Show("stop");
            }
        }

        public async Task fetchAnimeInfo(string id, List<string> rd, List<string> sd)
        {
            string animeid = "/" + id;
            StringBuilder sb = new StringBuilder();

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            var client = new HttpClient();
            var animeinfo = await client.GetStringAsync(defaultAnimeInfoPath + animeid);

            animeInfo info = System.Text.Json.JsonSerializer.Deserialize<animeInfo>(animeinfo);



            rd.Add(info.releaseDate.ToString());
            sd.Add(info.subOrDub.ToString());

        }

        public async Task fetchTopAir()
        {
            checkUrl = await UrlIsValid(defaultAPILink);
            if (checkUrl == true)
            {

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                var client = new HttpClient();
                var topair_anime = await client.GetStringAsync(defaultTopAirPath);


                var topair_anime_jsonLinq = JObject.Parse(topair_anime);

                // Find the first array using Linq
                //fetch Top air Anime
                var topair_anime_srcArray = topair_anime_jsonLinq.Descendants().Where(d => d is JArray).First();
                foreach (JObject row in topair_anime_srcArray.Children<JObject>())
                {
                    var cleanRow = new JObject();
                    foreach (JProperty column in row.Properties())
                    {
                        // Only include JValue types
                        // Use JArray to detect Genre or another array inside values
                        if (column.Value is JValue)
                        {
                            if (column.Name.ToString() == "id")
                            {
                                topair_id.Add(column.Value.ToString());
                                await fetchAnimeInfo(column.Value.ToString(), topair_releasedate, topair_subordub);
                            }

                            if (column.Name.ToString() == "title")
                            {
                                topair_title.Add(column.Value.ToString());
                            }

                            if (column.Name.ToString() == "image")
                            {
                                topair_img.Add(column.Value.ToString());
                            }

                            if (column.Name.ToString() == "url")
                            {
                                topair_url.Add(column.Value.ToString());
                            }

                            //SAMPLE TO GET VALUE AND NAME
                            //cleanRow.Add(column.Name, column.Value);
                        }

                        if (column.Value is JArray)
                        {
                            if (column.Name.ToString() == "genres")
                            {
                                topair_genre.Add(column.Value.ToString());
                            }
                        }
                    }
                }


                //Convert List to Array
                topair_array_id = topair_id.ToArray();
                topair_array_title = topair_title.ToArray();
                topair_array_img = topair_img.ToArray();
                topair_array_url = topair_url.ToArray();
                topair_array_genre = topair_genre.ToArray();
                topair_array_releasedate = topair_releasedate.ToArray();
                topair_array_subordub = topair_subordub.ToArray();

                //Count the total Top Air Anime
                var topair_total_id = topair_array_id.Count();
                var topair_total_title = topair_array_title.Count();
                var topair_total_img = topair_array_img.Count();
                var topair_total_url = topair_array_url.Count();
                var topair_total_genre = topair_array_genre.Count();
                var topair_total_releasedate = topair_array_releasedate.Count();
                var topair_total_subordub = topair_array_subordub.Count();

                //Generate Cards after fetching data

                generateTopAir();

            }

        }

        public async Task fetchRecent()
        {
            checkUrl = await UrlIsValid(defaultAPILink);
            if (checkUrl == true)
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

                var client = new HttpClient();
                var recent_anime = await client.GetStringAsync(defaultReleasePath);
                var recent_anime_jsonLinq = JObject.Parse(recent_anime);
                // Find the first array using Linq
                //fetch Top air Anime
                var recent_anime_srcArray = recent_anime_jsonLinq.Descendants().Where(d => d is JArray).First();
                foreach (JObject row in recent_anime_srcArray.Children<JObject>())
                {
                    var cleanRow = new JObject();
                    foreach (JProperty column in row.Properties())
                    {
                        // Only include JValue types
                        // Use JArray to detect Genre or another array inside values
                        if (column.Value is JValue)
                        {
                            if (column.Name.ToString() == "id")
                            {
                                recent_id.Add(column.Value.ToString());
                                //await fetchAnimeInfo(column.Value.ToString(), recent_releasedate, recent_subordub);
                            }

                            if (column.Name.ToString() == "episodeId")
                            {
                                recent_episodeid.Add(column.Value.ToString());
                            }

                            if (column.Name.ToString() == "episodeNumber")
                            {
                                recent_epNum.Add(column.Value.ToString());
                            }

                            if (column.Name.ToString() == "title")
                            {
                                recent_title.Add(column.Value.ToString());
                            }

                            if (column.Name.ToString() == "image")
                            {
                                recent_img.Add(column.Value.ToString());
                            }

                            if (column.Name.ToString() == "url")
                            {
                                recent_url.Add(column.Value.ToString());
                            }
                        }
                    }
                }

                //Convert List to Array
                recent_array_id = recent_id.ToArray();
                recent_array_episodeid = recent_episodeid.ToArray();
                recent_array_epNum = recent_epNum.ToArray();
                recent_array_title = recent_title.ToArray();
                recent_array_img = recent_img.ToArray();
                recent_array_url = recent_url.ToArray();
                recent_array_subordub = recent_subordub.ToArray();
                recent_array_releasedate = recent_releasedate.ToArray();

                //Count the total Recent Anime
                var recent_total_id = recent_array_id.Count();
                var recent_total_epid = recent_array_episodeid.Count();
                var recent_total_epnum = recent_array_epNum.Count();
                var recent_total_title = recent_array_title.Count();
                var recent_total_img = recent_array_img.Count();
                var recent_total_url = recent_array_url.Count();
                var recent_total_subordub = recent_array_subordub.Count();
                var recent_total_releasedate = recent_array_releasedate.Count();

                //FOR DEBUG PURPOSES ONLY
                //System.Windows.MessageBox.Show(recent_totalanimesubordub.ToString());

                //Generate Cards
                generateRecent();
            }

        }

        public void generateTopAir()
        {
            //Count the total Top Air Anime
            var topair_total_id = topair_array_id.Count();
            var topair_total_title = topair_array_title.Count();
            var topair_total_img = topair_array_img.Count();
            var topair_total_url = topair_array_url.Count();
            var topair_total_genre = topair_array_genre.Count();
            var topair_total_releasedate = topair_array_releasedate.Count();
            var topair_total_subordub = topair_array_subordub.Count();




            for (int i = 0; i < topair_total_id; i++)
            {

                var topairCardPanel = new System.Windows.Controls.StackPanel();
                var topairText = new System.Windows.Controls.TextBlock();
                var topairBadge = new Wpf.Ui.Controls.Badge();
                var topairCard = new Wpf.Ui.Controls.Button();

                //Declare and initialize image to bitmap
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(topair_array_img[i].ToString());
                bitmapImage.EndInit();

                //Cover Artwork without Opacity
                ImageBrush cardCover = new ImageBrush();
                cardCover.Stretch = Stretch.UniformToFill;
                cardCover.ImageSource = bitmapImage;

                //Cover Artwork with Opacity
                ImageBrush cardCoverOpacity = new ImageBrush();
                cardCoverOpacity.Stretch = Stretch.UniformToFill;
                cardCoverOpacity.ImageSource = bitmapImage;
                cardCoverOpacity.Opacity = 0.20;

                //Modify Popular Card Style
                topairCard.Background = cardCover;
                topairCard.Height = 200;
                topairCard.Width = 150;
                topairCard.MouseOverBackground = cardCoverOpacity;
                topairCard.Tag = new 
                { 
                    panel = topairCardPanel, 
                    selected_topair_id = topair_array_id[i].ToString(),
                    selected_topair_img = topair_array_img[i].ToString()
                };

                string releasedate = topair_array_releasedate[i].ToString();
                string subordub = topair_array_subordub[i].ToString().ToUpper();


                var title = topair_array_title[i].ToString();
                //System.Windows.MessageBox.Show(bb.Length.ToString());


                topairBadge.FontSize = 10;
                topairBadge.FontWeight = FontWeights.Bold;
                topairBadge.Background = Brushes.Gold;
                topairBadge.VerticalAlignment = VerticalAlignment.Top;
                topairBadge.HorizontalAlignment = HorizontalAlignment.Center;
                topairBadge.Content = releasedate +" | "+ subordub;

                topairText.FontSize = 14;
                topairText.FontWeight = FontWeights.Bold;
                topairText.TextAlignment = TextAlignment.Center;
                topairText.TextWrapping = TextWrapping.Wrap;
                topairText.Text = title;

                topairCardPanel.Children.Add(topairText);
                topairCardPanel.Children.Add(topairBadge);

                topairCard.Content = topairCardPanel;
                topairCardPanel.Visibility = Visibility.Hidden;

                //Margin for First Card and Last Card
                if (i == 0)
                {
                    topairCard.Margin = new System.Windows.Thickness(64, 0, 0, marginScroll);
                }
                else if (i == topair_total_id - 1)
                {
                    topairCard.Margin = new System.Windows.Thickness(12, 0, 64, marginScroll);
                }
                else
                {
                    topairCard.Margin = new System.Windows.Thickness(12, 0, 0, marginScroll);
                }

                //Render and Add the control for the popular Anime
                TopAirPanel.Children.Add(topairCard);

                topairCard.Click += new RoutedEventHandler(topairCards_Click);
                topairCard.MouseEnter += new MouseEventHandler(topairCards_MouseEnter);
                topairCard.MouseLeave += new MouseEventHandler(topairCards_MouseLeave);
            }


        }

        public void generateRecent()
        {
            //Count the total Recent Anime
            var recent_total_id = recent_array_id.Count();
            var recent_total_epid = recent_array_episodeid.Count();
            var recent_total_epnum = recent_array_epNum.Count();
            var recent_total_title = recent_array_title.Count();
            var recent_total_img = recent_array_img.Count();
            var recent_total_url = recent_array_url.Count();
            var recent_total_subordub = recent_array_subordub.Count();
            var recent_total_releasedate = recent_array_releasedate.Count();

            for (int i = 0; i < recent_total_id; i++)
            {
                var recentCardPanel = new System.Windows.Controls.StackPanel();
                var recentText = new System.Windows.Controls.TextBlock();
                var recentBadge = new Wpf.Ui.Controls.Badge();
                var recentCard = new Wpf.Ui.Controls.Button();

                //Declare and initialize image to bitmap
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(recent_array_img[i].ToString());
                bitmapImage.EndInit();

                //Cover Artwork without Opacity
                ImageBrush cardCover = new ImageBrush();
                cardCover.Stretch = Stretch.UniformToFill;
                cardCover.ImageSource = bitmapImage;

                //Cover Artwork with Opacity
                ImageBrush cardCoverOpacity = new ImageBrush();
                cardCoverOpacity.Stretch = Stretch.UniformToFill;
                cardCoverOpacity.ImageSource = bitmapImage;
                cardCoverOpacity.Opacity = 0.20;

                //Modify Popular Card Style
                recentCard.Background = cardCover;
                recentCard.Height = 200;
                recentCard.Width = 150;
                recentCard.MouseOverBackground = cardCoverOpacity;
                recentCard.Tag = new { panel = recentCardPanel, selected_recentanimearray = i };

                var recent_title = recent_array_title[i].ToString();
                var recent_episodeNum = recent_array_epNum[i].ToString();
                //var recent_subordub = recent_array_subordub[i].ToString().ToUpper();

                recentBadge.FontSize = 10;
                recentBadge.FontWeight = FontWeights.Bold;
                recentBadge.Background = Brushes.LimeGreen;
                recentBadge.VerticalAlignment = VerticalAlignment.Top;
                recentBadge.HorizontalAlignment = HorizontalAlignment.Center;
                recentBadge.Content = "EP " + recent_episodeNum;

                recentText.FontSize = 14;
                recentText.FontWeight = FontWeights.Bold;
                recentText.TextAlignment = TextAlignment.Center;
                recentText.TextWrapping = TextWrapping.Wrap;
                recentText.Text = recent_title;

                recentCardPanel.Name = "recentCardPanel";
                recentCardPanel.Children.Add(recentText);
                recentCardPanel.Children.Add(recentBadge);

                recentCard.Content = recentCardPanel;
                recentCardPanel.Visibility = Visibility.Hidden;

                //Margin for First Card and Last Card
                if (i == 0)
                {
                    recentCard.Margin = new System.Windows.Thickness(64, 0, 0, marginScroll);
                }
                else if (i == recent_total_id - 1)
                {
                    recentCard.Margin = new System.Windows.Thickness(12, 0, 64, marginScroll);
                }
                else
                {
                    recentCard.Margin = new System.Windows.Thickness(12, 0, 0, marginScroll);
                }

                //Render and Add the control for the recent Anime
                RecentPanel.Children.Add(recentCard);

                recentCard.Click += new RoutedEventHandler(recentCards_Click);
                recentCard.MouseEnter += new MouseEventHandler(recentCards_MouseEnter);
                recentCard.MouseLeave += new MouseEventHandler(recentCards_MouseLeave);
            }
        }

        //Popular Card Events
        private void topairCards_Click(object sender, RoutedEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonThatWasClicked = (Wpf.Ui.Controls.Button)sender;
            string topair_id = ((dynamic)buttonThatWasClicked.Tag).selected_topair_id;
            string topair_selected_img = ((dynamic)buttonThatWasClicked.Tag).selected_topair_img;

            this.NavigationService.Navigate(new SelectedAnime(topair_id, topair_selected_img));
            //System.Windows.MessageBox.Show(test.ToString());

        }

        private void topairCards_MouseEnter(object sender, MouseEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonMouseEnter = (Wpf.Ui.Controls.Button)sender;
            System.Windows.Controls.StackPanel panel = ((dynamic)buttonMouseEnter.Tag).panel;

            panel.Visibility = Visibility.Visible;
        }

        private void topairCards_MouseLeave(object sender, MouseEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonMouseEnter = (Wpf.Ui.Controls.Button)sender;
            System.Windows.Controls.StackPanel panel = ((dynamic)buttonMouseEnter.Tag).panel;

            panel.Visibility = Visibility.Hidden;
        }

        //Recent Card Events
        private void recentCards_Click(object sender, RoutedEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonThatWasClicked = (Wpf.Ui.Controls.Button)sender;
            int test = ((dynamic)buttonThatWasClicked.Tag).selected_recentanimearray;

            //System.Windows.MessageBox.Show(test.ToString());

        }

        private void recentCards_MouseEnter(object sender, MouseEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonMouseEnter = (Wpf.Ui.Controls.Button)sender;
            System.Windows.Controls.StackPanel panel = ((dynamic)buttonMouseEnter.Tag).panel;

            panel.Visibility = Visibility.Visible;
        }

        private void recentCards_MouseLeave(object sender, MouseEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonMouseEnter = (Wpf.Ui.Controls.Button)sender;
            System.Windows.Controls.StackPanel panel = ((dynamic)buttonMouseEnter.Tag).panel;

            panel.Visibility = Visibility.Hidden;
        }

        //Popular ScrollViewer Events
        private void svTopAirScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollviewer = sender as ScrollViewer;
            if (e.Delta > 0)
            {
                scrollviewer.LineLeft();
            }
            else
            {
                scrollviewer.LineRight();
            }
            e.Handled = true;
        }

        private void svTopAir_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            scrollMousePoint = e.GetPosition(svTopAir);
            hOff = svTopAir.HorizontalOffset;
            svTopAir.CaptureMouse();
        }

        private void svTopAir_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (svTopAir.IsMouseCaptured)
            {
                svTopAir.ScrollToHorizontalOffset(hOff + (scrollMousePoint.X - e.GetPosition(svTopAir).X));
            }
        }

        private void svTopAir_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            svTopAir.ReleaseMouseCapture();
        }

        private void svTopAir_MouseEnter(object sender, MouseEventArgs e)
        {
            svTopAir.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;

        }

        private void svTopAir_MouseLeave(object sender, MouseEventArgs e)
        {
            svTopAir.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }

        //Recent ScrollViewer Events
        private void svRecent_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollviewer = sender as ScrollViewer;
            if (e.Delta > 0)
            {
                scrollviewer.LineLeft();
            }
            else
            {
                scrollviewer.LineRight();
            }
            e.Handled = true;
        }

        private void svRecent_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            scrollMousePoint = e.GetPosition(svRecent);
            hOff = svRecent.HorizontalOffset;
            svRecent.CaptureMouse();
        }

        private void svRecent_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (svRecent.IsMouseCaptured)
            {
                svRecent.ScrollToHorizontalOffset(hOff + (scrollMousePoint.X - e.GetPosition(svRecent).X));
            }
        }

        private void svRecent_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            svRecent.ReleaseMouseCapture();
        }

        private void svRecent_MouseEnter(object sender, MouseEventArgs e)
        {
            svRecent.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
        }

        private void svRecent_MouseLeave(object sender, MouseEventArgs e)
        {
            svRecent.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;

        }

        //Dialog Action
        private void LeftClick_Dialog(object sender, MouseEventArgs e)
        {

        }

        private void UiPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (source != null)
            {
                source.Cancel();
                //System.Windows.MessageBox.Show("cancel async");
            }
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
                    mainForm.PageLoading.Visibility = Visibility.Hidden;
                    return true;
                }
                else
                {
                    mainForm.PageLoading.Visibility = Visibility.Visible;

                    //System.Windows.MessageBox.Show("error url");
                    //error path
                    //example: retry, check error content, log error, throw exception

                    //Console.WriteLine($"Request failed. Error status code: {(int)response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                mainForm.PageLoading.Visibility = Visibility.Visible;

                /*
                var messageBox = new Wpf.Ui.Controls.MessageBox();

                messageBox.ButtonLeftName = "Hello World";
                messageBox.ButtonRightName = "Just close me";
                
                */
                //System.Windows.MessageBox.Show("error url");

                return false;
            }
        }

        //API settings
        public void Settings_API()
        {
            string[] data = settingsAPI.ToArray();
            defaultAPILink = data[0];
            defaultTopAirPath = data[0] + data[1];
            defaultReleasePath = data[0] + data[2];
            defaultAnimeInfoPath = data[0] + data[3];
        }

        //Working for V1 API
        public void fetchPopularV1()
        {
            //WORKS WELL WITH V1 API
            /*

            var client = new HttpClient();
            var content = await client.GetStringAsync(defaultPopularPath);
            DataTable dt = JsonConvert.DeserializeObject<DataTable>(content);
            foreach (System.Data.DataRow r in dt.Rows)
            {
                foreach (System.Data.DataColumn c in dt.Columns)
                {
                    if (c.ColumnName.ToString() == "animeId")
                    {
                        popular_animeid.Add(r[c.ColumnName].ToString());
                    }

                    if (c.ColumnName.ToString() == "animeTitle")
                    {
                        popular_animetitle.Add(r[c.ColumnName].ToString());
                    }

                    if (c.ColumnName.ToString() == "animeImg")
                    {
                        popular_animeimg.Add(r[c.ColumnName].ToString());
                    }

                    if (c.ColumnName.ToString() == "releasedDate")
                    {
                        popular_releasedate.Add(r[c.ColumnName].ToString());
                    }

                    if (c.ColumnName.ToString() == "animeUrl")
                    {
                        popular_animeurl.Add(r[c.ColumnName].ToString());
                    }

                }
            }

            //Convert List to Array
            popular_array_animeid = popular_animeid.ToArray();
            popular_array_animetitle = popular_animetitle.ToArray();
            popular_array_animeimg = popular_animeimg.ToArray();
            popular_array_releasedate = popular_releasedate.ToArray();
            popular_array_animeurl = popular_animeurl.ToArray();

            //Count the total Popular Anime
            var popular_totalanimeid = popular_array_animeid.Count();
            var popular_totalanimetitle = popular_array_animetitle.Count();
            var popular_totalanimeimg = popular_array_animeimg.Count();
            var popular_totalreleasedate = popular_array_releasedate.Count();
            var popular_totalanimeurl = popular_array_animeurl.Count();

            //FOR DEBUG PURPOSES ONLY
            //System.Windows.MessageBox.Show(popular_totalanimeimg.ToString());

            //Generate Cards
            //generatePopular();
            //new Thread(generatePopular).Start();
            //generatePopular();
            //await Task.Run(() => generatePopular());

            */
        }
    }
}
