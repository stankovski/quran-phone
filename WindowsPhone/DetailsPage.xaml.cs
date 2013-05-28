using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Tasks;
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
            DataContext = null;

            if (NavigationContext.QueryString.TryGetValue("page", out selectedPage))
            {
                int page = int.Parse(selectedPage);
                App.DetailsViewModel.CurrentPageNumber = page;
                //Try extract translation from query
                var translation = SettingsUtils.Get<string>(Constants.PREF_ACTIVE_TRANSLATION);
                if (!string.IsNullOrEmpty(translation))
                {
                    if (App.DetailsViewModel.TranslationFile != translation.Split('|')[0] ||
                        App.DetailsViewModel.ShowTranslation != SettingsUtils.Get<bool>(Constants.PREF_SHOW_TRANSLATION) ||
                        App.DetailsViewModel.ShowArabicInTranslation != SettingsUtils.Get<bool>(Constants.PREF_SHOW_ARABIC_IN_TRANSLATION))
                    {
                        App.DetailsViewModel.Pages.Clear();
                    }
                    App.DetailsViewModel.TranslationFile = translation.Split('|')[0];
                    App.DetailsViewModel.ShowTranslation = SettingsUtils.Get<bool>(Constants.PREF_SHOW_TRANSLATION);
                    App.DetailsViewModel.ShowArabicInTranslation = SettingsUtils.Get<bool>(Constants.PREF_SHOW_ARABIC_IN_TRANSLATION);
                }
                else
                {
                    App.DetailsViewModel.TranslationFile = null;
                    App.DetailsViewModel.ShowTranslation = false;
                    App.DetailsViewModel.ShowArabicInTranslation = false;
                }
            }

            App.DetailsViewModel.LoadData();
            if (DataContext == null)
                DataContext = App.DetailsViewModel;
            radSlideView.SelectedItem = App.DetailsViewModel.Pages[App.DetailsViewModel.CurrentPageIndex];
            radSlideView.SelectionChanged += PageFlipped;
        }

        private void PageFlipped(object sender, SelectionChangedEventArgs e)
        {
            App.DetailsViewModel.CurrentPageIndex = App.DetailsViewModel.Pages.IndexOf((PageViewModel)radSlideView.SelectedItem);
        }        

        #region Menu Events

        private void Translation_Click(object sender, EventArgs e)
        {
            int pageNumber = ((DetailsViewModel)DataContext).CurrentPageNumber;
            if (!string.IsNullOrEmpty(App.DetailsViewModel.TranslationFile))
            {
                //App.DetailsViewModel.UpdatePages();
                App.DetailsViewModel.ShowTranslation = !App.DetailsViewModel.ShowTranslation;
                SettingsUtils.Set(Constants.PREF_SHOW_TRANSLATION, App.DetailsViewModel.ShowTranslation);
            }
            else
            {
                NavigationService.Navigate(new Uri("/TranslationListPage.xaml", UriKind.Relative));
            }
        }

        private void Bookmark_Click(object sender, EventArgs e)
        {
            try
            {
                using (var adapter = new BookmarksDBAdapter())
                {
                    adapter.AddBookmarkIfNotExists(null, null, App.DetailsViewModel.CurrentPageNumber);

                    App.DetailsViewModel.ToggleMenu();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("error creating bookmark");
            }
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            App.DetailsViewModel.ToggleMenu();

            int pageNumber = ((DetailsViewModel)DataContext).CurrentPageNumber;
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private void ScreenTap(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            App.DetailsViewModel.IsShowMenu = false;
        }

        #endregion Menu Events

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            NavigationContext.QueryString["page"] = SettingsUtils.Get<int>(Constants.PREF_LAST_PAGE).ToString(CultureInfo.InvariantCulture);
            foreach (var page in App.DetailsViewModel.Pages)
            {
                page.ImageSource = null;
            }
            App.DetailsViewModel.CurrentPageIndex = -1;
            radSlideView.SelectionChanged -= PageFlipped;            
        }

        private void ContactUs_Click(object sender, EventArgs e)
        {
            var email = new EmailComposeTask();
            email.To = "denis.stankovski@gmail.com";
            email.Subject = "Email from QuranPhone";
            email.Show();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            // if back key pressed when menu is visible, hide the menu
            // somehow, I (kemasdimas) frequently expect "back" key to hide menu,
            // instead of going back to previous page.
            if (App.DetailsViewModel.IsShowMenu)
            {
                App.DetailsViewModel.ToggleMenu();
                e.Cancel = true;
            }
            else
            {
                base.OnBackKeyPress(e);
            }
        }
#if DEBUG
        ~DetailsPage()
        {
            Console.WriteLine("Destroying DetailsPage");
        }
#endif
    }
}