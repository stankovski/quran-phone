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
using System.Globalization;

namespace QuranPhone
{
    public partial class TranslationListPage : PhoneApplicationPage
    {
        public TranslationListPage()
        {
            InitializeComponent();
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

            SettingsUtils.Set(Constants.PREF_ACTIVE_TRANSLATION, translation.FileName);
            SettingsUtils.Set(Constants.PREF_SHOW_TRANSLATION, true);
            NavigationService.GoBack();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            App.TranslationsListViewModel.NavigateRequested -= viewModel_NavigateRequested;
        }
    }
}