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
using QuranPhone.Utils;
using QuranPhone.Data;

namespace QuranPhone
{
    public partial class TranslationPage : PhoneApplicationPage
    {
        //QuranScreenInfo screenInfo;
        // Constructor
        public TranslationPage()
        {
            InitializeComponent();
            //screenInfo = QuranScreenInfo.GetInstance();
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        // When page is navigated to set data context to selected item in list
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string selectedPage = "";
            string selectedTranslation = "";
            if (NavigationContext.QueryString.TryGetValue("page", out selectedPage))
            {
                NavigationContext.QueryString.TryGetValue("translation", out selectedTranslation);
                if (string.IsNullOrEmpty(selectedTranslation))
                    selectedTranslation = SettingsUtils.Get<string>(Constants.PREF_ACTIVE_TRANSLATION);
                int page = int.Parse(selectedPage);
                App.TranslationViewModel.CurrentPageNumber = page;
                App.TranslationViewModel.TranslationFile = selectedTranslation;                 
            }

            App.TranslationViewModel.LoadData();
            DataContext = App.TranslationViewModel;
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            var viewModel = (TranslationViewModel)DataContext;
            NavigationService.Navigate(new Uri("/DetailsPage.xaml?noBack=true&page=" + viewModel.CurrentPageNumber, UriKind.Relative));
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            DataContext = null;
            foreach (var page in App.TranslationViewModel.Pages)
            {
                page.Verses.Clear();
            }
            App.TranslationViewModel.Pages.Clear();
            App.TranslationViewModel.CurrentPageIndex = -1;
        }
    }
}