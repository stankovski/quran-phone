using System;
using System.Collections.ObjectModel;
using System.Linq;
using Quran.Core;
using Quran.Core.Data;
using Quran.Core.Properties;
using Quran.Core.Utils;
using Quran.Core.ViewModels;
using Quran.Windows.Utils;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Quran.Windows.Views
{
    public partial class MainView
    {
        public MainViewModel ViewModel { get; set; }
        public SearchViewModel SearchViewModel { get; set; }
        public ObservableCollection<NavigationLink> NavigationLinks = new ObservableCollection<NavigationLink>();

        public MainView()
        {
            ViewModel = QuranApp.MainViewModel;
            SearchViewModel = QuranApp.SearchViewModel;
            InitializeComponent();
        }

        // Load data for the ViewModel Items
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.Initialize();
            await SearchViewModel.Initialize();
            JuzViewSource.Source = ViewModel.GetGrouppedJuzItems();
            BookmarksViewSource.Source = ViewModel.GetGrouppedBookmarks();
            BuildLocalizedApplicationBar();
            await LittleWatson.CheckForPreviousException();

            // Remove all back navigation options
            while (Frame.BackStack.Count() > 0)
            {
                Frame.BackStack.RemoveAt(Frame.BackStack.Count() - 1);
            }

            // Show welcome message
            showWelcomeMessage();
            
            // Show prompt to download content if not all images exist
            if (!await FileUtils.HaveAllImages())
            {
                try
                {
                    await ViewModel.Download();
                }
                catch (Exception ex)
                {
                    await QuranApp.NativeProvider.ShowErrorMessageBox("Failed to download Quran Data: " + ex.Message);
                }
            }
        }

        private void NavigateToLastPage(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(DetailsView));
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

        private void NavigateLink(object sender, TappedRoutedEventArgs e)
        {
            // If selected item is null (no selection) do nothing
            var list = sender as ListView;
            if (e.OriginalSource == null || !(e.OriginalSource is FrameworkElement))
            {
                return;
            }

            var selectedItem = ((FrameworkElement)e.OriginalSource).DataContext as ItemViewModel;

            try
            {
                SettingsUtils.Set<int>(Constants.PREF_LAST_PAGE, selectedItem.PageNumber);

                // Navigate to the new page
                if (selectedItem.SelectedAyah == null)
                {
                    Frame.Navigate(typeof(DetailsView));
                }
                else
                {
                    Frame.Navigate(typeof(DetailsView),
                        new NavigationData
                        {
                            Surah = selectedItem.SelectedAyah.Surah,
                            Ayah = selectedItem.SelectedAyah.Ayah
                        });
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
                {
                    ViewModel.DeleteBookmark(menuItem.DataContext as ItemViewModel);
                }
            }
            BookmarksViewSource.Source = ViewModel.GetGrouppedBookmarks();
        }

        private void HamburgerButtonClick(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
        }

        private void NavLinkItemClick(object sender, ItemClickEventArgs e)
        {
            MainSplitView.IsPaneOpen = false;
            var item = e.ClickedItem as NavigationLink;
            if (item != null)
            {
                item.Action();
            }
        }



        // Build a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            NavigationLinks.Add(new NavigationLink
            {
                Label = Quran.Core.Properties.Resources.search,
                Symbol = Symbol.Find,
                Action = () => { MainPivot.SelectedItem = SearchPivotItem; }
            });
        }

        private void SearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            SearchViewModel.Load(QuranSearchBox.Text);
        }

        private void GoToSettings(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsView), "general");
        }

        private void BookmarkRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
    }
}