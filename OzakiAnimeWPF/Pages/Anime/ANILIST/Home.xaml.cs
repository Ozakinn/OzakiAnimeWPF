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
using System.Security.Policy;
using System.IO;
using System.Drawing.Imaging;
using Ozaki_Anime.Pages.SelectedAnime;
using Pages;

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

        SettingsJson jsonSetting;

        private MainWindow mainForm;

        Point scrollMousePoint = new Point();
        double hOff = 1;

        bool checkUrl;

        CancellationTokenSource source = new CancellationTokenSource();

        int marginScroll = 0;

        string fetch_recent;
        string fetch_trending;
        BitmapImage trending_img_card;
        BitmapImage recent_img_card;
        BitmapImage cover_img;

        public Home(MainWindow main)
        {

            mainForm = main;
            InitializeComponent();

            
            jsonSetting = new SettingsJson();
            jsonSetting = SettingsFile.SettingRead();


            

            //DEBUG PURPOSES ONLY
            //System.Windows.MessageBox.Show(jsonSetting.defaultAPILink);


        }

        private async void UiPage_Loaded(object sender, RoutedEventArgs e)
        {
            SearchAnime.Visibility = Visibility.Hidden;
            TrendingStack.Visibility = Visibility.Collapsed;
            RecentStack.Visibility = Visibility.Collapsed;
            HomeLoader.Visibility = Visibility.Visible;

            try
            {
                PatchCheck ANIME_pathcheck = new PatchCheck();
                string errorMSG = ANIME_pathcheck.Animepathchecker();

                checkUrl = await UrlIsValid(jsonSetting.defaultAPILink);

                if (errorMSG != "")
                {
                    throw new Exception(errorMSG);
                }
                else if (checkUrl == false)
                {
                    throw new Exception("API is unreachable.");
                }
                else if(checkUrl == true)
                {
                    await Home_Load();
                }

                SearchAnime.Visibility = Visibility.Visible;
                TrendingStack.Visibility = Visibility.Visible;
                RecentStack.Visibility = Visibility.Visible;
                HomeLoader.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                await Task.Delay(2000);
                mainForm.RootDialog.Title = "Error: Loading Home";
                mainForm.dialogContent.Text = ex.Message;
                mainForm.RootDialog.ButtonLeftName = "Retry";
                mainForm.RootDialog.ButtonLeftClick += RootDialog_ButtonLeftClick;
                mainForm.RootDialog.ButtonRightClick += RootDialog_ButtonRightClick;
                mainForm.RootDialog.Show();

                SearchAnime.Visibility = Visibility.Hidden;
                TrendingStack.Visibility = Visibility.Collapsed;
                RecentStack.Visibility = Visibility.Collapsed;
                HomeLoader.Visibility = Visibility.Visible;

            }

        }

        private void RootDialog_ButtonRightClick(object sender, RoutedEventArgs e)
        {
            HomeLoader.Visibility = Visibility.Hidden;
            mainForm.RootDialog.Hide();
        }

        private async void RootDialog_ButtonLeftClick(object sender, RoutedEventArgs e)
        {
            mainForm.RootDialog.Hide();
            jsonSetting = SettingsFile.SettingRead();
            SearchAnime.Visibility = Visibility.Hidden;
            TrendingStack.Visibility = Visibility.Collapsed;
            RecentStack.Visibility = Visibility.Collapsed;
            HomeLoader.Visibility = Visibility.Visible;

            try
            {
                PatchCheck ANIME_pathcheck = new PatchCheck();
                string errorMSG = ANIME_pathcheck.Animepathchecker();

                checkUrl = await UrlIsValid(jsonSetting.defaultAPILink);

                if (errorMSG != "")
                {
                    throw new Exception(errorMSG);
                }
                else if (checkUrl == false)
                {
                    throw new Exception("API is unreachable.");
                }
                else if(checkUrl == true)
                {
                    await Home_Load();
                }

                SearchAnime.Visibility = Visibility.Visible;
                TrendingStack.Visibility = Visibility.Visible;
                RecentStack.Visibility = Visibility.Visible;
                HomeLoader.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                await Task.Delay(2000);
                mainForm.RootDialog.Title = "Error: Loading Home";
                mainForm.dialogContent.Text = ex.Message;
                mainForm.RootDialog.ButtonLeftName = "Retry";
                mainForm.RootDialog.ButtonLeftClick += RootDialog_ButtonLeftClick;
                mainForm.RootDialog.ButtonRightClick += RootDialog_ButtonRightClick;
                mainForm.RootDialog.Show();

                SearchAnime.Visibility = Visibility.Hidden;
                TrendingStack.Visibility = Visibility.Collapsed;
                RecentStack.Visibility = Visibility.Collapsed;
                HomeLoader.Visibility = Visibility.Visible;

            }
        }


        public async Task Home_Load()
        {
            if (!source.IsCancellationRequested)
            {
                var sw = new Stopwatch();
                sw.Start();
                await Task.WhenAll(fetchTrending(),fetchRecent());
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

        public async Task fetchTrending()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            await trending_urlValidation(jsonSetting.defaultAPILink + jsonSetting.defaultANILIST_TrendingPath);
            

            var client = new HttpClient();
            string trending_anime = await client.GetStringAsync(jsonSetting.defaultAPILink + jsonSetting.defaultANILIST_TrendingPath);

            //System.Windows.MessageBox.Show("TREND: "+trending_anime);
            anilistTrending trending_anilist = JsonConvert.DeserializeObject<anilistTrending>(fetch_trending);

            generateTrending(trending_anilist);

        }

        public async Task fetchRecent()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            await recent_urlValidation(jsonSetting.defaultAPILink + jsonSetting.defaultANILIST_RecentPath);

            var client = new HttpClient();
            //var recent_anime = await client.GetStringAsync(jsonSetting.defaultAPILink + jsonSetting.defaultANILIST_RecentPath);
            anilistRecent recent_anilist = JsonConvert.DeserializeObject<anilistRecent>(fetch_recent);
            generateRecent(recent_anilist);
        }
        public async void generateTrending(anilistTrending trending)
        {
            for (int i = 0; i < trending.results.Length; i++)
            {
                var trending_CardPanel = new System.Windows.Controls.StackPanel();
                var trending_Text = new System.Windows.Controls.TextBlock();
                var trending_Badge = new Wpf.Ui.Controls.Badge();
                var trending_Card = new Wpf.Ui.Controls.Button();

                //Declare and initialize image to bitmap
                await trending_image_urlValidation(trending.results[i].image);
                var httpClint = new HttpClient();
                //var imageBytes = await httpClint.GetStreamAsync(img_card);

                //Cover Artwork without Opacity
                ImageBrush trending_cardCover = new ImageBrush();
                trending_cardCover.Stretch = Stretch.UniformToFill;
                trending_cardCover.ImageSource = trending_img_card;

                //Cover Artwork with Opacity
                ImageBrush trending_cardCoverOpacity = new ImageBrush();
                trending_cardCoverOpacity.Stretch = Stretch.UniformToFill;
                trending_cardCoverOpacity.ImageSource = trending_img_card;
                trending_cardCoverOpacity.Opacity = 0.20;

                //Modify Popular Card Style
                trending_Card.Background = trending_cardCover;
                trending_Card.Height = 275;
                trending_Card.Width = 200;
                trending_Card.MouseOverBackground = trending_cardCoverOpacity;
                trending_Card.Tag = new
                {
                    panel = trending_CardPanel,
                    selected_trending_id = trending.results[i].id,
                    selected_trending_img = trending.results[i].cover
                };


                string releasedate = trending.results[i].releaseDate;
                //string releasedate = "2222";
                string type = trending.results[i].type;

                var title = trending.results[i].title.userPreferred;
                //System.Windows.MessageBox.Show(bb.Length.ToString());


                trending_Badge.FontSize = 14;
                trending_Badge.FontWeight = FontWeights.Bold;
                trending_Badge.Background = Brushes.Gold;
                trending_Badge.VerticalAlignment = VerticalAlignment.Top;
                trending_Badge.HorizontalAlignment = HorizontalAlignment.Center;
                trending_Badge.Content = releasedate + " | " + type;

                trending_Text.FontSize = 18;
                trending_Text.FontWeight = FontWeights.Bold;
                trending_Text.TextAlignment = TextAlignment.Center;
                trending_Text.TextWrapping = TextWrapping.Wrap;
                trending_Text.Text = title;

                trending_CardPanel.Children.Add(trending_Text);
                trending_CardPanel.Children.Add(trending_Badge);

                trending_Card.Content = trending_CardPanel;
                trending_CardPanel.Visibility = Visibility.Hidden;

                //Margin for First Card and Last Card
                if (i == 0)
                {
                    trending_Card.Margin = new System.Windows.Thickness(64, 0, 0, marginScroll);
                }
                else if (i == trending.results.Length - 1)
                {
                    trending_Card.Margin = new System.Windows.Thickness(12, 0, 64, marginScroll);
                }
                else
                {
                    trending_Card.Margin = new System.Windows.Thickness(12, 0, 0, marginScroll);
                }

                //Render and Add the control for the popular Anime
                TrendingPanel.Children.Add(trending_Card);

                trending_Card.Click += new RoutedEventHandler(trending_Cards_Click);
                trending_Card.MouseEnter += new MouseEventHandler(trending_Cards_MouseEnter);
                trending_Card.MouseLeave += new MouseEventHandler(trending_Cards_MouseLeave);
            }
        }

        public async void generateRecent(anilistRecent recent)
        {

            for (int i = 0; i < recent.results.Length; i++)
            {
                var recentCardPanel = new System.Windows.Controls.StackPanel();
                var recentText = new System.Windows.Controls.TextBlock();
                var recentBadge = new Wpf.Ui.Controls.Badge();
                var recentCard = new Wpf.Ui.Controls.Button();

                //Declare and initialize image to bitmap
                await recent_image_urlValidation(recent.results[i].image);

                //OLD BUT STILL WORKS, NO VALIDATION
                /*
                var httpClint = new HttpClient();
                var imageBytes = await httpClint.GetStreamAsync(recent.results[i].image);

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = imageBytes;
                bitmapImage.EndInit();
                */

                //Cover Artwork without Opacity
                ImageBrush cardCover = new ImageBrush();
                cardCover.Stretch = Stretch.UniformToFill;
                cardCover.ImageSource = recent_img_card;

                //Cover Artwork with Opacity
                ImageBrush cardCoverOpacity = new ImageBrush();
                cardCoverOpacity.Stretch = Stretch.UniformToFill;
                cardCoverOpacity.ImageSource = recent_img_card;
                cardCoverOpacity.Opacity = 0.20;

                //Modify Popular Card Style
                recentCard.Background = cardCover;
                recentCard.Height = 225;
                recentCard.Width = 150;
                recentCard.MouseOverBackground = cardCoverOpacity;
                recentCard.ToolTip = "EP "+ recent.results[i].episodeNumber + " | " + recent.results[i].episodeTitle;
                recentCard.Tag = new { 
                    panel = recentCardPanel,
                    selected_recent_id = recent.results[i].id,
                    selected_recent_epid = recent.results[i].episodeId,
                    selected_recent_epnum = (int.Parse(recent.results[i].episodeNumber) - 1).ToString()
                };

                var recent_title = recent.results[i].title.userPreferred;
                var recent_episodeNum = recent.results[i].episodeNumber;
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
                else if (i == recent.results.Length - 1)
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
        private void trending_Cards_Click(object sender, RoutedEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonThatWasClicked = (Wpf.Ui.Controls.Button)sender;
            string trending_id = ((dynamic)buttonThatWasClicked.Tag).selected_trending_id;
            string trending_selected_img = ((dynamic)buttonThatWasClicked.Tag).selected_trending_img;

            this.NavigationService.Navigate(new SelectedAnime(trending_id, trending_selected_img));
            //System.Windows.MessageBox.Show(test.ToString());

        }

        private async void trending_Cards_MouseEnter(object sender, MouseEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonMouseEnter = (Wpf.Ui.Controls.Button)sender;
            System.Windows.Controls.StackPanel panel = ((dynamic)buttonMouseEnter.Tag).panel;
            string trending_selected_img = ((dynamic)buttonMouseEnter.Tag).selected_trending_img;


            //Declare and initialize image to bitmap
            await cover_image_urlValidation(trending_selected_img);

            panel.Visibility = Visibility.Visible;
            HomeCover.ImageSource = cover_img;
            DoubleAnimation da = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromSeconds(1))
            };
            PanelCover.BeginAnimation(OpacityProperty, da);

            var scaler = buttonMouseEnter.LayoutTransform as ScaleTransform;

            if (scaler == null)
            {
                scaler = new ScaleTransform(1.0, 1.0);
                buttonMouseEnter.LayoutTransform = scaler;
            }

            // We'll need a DoubleAnimation object to drive
            // the ScaleX and ScaleY properties.

            DoubleAnimation animator = new DoubleAnimation()
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            };

            animator.To = 1.2;

            scaler.BeginAnimation(ScaleTransform.ScaleXProperty, animator);
            scaler.BeginAnimation(ScaleTransform.ScaleYProperty, animator);


        }

        private void trending_Cards_MouseLeave(object sender, MouseEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonMouseEnter = (Wpf.Ui.Controls.Button)sender;
            System.Windows.Controls.StackPanel panel = ((dynamic)buttonMouseEnter.Tag).panel;

            panel.Visibility = Visibility.Hidden;

            var scaler = buttonMouseEnter.LayoutTransform as ScaleTransform;

            if (scaler == null)
            {
                scaler = new ScaleTransform(1.0, 1.0);
                buttonMouseEnter.LayoutTransform = scaler;
            }

            // We'll need a DoubleAnimation object to drive
            // the ScaleX and ScaleY properties.

            DoubleAnimation animator = new DoubleAnimation()
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(100)),
            };

            animator.To = 1.0;

            scaler.BeginAnimation(ScaleTransform.ScaleXProperty, animator);
            scaler.BeginAnimation(ScaleTransform.ScaleYProperty, animator);
        }

        //Recent Card Events
        private void recentCards_Click(object sender, RoutedEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonThatWasClicked = (Wpf.Ui.Controls.Button)sender;
            string id = ((dynamic)buttonThatWasClicked.Tag).selected_recent_id;
            string epid = ((dynamic)buttonThatWasClicked.Tag).selected_recent_epid;
            string epnum = ((dynamic)buttonThatWasClicked.Tag).selected_recent_epnum;
            
            //System.Windows.MessageBox.Show(id+" | "+epid+" | "+epnum);

            this.NavigationService.Navigate(new PlayEpisode(id, epid, epnum));

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
        private void svTrendingScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
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

        private void svTrending_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            scrollMousePoint = e.GetPosition(svTrending);
            hOff = svTrending.HorizontalOffset;
            svTrending.CaptureMouse();
        }

        private void svTrending_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (svTrending.IsMouseCaptured)
            {
                svTrending.ScrollToHorizontalOffset(hOff + (scrollMousePoint.X - e.GetPosition(svTrending).X));
            }
        }

        private void svTrending_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            svTrending.ReleaseMouseCapture();
        }

        private void svTrending_MouseEnter(object sender, MouseEventArgs e)
        {
            svTrending.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;

        }

        private void svTrending_MouseLeave(object sender, MouseEventArgs e)
        {
            svTrending.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
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
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            

            if (response.IsSuccessStatusCode)
            {
                //mainForm.PageLoading.Visibility = Visibility.Hidden;
                //System.Windows.MessageBox.Show(response.StatusCode.ToString());
                return true;
            }
            else
            {
                //mainForm.PageLoading.Visibility = Visibility.Visible;

                //System.Windows.MessageBox.Show(response.StatusCode.ToString());
                //error path
                //example: retry, check error content, log error, throw exception

                //Console.WriteLine($"Request failed. Error status code: {(int)response.StatusCode}");
                return false;
            }
        }

        public async Task recent_urlValidation(string url)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = new HttpResponseMessage();
            bool checker = false;
            while (checker == false)
            {
                response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    checker = true;
                    var getResponsestring = await response.Content.ReadAsStringAsync();
                    //System.Windows.MessageBox.Show("Success Recent EP");
                    fetch_recent = getResponsestring;
                }
            }
            
            
        }

        public async Task trending_urlValidation(string url)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = new HttpResponseMessage();
            bool checker = false;
            while (checker == false)
            {
                response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    checker = true;
                    var getResponsestring = await response.Content.ReadAsStringAsync();
                    //System.Windows.MessageBox.Show("Success Trending EP");
                    fetch_trending = getResponsestring;
                }
            }

        }

        public async Task trending_image_urlValidation(string url)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = new HttpResponseMessage();
            
            response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var getResponsestring = await response.Content.ReadAsStreamAsync();
                    //System.Windows.MessageBox.Show("Success Trending IMG");
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = getResponsestring;
                    bitmapImage.EndInit();
                    trending_img_card = bitmapImage;
                }
                catch (Exception ex)
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string imageRelativePath = "Images\\Background\\Noimage.png";
                    string imagePath = Path.Combine(baseDirectory, imageRelativePath);

                    var bitmap = new System.Drawing.Bitmap(imagePath);
                    trending_img_card = ToBitmapImage(bitmap);
                }
            }
            else
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string imageRelativePath = "Images\\Background\\Noimage.png";
                string imagePath = Path.Combine(baseDirectory, imageRelativePath);

                var bitmap = new System.Drawing.Bitmap(imagePath);
                trending_img_card = ToBitmapImage(bitmap);

                //img_card = ToBitmapImage(bitmap);

                //var tet = new ImageBrush(new BitmapImage(new Uri(@"C:\Users\Krystler\source\repos\OzakiAnimeWPF\OzakiAnimeWPF\Images\Background\Noimage.png", UriKind.Relative)));
                //img_card = new BitmapImage(new Uri("pack://application:,,,/Images/Background/Noimage.png"));
                //System.Windows.MessageBox.Show("here");
            }
            

        }

        public async Task recent_image_urlValidation(string url)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = new HttpResponseMessage();

            response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var getResponsestring = await response.Content.ReadAsStreamAsync();
                    //System.Windows.MessageBox.Show("Success Trending IMG");
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = getResponsestring;
                    bitmapImage.EndInit();
                    recent_img_card = bitmapImage;
                }
                catch (Exception ex)
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string imageRelativePath = "Images\\Background\\Noimage.png";
                    string imagePath = Path.Combine(baseDirectory, imageRelativePath);

                    var bitmap = new System.Drawing.Bitmap(imagePath);
                    recent_img_card = ToBitmapImage(bitmap);
                }
            }
            else
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string imageRelativePath = "Images\\Background\\Noimage.png";
                string imagePath = Path.Combine(baseDirectory, imageRelativePath);

                var bitmap = new System.Drawing.Bitmap(imagePath);
                recent_img_card = ToBitmapImage(bitmap);

                //img_card = ToBitmapImage(bitmap);

                //var tet = new ImageBrush(new BitmapImage(new Uri(@"C:\Users\Krystler\source\repos\OzakiAnimeWPF\OzakiAnimeWPF\Images\Background\Noimage.png", UriKind.Relative)));
                //img_card = new BitmapImage(new Uri("pack://application:,,,/Images/Background/Noimage.png"));
                //System.Windows.MessageBox.Show("here");
            }


        }

        public async Task cover_image_urlValidation(string url)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = new HttpResponseMessage();

            response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var getResponsestring = await response.Content.ReadAsStreamAsync();
                    //System.Windows.MessageBox.Show("Success Trending IMG");
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = getResponsestring;
                    bitmapImage.EndInit();
                    cover_img = bitmapImage;
                }
                catch (Exception ex)
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string imageRelativePath = "Images\\Background\\Noimage.png";
                    string imagePath = Path.Combine(baseDirectory, imageRelativePath);

                    var bitmap = new System.Drawing.Bitmap(imagePath);
                    cover_img = ToBitmapImage(bitmap);
                }
            }
            else
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string imageRelativePath = "Images\\Background\\Noimage.png";
                string imagePath = Path.Combine(baseDirectory, imageRelativePath);

                var bitmap = new System.Drawing.Bitmap(imagePath);
                cover_img = ToBitmapImage(bitmap);

                //img_card = ToBitmapImage(bitmap);

                //var tet = new ImageBrush(new BitmapImage(new Uri(@"C:\Users\Krystler\source\repos\OzakiAnimeWPF\OzakiAnimeWPF\Images\Background\Noimage.png", UriKind.Relative)));
                //img_card = new BitmapImage(new Uri("pack://application:,,,/Images/Background/Noimage.png"));
                //System.Windows.MessageBox.Show("here");
            }


        }

        public static BitmapImage ToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
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

        private void SearchAnime_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.NavigationService.Navigate(new AdvancedSearch(SearchAnime.Text));
            }
        }
    }
}
