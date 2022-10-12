using Newtonsoft.Json;
using Ozaki_Anime.Pages.SelectedAnime;
using OzakiAnimeWPF.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;

namespace OzakiAnimeWPF.Pages
{
    /// <summary>
    /// Interaction logic for SelectedAnime.xaml
    /// </summary>
    public partial class SelectedAnime : UiPage
    {
        string trending_id;
        string trending_Img;

        string selected_anime;
        BitmapImage ep_imgs;
        BitmapImage char_imgs;
        BitmapImage charVA_imgs;
        BitmapImage rl_imgs;
        BitmapImage reco_imgs;

        //Settings API config
        List<string> settingsAPI = new List<string>();

        SettingsJson jsonSetting;

        //URL checker
        bool checkUrl;

        // Cancel Async
        CancellationTokenSource source = new CancellationTokenSource();


        DateTime AirDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        string ep;
        int marginScroll = 0;
        anilist_Info selectedanime;

        int lastEPCount = 0;
        bool isViewMore = false;

        //avoid duplicate of page load
        private bool _isLoaded;
        public SelectedAnime(string trendingid, string trending_img)
        {

            trending_id = trendingid;
            trending_Img = trending_img;

            jsonSetting = new SettingsJson();
            jsonSetting = SettingsFile.SettingRead();

            InitializeComponent();

            //this.Loaded += UiPage_Loaded;
        }

        private async void UiPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {

                _isLoaded = true;
                PageLoad.Visibility = Visibility.Visible;
                EPStack.Visibility = Visibility.Hidden;


                await SelectedAnime_Load();

                EPStack.Visibility = Visibility.Visible;
                PageLoad.Visibility = Visibility.Hidden;

            }


        }

