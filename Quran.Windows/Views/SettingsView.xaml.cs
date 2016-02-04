using System;
using System.Linq;
using System.Collections.ObjectModel;
using Quran.Core;
using Quran.Core.Properties;
using Quran.Core.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Animation;
using Windows.Graphics.Display;
using Windows.Foundation.Metadata;
using Windows.Phone.UI.Input;

namespace Quran.Windows.Views
{
    public partial class SettingsView : Page
    {
        public SettingsViewModel ViewModel { get; set; }
        public ObservableCollection<NavigationLink> NavigationLinks = new ObservableCollection<NavigationLink>();

        public SettingsView()
        {
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
            ViewModel = QuranApp.SettingsViewModel;
            InitializeComponent();
        }

        // When page is navigated to set data context to selected item in list
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.Initialize();
            BuildLocalizedApplicationBar();

            string tab = e.Parameter as string;

            if (!string.IsNullOrEmpty(tab))
            {
                if (tab == "general")
                    this.MainPivot.SelectedItem = this.General;
                if (tab == "about")
                    this.MainPivot.SelectedItem = this.About;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.SaveSettings();
            base.OnNavigatedFrom(e);
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
                Label = Quran.Core.Properties.Resources.home,
                Symbol = Symbol.Home,
                Action = () => { Frame.Navigate(typeof(MainView)); }
            });
        }

        private void ShowTranslations(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(TranslationListView), null, new DrillInNavigationTransitionInfo());
        }
        
        private void ShowReciters(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RecitersListView), null, new DrillInNavigationTransitionInfo());
        }

    }
}