using System;
using System.Collections.Generic;
using System.Globalization;
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
using Telerik.Windows.Controls;

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

            // Show welcome message
            showWelcomeMessage();

            if (!App.MainViewModel.IsDataLoaded)
            {
                App.MainViewModel.LoadData();
            }
            else
            {
                App.MainViewModel.RefreshData();
            }
            
            // Show prompt to download content if not all images exist
            if (!QuranFileUtils.HaveAllImages())
            {
                try
                {
                    App.MainViewModel.Download();
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
New in Version 0.3.1:
* Support for Ibn Katheer tafseer
* Ayah bookmarking
* Ability to generate Quranic Dua as bookmarks (accessible from Settings)
* Misc. layout changes and bug fixes

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
                        new Uri("/DetailsPage.xaml?page=" + selectedItem.PageNumber,
                                UriKind.Relative));
                }
                else
                {
                    NavigationService.Navigate(
                        new Uri(
                            string.Format(CultureInfo.InvariantCulture, "/DetailsPage.xaml?page={0}&surah={1}&ayah={2}",
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
                    App.MainViewModel.DeleteBookmark(menuItem.DataContext as ItemViewModel);
            }
        }
    }
}