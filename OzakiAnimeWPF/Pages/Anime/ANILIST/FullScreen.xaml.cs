using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace Pages
{
    /// <summary>
    /// Interaction logic for FullScreen.xaml
    /// </summary>
    public partial class FullScreen : UiWindow
    {
        string ep_url;
        bool isClose = false;
        public FullScreen(string url)
        {
            InitializeComponent();
            ep_url = url;
            webviewBrowser.Visibility = Visibility.Hidden;
            webviewBrowser.NavigationCompleted += WebviewBrowser_NavigationCompleted;
            InitializeAsync();
        }

        private async void WebviewBrowser_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (isClose == true)
            {
                this.Close();
            }
            else
            {
                string scriptResult = await webviewBrowser.ExecuteScriptAsync($"document.getElementById('m3u8-placeholder').value = '" + ep_url + "';");
                await webviewBrowser.ExecuteScriptAsync($"document.getElementById('play-btn').click();");
                await Task.Delay(1000);
                webviewBrowser.Visibility = Visibility.Visible;
            }
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

            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string exeFolder = System.IO.Path.GetDirectoryName(exePath);
            string websiteFolder = System.IO.Path.Combine(exeFolder, "Files/PlayEP2");

            webviewBrowser.CoreWebView2.SetVirtualHostNameToFolderMapping("appassets.example", websiteFolder, CoreWebView2HostResourceAccessKind.DenyCors);
            webviewBrowser.CoreWebView2.Navigate("https://appassets.example/index.html");

            //SOURCE CODE FOR STREAMING WEBAPP
            //https://github.com/bharadwajpro/m3u8-player
        }

        private void UiWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                isClose = true;
                webviewBrowser.CoreWebView2.Navigate("https://appassets.example/index.html");
            }
        }
    }
}
