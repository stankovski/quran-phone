using System;
using Quran.Core;
using Quran.Core.Utils;
using Quran.Core.ViewModels;
using Quran.Core.Data;
using Windows.UI.Xaml.Navigation;
using Windows.Graphics.Display;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace Quran.Windows.Views
{
    public partial class RecitersListView : Page
    {
        public RecitersListViewModel ViewModel { get; set; }

        public RecitersListView()
        {
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
            ViewModel = QuranApp.RecitersListViewModel;
            InitializeComponent();
        }

        // When page is navigated to set data context to selected item in list
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.Initialize();
            ReciterViewSource.Source = ViewModel.Groups;
        }

        private void ManageRequested(object sender, RoutedEventArgs e)
        {
            var list = sender as FrameworkElement;
            if (list == null || list.DataContext == null)
                return;

            var qari = (ObservableReciterItem)list.DataContext;
            if (qari == null)
            {
                return;
            }

            Frame.Navigate(typeof(SurahDownloadView), qari.Id, new SlideNavigationTransitionInfo());
        }


        private void NavigationBackRequested(object sender, TappedRoutedEventArgs e)
        {
            var list = sender as FrameworkElement;
            if (list == null || list.DataContext == null)
                return;

            var qari = (ObservableReciterItem)list.DataContext;
            if (qari == null)
            {
                return;
            }

            SettingsUtils.Set(Constants.PREF_ACTIVE_QARI, qari.Name);
            Frame.GoBack();
        }
    }
}