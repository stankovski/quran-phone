using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Tasks;
using QuranPhone.Resources;
using QuranPhone.UI;
using QuranPhone.ViewModels;
using QuranPhone.Utils;
using QuranPhone.Data;
using System.Globalization;
using Microsoft.Phone.Controls;
using Telerik.Windows.Controls;

namespace QuranPhone
{
    public partial class DetailsPage : PhoneApplicationPage
    {
        private RadContextMenu bookmarkMenu = new RadContextMenu();
            
        // Constructor
        public DetailsPage()
        {
            InitializeComponent();

            App.DetailsViewModel.Orientation = this.Orientation;
            
            bookmarkMenu.Items.Add(new RadContextMenuItem() { Content = AppResources.bookmark_ayah });
            bookmarkMenu.ItemTapped += BookmarkAyah_Click;
        }

        // When page is navigated to set data context to selected item in list
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string selectedPage;
            string selectedSurah;
            string selectedAyah;
            DataContext = null;

            NavigationContext.QueryString.TryGetValue("page", out selectedPage);
            NavigationContext.QueryString.TryGetValue("surah", out selectedSurah);
            NavigationContext.QueryString.TryGetValue("ayah", out selectedAyah);

            if (selectedPage != null)
            {
                int page = int.Parse(selectedPage);
                App.DetailsViewModel.CurrentPageNumber = page;
                //Select ayah
                if (selectedSurah != null && selectedAyah != null)
                {
                    int surah = int.Parse(selectedSurah);
                    int ayah = int.Parse(selectedAyah);
                    App.DetailsViewModel.SelectedAyah = new Common.QuranAyah(surah, ayah);
                }

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

            // set keepinfooverlay according to setting
            App.DetailsViewModel.KeepInfoOverlay = SettingsUtils.Get<bool>(Constants.PREF_KEEP_INFO_OVERLAY);
        }

        private void PageFlipped(object sender, SelectionChangedEventArgs e)
        {
            App.DetailsViewModel.CurrentPageIndex = App.DetailsViewModel.Pages.IndexOf((PageViewModel)radSlideView.SelectedItem);
        }

        private void ScreenTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.DetailsViewModel.ToggleMenu();
        }

        private void ImageHold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var ayah = CachedImage.GetAyahFromGesture(e.GetPosition(radSlideView), App.DetailsViewModel.CurrentPageNumber, radSlideView.ActualWidth);
            App.DetailsViewModel.SelectedAyah = ayah;
            bookmarkMenu.RegionOfInterest = new Rect(e.GetPosition(ThisPage), new Size(50, 50));
            bookmarkMenu.IsOpen = true;
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
                App.DetailsViewModel.ToggleMenu();
            }
            else
            {
                NavigationService.Navigate(new Uri("/TranslationListPage.xaml", UriKind.Relative));
            }
        }

        private void Bookmark_Click(object sender, EventArgs e)
        {
            App.DetailsViewModel.AddBookmark();
            App.DetailsViewModel.ToggleMenu();
        }

        private void BookmarkAyah_Click(object sender, ContextMenuItemSelectedEventArgs e)
        {
            App.DetailsViewModel.AddBookmark();
            App.DetailsViewModel.SelectedAyah = null;
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            App.DetailsViewModel.ToggleMenu();
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private void Search_Click(object sender, EventArgs e)
        {
            App.DetailsViewModel.ToggleMenu();
            NavigationService.Navigate(new Uri("/SearchPage.xaml", UriKind.Relative));
        }

        private void ContactUs_Click(object sender, EventArgs e)
        {
            var email = new EmailComposeTask();
            email.To = "quran.phone@gmail.com";
            email.Subject = "Email from QuranPhone";
            email.Show();
        }


        // TO BE USED IN THE FUTURE
        private void AyahTapped(object sender, Common.QuranAyahEventArgs e)
        {
            App.DetailsViewModel.SelectedAyah = e.QuranAyah;
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

        private void PageOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            App.DetailsViewModel.Orientation = e.Orientation;
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