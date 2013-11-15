using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Quran.Core;
using Quran.Core.Common;
using Quran.Core.Data;
using Quran.Core.Properties;
using Quran.Core.Utils;
using Quran.Core.ViewModels;
using Quran.WindowsPhone.UI;
using Quran.WindowsPhone.Utils;
using Telerik.Windows.Controls;

namespace Quran.WindowsPhone.Views
{
    public partial class DetailsView
    {
        private RadContextMenu ayahContextMenu = new RadContextMenu();
            
        // Constructor
        public DetailsView()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();

            QuranApp.DetailsViewModel.Orientation = PhoneUtils.PageOrientationConverter(this.Orientation);

            ayahContextMenu.Items.Add(new RadContextMenuItem() { Content = AppResources.bookmark_ayah });
            if (FileUtils.FileExists(Path.Combine(FileUtils.GetQuranDatabaseDirectory(false),
                                                       FileUtils.QURAN_ARABIC_DATABASE)))
            {
                ayahContextMenu.Items.Add(new RadContextMenuItem() {Content = AppResources.copy});
            }
            ayahContextMenu.Items.Add(new RadContextMenuItem() { Content = AppResources.recite_ayah });
            ayahContextMenu.ItemTapped += AyahContextMenuClick;
            ayahContextMenu.Closed += (obj, e) => QuranApp.DetailsViewModel.SelectedAyah = null;
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
                QuranApp.DetailsViewModel.CurrentPageNumber = page;
                
                //Update settings
                QuranApp.DetailsViewModel.IsNightMode = SettingsUtils.Get<bool>(Constants.PREF_NIGHT_MODE);

