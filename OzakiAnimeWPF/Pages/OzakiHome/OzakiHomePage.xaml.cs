using System;
using System.Collections.Generic;
using System.Linq;
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

namespace OzakiAnimeWPF.Pages.OzakiHome
{
    /// <summary>
    /// Interaction logic for OzakiHomePage.xaml
    /// </summary>
    public partial class OzakiHomePage : UiPage
    {
        public OzakiHomePage()
        {
            InitializeComponent();
        }

        private void StarRepo_Click(object sender, RoutedEventArgs e)
        {
            var destinationurl = "https://github.com/Ozakinn/OzakiAnimeWPF";
            var sInfo = new System.Diagnostics.ProcessStartInfo(destinationurl)
            {
                UseShellExecute = true,
            };
            System.Diagnostics.Process.Start(sInfo);
        }
    }
}
