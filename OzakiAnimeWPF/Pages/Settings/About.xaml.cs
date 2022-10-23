using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OzakiAnimeWPF;
using Squirrel;
using Wpf.Ui.Controls;

namespace Pages.Settings
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : UiPage
    {
        public About()
        {
            InitializeComponent();
            getVersion();
        }

        private void Github_Click(object sender, RoutedEventArgs e)
        {
            var destinationurl = "https://github.com/Ozakinn/OzakiAnimeWPF";
            var sInfo = new System.Diagnostics.ProcessStartInfo(destinationurl)
            {
                UseShellExecute = true,
            };
            System.Diagnostics.Process.Start(sInfo);
            showDialog();
        }

        public void getVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fileVersion.FileVersion;
            buildVersion.Content = "Build Version "+ version;
        }

        public void showDialog()
        {
            AboutDialog.Title = "Hey! Wait..";
            dialogContent.Text = "Don't forget to ⭐ this repo! Thank you!";
            AboutDialog.ButtonLeftClick += AboutDialog_ButtonLeftClick;
            AboutDialog.ButtonRightVisibility = Visibility.Hidden;
            AboutDialog.Show();
        }

        private void AboutDialog_ButtonLeftClick(object sender, RoutedEventArgs e)
        {
            AboutDialog.Hide();
        }

        private async void CheckUpdate_Click(object sender, RoutedEventArgs e)
        {

            using (var mgr = new GithubUpdateManager(@"https://github.com/Ozakinn/OzakiAnimeWPF"))
            {

                var newVersion = await mgr.UpdateApp();

                System.Windows.MessageBox.Show(newVersion.ToString());
                // optionally restart the app automatically, or ask the user if/when they want to restart
                if (newVersion != null)
                {
                    UpdateManager.RestartApp();
                }
            }
        }
    }
}
