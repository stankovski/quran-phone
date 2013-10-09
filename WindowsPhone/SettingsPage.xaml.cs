using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace QuranPhone
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            DataContext = App.SettingsViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            App.SettingsViewModel.LoadData();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            App.DetailsViewModel.ClearPages();
        }

        private void Translations_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/TranslationListPage.xaml", UriKind.Relative));
        }
    }
}