                //Monitor proprty changes
                QuranApp.DetailsViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == "CurrentPageIndex")
                    {
                        if (QuranApp.DetailsViewModel.CurrentPageIndex != -1)
                            radSlideView.SelectedItem = QuranApp.DetailsViewModel.Pages[QuranApp.DetailsViewModel.CurrentPageIndex];
                    }
                };

                //Try extract translation from query
                var translation = SettingsUtils.Get<string>(Constants.PREF_ACTIVE_TRANSLATION);
                if (!string.IsNullOrEmpty(translation))
                {
                    if (QuranApp.DetailsViewModel.TranslationFile != translation.Split('|')[0] ||
                        QuranApp.DetailsViewModel.ShowTranslation != SettingsUtils.Get<bool>(Constants.PREF_SHOW_TRANSLATION) ||
                        QuranApp.DetailsViewModel.ShowArabicInTranslation != SettingsUtils.Get<bool>(Constants.PREF_SHOW_ARABIC_IN_TRANSLATION))
                    {
                        QuranApp.DetailsViewModel.Pages.Clear();
                    }
                    QuranApp.DetailsViewModel.TranslationFile = translation.Split('|')[0];
                    QuranApp.DetailsViewModel.ShowTranslation = SettingsUtils.Get<bool>(Constants.PREF_SHOW_TRANSLATION);
                    QuranApp.DetailsViewModel.ShowArabicInTranslation = SettingsUtils.Get<bool>(Constants.PREF_SHOW_ARABIC_IN_TRANSLATION);
                }
                else
                {
                    QuranApp.DetailsViewModel.TranslationFile = null;
                    QuranApp.DetailsViewModel.ShowTranslation = false;
                    QuranApp.DetailsViewModel.ShowArabicInTranslation = false;
                }
            }

            QuranApp.DetailsViewModel.LoadData();
            if (DataContext == null)
                DataContext = QuranApp.DetailsViewModel;
            radSlideView.SelectionChanged += PageFlipped;

            // set keepinfooverlay according to setting
            QuranApp.DetailsViewModel.KeepInfoOverlay = SettingsUtils.Get<bool>(Constants.PREF_KEEP_INFO_OVERLAY);

            //Select ayah
            if (selectedSurah != null && selectedAyah != null)
            {
                int surah = int.Parse(selectedSurah, CultureInfo.InvariantCulture);
                int ayah = int.Parse(selectedAyah, CultureInfo.InvariantCulture);
                QuranApp.DetailsViewModel.SelectedAyah = new QuranAyah(surah, ayah);
            }
            else
            {
                QuranApp.DetailsViewModel.SelectedAyah = null;
            }
        }
        
        private void PageFlipped(object sender, SelectionChangedEventArgs e)
        {
            QuranApp.DetailsViewModel.SelectedAyah = null;
            QuranApp.DetailsViewModel.CurrentPageIndex = QuranApp.DetailsViewModel.Pages.IndexOf((PageViewModel)radSlideView.SelectedItem);
        }

        private void ScreenTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            QuranApp.DetailsViewModel.IsShowMenu = false;
        }

        private void MenuTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            QuranApp.DetailsViewModel.IsShowMenu = true;
            e.Handled = true;
        }

        private void ImageTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            QuranApp.DetailsViewModel.SelectedAyah = null;
        }

        private void ImageHold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (QuranApp.DetailsViewModel.AyahDetailsExist && sender != null)
            {
                var cachedImage = sender as CachedImage;
                if (cachedImage == null)
                    return;

                var ayah = CachedImage.GetAyahFromGesture(e.GetPosition(cachedImage.Image),
                                                          QuranApp.DetailsViewModel.CurrentPageNumber,
                                                          radSlideView.ActualWidth);
                QuranApp.DetailsViewModel.SelectedAyah = ayah;

                ayahContextMenu.RegionOfInterest = new Rect(e.GetPosition(ThisPage), new Size(50, 50));
                ayahContextMenu.IsOpen = true;
            }
        }

        private void ImageDoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (QuranApp.DetailsViewModel.AyahDetailsExist && sender != null && 
                !string.IsNullOrEmpty(QuranApp.DetailsViewModel.TranslationFile))
            {
                var cachedImage = sender as CachedImage;
                if (cachedImage == null)
                    return;

                var ayah = CachedImage.GetAyahFromGesture(e.GetPosition(cachedImage.Image),
                                                          QuranApp.DetailsViewModel.CurrentPageNumber,
                                                          radSlideView.ActualWidth);
                var currentPage = QuranApp.DetailsViewModel.CurrentPage;
                if (currentPage != null)
                {
                    QuranApp.DetailsViewModel.SelectedAyah = ayah;
                    QuranApp.DetailsViewModel.ShowTranslation = !QuranApp.DetailsViewModel.ShowTranslation;
                    SettingsUtils.Set(Constants.PREF_SHOW_TRANSLATION, QuranApp.DetailsViewModel.ShowTranslation);
                }
            }
        }

        private void ListBoxDoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (QuranApp.DetailsViewModel.AyahDetailsExist &&
                sender != null && sender is RadDataBoundListBox)
            {
                var selectedVerse = ((RadDataBoundListBox)sender).SelectedItem as VerseViewModel;
                if (selectedVerse != null)
                {
                    QuranApp.DetailsViewModel.SelectedAyah = new QuranAyah(selectedVerse.Surah, selectedVerse.Ayah);
                }
                QuranApp.DetailsViewModel.ShowTranslation = !QuranApp.DetailsViewModel.ShowTranslation;
                SettingsUtils.Set(Constants.PREF_SHOW_TRANSLATION, QuranApp.DetailsViewModel.ShowTranslation);
            }
        }

        #region Menu Events

        private void Translation_Click(object sender, EventArgs e)
        {
            int pageNumber = ((DetailsViewModel)DataContext).CurrentPageNumber;
            if (!string.IsNullOrEmpty(QuranApp.DetailsViewModel.TranslationFile))
            {
                //QuranApp.DetailsViewModel.UpdatePages();
                QuranApp.DetailsViewModel.ShowTranslation = !QuranApp.DetailsViewModel.ShowTranslation;
                SettingsUtils.Set(Constants.PREF_SHOW_TRANSLATION, QuranApp.DetailsViewModel.ShowTranslation);
                QuranApp.DetailsViewModel.IsShowMenu = false;
            }
            else
            {
                NavigationService.Navigate(new Uri("/Views/TranslationListView.xaml", UriKind.Relative));
            }
        }

        private void Bookmark_Click(object sender, EventArgs e)
        {
            QuranApp.DetailsViewModel.AddPageBookmark();
            QuranApp.DetailsViewModel.IsShowMenu = false;
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
                    QuranApp.DetailsViewModel.SelectedAyah = new QuranAyah(data.Surah, data.Ayah) { Translation = data.Text };
                }
            }

            if (menuItem == AppResources.bookmark_ayah)
            {
                QuranApp.DetailsViewModel.AddAyahBookmark(QuranApp.DetailsViewModel.SelectedAyah);
                QuranApp.DetailsViewModel.SelectedAyah = null;                
            } 
            else if (menuItem == AppResources.copy)
            {
                QuranApp.DetailsViewModel.CopyAyahToClipboard(QuranApp.DetailsViewModel.SelectedAyah);
                QuranApp.DetailsViewModel.SelectedAyah = null;
            }
            else if (menuItem == AppResources.recite_ayah)
            {
                Recite_Click(this, null);
            }
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            QuranApp.DetailsViewModel.IsShowMenu = false;
            NavigationService.Navigate(new Uri("/Views/SettingsView.xaml?tab=general", UriKind.Relative));
        }

        private void Recite_Click(object sender, EventArgs e)
        {
            var reciter = SettingsUtils.Get<string>(Constants.PREF_ACTIVE_QARI);
            if (string.IsNullOrEmpty(reciter))
            {
                NavigationService.Navigate(new Uri("/Views/RecitersListView.xaml", UriKind.Relative));
            }
            else
            {
                var selectedAyah = QuranApp.DetailsViewModel.SelectedAyah;
                if (selectedAyah == null)
                {
                    var bounds = QuranInfo.GetPageBounds(QuranApp.DetailsViewModel.CurrentPageNumber);
                    selectedAyah = new QuranAyah
                    {
                        Sura = bounds[0],
                        Ayah = bounds[1]
                    };
                    if (selectedAyah.Ayah == 1 && selectedAyah.Sura != Constants.SURA_TAWBA &&
                        selectedAyah.Sura != Constants.SURA_FIRST)
                    {
                        selectedAyah.Ayah = 0;
                    }
                }
                QuranApp.DetailsViewModel.PlayFromAyah(selectedAyah.Sura, selectedAyah.Ayah);
            }
        }

        private void Search_Click(object sender, EventArgs e)
        {
            QuranApp.DetailsViewModel.IsShowMenu = false;
            NavigationService.Navigate(new Uri("/Views/SearchView.xaml", UriKind.Relative));
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
                if (QuranApp.NativeProvider.IsPortaitOrientation)
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
        private void AyahTapped(object sender, QuranAyahEventArgs e)
        {
            QuranApp.DetailsViewModel.SelectedAyah = e.QuranAyah;
        }

        #endregion Menu Events

        #region AppBar creation
        // Build a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            var reciteButton = new ApplicationBarIconButton(new Uri("/Assets/Images/recite.png", UriKind.Relative)) { Text = AppResources.recite };
            reciteButton.Click += Recite_Click;
            ApplicationBar.Buttons.Add(reciteButton);
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
            ApplicationBar.BackgroundColor = QuranApp.DetailsViewModel.IsNightMode ? Colors.Black : Colors.White;
            ApplicationBar.ForegroundColor = Color.FromArgb(0xFF, 0x49, 0xA4, 0xC5);
            QuranApp.DetailsViewModel.IsShowMenu = QuranApp.NativeProvider.IsPortaitOrientation;

            ApplicationBar.Mode = ApplicationBarMode.Minimized;

            QuranApp.DetailsViewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "IsShowMenu")
                {
                    ApplicationBar.IsVisible = QuranApp.DetailsViewModel.IsShowMenu;
                }
                if (e.PropertyName == "IsNightMode")
                {
                    ApplicationBar.BackgroundColor = QuranApp.DetailsViewModel.IsNightMode ? Colors.Black : Colors.White;
                }
                else if (e.PropertyName == "Orientation")
                {
                    QuranApp.DetailsViewModel.IsShowMenu = QuranApp.NativeProvider.IsPortaitOrientation;
                }
            };
        }

        #endregion

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            NavigationContext.QueryString["page"] = SettingsUtils.Get<int>(Constants.PREF_LAST_PAGE).ToString(CultureInfo.InvariantCulture);
            foreach (var page in QuranApp.DetailsViewModel.Pages)
            {
                page.ImageSource = null;
            }
            QuranApp.DetailsViewModel.CurrentPageIndex = -1;
            radSlideView.SelectionChanged -= PageFlipped;            
        }

        private void PageOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            QuranApp.DetailsViewModel.Orientation = PhoneUtils.PageOrientationConverter(e.Orientation);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            // if back key pressed when menu is visible, hide the menu
            // somehow, I (kemasdimas) frequently expect "back" key to hide menu,
            // instead of going back to previous page.
            if (QuranApp.DetailsViewModel.IsShowMenu && !QuranApp.NativeProvider.IsPortaitOrientation)
            {
                QuranApp.DetailsViewModel.IsShowMenu = false;
                e.Cancel = true;
            }
            else if (QuranApp.DetailsViewModel.AudioPlayerState != AudioState.Stopped)
            {
                QuranApp.DetailsViewModel.AudioPlayerState = AudioState.Stopped;
                QuranApp.NativeProvider.AudioProvider.Stop();
                e.Cancel = true;
            }
            else
            {
                base.OnBackKeyPress(e);
            }
        }
#if DEBUG
        ~DetailsView()
        {
            Console.WriteLine("Destroying DetailsView");
        }
#endif
    }
}