        public async Task SelectedAnime_Load()
        {
            if (!source.IsCancellationRequested)
            {
                var sw = new Stopwatch();
                sw.Start();
                await Task.WhenAll(fetch_SelectedAnime());
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

        public async Task fetch_SelectedAnime()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            await selectedanime_urlValidation(jsonSetting.defaultAPILink + jsonSetting.defaultANILIST_AnimeInfoPath + trending_id);

            var client = new HttpClient();
            //string selected_anime = await client.GetStringAsync(jsonSetting.defaultAPILink + jsonSetting.defaultANILIST_AnimeInfoPath + trending_id);
            //System.Windows.MessageBox.Show(jsonSetting.defaultAPILink + jsonSetting.defaultANILIST_AnimeInfoPath + trending_id);
            selectedanime = JsonConvert.DeserializeObject<anilist_Info>(selected_anime);

            renderData(selectedanime);
        }

        public void renderData(anilist_Info info)
        {
            EpisodeMenu(info);

            //CHECK IF CHARACTERS IS AVAILABLE
            if (info.characters is not null)
            {
                SelectedAnime_CharactersMenu.Visibility = Visibility.Visible;
                CharactersMenu(info);
            }
            else
            {
                SelectedAnime_CharactersMenu.Visibility = Visibility.Collapsed;
            }
            //CHECK IF RELATIONS IS AVAILABLE
            if (info.relations is not null)
            {
                SelectedAnime_RelationsMenu.Visibility = Visibility.Visible;
                RelationsMenu(info);
            }
            else
            {
                SelectedAnime_RelationsMenu.Visibility = Visibility.Collapsed;
            }
            //CHECK IF RECOMMENDATIONS IS AVAILABLE
            if (info.recommendations is not null)
            {
                SelectedAnime_ReccomendationsMenu.Visibility = Visibility.Visible;
                RecommendationsMenu(info);
            }
            else
            {
                SelectedAnime_ReccomendationsMenu.Visibility = Visibility.Collapsed;
            }
        }

        public async void EpisodeMenu(anilist_Info info)
        {
            //Declare and initialize image to bitmap
            await ep_image_urlValidation(info.image);

            //IMAGE DETAILS
            animeImg.ImageSource = ep_imgs;
            //AIRING COUNTDOWN
            if (info.nextAiringEpisode is not null)
            {
                AiringPanel.Visibility = Visibility.Visible;
                calculateAiring(info.nextAiringEpisode.airingTime, info.nextAiringEpisode.timeUntilAiring, info.nextAiringEpisode.episode.ToString());
            }
            else
            {
                AiringPanel.Visibility = Visibility.Collapsed;
            }
            //FORMAT
            if (info.type is not null)
            {
                FormatPanel.Visibility = Visibility.Visible;
                FormatControl.Text = info.type;
            }
            else
            {
                FormatPanel.Visibility = Visibility.Collapsed;
            }
            //EPISODES
            if (info.totalEpisodes is not null)
            {
                EpisodesPanel.Visibility = Visibility.Visible;
                EpisodesControl.Text = info.totalEpisodes;
            }
            else
            {
                EpisodesPanel.Visibility = Visibility.Collapsed;
            }
            //EPISODE DURATION
            if (info.duration is not null)
            {
                EpisodeDurationPanel.Visibility = Visibility.Visible;
                EpisodeDurationControl.Text = info.duration + " mins";
            }
            else
            {
                EpisodeDurationPanel.Visibility = Visibility.Collapsed;
            }
            //STATUS
            if (info.status is not null)
            {
                StatusPanel.Visibility = Visibility.Visible;
                StatusControl.Text = info.status;
            }
            else
            {
                StatusPanel.Visibility = Visibility.Collapsed;
            }
            //RELEASE DATE
            //sa API this is startDate
            if (info.startDate is not null)
            {
                ReleaseDatePanel.Visibility = Visibility.Visible;
                releaseDate(info.startDate.year, info.startDate.month, info.startDate.day);
            }
            else
            {
                ReleaseDatePanel.Visibility = Visibility.Collapsed;
            }
            //END DATE
            if (info.endDate is not null)
            {
                EndDatePanel.Visibility = Visibility.Visible;
                endDate(info.endDate.year, info.endDate.month, info.endDate.day);
            }
            else
            {
                EndDatePanel.Visibility = Visibility.Collapsed;
            }
            //SEASON
            if (info.season is not null && info.releaseDate is not null)
            {
                SeasonPanel.Visibility = Visibility.Visible;
                SeasonControl.Text = info.season + " " + info.releaseDate;
            }
            else
            {
                SeasonPanel.Visibility = Visibility.Collapsed;
            }
            //AVERAGE SCORE
            if (info.rating is not null)
            {
                AvgScorePanel.Visibility = Visibility.Visible;
                AvgScoreControl.Text = info.rating + "%";
            }
            else
            {
                AvgScorePanel.Visibility = Visibility.Collapsed;
            }
            //POPULARITY
            if (info.rating is not null)
            {
                PopularityPanel.Visibility = Visibility.Visible;
                PopularityControl.Text = info.popularity;
            }
            else
            {
                PopularityPanel.Visibility = Visibility.Collapsed;
            }
            //STUDIOS
            if (info.studios is not null)
            {
                StudiosPanel.Visibility = Visibility.Visible;
                StudiosControl.Text = string.Join(", ", info.studios);
            }
            else
            {
                StudiosPanel.Visibility = Visibility.Collapsed;
            }
            //GENRES
            //should be clickable next time
            if (info.genres is not null)
            {
                GenresPanel.Visibility = Visibility.Visible;
                GenresControl.Text = string.Join("\n", info.genres);
            }
            else
            {
                GenresPanel.Visibility = Visibility.Collapsed;
            }

            //ANIME COVER
            animeCoverBitImage(info.cover);
            //TITLE
            AnimeTitle.Text = info.title.romaji;
            //ALT TITLE
            if (info.title is not null && info.synonyms is not null)
            {
                AnimeAltTitlePanel.Visibility = Visibility.Visible;
                AnimeAltTitle.Text = info.title.english + ", " + info.title.native + " | " + string.Join(", ", info.synonyms);
            }
            else
            {
                AnimeAltTitlePanel.Visibility = Visibility.Collapsed;
            }
            //DESCRIPTION
            if (info.description is not null)
            {
                AnimeDescription.Text = RemoveHTMLTags(info.description);
            }
            else
            {
                AnimeDescription.Text = "No Available Descriptions!";
            }

            //GENERATE EPISODES CARD
            GenerateEpisodesCard(info, isViewMore);
        }

        public async void CharactersMenu(anilist_Info info)
        {
            for (int i = 0; i < info.characters.Length; i++)
            {
                var charBorder = new Border();
                var charGrid = new Grid();
                var charImg = new Border();
                var charVA = new Border();
                var charName = new TextBlock();
                var charRole = new TextBlock();
                var charNameVA = new TextBlock();

                // charGrid
                ColumnDefinition gridCol1 = new ColumnDefinition();
                gridCol1.Width = GridLength.Auto;
                ColumnDefinition gridCol2 = new ColumnDefinition();
                gridCol2.Width = new GridLength(1, GridUnitType.Star);
                ColumnDefinition gridCol3 = new ColumnDefinition();
                gridCol3.Width = GridLength.Auto;

                charGrid.ColumnDefinitions.Add(gridCol1);
                charGrid.ColumnDefinitions.Add(gridCol2);
                charGrid.ColumnDefinitions.Add(gridCol3);

                RowDefinition gridRow1 = new RowDefinition();
                gridRow1.Height = GridLength.Auto;
                RowDefinition gridRow2 = new RowDefinition();
                gridRow2.Height = GridLength.Auto;
                RowDefinition gridRow3 = new RowDefinition();
                gridRow3.Height = new GridLength(1, GridUnitType.Star);

                charGrid.RowDefinitions.Add(gridRow1);
                charGrid.RowDefinitions.Add(gridRow2);
                charGrid.RowDefinitions.Add(gridRow3);

                //charGridMiddle
                /*
                ColumnDefinition gridMidCol1 = new ColumnDefinition();
                gridMidCol1.Width = GridLength.Auto;
                ColumnDefinition gridMidCol2 = new ColumnDefinition();
                gridMidCol2.Width = new GridLength(1, GridUnitType.Star);
                ColumnDefinition gridMidCol3 = new ColumnDefinition();
                gridMidCol3.Width = GridLength.Auto;

                charGridMiddle.ColumnDefinitions.Add(gridMidCol1);
                charGridMiddle.ColumnDefinitions.Add(gridMidCol2);
                charGridMiddle.ColumnDefinitions.Add(gridMidCol3);

                RowDefinition gridMidRow1 = new RowDefinition();
                gridMidRow1.Height = GridLength.Auto;
                RowDefinition gridMidRow2 = new RowDefinition();
                gridMidRow2.Height = new GridLength(1, GridUnitType.Star);

                charGridMiddle.RowDefinitions.Add(gridMidRow1);
                charGridMiddle.RowDefinitions.Add(gridMidRow2);

                Grid.SetColumn(charGridMiddle, 1);
                Grid.SetRow(charGridMiddle, 0);
                Grid.SetRowSpan(charGridMiddle, 2);
                */

                // charBorder
                charBorder.Width = 510;
                charBorder.Height = 150;
                charBorder.CornerRadius = new CornerRadius(3);
                BrushConverter bc = new BrushConverter();
                Brush brush = (Brush)bc.ConvertFrom("#242424");
                brush.Freeze();
                charBorder.Background = brush;
                charBorder.Margin = new System.Windows.Thickness(15, 7.5, 0, 7.5);
                charBorder.Child = charGrid;

                // Character Image
                await charimg_urlValidation(info.characters[i].image);

                ImageBrush charIMG = new ImageBrush();
                charIMG.Stretch = Stretch.UniformToFill;
                charIMG.ImageSource = char_imgs;
                charIMG.Opacity = 1;

                charImg.Background = charIMG;
                charImg.Height = 150;
                charImg.Width = 125;
                charImg.CornerRadius = new CornerRadius(4);
                Grid.SetColumn(charImg, 0);
                Grid.SetRow(charImg,0);
                Grid.SetRowSpan(charImg, 3);

                // Vocie Actor Image
                //System.Windows.MessageBox.Show(info.characters[i].voiceActors[0].image);
                try
                {
                    await charimgVA_urlValidation(info.characters[i].voiceActors[0].image);
                }
                catch (Exception ex)
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string imageRelativePath = "Images\\Background\\Noimage.png";
                    string imagePath = System.IO.Path.Combine(baseDirectory, imageRelativePath);

                    var bitmap = new System.Drawing.Bitmap(imagePath);
                    charVA_imgs = ToBitmapImage(bitmap);
                }

                ImageBrush vaIMG = new ImageBrush();
                vaIMG.Stretch = Stretch.UniformToFill;
                vaIMG.ImageSource = charVA_imgs;
                vaIMG.Opacity = 1;

                charVA.Background = vaIMG;
                charVA.Height = 150;
                charVA.Width = 125;
                charVA.CornerRadius = new CornerRadius(4);
                Grid.SetColumn(charVA, 2);
                Grid.SetRow(charVA, 0);
                Grid.SetRowSpan(charVA, 3);

                // Character Name
                charName.Text = info.characters[i].name.userPreferred;
                charName.FontSize = 16;
                charName.FontWeight = FontWeights.Medium;
                charName.HorizontalAlignment = HorizontalAlignment.Center;
                charName.Margin = new Thickness(5,35,5,0);
                charName.TextWrapping = TextWrapping.WrapWithOverflow;
                Grid.SetColumn(charName, 1);
                Grid.SetRow(charName, 0);

                // Character Role
                charRole.Text = info.characters[i].role;
                charRole.FontSize = 14;
                charRole.FontWeight = FontWeights.Regular;
                charRole.HorizontalAlignment = HorizontalAlignment.Center;
                charRole.Margin = new Thickness(5,0,5,0);
                Grid.SetColumn(charRole, 1);
                Grid.SetRow(charRole, 1);

                // Character Name VA
                try
                {
                    charNameVA.Text = "VA: "+info.characters[i].voiceActors[0].name.userPreferred;
                    charNameVA.FontSize = 14;
                    charNameVA.FontWeight = FontWeights.Medium;
                    charNameVA.HorizontalAlignment = HorizontalAlignment.Center;
                    charNameVA.Margin = new Thickness(5,10,5,0);
                    Grid.SetColumn(charNameVA, 1);
                    Grid.SetRow(charNameVA, 2);
                }
                catch(Exception ex) 
                { 
                }

                // chargridMiddle Assign
                /*
                charGridMiddle.Children.Add(charName);
                charGridMiddle.Children.Add(charRole);
                charGridMiddle.Children.Add(charNameVA);
                */

                // charGrid Assign
                //charGrid.ShowGridLines = true;
                charGrid.Children.Add(charImg);
                charGrid.Children.Add(charVA);
                charGrid.Children.Add(charName);
                charGrid.Children.Add(charRole);
                charGrid.Children.Add(charNameVA);
                //charGrid.Children.Add(charGridMiddle);

                CharactersPanel.Children.Add(charBorder);


            }
        }

