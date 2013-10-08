using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using AppBarUtils;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using QuranPhone.Common;
using QuranPhone.Resources;
using QuranPhone.UI;
using QuranPhone.ViewModels;
using QuranPhone.Utils;
using QuranPhone.Data;
using System.Globalization;
using Microsoft.Phone.Controls;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace QuranPhone
{
    public partial class DetailsPage : PhoneApplicationPage
    {
        private RadContextMenu ayahContextMenu = new RadContextMenu();
            
        // Constructor
        public DetailsPage()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();

            App.DetailsViewModel.Orientation = this.Orientation;

            ayahContextMenu.Items.Add(new RadContextMenuItem() { Content = AppResources.bookmark_ayah });
            if (QuranFileUtils.FileExists(Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false),
                                                       QuranFileUtils.QURAN_ARABIC_DATABASE)))
            {
                ayahContextMenu.Items.Add(new RadContextMenuItem() {Content = AppResources.copy});
            }
            ayahContextMenu.ItemTapped += AyahContextMenuClick;
            ayahContextMenu.Closed += (obj, e) => App.DetailsViewModel.SelectedAyah = null;
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
                int page = int.Parse(selectedPage, CultureInfo.InvariantCulture);
                App.DetailsViewModel.CurrentPageNumber = page;
                
                //Update settings
                App.DetailsViewModel.IsNightMode = SettingsUtils.Get<bool>(Constants.PREF_NIGHT_MODE);

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

            //Select ayah
            if (selectedSurah != null && selectedAyah != null)
            {
                int surah = int.Parse(selectedSurah, CultureInfo.InvariantCulture);
                int ayah = int.Parse(selectedAyah, CultureInfo.InvariantCulture);
                App.DetailsViewModel.SelectedAyah = new Common.QuranAyah(surah, ayah);
            }
            else
            {
                App.DetailsViewModel.SelectedAyah = null;
            }
        }

        private void PageFlipped(object sender, SelectionChangedEventArgs e)
        {
            App.DetailsViewModel.SelectedAyah = null;
            App.DetailsViewModel.CurrentPageIndex = App.DetailsViewModel.Pages.IndexOf((PageViewModel)radSlideView.SelectedItem);
        }

        private void ScreenTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.DetailsViewModel.IsShowMenu = false;
        }

        private void MenuTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.DetailsViewModel.IsShowMenu = true;
            e.Handled = true;
        }

        private void ImageTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.DetailsViewModel.SelectedAyah = null;
        }

        private void ImageHold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (App.DetailsViewModel.AyahDetailsExist && sender != null)
            {
                var cachedImage = sender as CachedImage;
                if (cachedImage == null)
                    return;

                var ayah = CachedImage.GetAyahFromGesture(e.GetPosition(cachedImage.Image),
                                                          App.DetailsViewModel.CurrentPageNumber,
                                                          radSlideView.ActualWidth);
                App.DetailsViewModel.SelectedAyah = ayah;

                ayahContextMenu.RegionOfInterest = new Rect(e.GetPosition(ThisPage), new Size(50, 50));
                ayahContextMenu.IsOpen = true;
            }
        }

        private void ImageDoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (App.DetailsViewModel.AyahDetailsExist && sender != null && 
                !string.IsNullOrEmpty(App.DetailsViewModel.TranslationFile))
            {
                var cachedImage = sender as CachedImage;
                if (cachedImage == null)
                    return;

                var ayah = CachedImage.GetAyahFromGesture(e.GetPosition(cachedImage.Image),
                                                          App.DetailsViewModel.CurrentPageNumber,
                                                          radSlideView.ActualWidth);
                var currentPage = App.DetailsViewModel.CurrentPage;
                if (currentPage != null)
                {
                    App.DetailsViewModel.SelectedAyah = ayah;
                    App.DetailsViewModel.ShowTranslation = !App.DetailsViewModel.ShowTranslation;
                    SettingsUtils.Set(Constants.PREF_SHOW_TRANSLATION, App.DetailsViewModel.ShowTranslation);
                }
            }
        }

        private void ListBoxDoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (App.DetailsViewModel.AyahDetailsExist &&
                sender != null && sender is RadDataBoundListBox)
            {
                var selectedVerse = ((RadDataBoundListBox)sender).SelectedItem as VerseViewModel;
                if (selectedVerse != null)
                {
                    App.DetailsViewModel.SelectedAyah = new QuranAyah(selectedVerse.Surah, selectedVerse.Ayah);
                }
                App.DetailsViewModel.ShowTranslation = !App.DetailsViewModel.ShowTranslation;
                SettingsUtils.Set(Constants.PREF_SHOW_TRANSLATION, App.DetailsViewModel.ShowTranslation);
            }
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

        private async void AyahContextMenuClick(object sender, ContextMenuItemSelectedEventArgs e)
        {
            var menuItem = e.SelectedItem as string;
            if (menuItem == null)
                return;

            if (sender is RadContextMenuItem)
            {
                var menu = sender as RadContextMenuItem;
                var data = menu.DataContext as VerseViewModel;
                if (data != null)
                {
                    App.DetailsViewModel.SelectedAyah = new QuranAyah(data.Surah, data.Ayah) { Translation = data.Text };
                }
            }

            if (menuItem == AppResources.bookmark_ayah)
            {
                App.DetailsViewModel.AddAyahBookmark(App.DetailsViewModel.SelectedAyah);
                App.DetailsViewModel.SelectedAyah = null;
            } 
            else if (menuItem == AppResources.copy)
            {
                App.DetailsViewModel.CopyAyahToClipboard(App.DetailsViewModel.SelectedAyah);
                App.DetailsViewModel.SelectedAyah = null;
            }
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

        private void ContactUs_Click(object sender, EventArgs e)
        {
            var email = new EmailComposeTask();
            email.To = "quran.phone@gmail.com";
            email.Subject = "Email from QuranPhone";
            email.Show();
        }

        private void KeepOrientation_Click(object sender, EventArgs e)
        {
            var button = sender as ApplicationBarMenuItem;
            if (button == null)
                return;

            if (this.SupportedOrientations == SupportedPageOrientation.PortraitOrLandscape)
            {
                button.Text = AppResources.auto_orientation;
                if (PhoneUtils.IsPortaitOrientation)
                    this.SupportedOrientations = SupportedPageOrientation.Portrait;
                else
                    this.SupportedOrientations = SupportedPageOrientation.Landscape;
            }
            else
            {
                button.Text = AppResources.keep_orientation;
                this.SupportedOrientations = SupportedPageOrientation.PortraitOrLandscape;
            }
        }

        // TO BE USED IN THE FUTURE
        private void AyahTapped(object sender, Common.QuranAyahEventArgs e)
        {
            App.DetailsViewModel.SelectedAyah = e.QuranAyah;
        }

        #endregion Menu Events

        #region AppBar creation
        // Build a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            var searchButton = new ApplicationBarIconButton(new Uri("/Assets/Images/search.png", UriKind.Relative)) { Text = AppResources.search };
            searchButton.Click += Search_Click;
            ApplicationBar.Buttons.Add(searchButton);
            var bookmarkButton = new ApplicationBarIconButton(new Uri("/Assets/Images/favorite.png", UriKind.Relative)) { Text = AppResources.bookmark };
            bookmarkButton.Click += Bookmark_Click;
            ApplicationBar.Buttons.Add(bookmarkButton);
            var translationButton = new ApplicationBarIconButton(new Uri("/Assets/Images/appbar.globe.png", UriKind.Relative)) { Text = AppResources.translation };
            translationButton.Click += Translation_Click;
            ApplicationBar.Buttons.Add(translationButton);

            // Create a new menu item with the localized string from AppResources.
            var settingsButton = new ApplicationBarMenuItem(AppResources.settings);
            settingsButton.Click += Settings_Click;
            ApplicationBar.MenuItems.Add(settingsButton);
            var contactButton = new ApplicationBarMenuItem(AppResources.contact_us);
            contactButton.Click += ContactUs_Click;
            ApplicationBar.MenuItems.Add(contactButton);
            var orientationButton = new ApplicationBarMenuItem(AppResources.keep_orientation);
            orientationButton.Click += KeepOrientation_Click;
            ApplicationBar.MenuItems.Add(orientationButton);

            // Set style
            ApplicationBar.Opacity = 0.9;
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
#if DEBUG
        ~DetailsPage()
        {
            Console.WriteLine("Destroying DetailsPage");
        }
#endif
    }
}