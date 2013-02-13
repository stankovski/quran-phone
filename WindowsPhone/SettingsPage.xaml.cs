using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
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

        private void Translations_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/TranslationListPage.xaml", UriKind.Relative));
        }
    }
}