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
    }
}
