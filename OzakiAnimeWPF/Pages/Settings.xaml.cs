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
using Wpf.Ui.Common;
using Wpf.Ui.Controls;

namespace OzakiAnimeWPF.Pages
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UiPage
    {
        private MainWindow mainForm;

        SettingsFile settingconfig = new SettingsFile();

        public Settings(MainWindow main)
        {
            InitializeComponent();
            mainForm = main;
        }

        private void btnSave_DeveloperSettings_Click(object sender, RoutedEventArgs e)
        {
            if (txtAPILink.Text.Length != 0)
            {
                settingconfig.SaveCustomSetting(txtAPILink.Text);
                Snackbar("Developer Settings", "Save Successfully!", SymbolRegular.CheckmarkCircle48);

            }
            else
            {
                Snackbar("Developer Settings", "No changes occured.", SymbolRegular.Info28);
            }
        }

        private void UiPage_Loaded(object sender, RoutedEventArgs e)
        {
            mainForm.PageLoading.Visibility = Visibility.Hidden;
        }

        public async void Snackbar(string title, string msgs, SymbolRegular icon)
        {
            mainForm.RootSnackbar.Title = title;
            mainForm.RootSnackbar.Message = msgs;
            mainForm.RootSnackbar.Icon = icon;
            await mainForm.RootSnackbar.ShowAsync();
        }

        private void btnReset_DeveloperSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsFile.DefaultDevSettingSave(true);
            Snackbar("Developer Settings", "Reset Successfully!", SymbolRegular.CheckmarkCircle48);
        }
    }
}
