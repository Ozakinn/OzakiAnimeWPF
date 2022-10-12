using Newtonsoft.Json;
using OzakiAnimeWPF.Data;
using OzakiAnimeWPF.Pages;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;
using OzakiAnimeWPF;
using System.Windows.Markup;
using System.Drawing.Imaging;
using System.IO;
using Ozaki_Anime.Pages.SelectedAnime;

namespace Pages
{
    /// <summary>
    /// Interaction logic for AdvancedSearch.xaml
    /// </summary>
    public partial class AdvancedSearch : UiPage
    {
        string UserSearchContent;
        bool isTextChange;

        string animeSearchJSON;
        AnilistAdvancedSearch animeSearch;
        SettingsJson jsonSetting;

        BitmapImage card_imgs;
        //avoid duplicate of page load
        private bool _isLoaded;
        public AdvancedSearch(string userSearch)
        {
            UserSearchContent = userSearch;
            jsonSetting = new SettingsJson();
            jsonSetting = SettingsFile.SettingRead();

            InitializeComponent();
            generateYear();

            txtSearch.Text = UserSearchContent;
        }

        public void generateYear()
        {
            var lblGenre = new TextBlock();
            lblGenre.Text = "Any";
            lblGenre.FontWeight = FontWeights.Medium;
            cboYear.Items.Add(lblGenre);

            for (int i = DateTime.Now.Year; i >= 1940; i--)
            {
                cboYear.Items.Add(i);
            }
        }

