using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using OzakiAnimeWPF.Data;
using OzakiAnimeWPF.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
using System.Security.Policy;
using Pages;

namespace Ozaki_Anime.Pages.SelectedAnime
{
    /// <summary>
    /// Interaction logic for PlayEpisode.xaml
    /// </summary>
    public partial class PlayEpisode : UiPage
    {
        string anime_id;
        string ep_id;
        string ep_num;
        bool checkUrl;
        SettingsJson jsonSetting;
        anilistEpisodeStreaming selectedepisode;
        anilist_Info selectedanime;

        string fetch_selectedanimeEP;
        string fetch_selectedanime;

        bool isNavigateComplete = false;
        bool isDataComplete = false;
        bool isChangeQualityClick = false;
        bool isNextorPrev = false;

        string urlQualityContent;

        //avoid duplicate of page load
        private bool _isLoaded;

        public PlayEpisode(string animeid,string epid, string epnum)
        {
            jsonSetting = new SettingsJson();
            jsonSetting = SettingsFile.SettingRead();
            InitializeComponent();
            anime_id = animeid;
            ep_id = epid;
            ep_num = epnum;

            webviewBrowser.NavigationCompleted += WebviewBrowser_NavigationCompleted;
            InitializeAsync();
        }
        async void InitializeAsync()
        {
            // must create a data folder if running out of a secured folder that can't write like Program Files
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MarkdownMonster_Browser");
            var env = await CoreWebView2Environment.CreateAsync(userDataFolder: path);

            // NOTE: this waits until the first page is navigated - then continues
            //       executing the next line of code!
            await webviewBrowser.EnsureCoreWebView2Async(env);

            // Optional: Map a folder from the Executable Folder to a virtual domain
            // NOTE: This requires a Canary preview currently (.720+)
            /*
            webviewBrowser.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "test.editor", "HtmlSample",
                CoreWebView2HostResourceAccessKind.Allow);
            */

            // You can then navigate the file from disk with the domain
            //webviewBrowser.Source = new Uri(@"https://formidable-lifetime.000webhostapp.com/");

            readHTML();

            //SOURCE CODE FOR STREAMING WEBAPP
            //https://github.com/bharadwajpro/m3u8-player
        }

        private async void WebviewBrowser_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            isNavigateComplete = true;
            if (isChangeQualityClick == true)
            {
                isChangeQualityClick = false;
                SendLinkToPlayer(urlQualityContent);
                await Task.Delay(1000);
                webviewBrowser.Visibility = Visibility.Visible;
            }

            if (isNextorPrev == true)
            {
                isNextorPrev = false;
                SendLinkToPlayer(urlQualityContent);
                await Task.Delay(1000);
                webviewBrowser.Visibility = Visibility.Visible;
                PageLoad.Visibility = Visibility.Hidden;
            }
        }

        public void readHTML()
        {

            //string scriptResult = await webviewBrowser.ExecuteScriptAsync("document.getElementById(\"play-btn\").click();");
            //System.Windows.MessageBox.Show(scriptResult);
            //string path = System.IO.Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            //var htmlstring = File.ReadAllText(path += @"\Files\PlayEP2\index.html");

            //webviewBrowser.NavigateToString(htmlstring.ToString());
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string exeFolder = System.IO.Path.GetDirectoryName(exePath);
            string websiteFolder = System.IO.Path.Combine(exeFolder, "Files/PlayEP2");

            webviewBrowser.CoreWebView2.SetVirtualHostNameToFolderMapping("appassets.example", websiteFolder, CoreWebView2HostResourceAccessKind.DenyCors);
            webviewBrowser.CoreWebView2.Navigate("https://appassets.example/index.html");

        }

