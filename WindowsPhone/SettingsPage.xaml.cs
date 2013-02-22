using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using QuranPhone.Data;
using QuranPhone.Utils;
using QuranPhone.ViewModels;

namespace QuranPhone
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        // When page is navigated to set data context to selected item in list
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            App.SettingsViewModel.LoadData();
            DataContext = App.SettingsViewModel;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SettingsUtils.Set(Constants.PREF_TRANSLATION_TEXT_SIZE, App.SettingsViewModel.TextSize);
        }

        private void Translations_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/TranslationListPage.xaml", UriKind.Relative));
        }

        private void LinkTap(object sender, RoutedEventArgs e)
        {
            var link = e.OriginalSource as Hyperlink;
            if (link != null)
            {
                var task = new WebBrowserTask() {Uri = link.NavigateUri};
                task.Show();
            }
        }
    }
}