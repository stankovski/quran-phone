using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using QuranPhone.Resources;
using QuranPhone.ViewModels;
using QuranPhone.Utils;
using QuranPhone.Data;
using System.Globalization;

namespace QuranPhone
{
    public partial class DetailsPage : PhoneApplicationPage
    {
        //QuranScreenInfo screenInfo;
        // Constructor
        public DetailsPage()
        {
            InitializeComponent();
        }

        // When page is navigated to set data context to selected item in list
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string selectedPage = "";
            if (NavigationContext.QueryString.TryGetValue("page", out selectedPage))
            {
                int page = int.Parse(selectedPage);
                App.DetailsViewModel.CurrentPageNumber = page;
            }

            App.DetailsViewModel.LoadData();
            DataContext = App.DetailsViewModel;
        }

        //private void Pivot_LoadingItem(object sender, PivotItemEventArgs e)
        //{
        //    var pageModel = (PageViewModel)e.Item.DataContext;
        //    pageModel.ImageSource = QuranFileUtils.GetImageFromWeb(QuranFileUtils.GetPageFileName(pageModel.PageNumber));
        //}

        private void Translation_Click(object sender, EventArgs e)
        {
            // Navigate to the translation page
            int pageNumber = ((DetailsViewModel)DataContext).CurrentPageNumber;
            var translation = SettingsUtils.Get<string>(Constants.PREF_ACTIVE_TRANSLATION);
            if (!string.IsNullOrEmpty(translation))
                NavigationService.Navigate(new Uri(string.Format(CultureInfo.InvariantCulture, "/TranslationPage.xaml?page={0}&translation={1}", pageNumber, translation), UriKind.Relative));
            else
                NavigationService.Navigate(new Uri(string.Format(CultureInfo.InvariantCulture, "/TranslationListPage.xaml?page={0}", pageNumber), UriKind.Relative));
        }

        private void ScreenTap(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.ApplicationBar.IsVisible = false;
            menuToggleButton.Visibility = System.Windows.Visibility.Visible;
        }

        private void MenuToggle(object sender, RoutedEventArgs e)
        {
            this.ApplicationBar.IsVisible = true;
            menuToggleButton.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void OnBackNavigation(object sender, System.ComponentModel.CancelEventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            this.DataContext = null;
            foreach (var page in App.DetailsViewModel.Pages)
            {
                page.ImageSource = null;
            }
            App.DetailsViewModel.Pages.Clear();
            App.DetailsViewModel.CurrentPageIndex = -1;
        }
    }
}