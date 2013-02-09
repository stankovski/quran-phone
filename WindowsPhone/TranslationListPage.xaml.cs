using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using QuranPhone.ViewModels;
using QuranPhone.Common;
using QuranPhone.UI;
using QuranPhone.Utils;
using QuranPhone.Data;

namespace QuranPhone
{
    public partial class TranslationListPage : PhoneApplicationPage
    {
        public TranslationListPage()
        {
            InitializeComponent();
            header.NavigationRequest += header_NavigationRequest;
        }

        void header_NavigationRequest(object sender, NavigationEventArgs e)
        {
            NavigationService.Navigate(e.Uri);
        }

        // When page is navigated to set data context to selected item in list
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (DataContext == null)
            {
                DataContext = App.TranslationsListViewModel;                
            }
            if (!App.TranslationsListViewModel.IsDataLoaded)
                App.TranslationsListViewModel.LoadData();
            App.TranslationsListViewModel.NavigateRequested += viewModel_NavigateRequested;
        }

        void viewModel_NavigateRequested(object sender, EventArgs e)
        {
            var translation = sender as ObservableTranslationItem;
            if (translation == null)
                return;

            SettingsUtils.Set<string>(Constants.PREF_ACTIVE_TRANSLATION, translation.FileName);
            NavigationService.Navigate(new Uri("/TranslationPage.xaml?page=1&translation=" + translation.FileName, UriKind.Relative));
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            App.TranslationsListViewModel.NavigateRequested -= viewModel_NavigateRequested;
        }
    }
}