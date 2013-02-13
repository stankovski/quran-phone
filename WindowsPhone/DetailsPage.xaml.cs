using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using QuranPhone.ViewModels;
using QuranPhone.Utils;
using QuranPhone.Data;
using System.Globalization;
using Microsoft.Phone.Controls;

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
            string selectedPage;
            if (NavigationContext.QueryString.TryGetValue("page", out selectedPage))
            {
                int page = int.Parse(selectedPage);
                App.TranslationViewModel.CurrentPageNumber = page;
                //Try extract translation from query
                var translation = SettingsUtils.Get<string>(Constants.PREF_ACTIVE_TRANSLATION);
                if (!string.IsNullOrEmpty(translation))
                {
                    App.TranslationViewModel.TranslationFile = translation;
                    App.TranslationViewModel.ShowTranslation = SettingsUtils.Get<bool>(Constants.PREF_SHOW_TRANSLATION);
                }
                else
                {
                    App.TranslationViewModel.ShowTranslation = false;
                }
            }

            App.TranslationViewModel.LoadData();
            DataContext = App.TranslationViewModel;
            radSlideView.SelectedItem = App.TranslationViewModel.Pages[TranslationViewModel.PAGES_TO_PRELOAD];
            radSlideView.SelectionChanged += PageFlipped;
        }

        private void PageFlipped(object sender, SelectionChangedEventArgs e)
        {
            App.TranslationViewModel.CurrentPageIndex = App.TranslationViewModel.Pages.IndexOf((PageViewModel)radSlideView.SelectedItem);
        }        

        #region Menu Events

        private void Translation_Click(object sender, EventArgs e)
        {
            int pageNumber = ((TranslationViewModel)DataContext).CurrentPageNumber;
            if (!string.IsNullOrEmpty(App.TranslationViewModel.TranslationFile))
            {
                App.TranslationViewModel.UpdatePages();
                App.TranslationViewModel.ShowTranslation = !App.TranslationViewModel.ShowTranslation;
            }
            else
            {
                NavigationService.Navigate(new Uri("/TranslationListPage.xaml", UriKind.Relative));
            }
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            int pageNumber = ((TranslationViewModel)DataContext).CurrentPageNumber;
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private void ScreenTap(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.ApplicationBar.IsVisible = false;
            menuToggleButton.Visibility = Visibility.Visible;
        }

        private void MenuToggle(object sender, RoutedEventArgs e)
        {
            this.ApplicationBar.IsVisible = true;
            menuToggleButton.Visibility = Visibility.Collapsed;
        }

        #endregion Menu Events

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            this.DataContext = null;
            foreach (var page in App.TranslationViewModel.Pages)
            {
                page.ImageSource = null;
            }
            App.TranslationViewModel.Pages.Clear();
            App.TranslationViewModel.CurrentPageIndex = -1;
            radSlideView.SelectionChanged -= PageFlipped;            
        }

#if DEBUG
        ~DetailsPage()
        {
            Console.WriteLine("Destroying DetailsPage");
        }
#endif
    }
}