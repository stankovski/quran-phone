using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using QuranPhone.Resources;
using QuranPhone.Utils;
using QuranPhone.ViewModels;

namespace QuranPhone
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the LongListSelector control to the sample data
            DataContext = App.MainViewModel;
            header.NavigationRequest += header_NavigationRequest;
        }

        void header_NavigationRequest(object sender, NavigationEventArgs e)
        {
            NavigationService.Navigate(e.Uri);
        }

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Remove all back navigation options
            while (NavigationService.BackStack.Count() > 0)
                NavigationService.RemoveBackEntry();

            if (!App.MainViewModel.IsDataLoaded)
            {
                App.MainViewModel.LoadData();
            }
            // Show prompt to download content if nomedia file exists
            if (QuranFileUtils.HaveAllImages())
            {
                try
                {
                    DownloadAndExtractQuranData();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("failed to download quran data: " + ex.Message);
                }
            }
        }

        private async void DownloadAndExtractQuranData()
        {
            App.MainViewModel.IsInstalling = true;

            // If downloaded offline and stuck in temp storage
            if (App.MainViewModel.QuranData.IsInTempStorage)
            {
                App.MainViewModel.QuranData.FinishPreviousDownload();
                await Task.Run(() => QuranFileUtils.ExtractZipFile(App.MainViewModel.QuranData.LocalUrl,
                                                                   QuranFileUtils.QURAN_BASE));
            }
                // If downloaded offline and stuck in temp storage
            else if (App.MainViewModel.QuranData.IsDownloaded)
            {
                await Task.Run(() => QuranFileUtils.ExtractZipFile(App.MainViewModel.QuranData.LocalUrl,
                                              QuranFileUtils.QURAN_BASE));
            }
            else
            {
                var response = MessageBox.Show(AppResources.downloadPrompt, AppResources.downloadPrompt_title,
                                               MessageBoxButton.OKCancel);
                if (response == MessageBoxResult.OK)
                {
                    App.MainViewModel.Download();
                    await Task.Run(() => QuranFileUtils.ExtractZipFile(App.MainViewModel.QuranData.LocalUrl,
                                                  QuranFileUtils.QURAN_BASE));
                }
            }

            App.MainViewModel.IsInstalling = false;
        }

        // Handle selection changed on LongListSelector
        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If selected item is null (no selection) do nothing
            var list = sender as LongListSelector;
            if (list == null || list.SelectedItem == null)
                return;

            // Navigate to the new page
            NavigationService.Navigate(new Uri("/DetailsPage.xaml?page=" + (list.SelectedItem as ItemViewModel).PageNumber, UriKind.Relative));

            // Reset selected item to null (no selection)
            list.SelectedItem = null;
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}