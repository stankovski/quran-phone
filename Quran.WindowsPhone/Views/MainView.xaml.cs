using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Quran.Core;
using Quran.Core.Data;
using Quran.Core.Utils;
using Quran.Core.ViewModels;
using Quran.WindowsPhone.Utils;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Quran.WindowsPhone.Views
{
    public partial class MainView
    {
        public MainViewModel ViewModel { get; set; }
        // Constructor
        public MainView()
        {
            InitializeComponent();

            ViewModel = QuranApp.MainViewModel;
            //header.NavigationRequest += header_NavigationRequest;
            LittleWatson.CheckForPreviousException().ConfigureAwait(false).GetAwaiter().GetResult();
            DataContextChanged += (s, e) => { ViewModel = DataContext as MainViewModel; };
        }

        void header_NavigationRequest(object sender, Type viewType, object[] parameters)
        {
            Frame.Navigate(viewType, parameters);
        }

        // Load data for the ViewModel Items
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Remove all back navigation options
            while (Frame.BackStack.Count() > 0)
            {
                Frame.BackStack.RemoveAt(Frame.BackStack.Count() - 1);
            }

            // Show welcome message
            showWelcomeMessage();

            if (!QuranApp.MainViewModel.IsDataLoaded)
            {
                QuranApp.MainViewModel.LoadData();
            }
            else
            {
                QuranApp.MainViewModel.RefreshData();
            }
            
            // Show prompt to download content if not all images exist
            if (!await FileUtils.HaveAllImages())
            {
                try
                {
                    await QuranApp.MainViewModel.Download();
                }
                catch (Exception)
                {
                    //Console.WriteLine("failed to download quran data: " + ex.Message);
                }
            }
        }

        private void showWelcomeMessage()
        {
            var versionFromConfig = new Version(SettingsUtils.Get<string>(Constants.PREF_CURRENT_VERSION));
            var nameHelper = SystemInfo.ApplicationName;
            var versionFromAssembly = new Version(SystemInfo.ApplicationVersion);
            if (versionFromAssembly > versionFromConfig)
            {
                var message =
                    @"Assalamu Aleikum,

Thank you for downloading Quran Phone. Please note that this is a BETA release and is still work in progress. 
New in Version 0.4.2:
* Bug fixes

If you find any issues with the app or would like to provide suggestions, please use Contact Us option available via the menu. 

Jazzakum Allahu Kheiran,
Quran Phone Team";
                QuranApp.NativeProvider.ShowInfoMessageBox(message, "Welcome");
                SettingsUtils.Set(Constants.PREF_CURRENT_VERSION, versionFromAssembly.ToString());
            }
        }

        // Handle selection changed on LongListSelector
        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If selected item is null (no selection) do nothing
            var list = sender as ListView;
            if (list == null || list.SelectedItem == null)
                return;

            var selectedItem = (ItemViewModel)list.SelectedItem;

            try
            {
                // Navigate to the new page
                if (selectedItem.SelectedAyah == null)
                {
                    //NavigationService.Navigate(
                    //    new Uri("/Views/DetailsView.xaml?page=" + selectedItem.PageNumber,
                    //            UriKind.Relative));
                }
                else
                {
                    //NavigationService.Navigate(
                    //    new Uri(
                    //        string.Format(CultureInfo.InvariantCulture, "/Views/DetailsView.xaml?page={0}&surah={1}&ayah={2}",
                    //                      selectedItem.PageNumber,
                    //                      selectedItem.SelectedAyah.Surah,
                    //                      selectedItem.SelectedAyah.Ayah), UriKind.Relative));
                }
            }
            catch
            {
                // Navigation exception - ignore
            }

            // Reset selected item to null (no selection)
            list.SelectedItem = null;
        }

        private void DeleteBookmark(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuFlyoutItem;
            if (menuItem != null)
            {
                if (menuItem.DataContext != null)
                    QuranApp.MainViewModel.DeleteBookmark(menuItem.DataContext as ItemViewModel);
            }
        }
    }
}