        public async void RelationsMenu(anilist_Info info)
        {
            for (int i = 0; i < info.relations.Length; i++)
            {
                var RLCardPanel = new System.Windows.Controls.StackPanel();
                var RLText = new System.Windows.Controls.TextBlock();
                var RLBadge = new Wpf.Ui.Controls.Badge();
                var RLCard = new Wpf.Ui.Controls.Button();

                //Declare and initialize image to bitmap
                await rl_image_urlValidation(info.relations[i].image);
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
                cardCover.ImageSource = rl_imgs;


                //Cover Artwork with Opacity
                ImageBrush cardCoverOpacity = new ImageBrush();
                cardCoverOpacity.Stretch = Stretch.UniformToFill;
                cardCoverOpacity.ImageSource = rl_imgs;
                cardCoverOpacity.Opacity = 1;

                //Modify RL Card Style
                RLCard.Background = cardCover;
                RLCard.Height = 285;
                RLCard.Width = 210;
                RLCard.MouseOverBackground = cardCoverOpacity;
                RLCard.ToolTip = info.relations[i].relationType;
                RLCard.Tag = new
                {
                    panel = RLCardPanel,
                    episode_id = info.relations[i].id
                };

                var recent_title = info.relations[i].title.userPreferred;
                //var recent_subordub = recent_array_subordub[i].ToString().ToUpper();

                RLBadge.FontSize = 10;
                RLBadge.FontWeight = FontWeights.Medium;
                RLBadge.Background = Brushes.LimeGreen;
                RLBadge.VerticalAlignment = VerticalAlignment.Top;
                RLBadge.HorizontalAlignment = HorizontalAlignment.Center;
                RLBadge.Content = info.relations[i].type;

                RLText.FontSize = 16;
                RLText.FontWeight = FontWeights.Bold;
                RLText.TextAlignment = TextAlignment.Center;
                RLText.TextWrapping = TextWrapping.Wrap;
                RLText.Text = recent_title;

                RLCardPanel.Name = "RLCardPanel";
                RLCardPanel.Children.Add(RLText);
                RLCardPanel.Children.Add(RLBadge);

                RLCard.Content = RLCardPanel;
                RLCardPanel.Visibility = Visibility.Visible;

                RLCard.Margin = new System.Windows.Thickness(15, 7.5, 0, 7.5);

                //Render and Add the control for the episodes
                RelationsPanel.Children.Add(RLCard);
                //RLCard.Click += new RoutedEventHandler(EPCard_Click);
                RLCard.MouseEnter += new MouseEventHandler(rlCard_MouseEnter);
                RLCard.MouseLeave += new MouseEventHandler(rlCard_MouseLeave);


                
            }
        }

