using OzakiAnimeWPF.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Services;
using Wpf.Ui.TaskBar;

namespace OzakiAnimeWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : UiWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            SettingsFile.DefaultDevSettingSave(false);

            //SplashScreen();
        }

        private async void UiWindow_Loaded(object sender, RoutedEventArgs e)
        {

            SplashUI.Visibility = Visibility.Visible;
            MainGrid.Visibility = Visibility.Hidden;


            RootFrame.Navigate(new Home(this));
            resetNavSelection();
            navHome.IsActive = true;

            await Task.Delay(7000);
            SplashUI.Visibility = Visibility.Hidden;
            MainGrid.Visibility = Visibility.Visible;
        }

        public  void SplashScreen()
        {
            
            Task.Run(async () =>
            {


                // Remember to always include Delays and Sleeps in
                // your applications to be able to charge the client for optimizations later.




                await Dispatcher.InvokeAsync( () =>
                {

                    //navService.Source = new Uri("Pages/Home.xaml", UriKind.Relative);
                    //System.Windows.MessageBox.Show(RootFrame.CurrentSource.ToString());




                });

                


                return true;
            });






        }

        private void navHome_Click(object sender, RoutedEventArgs e)
        {
            RootFrame.Navigate(new Home(this));
            resetNavSelection();
            navHome.IsActive = true;
        }

        private void NavigationItem_Click(object sender, RoutedEventArgs e)
        {
            RootFrame.Navigate(new Settings(this));
            resetNavSelection();
            navSettings.IsActive = true;
        }

        private void Account_Click(object sender, RoutedEventArgs e)
        {
            //RootFrame.Navigate(new SelectedAnime());
            //resetNavSelection();
            //Account.IsActive = true;
        }

        public void resetNavSelection()
        {
            Account.IsActive = false;
            navHome.IsActive = false;
            navSettings.IsActive = false;
        }




        void navihone()
        {
            // Navigate to URI using the Source property
            //RootFrame.Source = new Uri("Page/Home.xaml", UriKind.Relative);

            // Navigate to URI using the Navigate method
            //RootFrame.Navigate(new Uri("Pages/Home.xaml", UriKind.Relative));

            // Navigate to object using the Navigate method
            //RootFrame.Navigate(new Home(this));
        }


        private void TitleBar_CloseClicked(object sender, RoutedEventArgs e)
        {

            base.OnClosed(e);

            // Make sure that closing this window will begin the process of closing the application.
            Application.Current.Shutdown();
        }

        private void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            
            bool check = RootFrame.CanGoBack;
            //System.Windows.MessageBox.Show("navigated and canGoBack = "+ RootFrame.CanGoBack.ToString());
            if (check == true)
            {
                btnBack.Visibility = Visibility.Visible;
            }else if (check == false)
            {
                btnBack.Visibility = Visibility.Hidden;
            }

            //button.Content = RootFrame.CanGoBack.ToString();

            
        }

        private void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            //Fade Effect
            /*
            var fa = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3));
            (e.Content as Page).BeginAnimation(OpacityProperty, fa);
            */

            //Slide bottom to top
            var ta = new ThicknessAnimation();
            ta.Duration = TimeSpan.FromSeconds(0.2);
            ta.DecelerationRatio = 0.7;
            ta.To = new Thickness(0, 0, 0, 0);
            if (e.NavigationMode == NavigationMode.New)
            {
                ta.From = new Thickness(0, 100, 0, 0);
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                ta.From = new Thickness(100, 0, 0, 0);
            }
            (e.Content as Page).BeginAnimation(MarginProperty, ta);
            
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            RootFrame.GoBack();
        }

        //Reference for Future
        // About Invoke and Async : https://stackoverflow.com/questions/9732709/the-calling-thread-cannot-access-this-object-because-a-different-thread-owns-it
    }
}
