using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using QuranPhone.Data;
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
            LittleWatson.CheckForPreviousException();
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

            // Show welcom message
            showWelcomeMessage();

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

        private void showWelcomeMessage()
        {
            var versionFromConfig = new Version(SettingsUtils.Get<string>(Constants.PREF_CURRENT_VERSION));
            var nameHelper = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
            var versionFromAssembly = nameHelper.Version;
            if (versionFromAssembly > versionFromConfig)
            {
                var message =
                    "Assalamu Aleikum,\n\nThank you for downloading Quran Phone. Please note that this is a BETA release and is still work in progress. More features such as recitation will be added in the future inshaAllah. If you find any issues with the app or would like to provide suggestions, please use Contact Us option available via the menu. \n\nJazzakum Allahu Kheiran,\nDenis S.";
                MessageBox.Show(message, "Welcome", MessageBoxButton.OK);
                SettingsUtils.Set(Constants.PREF_CURRENT_VERSION, versionFromAssembly.ToString());
            }
        }

        private async void downloadAndExtractQuranData()
        {
            bool finalizeSucceded = true;
            bool isCurrentlyDownloading = IsCurrentlyDownloading();
            // If downloaded offline and stuck in temp storage
            if (App.MainViewModel.QuranData.IsInTempStorage && !isCurrentlyDownloading)
            {
                App.MainViewModel.IsInstalling = true;
                App.MainViewModel.QuranData.FinishPreviousDownload();
                finalizeSucceded = await App.MainViewModel.ExtractZipAndFinalize();
            }
            // If downloaded offline and stuck in temp storage
            else if (App.MainViewModel.QuranData.IsInLocalStorage && !isCurrentlyDownloading)
            {
                App.MainViewModel.IsInstalling = true;
                finalizeSucceded = await App.MainViewModel.ExtractZipAndFinalize();
            }

            if (!App.MainViewModel.HasAskedToDownload || !finalizeSucceded)
            {
                App.MainViewModel.HasAskedToDownload = true;
                MessageBoxResult askingToDownloadResult = MessageBoxResult.OK;

                if (!isCurrentlyDownloading)
                {
                    askingToDownloadResult = MessageBox.Show(AppResources.downloadPrompt,
                                                             AppResources.downloadPrompt_title,
                                                             MessageBoxButton.OKCancel);
                }

                if (isCurrentlyDownloading || askingToDownloadResult == MessageBoxResult.OK)
                {
                    App.MainViewModel.IsInstalling = true;
                    App.MainViewModel.Download();
                    var downloadId = App.MainViewModel.QuranData.DownloadId;
                    SettingsUtils.Set(Constants.PREF_LAST_DOWNLOAD_ID, downloadId);
                }
            }
        }

        private static bool IsCurrentlyDownloading()
        {
            bool isAlreadyDownloading = false;
            string lastDownloadId = SettingsUtils.Get<string>(Constants.PREF_LAST_DOWNLOAD_ID);
            if (lastDownloadId != null)
            {
                var status = App.MainViewModel.QuranData.GetDownloadStatus(lastDownloadId);
                if (status == Microsoft.Phone.BackgroundTransfer.TransferStatus.Transferring ||
                    status == Microsoft.Phone.BackgroundTransfer.TransferStatus.Waiting ||
                    status == Microsoft.Phone.BackgroundTransfer.TransferStatus.WaitingForExternalPower ||
                    status ==
                    Microsoft.Phone.BackgroundTransfer.TransferStatus.WaitingForExternalPowerDueToBatterySaverMode ||
                    status == Microsoft.Phone.BackgroundTransfer.TransferStatus.WaitingForNonVoiceBlockingNetwork ||
                    status == Microsoft.Phone.BackgroundTransfer.TransferStatus.WaitingForWiFi)
                    isAlreadyDownloading = true;
            }
            return isAlreadyDownloading;
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