        public async void RecommendationsMenu(anilist_Info info)
        {
            for (int i = 0; i < info.recommendations.Length; i++)
            {
                var RecoCardPanel = new System.Windows.Controls.StackPanel();
                var RecoText = new System.Windows.Controls.TextBlock();
                var RecoBadge = new Wpf.Ui.Controls.Badge();
                var RecoCard = new Wpf.Ui.Controls.Button();

                //Declare and initialize image to bitmap
                await reco_image_urlValidation(info.recommendations[i].image);
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
                cardCover.ImageSource = reco_imgs;


                //Cover Artwork with Opacity
                ImageBrush cardCoverOpacity = new ImageBrush();
                cardCoverOpacity.Stretch = Stretch.UniformToFill;
                cardCoverOpacity.ImageSource = reco_imgs;
                cardCoverOpacity.Opacity = 1;

                //Modify Reco Card Style
                RecoCard.Background = cardCover;
                RecoCard.Height = 285;
                RecoCard.Width = 210;
                RecoCard.MouseOverBackground = cardCoverOpacity;
                RecoCard.ToolTip = info.recommendations[i].status;
                RecoCard.Tag = new
                {
                    panel = RecoCardPanel,
                    id = info.recommendations[i].id,
                    cover = info.recommendations[i].cover
                };

                var recent_title = info.recommendations[i].title.userPreferred;
                //var recent_subordub = recent_array_subordub[i].ToString().ToUpper();

                RecoBadge.FontSize = 10;
                RecoBadge.FontWeight = FontWeights.Medium;
                RecoBadge.Background = Brushes.LimeGreen;
                RecoBadge.VerticalAlignment = VerticalAlignment.Top;
                RecoBadge.HorizontalAlignment = HorizontalAlignment.Center;
                RecoBadge.Content = info.recommendations[i].type;

                RecoText.FontSize = 16;
                RecoText.FontWeight = FontWeights.Bold;
                RecoText.TextAlignment = TextAlignment.Center;
                RecoText.TextWrapping = TextWrapping.Wrap;
                RecoText.Text = recent_title;

                RecoCardPanel.Name = "RecoCardPanel";
                RecoCardPanel.Children.Add(RecoText);
                RecoCardPanel.Children.Add(RecoBadge);

                RecoCard.Content = RecoCardPanel;
                RecoCardPanel.Visibility = Visibility.Visible;

                RecoCard.Margin = new System.Windows.Thickness(15, 7.5, 0, 7.5);

                //Render and Add the control for the episodes
                RecommendationsPanel.Children.Add(RecoCard);
                RecoCard.Click += new RoutedEventHandler(recoCard_Click);
                RecoCard.MouseEnter += new MouseEventHandler(recoCard_MouseEnter);
                RecoCard.MouseLeave += new MouseEventHandler(recoCard_MouseLeave);
            }
        }

