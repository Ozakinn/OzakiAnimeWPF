using Newtonsoft.Json;
using OzakiAnimeWPF.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
using System.IO;
using System.Drawing.Imaging;
using Wpf.Ui.Common;

namespace OzakiAnimeWPF.Pages.UserAccount
{
    /// <summary>
    /// Interaction logic for UserAccount.xaml
    /// </summary>
    public partial class UserAccount : UiPage
    {
        BitmapImage anime_imgs;
        UserAccountParse account;
        //avoid duplicate of page load
        private bool _isLoaded;
        public UserAccount()
        {
            InitializeComponent();
        }

        private async void UiPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {

                _isLoaded = true;
                PageLoad.Visibility = Visibility.Visible;
                FavoritesStack.Visibility = Visibility.Hidden;


                await Account_Load();

                await Task.Delay(1000);
                FavoritesStack.Visibility = Visibility.Visible;
                PageLoad.Visibility = Visibility.Collapsed;

            }
        }

        public async Task Account_Load()
        {
            var sw = new Stopwatch();
            sw.Start();
            await Task.WhenAll(fetch_Account());
            //System.Windows.MessageBox.Show("done async");
            sw.Stop();

            var elapsed = sw.ElapsedMilliseconds;
            //System.Windows.MessageBox.Show("took: " + elapsed + "ms");
            
        }

        public async Task fetch_Account()
        {
            

            var jsonData = File.ReadAllText("Account.json");
            account = JsonConvert.DeserializeObject<UserAccountParse>(jsonData);


            GenerateFavorite(account);
        }

        public async void GenerateFavorite(UserAccountParse data)
        {
            for (int i = 0; i < data.Favorites.Anime.Anilist.Length; i++)
            {
                var Anime_CardPanel = new System.Windows.Controls.StackPanel();
                var Anime_Text = new System.Windows.Controls.TextBlock();
                var Anime_Badge = new Wpf.Ui.Controls.Badge();
                var Anime_Card = new Wpf.Ui.Controls.Button();

                //Declare and initialize image to bitmap
                await anime_image_urlValidation(data.Favorites.Anime.Anilist[i].Image);
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
                cardCover.ImageSource = anime_imgs;


                //Cover Artwork with Opacity
                ImageBrush cardCoverOpacity = new ImageBrush();
                cardCoverOpacity.Stretch = Stretch.UniformToFill;
                cardCoverOpacity.ImageSource = anime_imgs;
                cardCoverOpacity.Opacity = 1;

                //Modify Anime Card Style
                Anime_Card.Background = cardCover;
                Anime_Card.Height = 285;
                Anime_Card.Width = 210;
                Anime_Card.MouseOverBackground = cardCoverOpacity;
                Anime_Card.ToolTip = "Favorite since: "+data.Favorites.Anime.Anilist[i].DateAdded;
                Anime_Card.Tag = new
                {
                    panel = Anime_CardPanel,
                    id = data.Favorites.Anime.Anilist[i].AnimeId
                };

                var title = data.Favorites.Anime.Anilist[i].title;
                var subordub = data.Favorites.Anime.Anilist[i].subOrdub;

                Anime_Badge.FontSize = 10;
                Anime_Badge.FontWeight = FontWeights.Medium;
                Anime_Badge.Background = Brushes.LimeGreen;
                Anime_Badge.VerticalAlignment = VerticalAlignment.Top;
                Anime_Badge.HorizontalAlignment = HorizontalAlignment.Center;
                Anime_Badge.Content = data.Favorites.Anime.Anilist[i].type +" | "+ subordub;

                Anime_Text.FontSize = 16;
                Anime_Text.FontWeight = FontWeights.Bold;
                Anime_Text.TextAlignment = TextAlignment.Center;
                Anime_Text.TextWrapping = TextWrapping.Wrap;
                Anime_Text.Text = title;

                Anime_CardPanel.Name = "Anime_CardPanel";
                Anime_CardPanel.Children.Add(Anime_Text);
                Anime_CardPanel.Children.Add(Anime_Badge);

                Anime_Card.Content = Anime_CardPanel;
                Anime_CardPanel.Visibility = Visibility.Visible;

                Anime_Card.Margin = new System.Windows.Thickness(0, 7.5, 15, 7.5);

                //Render and Add the control for the episodes
                FavoritesPanel.Children.Add(Anime_Card);
                Anime_Card.Click += new RoutedEventHandler(Anime_Card_Click);
                Anime_Card.MouseEnter += new MouseEventHandler(Anime_Card_MouseEnter);
                Anime_Card.MouseLeave += new MouseEventHandler(Anime_Card_MouseLeave);
            }
        }
        private void Anime_Card_Click(object sender, RoutedEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonThatWasClicked = (Wpf.Ui.Controls.Button)sender;
            string id = ((dynamic)buttonThatWasClicked.Tag).id;

            this.NavigationService.Navigate(new SelectedAnime(id));

        }

        private void Anime_Card_MouseEnter(object sender, MouseEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonMouseEnter = (Wpf.Ui.Controls.Button)sender;
            System.Windows.Controls.StackPanel panel = ((dynamic)buttonMouseEnter.Tag).panel;

            panel.Visibility = Visibility.Hidden;
        }
        private void Anime_Card_MouseLeave(object sender, MouseEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonMouseEnter = (Wpf.Ui.Controls.Button)sender;
            System.Windows.Controls.StackPanel panel = ((dynamic)buttonMouseEnter.Tag).panel;

            panel.Visibility = Visibility.Visible;
        }

        public async Task anime_image_urlValidation(string url)
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            HttpClient client = new HttpClient(clientHandler);
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
                    anime_imgs = bitmapImage;
                }
                catch (Exception ex)
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string imageRelativePath = "Images\\Background\\Noimage.png";
                    string imagePath = System.IO.Path.Combine(baseDirectory, imageRelativePath);

                    var bitmap = new System.Drawing.Bitmap(imagePath);
                    anime_imgs = ToBitmapImage(bitmap);
                }
            }
            else
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string imageRelativePath = "Images\\Background\\Noimage.png";
                string imagePath = System.IO.Path.Combine(baseDirectory, imageRelativePath);

                var bitmap = new System.Drawing.Bitmap(imagePath);
                anime_imgs = ToBitmapImage(bitmap);

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

        public async void Snackbar(string title, string msgs, SymbolRegular icon)
        {
            SnackbarControl.Title = title;
            SnackbarControl.Message = msgs;
            SnackbarControl.Icon = icon;
            await SnackbarControl.ShowAsync();
        }
    }
}
