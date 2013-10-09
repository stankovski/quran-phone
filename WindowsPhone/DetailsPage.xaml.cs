using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using QuranPhone.Common;
using QuranPhone.Data;
using QuranPhone.Resources;
using QuranPhone.UI;
using QuranPhone.Utils;
using QuranPhone.ViewModels;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace QuranPhone
{
    public partial class DetailsPage : PhoneApplicationPage
    {
        private readonly RadContextMenu _bookmarkMenu = new RadContextMenu();

        public DetailsPage()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
            App.DetailsViewModel.Orientation = Orientation;
            _bookmarkMenu.Items.Add(new RadContextMenuItem {Content = AppResources.bookmark_ayah});
            _bookmarkMenu.ItemTapped += BookmarkAyah_Click;
            _bookmarkMenu.Closed += (obj, e) => App.DetailsViewModel.SelectedAyah = null;
        }

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
                int page = int.Parse(selectedPage, CultureInfo.InvariantCulture);
                App.DetailsViewModel.CurrentPageNumber = page;

                //Update settings
                App.DetailsViewModel.IsNightMode = SettingsUtils.Get<bool>(Constants.PrefNightMode);

                //Try extract translation from query
                var translation = SettingsUtils.Get<string>(Constants.PrefActiveTranslation);
                if (!string.IsNullOrEmpty(translation))
                {
                    if (App.DetailsViewModel.TranslationFile != translation.Split('|')[0] ||
                        App.DetailsViewModel.ShowTranslation != SettingsUtils.Get<bool>(Constants.PrefShowTranslation) ||
                        App.DetailsViewModel.ShowArabicInTranslation !=
                        SettingsUtils.Get<bool>(Constants.PrefShowArabicInTranslation))
                    {
                        App.DetailsViewModel.Pages.Clear();
                    }
                    App.DetailsViewModel.TranslationFile = translation.Split('|')[0];
                    App.DetailsViewModel.ShowTranslation = SettingsUtils.Get<bool>(Constants.PrefShowTranslation);
                    App.DetailsViewModel.ShowArabicInTranslation =
                        SettingsUtils.Get<bool>(Constants.PrefShowArabicInTranslation);
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
            {
                DataContext = App.DetailsViewModel;
            }
            radSlideView.SelectedItem = App.DetailsViewModel.Pages[App.DetailsViewModel.CurrentPageIndex];
            radSlideView.SelectionChanged += PageFlipped;

            // set keepinfooverlay according to setting
            App.DetailsViewModel.KeepInfoOverlay = SettingsUtils.Get<bool>(Constants.PrefKeepInfoOverlay);

            //Select ayah
            if (selectedSurah != null && selectedAyah != null)
            {
                int surah = int.Parse(selectedSurah, CultureInfo.InvariantCulture);
                int ayah = int.Parse(selectedAyah, CultureInfo.InvariantCulture);
                App.DetailsViewModel.SelectedAyah = new QuranAyah(surah, ayah);
            }
            else
            {
                App.DetailsViewModel.SelectedAyah = null;
            }
        }

        private void PageFlipped(object sender, SelectionChangedEventArgs e)
        {
            App.DetailsViewModel.SelectedAyah = null;
            App.DetailsViewModel.CurrentPageIndex =
                App.DetailsViewModel.Pages.IndexOf((PageViewModel) radSlideView.SelectedItem);
        }

        private void ScreenTap(object sender, GestureEventArgs e)
        {
            App.DetailsViewModel.IsShowMenu = false;
        }

        private void MenuTap(object sender, GestureEventArgs e)
        {
            App.DetailsViewModel.IsShowMenu = true;
            e.Handled = true;
        }

        private void ImageTap(object sender, GestureEventArgs e)
        {
            App.DetailsViewModel.SelectedAyah = null;
        }

        private void ImageHold(object sender, GestureEventArgs e)
        {
            if (App.DetailsViewModel.AyahDetailsExist && sender != null)
            {
                var cachedImage = sender as CachedImage;
                if (cachedImage == null)
                {
                    return;
                }

                QuranAyah ayah = CachedImage.GetAyahFromGesture(e.GetPosition(cachedImage.Image),
                    App.DetailsViewModel.CurrentPageNumber, radSlideView.ActualWidth);
                App.DetailsViewModel.SelectedAyah = ayah;

                _bookmarkMenu.RegionOfInterest = new Rect(e.GetPosition(ThisPage), new Size(50, 50));
                _bookmarkMenu.IsOpen = true;
            }
        }

        private void ImageDoubleTap(object sender, GestureEventArgs e)
        {
            if (App.DetailsViewModel.AyahDetailsExist && sender != null &&
                !string.IsNullOrEmpty(App.DetailsViewModel.TranslationFile))
            {
                var cachedImage = sender as CachedImage;
                if (cachedImage == null)
                {
                    return;
                }

                QuranAyah ayah = CachedImage.GetAyahFromGesture(e.GetPosition(cachedImage.Image),
                    App.DetailsViewModel.CurrentPageNumber, radSlideView.ActualWidth);
                PageViewModel currentPage = App.DetailsViewModel.CurrentPage;
                if (currentPage != null)
                {
                    App.DetailsViewModel.SelectedAyah = ayah;
                    App.DetailsViewModel.ShowTranslation = !App.DetailsViewModel.ShowTranslation;
                    SettingsUtils.Set(Constants.PrefShowTranslation, App.DetailsViewModel.ShowTranslation);
                }
            }
        }

        private void ListBoxDoubleTap(object sender, GestureEventArgs e)
        {
            if (App.DetailsViewModel.AyahDetailsExist && sender != null && sender is RadDataBoundListBox)
            {
                var selectedVerse = ((RadDataBoundListBox) sender).SelectedItem as VerseViewModel;
                if (selectedVerse != null)
                {
                    App.DetailsViewModel.SelectedAyah = new QuranAyah(selectedVerse.Surah, selectedVerse.Ayah);
                }
                App.DetailsViewModel.ShowTranslation = !App.DetailsViewModel.ShowTranslation;
                SettingsUtils.Set(Constants.PrefShowTranslation, App.DetailsViewModel.ShowTranslation);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            NavigationContext.QueryString["page"] =
                SettingsUtils.Get<int>(Constants.PrefLastPage).ToString(CultureInfo.InvariantCulture);
            foreach (PageViewModel page in App.DetailsViewModel.Pages)
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

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (App.DetailsViewModel.IsShowMenu && !PhoneUtils.IsPortaitOrientation)
            {
                App.DetailsViewModel.IsShowMenu = false;
                e.Cancel = true;
            }
            else
            {
                base.OnBackKeyPress(e);
            }
        }

        #region AppBar creation

        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();

            var searchButton = new ApplicationBarIconButton(new Uri("/Assets/Images/search.png", UriKind.Relative))
            {
                Text = AppResources.search
            };
            searchButton.Click += Search_Click;
            ApplicationBar.Buttons.Add(searchButton);

            var bookmarkButton = new ApplicationBarIconButton(new Uri("/Assets/Images/favorite.png", UriKind.Relative))
            {
                Text = AppResources.bookmark
            };
            bookmarkButton.Click += Bookmark_Click;
            ApplicationBar.Buttons.Add(bookmarkButton);

            var translationButton =
                new ApplicationBarIconButton(new Uri("/Assets/Images/appbar.globe.png", UriKind.Relative))
                {
                    Text = AppResources.translation
                };
            translationButton.Click += Translation_Click;
            ApplicationBar.Buttons.Add(translationButton);

            var settingsButton = new ApplicationBarMenuItem(AppResources.settings);
            settingsButton.Click += Settings_Click;
            ApplicationBar.MenuItems.Add(settingsButton);

            ApplicationBar.Opacity = 0.7;
            ApplicationBar.BackgroundColor = App.DetailsViewModel.IsNightMode ? Colors.Black : Colors.White;
            ApplicationBar.ForegroundColor = Color.FromArgb(0xFF, 0x49, 0xA4, 0xC5);
            App.DetailsViewModel.IsShowMenu = PhoneUtils.IsPortaitOrientation;

            ApplicationBar.Mode = ApplicationBarMode.Minimized;

            App.DetailsViewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "IsShowMenu")
                {
                    ApplicationBar.IsVisible = App.DetailsViewModel.IsShowMenu;
                }
                if (e.PropertyName == "IsNightMode")
                {
                    ApplicationBar.BackgroundColor = App.DetailsViewModel.IsNightMode ? Colors.Black : Colors.White;
                }
                else if (e.PropertyName == "Orientation")
                {
                    App.DetailsViewModel.IsShowMenu = PhoneUtils.IsPortaitOrientation;
                }
            };
        }

        #endregion

        #region Menu Events

        private void Translation_Click(object sender, EventArgs e)
        {
            int pageNumber = ((DetailsViewModel) DataContext).CurrentPageNumber;
            if (!string.IsNullOrEmpty(App.DetailsViewModel.TranslationFile))
            {
                //App.DetailsViewModel.UpdatePages();
                App.DetailsViewModel.ShowTranslation = !App.DetailsViewModel.ShowTranslation;
                SettingsUtils.Set(Constants.PrefShowTranslation, App.DetailsViewModel.ShowTranslation);
                App.DetailsViewModel.IsShowMenu = false;
            }
            else
            {
                NavigationService.Navigate(new Uri("/TranslationListPage.xaml", UriKind.Relative));
            }
        }

        private void Bookmark_Click(object sender, EventArgs e)
        {
            App.DetailsViewModel.AddPageBookmark();
            App.DetailsViewModel.IsShowMenu = false;
        }

        private void BookmarkAyah_Click(object sender, ContextMenuItemSelectedEventArgs e)
        {
            App.DetailsViewModel.AddAyahBookmark(App.DetailsViewModel.SelectedAyah);
            App.DetailsViewModel.SelectedAyah = null;
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            App.DetailsViewModel.IsShowMenu = false;
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private void Search_Click(object sender, EventArgs e)
        {
            App.DetailsViewModel.IsShowMenu = false;
            NavigationService.Navigate(new Uri("/SearchPage.xaml", UriKind.Relative));
        }

        private void AyahTapped(object sender, QuranAyahEventArgs e)
        {
            App.DetailsViewModel.SelectedAyah = e.QuranAyah;
        }

        #endregion Menu Events
    }
}