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
using System.Linq;
using System.Runtime.CompilerServices;
using System.DirectoryServices.ActiveDirectory;

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

        string advSearchParameter;
        string query;
        string userInputPARAM;
        string GenrePARAM;
        string YearPARAM;
        string SeasonPARAM;
        string FormatPARAM;
        string AiringStatusPARAM;

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
            generateSeason();
            generateFormat();
            generateAiringStatus();

            txtSearch.Text = UserSearchContent;
            advSearchParameter = jsonSetting.defaultAPILink + jsonSetting.defaultANILIST_AdvSearchPath + "?"+ "perPage=50";
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

        public void generateSeason()
        {
            var lblSeason = new TextBlock();
            lblSeason.Text = "Any";
            lblSeason.FontWeight = FontWeights.Medium;
            cboSeason.Items.Add(lblSeason);
            cboSeason.Items.Add("Winter");
            cboSeason.Items.Add("Spring");
            cboSeason.Items.Add("Summer");
            cboSeason.Items.Add("Fall");

        }

        public void generateFormat()
        {
            var lblFormat = new TextBlock();
            lblFormat.Text = "Any";
            lblFormat.FontWeight = FontWeights.Medium;
            cboFormat.Items.Add(lblFormat);
            cboFormat.Items.Add("TV Show");
            cboFormat.Items.Add("TV Short");
            cboFormat.Items.Add("OVA");
            cboFormat.Items.Add("ONA");
            cboFormat.Items.Add("Movie");
            cboFormat.Items.Add("Special");
            cboFormat.Items.Add("Music");
        }

        public void generateAiringStatus()
        {
            var lblAiring = new TextBlock();
            lblAiring.Text = "Any";
            lblAiring.FontWeight = FontWeights.Medium;
            cboAiringStatus.Items.Add(lblAiring);
            cboAiringStatus.Items.Add("Airing");
            cboAiringStatus.Items.Add("Finished");
            cboAiringStatus.Items.Add("Not Yet Aired");
            cboAiringStatus.Items.Add("Cancelled");
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

            //CHECK FOR PARAMETERS FIRST
            checkUserInput();
            checkGenres();
            checkYear();
            checkSeason();
            checkFormat();
            checkAiring();

            //BUILD THE QUERY
            queryBuilder();


            //System.Windows.MessageBox.Show(query);
            await searchAnime_urlValidation(query);
            

            var client = new HttpClient();
            //string selected_anime = await client.GetStringAsync(jsonSetting.defaultAPILink + jsonSetting.defaultANILIST_AnimeInfoPath + trending_id);
            animeSearch = JsonConvert.DeserializeObject<AnilistAdvancedSearch>(animeSearchJSON);

            await Task.WhenAll(generateResult(animeSearch));
            //await generateResult(animeSearch);
        }


        public async Task generateResult(AnilistAdvancedSearch query)
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

        //BUILDS THE HTTP REQUEST
        public void queryBuilder()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(advSearchParameter);

            if (userInputPARAM != "")
            {
                sb.Append("&"+userInputPARAM);
            }
            if (GenrePARAM != "")
            {
                sb.Append("&"+GenrePARAM);
            }
            if (YearPARAM != "")
            {
                sb.Append("&" + YearPARAM);
            }
            if (SeasonPARAM != "")
            {
                sb.Append("&" + SeasonPARAM);
            }
            if (FormatPARAM != "")
            {
                sb.Append("&" + FormatPARAM);
            }
            if (AiringStatusPARAM != "")
            {
                sb.Append("&" + AiringStatusPARAM);
            }

            query = sb.ToString();
        }
        //CHECK FOR USER INPUT
        public void checkUserInput()
        {
            string queryUserinput = txtSearch.Text.Replace(" ", "");
            if (queryUserinput.Length >= 1)
            {
                userInputPARAM = "query=" + txtSearch.Text;
            }
            else
            {
                userInputPARAM = "";
            }
        }

        //CHECK FOR GENRES
        public void checkGenres()
        {
            List<string> genres = new List<string>();
            string[] genresArray;
            if (cboAction.IsChecked == true)
            {
                genres.Add("\"Action\"");
            }
            if (cboAdventure.IsChecked == true)
            {
                genres.Add("\"Adventure\"");
            }
            if (cboCars.IsChecked == true)
            {
                genres.Add("\"Cars\"");
            }
            if (cboComedy.IsChecked == true)
            {
                genres.Add("\"Comedy\"");
            }
            if (cboDrama.IsChecked == true)
            {
                genres.Add("\"Drama\"");
            }
            if (cboEcchi.IsChecked == true)
            {
                genres.Add("\"Ecchi\"");
            }
            if (cboFantasy.IsChecked == true)
            {
                genres.Add("\"Fantasy\"");
            }
            if (cboHorror.IsChecked == true)
            {
                genres.Add("\"Horror\"");
            }
            if (cboMahouShoujo.IsChecked == true)
            {
                genres.Add("\"Mahou Shoujo\"");
            }
            if (cboMecha.IsChecked == true)
            {
                genres.Add("\"Mecha\"");
            }
            if (cboMusic.IsChecked == true)
            {
                genres.Add("\"Music\"");
            }
            if (cboMystery.IsChecked == true)
            {
                genres.Add("\"Mystery\"");
            }
            if (cboPsychological.IsChecked == true)
            {
                genres.Add("\"Psychological\"");
            }
            if (cboRomance.IsChecked == true)
            {
                genres.Add("\"Romance\"");
            }
            if (cboSciFi.IsChecked == true)
            {
                genres.Add("\"Sci-Fi\"");
            }
            if (cboSliceofLife.IsChecked == true)
            {
                genres.Add("\"Slice of Life\"");
            }
            if (cboSports.IsChecked == true)
            {
                genres.Add("\"Sports\"");
            }
            if (cboSupernatural.IsChecked == true)
            {
                genres.Add("\"Supernatural\"");
            }
            if (cboThriller.IsChecked == true)
            {
                genres.Add("\"Thriller\"");
            }
            if (cboAny.IsChecked == true)
            {
                genres.Clear();
            }
            genresArray = genres.ToArray();
            if (genresArray.Length != 0)
            {
                GenrePARAM = "genres=[" + String.Join(",", genresArray) + "]";
            }
            else
            {
                GenrePARAM = "";
            }

            //System.Windows.MessageBox.Show(GenrePARAM);
        }

        //CHECK FOR YEAR
        public void checkYear()
        {
            if (cboYear.SelectedIndex != 0)
            {
                YearPARAM = "year=" + cboYear.SelectedItem.ToString();
            }
            else
            {
                YearPARAM = "";
            }
        }

        //CHECK FOR SEASON
        public void checkSeason()
        {
            if (cboSeason.SelectedIndex != 0)
            {
                SeasonPARAM = "season=" + cboSeason.SelectedItem.ToString().ToUpper();
            }
            else
            {
                SeasonPARAM = "";
            }
        }

        //CHECK FOR FORMAT
        public void checkFormat()
        {
            if (cboFormat.SelectedIndex != 0)
            {
                string format = cboFormat.SelectedItem.ToString().ToUpper();
                string formatSTEP1 = format.Replace(" ", "_");
                if (formatSTEP1 == "TV_SHOW")
                {
                    formatSTEP1 = "TV";
                    FormatPARAM = "format=" + formatSTEP1;
                }
                else
                {
                    FormatPARAM = "format=" + formatSTEP1;
                }
            }
            else
            {
                FormatPARAM = "";
            }
        }

        //CHECK FOR AIRING STUTUS
        public void checkAiring()
        {
            if (cboAiringStatus.SelectedIndex != 0)
            {
                string status = cboAiringStatus.SelectedItem.ToString().ToUpper();
                string statusSTEP1 = status.Replace(" ", "_");

                if (statusSTEP1 == "AIRING")
                {
                    statusSTEP1 = "RELEASING";
                    AiringStatusPARAM = "status=" + statusSTEP1;
                }
                else if (statusSTEP1 == "NOT_YET_AIRED")
                {
                    statusSTEP1 = "NOT_YET_RELEASED";
                    AiringStatusPARAM = "status=" + statusSTEP1;
                }
                else
                {
                    AiringStatusPARAM = "status=" + statusSTEP1;
                }
            }
            else
            {
                AiringStatusPARAM = "";
            }
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
