using System;
using System.Collections.ObjectModel;
using Quran.Core;
using Quran.Core.Properties;
using Quran.Core.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Quran.WindowsPhone.Views
{
    public partial class SettingsView : Page
    {
        public SettingsViewModel ViewModel { get; set; }
        public ObservableCollection<NavigationLink> NavigationLinks = new ObservableCollection<NavigationLink>();

        public SettingsView()
        {
        }

        // When page is navigated to set data context to selected item in list
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel = await QuranApp.GetSettingsViewModel();
            await ViewModel.LoadData();
            InitializeComponent();
            BuildLocalizedApplicationBar();

            string tab = e.Parameter as string;

            if (!string.IsNullOrEmpty(tab))
            {
                if (tab == "general")
                    this.MainPivot.SelectedItem = this.General;
                if (tab == "audio")
                    this.MainPivot.SelectedItem = this.Audio;
                if (tab == "about")
                    this.MainPivot.SelectedItem = this.About;
            }
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            QuranApp.SyncViewModelsWithSettings();
            await QuranApp.DetailsViewModel.RefreshCurrentPage();
            base.OnNavigatedFrom(e);
        }
        
        private void Translations_Click(object sender, RoutedEventArgs e)
        {
            //NavigationService.Navigate(new Uri("/Views/TranslationListView.xaml", UriKind.Relative));
        }

        private void Reciters_Click(object sender, RoutedEventArgs e)
        {
            //NavigationService.Navigate(new Uri("/Views/RecitersListView.xaml", UriKind.Relative));
        }

        private void LinkTap(object sender, RoutedEventArgs e)
        {
            //var link = e.OriginalSource as Hyperlink;
            //if (link != null)
            //{
            //    var task = new WebBrowserTask() {Uri = link.NavigateUri};
            //    task.Show();
            //}
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
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
                Label = "Home",
                Symbol = Symbol.Home,
                Action = () => { Frame.Navigate(typeof(MainView)); }
            });
        }
    }
}