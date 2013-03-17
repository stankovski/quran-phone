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
            else
            {
                App.MainViewModel.RefreshData();
            }
            // Show prompt to download content if nomedia file exists
            if (!QuranFileUtils.HaveAllImages())
            {
                try
                {
                    downloadAndExtractQuranData();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("failed to download quran data: " + ex.Message);
                }
            }
        }

        private void downloadAndExtractQuranData()
        {
            // If downloaded offline and stuck in temp storage
            if (App.MainViewModel.QuranData.IsInTempStorage)
            {
                App.MainViewModel.IsInstalling = true;
                App.MainViewModel.QuranData.FinishPreviousDownload();
                App.MainViewModel.ExtractZipAndFinalize();
            }
                // If downloaded offline and stuck in temp storage
            else if (App.MainViewModel.QuranData.IsDownloaded)
            {
                App.MainViewModel.IsInstalling = true;
                App.MainViewModel.ExtractZipAndFinalize();
            }
            else
            {
                if (!App.MainViewModel.HasAskedToDownload)
                {
                    App.MainViewModel.HasAskedToDownload = true;
                    var response = MessageBox.Show(AppResources.downloadPrompt, AppResources.downloadPrompt_title,
                                                   MessageBoxButton.OKCancel);
                    if (response == MessageBoxResult.OK)
                    {
                        App.MainViewModel.IsInstalling = true;
                        App.MainViewModel.Download();
                    }
                }
            }
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

        private void DeleteBookmark(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                if (menuItem.DataContext != null)
                    App.MainViewModel.DeleteBookmark(menuItem.DataContext as ItemViewModel);
            }
        }
    }
}