        private async void UiPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;
                PageLoad.Visibility = Visibility.Visible;
                webviewBrowser.Visibility = Visibility.Hidden;
                await SelectedAnimeEpID_Load();
                isDataComplete = true;
                int x = 0;
                while (x != 1)
                {
                    if (isNavigateComplete == true && isDataComplete == true)
                    {
                        x = 1;
                        urlQualityContent = selectedepisode.sources[0].url;
                        SendLinkToPlayer(selectedepisode.sources[0].url);

                        await Task.Delay(1000);
                        webviewBrowser.Visibility = Visibility.Visible;
                        PageLoad.Visibility = Visibility.Hidden;
                    }
                }
            }
            else
            {
                SendLinkToPlayer(selectedepisode.sources[0].url);
            }
        }

        private void UiPage_Unloaded(object sender, RoutedEventArgs e)
        {
            webviewBrowser.CoreWebView2.Navigate("https://appassets.example/index.html");
        }

        public async void SendLinkToPlayer(string url) 
        {
            string scriptResult = await webviewBrowser.ExecuteScriptAsync($"document.getElementById('m3u8-placeholder').value = '" + url + "';");
            await webviewBrowser.ExecuteScriptAsync($"document.getElementById('play-btn').click();");
        }

        public async Task SelectedAnimeEpID_Load()
        {
            var sw = new Stopwatch();
            sw.Start();
            await Task.WhenAll(fetch_SelectedAnimeEpisode(), fetch_SelectedAnime());
            //System.Windows.MessageBox.Show("done async");
            sw.Stop();

            var elapsed = sw.ElapsedMilliseconds;
            //System.Windows.MessageBox.Show("took: " + elapsed + "ms");
        }

        public async Task fetch_SelectedAnimeEpisode()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            await selectedanimeEP_urlValidation(jsonSetting.defaultAPILink + jsonSetting.defaultANILIST_EpisodeStreamLinkPath + ep_id);

            var client = new HttpClient();
            //string selected_episode = await client.GetStringAsync(jsonSetting.defaultAPILink + jsonSetting.defaultANILIST_EpisodeStreamLinkPath + ep_id);
            selectedepisode = JsonConvert.DeserializeObject<anilistEpisodeStreaming>(fetch_selectedanimeEP);


            //System.Windows.MessageBox.Show(selectedepisode.sources[0].url);

            generateQualitySelection(selectedepisode);
                
        }

        public async Task fetch_SelectedAnime()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            await selectedanime_urlValidation(jsonSetting.defaultAPILink + jsonSetting.defaultANILIST_AnimeInfoPath + anime_id);

            var client = new HttpClient();
            //string selected_anime = await client.GetStringAsync(jsonSetting.defaultAPILink + jsonSetting.defaultANILIST_AnimeInfoPath + anime_id);

            selectedanime = JsonConvert.DeserializeObject<anilist_Info>(fetch_selectedanime);
            renderData(selectedanime);
            
        }

        public void renderData(anilist_Info info)
        {
            AnimeTitle.Text = info.title.romaji;
            int num = int.Parse(ep_num);
            int episodenumber = num + 1;
            int totalep = info.episodes.Length;


            if (info.episodes[num].title is not null)
            {
                AnimeEpisodeTitle.Text = "Episode " + episodenumber + ": "+ info.episodes[num].title;
            }
            else
            {
                AnimeEpisodeTitle.Text = "Episode " + episodenumber;
            }
            EpisodeDescription.Text = info.episodes[num].description;


            //Previous Episode
            if (episodenumber == 1)
            {
                btnPreviousEP.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnPreviousEP.Visibility = Visibility.Visible;
                btnPreviousEP.ToolTip = "Episode " + info.episodes[episodenumber-2].number;
            }

            //Next Episode
            if (totalep == episodenumber)
            {
                btnNextEP.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnNextEP.Visibility = Visibility.Visible;
                btnNextEP.ToolTip = "Episode " + info.episodes[episodenumber].number;
            }
        }

        public void generateQualitySelection(anilistEpisodeStreaming info)
        {
            if (isNextorPrev == true)
            {
                WrapPanel dummy = new WrapPanel(); 
                for (int i = 0; i < info.sources.Length; i++)
                {

                    var qualityText = new System.Windows.Controls.TextBlock();
                    var quality_Card = new Wpf.Ui.Controls.Button();


                    //modify quality text
                    qualityText.FontSize = 12;
                    qualityText.FontWeight = FontWeights.Medium;
                    qualityText.TextAlignment = TextAlignment.Center;
                    qualityText.TextWrapping = TextWrapping.Wrap;
                    qualityText.Text = info.sources[i].quality;

                    //Modify quality Card Style
                    quality_Card.Tag = new
                    {
                        url = info.sources[i].url,
                        quality = info.sources[i].quality
                    };

                    quality_Card.Margin = new System.Windows.Thickness(0, 0, 5, 0);

                    quality_Card.Content = qualityText;

                    //Render and Add the control for the popular Anime
                    dummy.Children.Add(quality_Card);

                    quality_Card.Click += new RoutedEventHandler(quality_Card_Click);
                    //trending_Card.MouseEnter += new MouseEventHandler(trending_Cards_MouseEnter);
                    //trending_Card.MouseLeave += new MouseEventHandler(trending_Cards_MouseLeave);
                }

                List<UIElement> elementsdummy = new List<UIElement>();
                var dummyChild = dummy.Children;
                foreach (UIElement yow in dummyChild)
                {
                    elementsdummy.Add(yow);
                }

                QualitySelect.Children.Clear();

                foreach (UIElement childs in elementsdummy)
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

                    QualitySelect.Children.Add(childs);
                }
            }
            else
            {
                for (int i = 0; i < info.sources.Length; i++)
                {

                    var qualityText = new System.Windows.Controls.TextBlock();
                    var quality_Card = new Wpf.Ui.Controls.Button();


                    //modify quality text
                    qualityText.FontSize = 12;
                    qualityText.FontWeight = FontWeights.Medium;
                    qualityText.TextAlignment = TextAlignment.Center;
                    qualityText.TextWrapping = TextWrapping.Wrap;
                    qualityText.Text = info.sources[i].quality;

                    //Modify quality Card Style
                    quality_Card.Tag = new
                    {
                        url = info.sources[i].url,
                        quality = info.sources[i].quality
                    };

                    quality_Card.Margin = new System.Windows.Thickness(0, 0, 5, 0);

                    quality_Card.Content = qualityText;

                    //Render and Add the control for the popular Anime
                    QualitySelect.Children.Add(quality_Card);

                    quality_Card.Click += new RoutedEventHandler(quality_Card_Click);
                    //trending_Card.MouseEnter += new MouseEventHandler(trending_Cards_MouseEnter);
                    //trending_Card.MouseLeave += new MouseEventHandler(trending_Cards_MouseLeave);
                }
            }
            
        }

        private void quality_Card_Click(object sender, RoutedEventArgs e)
        {
            Wpf.Ui.Controls.Button buttonThatWasClicked = (Wpf.Ui.Controls.Button)sender;
            string seleted_urlQuality = ((dynamic)buttonThatWasClicked.Tag).url;
            string txtQuality = ((dynamic)buttonThatWasClicked.Tag).quality;

            isChangeQualityClick = true;
            urlQualityContent = seleted_urlQuality;

            webviewBrowser.Visibility = Visibility.Hidden;
            webviewBrowser.CoreWebView2.Navigate("https://appassets.example/index.html");
            
        }

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
                    //PageLoad.Visibility = Visibility.Visible;

                    return false;
                }
            }
            catch (Exception ex)
            {
                //PageLoad.Visibility = Visibility.Visible;


                return false;
            }
        }
        public async Task selectedanimeEP_urlValidation(string url)
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
                    fetch_selectedanimeEP = getResponsestring;
                }
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
                    //System.Windows.MessageBox.Show("Success Trending EP");
                    fetch_selectedanime = getResponsestring;
                }
            }

        }

        private void btnFullscreen_Click(object sender, RoutedEventArgs e)
        {
            FullScreen fl = new FullScreen(urlQualityContent);
            fl.ShowDialog();
        }

        private async void btnNextEP_Click(object sender, RoutedEventArgs e)
        {
            PageLoad.Visibility = Visibility.Visible;
            webviewBrowser.Visibility = Visibility.Hidden;

            ep_id = selectedanime.episodes[int.Parse(ep_num) + 1].id;
            ep_num = selectedanime.episodes[int.Parse(ep_num) + 1].number;

            //System.Windows.MessageBox.Show(ep_id +" | " + ep_num);
            isNextorPrev = true;

            await Task.WhenAll(fetch_SelectedAnimeEpisode());

            
            urlQualityContent = selectedepisode.sources[0].url;

            Next(selectedanime);


            webviewBrowser.CoreWebView2.Navigate("https://appassets.example/index.html");



        }

        private async void btnPreviousEP_Click(object sender, RoutedEventArgs e)
        {
            PageLoad.Visibility = Visibility.Visible;
            webviewBrowser.Visibility = Visibility.Hidden;

            ep_id = selectedanime.episodes[int.Parse(ep_num) - 1].id;
            ep_num = selectedanime.episodes[int.Parse(ep_num) - 1].number;

            //System.Windows.MessageBox.Show(ep_id +" | " + ep_num);
            isNextorPrev = true;

            await Task.WhenAll(fetch_SelectedAnimeEpisode());


            urlQualityContent = selectedepisode.sources[0].url;

            Prev(selectedanime);


            webviewBrowser.CoreWebView2.Navigate("https://appassets.example/index.html");
        }

        public void Next(anilist_Info info)
        {
            AnimeTitle.Text = info.title.romaji;
            int num = int.Parse(ep_num);
            int episodenumber = num - 1;
            int totalep = info.episodes.Length;

            //System.Windows.MessageBox.Show(episodenumber.ToString());


            if (info.episodes[episodenumber].title is not null)
            {
                AnimeEpisodeTitle.Text = "Episode " + num + ": " + info.episodes[episodenumber].title;
            }
            else
            {
                AnimeEpisodeTitle.Text = "Episode " + num;
            }
            EpisodeDescription.Text = info.episodes[episodenumber].description;


            //Previous Episode
            if (num == 1)
            {
                btnPreviousEP.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnPreviousEP.Visibility = Visibility.Visible;
                btnPreviousEP.ToolTip = "Episode " + info.episodes[num - 2].number;
            }

            //Next Episode
            if (totalep == num)
            {
                btnNextEP.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnNextEP.Visibility = Visibility.Visible;
                btnNextEP.ToolTip = "Episode " + info.episodes[num].number;
            }

            ep_num = episodenumber.ToString();
            //System.Windows.MessageBox.Show("NEXT " + ep_num);
        }
        public void Prev(anilist_Info info)
        {
            AnimeTitle.Text = info.title.romaji;
            int num = int.Parse(ep_num);
            int episodenumber = num - 1;
            int totalep = info.episodes.Length;

            //System.Windows.MessageBox.Show(episodenumber.ToString());


            if (info.episodes[episodenumber].title is not null)
            {
                AnimeEpisodeTitle.Text = "Episode " + num + ": " + info.episodes[episodenumber].title;
            }
            else
            {
                AnimeEpisodeTitle.Text = "Episode " + num;
            }
            EpisodeDescription.Text = info.episodes[episodenumber].description;


            //Previous Episode
            if (num == 1)
            {
                btnPreviousEP.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnPreviousEP.Visibility = Visibility.Visible;
                btnPreviousEP.ToolTip = "Episode " + info.episodes[num - 2].number;
            }

            //Next Episode
            if (totalep == num)
            {
                btnNextEP.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnNextEP.Visibility = Visibility.Visible;
                btnNextEP.ToolTip = "Episode " + info.episodes[num].number;
            }

            ep_num = episodenumber.ToString();
            //System.Windows.MessageBox.Show("PREV "+ep_num);
        }
    }
}