        private async void UiPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;
                PageLoad.Visibility = Visibility.Visible;
                await Search_Load();
                PageLoad.Visibility = Visibility.Hidden;
            }
        }

        private async void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                isTextChange = true;
                SearchLoad.Visibility = Visibility.Visible;
                ResultPanel.Visibility = Visibility.Hidden;
                await Task.WhenAll(fetch_SearchAnime());
                SearchLoad.Visibility = Visibility.Collapsed;
            }
        }

        public async Task Search_Load()
        {
            SearchLoad.Visibility = Visibility.Visible;
            ResultPanel.Visibility = Visibility.Hidden;
            await Task.WhenAll(fetch_SearchAnime());
            SearchLoad.Visibility = Visibility.Collapsed;
        }

        public async Task fetch_SearchAnime()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            string query = txtSearch.Text.Replace(" ","");
            string paramKey = "query=" + txtSearch.Text;
            string paramPerPage = "perPage=50";
            if (query.Length >= 1)
            {
                await searchAnime_urlValidation(jsonSetting.defaultAPILink + jsonSetting.defaultANILIST_AdvSearchPath + "?" + paramKey + "&" + paramPerPage);
            }
            else
            {
                await searchAnime_urlValidation(jsonSetting.defaultAPILink + jsonSetting.defaultANILIST_AdvSearchPath + "?" + paramPerPage);
            }

            var client = new HttpClient();
            //string selected_anime = await client.GetStringAsync(jsonSetting.defaultAPILink + jsonSetting.defaultANILIST_AnimeInfoPath + trending_id);
            //System.Windows.MessageBox.Show(jsonSetting.defaultAPILink + jsonSetting.defaultANILIST_AnimeInfoPath + trending_id);
            animeSearch = JsonConvert.DeserializeObject<AnilistAdvancedSearch>(animeSearchJSON);
            
            generateResult(animeSearch);
        }


        public async void generateResult(AnilistAdvancedSearch query)
        {
            WrapPanel dummy = new WrapPanel();
            for (int i = 0; i < query.results.Length; i++)
            {
                var result_CardPanel = new System.Windows.Controls.StackPanel();
                var result_Text = new System.Windows.Controls.TextBlock();
                var result_Badge = new Wpf.Ui.Controls.Badge();
                var result_Card = new Wpf.Ui.Controls.Button();

                //Declare and initialize image to bitmap
                await card_image_urlValidation(query.results[i].image);
                /*
                var httpClint = new HttpClient();
                var imageBytes = await httpClint.GetStreamAsync(data.episodes[i].image);

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = imageBytes;
                bitmapImage.EndInit();
                */

                //Cover Artwork without Opacity
                ImageBrush cardCover = new ImageBrush();
                cardCover.Stretch = Stretch.UniformToFill;
                cardCover.Opacity = 0.20;
                cardCover.ImageSource = card_imgs;


                //Cover Artwork with Opacity
                ImageBrush cardCoverOpacity = new ImageBrush();
                cardCoverOpacity.Stretch = Stretch.UniformToFill;
                cardCoverOpacity.ImageSource = card_imgs;
                cardCoverOpacity.Opacity = 1;

                //Modify Result Card Style
                result_Card.Background = cardCover;
                result_Card.Height = 240;
                result_Card.Width = 165;
                result_Card.MouseOverBackground = cardCoverOpacity;
                result_Card.ToolTip = query.results[i].status;
                result_Card.Tag = new
                {
                    panel = result_CardPanel,
                    id = query.results[i].id,
                    cover = query.results[i].cover
                };

                var title = query.results[i].title.userPreferred;
                string releasedate = query.results[i].releaseDate;
                string type = query.results[i].type;

                result_Badge.FontSize = 10;
                result_Badge.FontWeight = FontWeights.Medium;
                result_Badge.Background = Brushes.LimeGreen;
                result_Badge.VerticalAlignment = VerticalAlignment.Top;
                result_Badge.HorizontalAlignment = HorizontalAlignment.Center;
                result_Badge.Content = releasedate + " | " + type;

                result_Text.FontSize = 16;
                result_Text.FontWeight = FontWeights.Bold;
                result_Text.TextAlignment = TextAlignment.Center;
                result_Text.TextWrapping = TextWrapping.Wrap;
                result_Text.Text = title;

                result_CardPanel.Name = "result_CardPanel";
                result_CardPanel.Children.Add(result_Text);
                result_CardPanel.Children.Add(result_Badge);

                result_Card.Content = result_CardPanel;
                result_CardPanel.Visibility = Visibility.Visible;

                result_Card.Margin = new System.Windows.Thickness(7.5, 7.5, 7.5, 7.5);

                //Render and Add the control for the episodes
                dummy.Children.Add(result_Card);
                result_Card.Click += new RoutedEventHandler(result_Card_Click);
                result_Card.MouseEnter += new MouseEventHandler(result_Card_MouseEnter);
                result_Card.MouseLeave += new MouseEventHandler(result_Card_MouseLeave);
            }

            
            var dummyChild = dummy.Children;
            List<UIElement> elements = new List<UIElement>();
            ResultPanel.Children.Clear();

            foreach (UIElement yow in dummyChild)
            {
                elements.Add(yow);
            }

            foreach (UIElement childs in elements)
            {

                var parent = VisualTreeHelper.GetParent(childs);
                var parentAsPanel = parent as Panel;
                if (parentAsPanel != null)
                {
                    parentAsPanel.Children.Remove(childs);
                }
                var parentAsContentControl = parent as ContentControl;
                if (parentAsContentControl != null)
                {
                    parentAsContentControl.Content = null;
                }
                var parentAsDecorator = parent as Decorator;
                if (parentAsDecorator != null)
                {
                    parentAsDecorator.Child = null;
                }

                ResultPanel.Children.Add(childs);
            }

            ResultPanel.Visibility = Visibility.Visible;

        }
        private void result_Card_Click(object sender, RoutedEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonThatWasClicked = (Wpf.Ui.Controls.Button)sender;
            string id = ((dynamic)buttonThatWasClicked.Tag).id;
            string cover = ((dynamic)buttonThatWasClicked.Tag).cover;

            this.NavigationService.Navigate(new SelectedAnime(id, cover));

        }

        private void result_Card_MouseEnter(object sender, MouseEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonMouseEnter = (Wpf.Ui.Controls.Button)sender;
            System.Windows.Controls.StackPanel panel = ((dynamic)buttonMouseEnter.Tag).panel;

            panel.Visibility = Visibility.Hidden;
        }
        private void result_Card_MouseLeave(object sender, MouseEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonMouseEnter = (Wpf.Ui.Controls.Button)sender;
            System.Windows.Controls.StackPanel panel = ((dynamic)buttonMouseEnter.Tag).panel;

            panel.Visibility = Visibility.Visible;
        }

        public async Task searchAnime_urlValidation(string url)
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
                    //System.Windows.MessageBox.Show("Success selected anime");
                    animeSearchJSON = getResponsestring;
                }
            }

        }

        public async Task card_image_urlValidation(string url)
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
                    card_imgs = bitmapImage;
                }
                catch (Exception ex)
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string imageRelativePath = "Images\\Background\\Noimage.png";
                    string imagePath = System.IO.Path.Combine(baseDirectory, imageRelativePath);

                    var bitmap = new System.Drawing.Bitmap(imagePath);
                    card_imgs = ToBitmapImage(bitmap);
                }
            }
            else
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string imageRelativePath = "Images\\Background\\Noimage.png";
                string imagePath = System.IO.Path.Combine(baseDirectory, imageRelativePath);

                var bitmap = new System.Drawing.Bitmap(imagePath);
                card_imgs = ToBitmapImage(bitmap);

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
    }
}