        public async void GenerateEpisodesCard(anilist_Info data, bool isViewmore)
        {
            int sum = lastEPCount - data.episodes.Length;
            int sumPositive = sum * -1;

            ViewMoreLoadControl.Visibility = Visibility.Visible;
            if (data.episodes.Length >= 100 && isViewmore == false)
            {
                ViewMoreEPControl.Visibility = Visibility.Collapsed;
                lastEPCount = 100;
                for (int i = 0; i < 100; i++)
                {
                    var epCardPanel = new System.Windows.Controls.StackPanel();
                    var epText = new System.Windows.Controls.TextBlock();
                    var epBadge = new Wpf.Ui.Controls.Badge();
                    var epCard = new Wpf.Ui.Controls.Button();

                    //Declare and initialize image to bitmap
                    await ep_image_urlValidation(data.episodes[i].image);
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
                    cardCover.ImageSource = ep_imgs;


                    //Cover Artwork with Opacity
                    ImageBrush cardCoverOpacity = new ImageBrush();
                    cardCoverOpacity.Stretch = Stretch.UniformToFill;
                    cardCoverOpacity.ImageSource = ep_imgs;
                    cardCoverOpacity.Opacity = 1;

                    //Modify Popular Card Style
                    epCard.Background = cardCover;
                    epCard.Height = 175;
                    epCard.Width = 300;
                    epCard.MouseOverBackground = cardCoverOpacity;
                    epCard.ToolTip = data.episodes[i].description;
                    epCard.Tag = new
                    {
                        panel = epCardPanel,
                        episode_id = data.episodes[i].id,
                        ep_num = i.ToString()
                    };

                    var recent_title = data.episodes[i].title;
                    var recent_episodeNum = data.episodes[i].number;
                    //var recent_subordub = recent_array_subordub[i].ToString().ToUpper();

                    epBadge.FontSize = 10;
                    epBadge.FontWeight = FontWeights.Medium;
                    epBadge.Background = Brushes.LimeGreen;
                    epBadge.VerticalAlignment = VerticalAlignment.Top;
                    epBadge.HorizontalAlignment = HorizontalAlignment.Center;
                    epBadge.Content = "EP " + recent_episodeNum;

                    epText.FontSize = 16;
                    epText.FontWeight = FontWeights.Bold;
                    epText.TextAlignment = TextAlignment.Center;
                    epText.TextWrapping = TextWrapping.Wrap;
                    epText.Text = recent_title;

                    epCardPanel.Name = "epCardPanel";
                    epCardPanel.Children.Add(epText);
                    epCardPanel.Children.Add(epBadge);

                    epCard.Content = epCardPanel;
                    epCardPanel.Visibility = Visibility.Visible;

                    epCard.Margin = new System.Windows.Thickness(15, 7.5, 0, 7.5);

                    //Render and Add the control for the episodes
                    EpisodesListPanel.Children.Add(epCard);

                    epCard.Click += new RoutedEventHandler(EPCard_Click);
                    epCard.MouseEnter += new MouseEventHandler(epCard_MouseEnter);
                    epCard.MouseLeave += new MouseEventHandler(epCard_MouseLeave);


                }

                ViewMoreEPControl.Visibility = Visibility.Visible;
                ViewMoreLoadControl.Visibility = Visibility.Collapsed;

            }
            else if (isViewmore == true && sumPositive >= 100)
            {

                var lastChild = EpisodesListPanel.Children;
                WrapPanel dummy = new WrapPanel();
                int limiter = lastEPCount + 100;

                //System.Windows.MessageBox.Show("1 | "+ lastEPCount.ToString());

                for (int i = lastEPCount; i < limiter; i++)
                {
                    var epCardPanel = new System.Windows.Controls.StackPanel();
                    var epText = new System.Windows.Controls.TextBlock();
                    var epBadge = new Wpf.Ui.Controls.Badge();
                    var epCard = new Wpf.Ui.Controls.Button();

                    //Declare and initialize image to bitmap
                    await ep_image_urlValidation(data.episodes[i].image);
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
                    cardCover.ImageSource = ep_imgs;


                    //Cover Artwork with Opacity
                    ImageBrush cardCoverOpacity = new ImageBrush();
                    cardCoverOpacity.Stretch = Stretch.UniformToFill;
                    cardCoverOpacity.ImageSource = ep_imgs;
                    cardCoverOpacity.Opacity = 1;

                    //Modify Popular Card Style
                    epCard.Background = cardCover;
                    epCard.Height = 175;
                    epCard.Width = 300;
                    epCard.MouseOverBackground = cardCoverOpacity;
                    epCard.ToolTip = data.episodes[i].description;
                    epCard.Tag = new
                    {
                        panel = epCardPanel,
                        episode_id = data.episodes[i].id,
                        ep_num = i.ToString()
                    };

                    var recent_title = data.episodes[i].title;
                    var recent_episodeNum = data.episodes[i].number;
                    //var recent_subordub = recent_array_subordub[i].ToString().ToUpper();

                    epBadge.FontSize = 10;
                    epBadge.FontWeight = FontWeights.Medium;
                    epBadge.Background = Brushes.LimeGreen;
                    epBadge.VerticalAlignment = VerticalAlignment.Top;
                    epBadge.HorizontalAlignment = HorizontalAlignment.Center;
                    epBadge.Content = "EP " + recent_episodeNum;

                    epText.FontSize = 16;
                    epText.FontWeight = FontWeights.Bold;
                    epText.TextAlignment = TextAlignment.Center;
                    epText.TextWrapping = TextWrapping.Wrap;
                    epText.Text = recent_title;

                    epCardPanel.Name = "epCardPanel";
                    epCardPanel.Children.Add(epText);
                    epCardPanel.Children.Add(epBadge);

                    epCard.Content = epCardPanel;
                    epCardPanel.Visibility = Visibility.Visible;

                    epCard.Margin = new System.Windows.Thickness(15, 7.5, 0, 7.5);

                    //Render and Add the control for the episodes
                    dummy.Children.Add(epCard); 
                    epCard.Click += new RoutedEventHandler(EPCard_Click);
                    epCard.MouseEnter += new MouseEventHandler(epCard_MouseEnter);
                    epCard.MouseLeave += new MouseEventHandler(epCard_MouseLeave);
                    lastEPCount++;

                }

                ViewMoreEPControl.Visibility = Visibility.Visible;
                ViewMoreLoadControl.Visibility = Visibility.Collapsed;

                List<UIElement> elementslast = new List<UIElement>();
                List<UIElement> elementsdummy = new List<UIElement>();
                foreach (UIElement yow in lastChild)
                {
                    elementslast.Add(yow);
                }

                var dummyChild = dummy.Children;
                foreach (UIElement yow in dummyChild)
                {
                    elementsdummy.Add(yow);
                }

                elementsdummy.ForEach(item => elementslast.Add(item));
                List<UIElement> result = new List<UIElement>();
                result.AddRange(elementslast);
                EpisodesListPanel.Children.Clear();

                


                foreach (UIElement childs in result)
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

                    EpisodesListPanel.Children.Add(childs);
                }



            }
            else if (isViewmore == true && sumPositive <= 99)
            {
                var lastChild = EpisodesListPanel.Children;
                WrapPanel dummy = new WrapPanel();
                int limiter = data.episodes.Length;

                //System.Windows.MessageBox.Show("2 | "+ lastEPCount.ToString());

                for (int i = lastEPCount; i < limiter; i++)
                {
                    var epCardPanel = new System.Windows.Controls.StackPanel();
                    var epText = new System.Windows.Controls.TextBlock();
                    var epBadge = new Wpf.Ui.Controls.Badge();
                    var epCard = new Wpf.Ui.Controls.Button();

                    //Declare and initialize image to bitmap
                    await ep_image_urlValidation(data.episodes[i].image);
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
                    cardCover.ImageSource = ep_imgs;


                    //Cover Artwork with Opacity
                    ImageBrush cardCoverOpacity = new ImageBrush();
                    cardCoverOpacity.Stretch = Stretch.UniformToFill;
                    cardCoverOpacity.ImageSource = ep_imgs;
                    cardCoverOpacity.Opacity = 1;

                    //Modify Popular Card Style
                    epCard.Background = cardCover;
                    epCard.Height = 175;
                    epCard.Width = 300;
                    epCard.MouseOverBackground = cardCoverOpacity;
                    epCard.ToolTip = data.episodes[i].description;
                    epCard.Tag = new
                    {
                        panel = epCardPanel,
                        episode_id = data.episodes[i].id,
                        ep_num = i.ToString()
                    };

                    var recent_title = data.episodes[i].title;
                    var recent_episodeNum = data.episodes[i].number;
                    //var recent_subordub = recent_array_subordub[i].ToString().ToUpper();

                    epBadge.FontSize = 10;
                    epBadge.FontWeight = FontWeights.Medium;
                    epBadge.Background = Brushes.LimeGreen;
                    epBadge.VerticalAlignment = VerticalAlignment.Top;
                    epBadge.HorizontalAlignment = HorizontalAlignment.Center;
                    epBadge.Content = "EP " + recent_episodeNum;

                    epText.FontSize = 16;
                    epText.FontWeight = FontWeights.Bold;
                    epText.TextAlignment = TextAlignment.Center;
                    epText.TextWrapping = TextWrapping.Wrap;
                    epText.Text = recent_title;

                    epCardPanel.Name = "epCardPanel";
                    epCardPanel.Children.Add(epText);
                    epCardPanel.Children.Add(epBadge);

                    epCard.Content = epCardPanel;
                    epCardPanel.Visibility = Visibility.Visible;

                    epCard.Margin = new System.Windows.Thickness(15, 7.5, 0, 7.5);

                    //Render and Add the control for the episodes
                    dummy.Children.Add(epCard);
                    epCard.Click += new RoutedEventHandler(EPCard_Click);
                    epCard.MouseEnter += new MouseEventHandler(epCard_MouseEnter);
                    epCard.MouseLeave += new MouseEventHandler(epCard_MouseLeave);
                    lastEPCount++;

                }

                ViewMoreEPControl.Visibility = Visibility.Collapsed;
                ViewMoreLoadControl.Visibility = Visibility.Collapsed;

                List<UIElement> elementslast = new List<UIElement>();
                List<UIElement> elementsdummy = new List<UIElement>();
                foreach (UIElement yow in lastChild)
                {
                    elementslast.Add(yow);
                }

                var dummyChild = dummy.Children;
                foreach (UIElement yow in dummyChild)
                {
                    elementsdummy.Add(yow);
                }

                elementsdummy.ForEach(item => elementslast.Add(item));
                List<UIElement> result = new List<UIElement>();
                result.AddRange(elementslast);
                EpisodesListPanel.Children.Clear();



                //Disconnect element then add new elements
                foreach (UIElement childs in result)
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

                    EpisodesListPanel.Children.Add(childs);
                }
            }
            else
            {
                ViewMoreEPPanel.Visibility = Visibility.Collapsed;

                for (int i = 0; i < data.episodes.Length; i++)
                {
                    var epCardPanel = new System.Windows.Controls.StackPanel();
                    var epText = new System.Windows.Controls.TextBlock();
                    var epBadge = new Wpf.Ui.Controls.Badge();
                    var epCard = new Wpf.Ui.Controls.Button();

                    //Declare and initialize image to bitmap
                    await ep_image_urlValidation(data.episodes[i].image);
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
                    cardCover.ImageSource = ep_imgs;


                    //Cover Artwork with Opacity
                    ImageBrush cardCoverOpacity = new ImageBrush();
                    cardCoverOpacity.Stretch = Stretch.UniformToFill;
                    cardCoverOpacity.ImageSource = ep_imgs;
                    cardCoverOpacity.Opacity = 1;

                    //Modify Popular Card Style
                    epCard.Background = cardCover;
                    epCard.Height = 175;
                    epCard.Width = 300;
                    epCard.MouseOverBackground = cardCoverOpacity;
                    epCard.ToolTip = data.episodes[i].description;
                    epCard.Tag = new
                    {
                        panel = epCardPanel,
                        episode_id = data.episodes[i].id,
                        ep_num = i.ToString()
                    };

                    var recent_title = data.episodes[i].title;
                    var recent_episodeNum = data.episodes[i].number;
                    //var recent_subordub = recent_array_subordub[i].ToString().ToUpper();

                    epBadge.FontSize = 10;
                    epBadge.FontWeight = FontWeights.Medium;
                    epBadge.Background = Brushes.LimeGreen;
                    epBadge.VerticalAlignment = VerticalAlignment.Top;
                    epBadge.HorizontalAlignment = HorizontalAlignment.Center;
                    epBadge.Content = "EP " + recent_episodeNum;

                    epText.FontSize = 16;
                    epText.FontWeight = FontWeights.Bold;
                    epText.TextAlignment = TextAlignment.Center;
                    epText.TextWrapping = TextWrapping.Wrap;
                    epText.Text = recent_title;

                    epCardPanel.Name = "epCardPanel";
                    epCardPanel.Children.Add(epText);
                    epCardPanel.Children.Add(epBadge);

                    epCard.Content = epCardPanel;
                    epCardPanel.Visibility = Visibility.Visible;

                    epCard.Margin = new System.Windows.Thickness(15, 7.5, 0, 7.5);

                    //Render and Add the control for the episodes
                    EpisodesListPanel.Children.Add(epCard); 
                    epCard.Click += new RoutedEventHandler(EPCard_Click);
                    epCard.MouseEnter += new MouseEventHandler(epCard_MouseEnter);
                    epCard.MouseLeave += new MouseEventHandler(epCard_MouseLeave);


                }
            }
            
        }
        private void EPCard_Click(object sender, RoutedEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonThatWasClicked = (Wpf.Ui.Controls.Button)sender;
            string ep_id = ((dynamic)buttonThatWasClicked.Tag).episode_id;
            string epnumber = ((dynamic)buttonThatWasClicked.Tag).ep_num;
            //System.Windows.MessageBox.Show(selectedanime.id + " | " + ep_id + " | " + epnumber);
            this.NavigationService.Navigate(new PlayEpisode(selectedanime.id, ep_id, epnumber));

        }

        private void epCard_MouseLeave(object sender, MouseEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonMouseEnter = (Wpf.Ui.Controls.Button)sender;
            System.Windows.Controls.StackPanel panel = ((dynamic)buttonMouseEnter.Tag).panel;

            panel.Visibility = Visibility.Visible;
        }

        private void epCard_MouseEnter(object sender, MouseEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonMouseEnter = (Wpf.Ui.Controls.Button)sender;
            System.Windows.Controls.StackPanel panel = ((dynamic)buttonMouseEnter.Tag).panel;

            panel.Visibility = Visibility.Hidden;
        }

        private void rlCard_MouseEnter(object sender, MouseEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonMouseEnter = (Wpf.Ui.Controls.Button)sender;
            System.Windows.Controls.StackPanel panel = ((dynamic)buttonMouseEnter.Tag).panel;

            panel.Visibility = Visibility.Hidden;
        }

        private void rlCard_MouseLeave(object sender, MouseEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonMouseEnter = (Wpf.Ui.Controls.Button)sender;
            System.Windows.Controls.StackPanel panel = ((dynamic)buttonMouseEnter.Tag).panel;

            panel.Visibility = Visibility.Visible;
        }
        private void recoCard_Click(object sender, RoutedEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonThatWasClicked = (Wpf.Ui.Controls.Button)sender;
            string id = ((dynamic)buttonThatWasClicked.Tag).id;
            string cover = ((dynamic)buttonThatWasClicked.Tag).cover;

            this.NavigationService.Navigate(new SelectedAnime(id, cover));

        }

        private void recoCard_MouseEnter(object sender, MouseEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonMouseEnter = (Wpf.Ui.Controls.Button)sender;
            System.Windows.Controls.StackPanel panel = ((dynamic)buttonMouseEnter.Tag).panel;

            panel.Visibility = Visibility.Hidden;
        }

        private void recoCard_MouseLeave(object sender, MouseEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonMouseEnter = (Wpf.Ui.Controls.Button)sender;
            System.Windows.Controls.StackPanel panel = ((dynamic)buttonMouseEnter.Tag).panel;

            panel.Visibility = Visibility.Visible;
        }

        static String RemoveHTMLTags(String str)
        {

            // Use replaceAll function in regex
            // to erase every tags enclosed in <>
            // str = Regex.Replace(str, "<.*?>", String.Empty)
            System.Text.RegularExpressions.Regex rx =
            new System.Text.RegularExpressions.Regex("<[^>]*>");

            str = rx.Replace(str, "");
            return str;
        }

        public async void animeCoverBitImage(string coverLink)
        {
            //Declare and initialize image to bitmap
            await ep_image_urlValidation(coverLink);
            AnimeCover.ImageSource = ep_imgs;
        }

        public void releaseDate(string year, string month, string day)
        {
            try
            {
                int y = int.Parse(year);
                int m = int.Parse(month);
                int d = int.Parse(day);
                DateTime releasedate = new DateTime(y, m, d);

                ReleaseDateControl.Text = releasedate.ToString("MMM d, yyyy");
            }
            catch (Exception ex)
            {
                ReleaseDatePanel.Visibility = Visibility.Collapsed;
            }
        }

        public void endDate(string year, string month, string day)
        {
            try
            {
                int y = int.Parse(year);
                int m = int.Parse(month);
                int d = int.Parse(day);
                DateTime enddate = new DateTime(y, m, d);

                EndDateControl.Text = enddate.ToString("MMM d, yyyy");
            }
            catch (Exception ex)
            {
                EndDatePanel.Visibility = Visibility.Collapsed;
            }
        }


        public void calculateAiring(string airingtime, string tiemuntilair, string episode)
        {
            int time_air = int.Parse(airingtime);

            AirDate = AirDate.AddSeconds(time_air).ToLocalTime();
            ep = episode;

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;

            TimeSpan span = AirDate.Subtract(DateTime.Now);
            countdownFormat(span, episode);


            AiringTimer.ToolTip = AirDate.ToString("ddd, d MMM yyyy, h:mm tt zzz");

            dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            TimeSpan span = AirDate.Subtract(DateTime.Now);
            countdownFormat(span, ep);
        }

        public void countdownFormat(TimeSpan ts, string ep)
        {
            if (ts.Days == 0)
            {
                string yow = String.Format("Ep " + ep + ": {0}h {1}m {2}s",
                    ts.Hours, ts.Minutes, ts.Seconds);
                AiringTimer.Text = yow;
            }
            else if (ts.Days == 0 && ts.Hours == 0)
            {
                string yow = String.Format("Ep " + ep + ": {0}m {1}s",
                    ts.Minutes, ts.Seconds);
                AiringTimer.Text = yow;
            }
            else if (ts.Days == 0 && ts.Hours == 0 && ts.Minutes == 0)
            {
                string yow = String.Format("Ep " + ep + ": {0}s",
                    ts.Seconds);
                AiringTimer.Text = yow;
            }
            else
            {
                string yow = String.Format("Ep "+ ep + ": {0}d {1}h {2}m {3}s",
                    ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
                AiringTimer.Text = yow;
            }
        }

        private void SelectedAnime_EpisodesMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuTitle.Text = "Episodes";
            EPStack.Visibility = Visibility.Visible;
            CharactersStack.Visibility = Visibility.Collapsed;
            RelationsStack.Visibility = Visibility.Collapsed;
            RecommendationsStack.Visibility = Visibility.Collapsed;
        }

        private void SelectedAnime_CharactersMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuTitle.Text = "Characters";
            EPStack.Visibility = Visibility.Collapsed;
            CharactersStack.Visibility = Visibility.Visible;
            RelationsStack.Visibility = Visibility.Collapsed;
            RecommendationsStack.Visibility = Visibility.Collapsed;
        }
        private void SelectedAnime_RelationsMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuTitle.Text = "Relations";
            EPStack.Visibility = Visibility.Collapsed;
            CharactersStack.Visibility = Visibility.Collapsed;
            RelationsStack.Visibility = Visibility.Visible;
            RecommendationsStack.Visibility = Visibility.Collapsed;
        }

        private void SelectedAnime_ReccomendationsMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuTitle.Text = "Recommendations";
            EPStack.Visibility = Visibility.Collapsed;
            CharactersStack.Visibility = Visibility.Collapsed;
            RelationsStack.Visibility = Visibility.Collapsed;
            RecommendationsStack.Visibility = Visibility.Visible;
        }

        //GET Most used Color of the Image
        public async Task getColor()
        {
            /*
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
            */
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

        public async Task selectedanime_urlValidation(string url)
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
                    selected_anime = getResponsestring;
                }
            }

        }

        public async Task ep_image_urlValidation(string url)
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
                    ep_imgs = bitmapImage;
                }
                catch (Exception ex)
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string imageRelativePath = "Images\\Background\\Noimage.png";
                    string imagePath = System.IO.Path.Combine(baseDirectory, imageRelativePath);

                    var bitmap = new System.Drawing.Bitmap(imagePath);
                    ep_imgs = ToBitmapImage(bitmap);
                }
            }
            else
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string imageRelativePath = "Images\\Background\\Noimage.png";
                string imagePath = System.IO.Path.Combine(baseDirectory, imageRelativePath);

                var bitmap = new System.Drawing.Bitmap(imagePath);
                ep_imgs = ToBitmapImage(bitmap);

                //img_card = ToBitmapImage(bitmap);

                //var tet = new ImageBrush(new BitmapImage(new Uri(@"C:\Users\Krystler\source\repos\OzakiAnimeWPF\OzakiAnimeWPF\Images\Background\Noimage.png", UriKind.Relative)));
                //img_card = new BitmapImage(new Uri("pack://application:,,,/Images/Background/Noimage.png"));
                //System.Windows.MessageBox.Show("here");
            }


        }

        public async Task rl_image_urlValidation(string url)
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
                    rl_imgs = bitmapImage;
                }
                catch (Exception ex)
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string imageRelativePath = "Images\\Background\\Noimage.png";
                    string imagePath = System.IO.Path.Combine(baseDirectory, imageRelativePath);

                    var bitmap = new System.Drawing.Bitmap(imagePath);
                    rl_imgs = ToBitmapImage(bitmap);
                }
            }
            else
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string imageRelativePath = "Images\\Background\\Noimage.png";
                string imagePath = System.IO.Path.Combine(baseDirectory, imageRelativePath);

                var bitmap = new System.Drawing.Bitmap(imagePath);
                rl_imgs = ToBitmapImage(bitmap);

                //img_card = ToBitmapImage(bitmap);

                //var tet = new ImageBrush(new BitmapImage(new Uri(@"C:\Users\Krystler\source\repos\OzakiAnimeWPF\OzakiAnimeWPF\Images\Background\Noimage.png", UriKind.Relative)));
                //img_card = new BitmapImage(new Uri("pack://application:,,,/Images/Background/Noimage.png"));
                //System.Windows.MessageBox.Show("here");
            }


        }
        public async Task reco_image_urlValidation(string url)
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
                    reco_imgs = bitmapImage;
                }
                catch (Exception ex)
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string imageRelativePath = "Images\\Background\\Noimage.png";
                    string imagePath = System.IO.Path.Combine(baseDirectory, imageRelativePath);

                    var bitmap = new System.Drawing.Bitmap(imagePath);
                    reco_imgs = ToBitmapImage(bitmap);
                }
            }
            else
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string imageRelativePath = "Images\\Background\\Noimage.png";
                string imagePath = System.IO.Path.Combine(baseDirectory, imageRelativePath);

                var bitmap = new System.Drawing.Bitmap(imagePath);
                rl_imgs = ToBitmapImage(bitmap);

                //img_card = ToBitmapImage(bitmap);

                //var tet = new ImageBrush(new BitmapImage(new Uri(@"C:\Users\Krystler\source\repos\OzakiAnimeWPF\OzakiAnimeWPF\Images\Background\Noimage.png", UriKind.Relative)));
                //img_card = new BitmapImage(new Uri("pack://application:,,,/Images/Background/Noimage.png"));
                //System.Windows.MessageBox.Show("here");
            }


        }

        public async Task charimg_urlValidation(string url)
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
                    char_imgs = bitmapImage;
                }
                catch (Exception ex)
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string imageRelativePath = "Images\\Background\\Noimage.png";
                    string imagePath = System.IO.Path.Combine(baseDirectory, imageRelativePath);

                    var bitmap = new System.Drawing.Bitmap(imagePath);
                    char_imgs = ToBitmapImage(bitmap);
                }
            }
            else
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string imageRelativePath = "Images\\Background\\Noimage.png";
                string imagePath = System.IO.Path.Combine(baseDirectory, imageRelativePath);

                var bitmap = new System.Drawing.Bitmap(imagePath);
                char_imgs = ToBitmapImage(bitmap);

                //img_card = ToBitmapImage(bitmap);

                //var tet = new ImageBrush(new BitmapImage(new Uri(@"C:\Users\Krystler\source\repos\OzakiAnimeWPF\OzakiAnimeWPF\Images\Background\Noimage.png", UriKind.Relative)));
                //img_card = new BitmapImage(new Uri("pack://application:,,,/Images/Background/Noimage.png"));
                //System.Windows.MessageBox.Show("here");
            }


        }
        public async Task charimgVA_urlValidation(string url)
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
                    charVA_imgs = bitmapImage;
                }
                catch (Exception ex)
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string imageRelativePath = "Images\\Background\\Noimage.png";
                    string imagePath = System.IO.Path.Combine(baseDirectory, imageRelativePath);

                    var bitmap = new System.Drawing.Bitmap(imagePath);
                    charVA_imgs = ToBitmapImage(bitmap);
                }
            }
            else
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string imageRelativePath = "Images\\Background\\Noimage.png";
                string imagePath = System.IO.Path.Combine(baseDirectory, imageRelativePath);

                var bitmap = new System.Drawing.Bitmap(imagePath);
                charVA_imgs = ToBitmapImage(bitmap);

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

        private void ViewMoreEPControl_Click(object sender, RoutedEventArgs e)
        {
            isViewMore = true;
            ViewMoreEPControl.Visibility = Visibility.Collapsed;
            ViewMoreLoadControl.Visibility = Visibility.Visible;
            GenerateEpisodesCard(selectedanime, isViewMore);
        }
    }
}
