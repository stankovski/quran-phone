using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Quran.Core;
using Quran.Core.Data;
using Quran.Core.Utils;
using Quran.Core.ViewModels;
using Quran.WindowsPhone.Utils;
using Telerik.Windows.Controls;

namespace Quran.WindowsPhone.Views
{
    public partial class MainView
    {
        // Constructor
        public MainView()
        {
            InitializeComponent();

            QuranApp.MainViewModel = new MainViewModelWindowsPhone();
            DataContext = QuranApp.MainViewModel;
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
            if (!FileUtils.HaveAllImages())
            {
                try
                {
                    QuranApp.MainViewModel.Download();
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
                    @"Assalamu Aleikum,

Thank you for downloading Quran Phone. Please note that this is a BETA release and is still work in progress. 
New in Version 0.4.0:
* Added non-streaming audio recitation (work in progress so please report any issues)
* Misc. bug fixes

If you find any issues with the app or would like to provide suggestions, please use Contact Us option available via the menu. 

Jazzakum Allahu Kheiran,
Quran Phone Team";
                MessageBox.Show(message, "Welcome", MessageBoxButton.OK);
                SettingsUtils.Set(Constants.PREF_CURRENT_VERSION, versionFromAssembly.ToString());
            }
        }

        // Handle selection changed on LongListSelector
        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If selected item is null (no selection) do nothing
            var list = sender as RadDataBoundListBox;
            if (list == null || list.SelectedItem == null)
                return;

            var selectedItem = (ItemViewModel)list.SelectedItem;

            try
            {
                // Navigate to the new page
                if (selectedItem.SelectedAyah == null)
                {
                    NavigationService.Navigate(
                        new Uri("/Views/DetailsView.xaml?page=" + selectedItem.PageNumber,
                                UriKind.Relative));
                }
                else
                {
                    NavigationService.Navigate(
                        new Uri(
                            string.Format(CultureInfo.InvariantCulture, "/Views/DetailsView.xaml?page={0}&surah={1}&ayah={2}",
                                          selectedItem.PageNumber,
                                          selectedItem.SelectedAyah.Sura,
                                          selectedItem.SelectedAyah.Ayah), UriKind.Relative));
                }
            }
            catch
            {
                // Navigation exception - ignore
            }

            // Reset selected item to null (no selection)
            list.SelectedItem = null;
        }

        private void DeleteBookmark(object sender, ContextMenuItemSelectedEventArgs e)
        {
            var menuItem = sender as RadContextMenuItem;
            if (menuItem != null)
            {
                if (menuItem.DataContext != null)
                    QuranApp.MainViewModel.DeleteBookmark(menuItem.DataContext as ItemViewModel);
            }
        }
    }
}