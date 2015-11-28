using System;
using System.Globalization;
using Windows.UI.Xaml.Controls;
using Quran.Core;
using Quran.Core.Common;
using Quran.Core.Data;
using Quran.Core.Properties;
using Quran.Core.Utils;
using Quran.Core.ViewModels;
using Quran.WindowsPhone.UI;
using Quran.WindowsPhone.Utils;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;

namespace Quran.WindowsPhone.Views
{
    public partial class DetailsView : Page
    {
        public DetailsViewModel ViewModel { get; set; }

        // When page is navigated to set data context to selected item in list
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel = QuranApp.DetailsViewModel;
            await ViewModel.Initialize();
            InitializeComponent();
            //BuildLocalizedApplicationBar();

            //ViewModel.Orientation = QuranApp.NativeProvider.IsPortaitOrientation ? 
            //    ScreenOrientation.Portrait : 
            //    ScreenOrientation.Landscape;

            //DataContext = ViewModel;

            //ayahContextMenu.Items.Add(new RadContextMenuItem() { Content = AppResources.bookmark_ayah });
            //if (FileUtils.HaveArabicSearchFile())
            //{
            //    ayahContextMenu.Items.Add(new RadContextMenuItem() {Content = AppResources.copy});
            //}
            //ayahContextMenu.Items.Add(new RadContextMenuItem() { Content = AppResources.recite_ayah });
            //ayahContextMenu.Items.Add(new RadContextMenuItem() { Content = AppResources.share_ayah });
            //ayahContextMenu.ItemTapped += AyahContextMenuClick;
            //ayahContextMenu.Closed += (obj, e) => ViewModel.SelectedAyah = null;

            string selectedPage = "1";
            string selectedSurah;
            string selectedAyah;

            //NavigationContext.QueryString.TryGetValue("page", out selectedPage);
            //NavigationContext.QueryString.TryGetValue("surah", out selectedSurah);
            //NavigationContext.QueryString.TryGetValue("ayah", out selectedAyah);

            if (selectedPage != null)
            {
                int page = int.Parse(selectedPage, CultureInfo.InvariantCulture);
                ViewModel.CurrentPageNumber = page;
                
                //Update settings
                ViewModel.IsNightMode = SettingsUtils.Get<bool>(Constants.PREF_NIGHT_MODE);

                //Monitor proprty changes
                ViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == "CurrentPageIndex")
                    {
                        if (ViewModel.CurrentPageIndex != -1)
                        {
                            radSlideView.SelectedItem = ViewModel.Pages[ViewModel.CurrentPageIndex];
                        }
                    }
                };

                //Try extract translation from query
                var translation = SettingsUtils.Get<string>(Constants.PREF_ACTIVE_TRANSLATION);
                if (!string.IsNullOrEmpty(translation))
                {
                    if (ViewModel.TranslationFile != translation.Split('|')[0] ||
                        ViewModel.ShowTranslation != SettingsUtils.Get<bool>(Constants.PREF_SHOW_TRANSLATION) ||
                        ViewModel.ShowArabicInTranslation != SettingsUtils.Get<bool>(Constants.PREF_SHOW_ARABIC_IN_TRANSLATION))
                    {
                        ViewModel.Pages.Clear();
                    }
                    ViewModel.TranslationFile = translation.Split('|')[0];
                    ViewModel.ShowTranslation = SettingsUtils.Get<bool>(Constants.PREF_SHOW_TRANSLATION);
                    ViewModel.ShowArabicInTranslation = SettingsUtils.Get<bool>(Constants.PREF_SHOW_ARABIC_IN_TRANSLATION);
                }
                else
                {
                    ViewModel.TranslationFile = null;
                    ViewModel.ShowTranslation = false;
                    ViewModel.ShowArabicInTranslation = false;
                }
            }

            ViewModel.LoadData();
            
            // set keepinfooverlay according to setting
            ViewModel.KeepInfoOverlay = SettingsUtils.Get<bool>(Constants.PREF_KEEP_INFO_OVERLAY);

            ////Select ayah
            //if (selectedSurah != null && selectedAyah != null)
            //{
            //    int surah = int.Parse(selectedSurah, CultureInfo.InvariantCulture);
            //    int ayah = int.Parse(selectedAyah, CultureInfo.InvariantCulture);
            //    ViewModel.SelectedAyah = new QuranAyah(surah, ayah);
            //}
            //else
            //{
            //    ViewModel.SelectedAyah = null;
            //}
        }
        
        private void PageFlipped(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.SelectedAyah = null;
            //ViewModel.CurrentPageIndex = ViewModel.Pages.IndexOf((PageViewModel)radSlideView.SelectedItem);
        }

        private void ScreenTap(object sender, RoutedEventArgs e)
        {
            ViewModel.IsShowMenu = false;
        }

        private void MenuTap(object sender, RoutedEventArgs e)
        {
            ViewModel.IsShowMenu = true;
            //e.Handled = true;
        }

        private void ImageTap(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedAyah = null;
        }

        private async void ImageHold(object sender, RoutedEventArgs e)
        {
            //if (sender != null)
            //{
            //    if (!await FileUtils.HaveAyaPositionFile())
            //    {
            //        await ViewModel.DownloadAyahPositionFile();
            //    }

            //    var cachedImage = sender as CachedImage;
            //    if (cachedImage == null)
            //        return;

            //    var ayah = await CachedImage.GetAyahFromGesture(e.GetPosition(cachedImage.Image),
            //                                              ViewModel.CurrentPageNumber,
            //                                              radSlideView.ActualWidth);
            //    ViewModel.SelectedAyah = ayah;

            //    //ayahContextMenu.RegionOfInterest = new Rect(e.GetPosition(ThisPage), new Size(50, 50));
            //    //ayahContextMenu.IsOpen = true;
            //}
        }

        private async void ImageDoubleTap(object sender, RoutedEventArgs e)
        {
            //if (sender != null && !string.IsNullOrEmpty(ViewModel.TranslationFile))
            //{
            //    if (!await FileUtils.HaveAyaPositionFile())
            //    {
            //        await ViewModel.DownloadAyahPositionFile();
            //    }


            //    var cachedImage = sender as CachedImage;
            //    if (cachedImage == null)
            //        return;

            //    var ayah = await CachedImage.GetAyahFromGesture(e.GetPosition(cachedImage.Image),
            //                                              ViewModel.CurrentPageNumber,
            //                                              radSlideView.ActualWidth);
            //    var currentPage = ViewModel.CurrentPage;
            //    if (currentPage != null)
            //    {
            //        ViewModel.SelectedAyah = ayah;
            //        ViewModel.ShowTranslation = !ViewModel.ShowTranslation;
            //        SettingsUtils.Set(Constants.PREF_SHOW_TRANSLATION, ViewModel.ShowTranslation);
            //    }
            //}
        }

        private async void ListBoxDoubleTap(object sender, RoutedEventArgs e)
        {
            //if (sender != null && sender is RadDataBoundListBox)
            //{
            //    if (!await FileUtils.HaveAyaPositionFile())
            //    {
            //        await ViewModel.DownloadAyahPositionFile();
            //    }

            //    var selectedVerse = ((RadDataBoundListBox)sender).SelectedItem as VerseViewModel;
            //    if (selectedVerse != null)
            //    {
            //        ViewModel.SelectedAyah = new QuranAyah(selectedVerse.Surah, selectedVerse.Ayah);
            //    }
            //    ViewModel.ShowTranslation = !ViewModel.ShowTranslation;
            //    SettingsUtils.Set(Constants.PREF_SHOW_TRANSLATION, ViewModel.ShowTranslation);
            //}
        }

        #region Menu Events

        private void Translation_Click(object sender, EventArgs e)
        {
            int pageNumber = ((DetailsViewModel)DataContext).CurrentPageNumber;
            if (!string.IsNullOrEmpty(ViewModel.TranslationFile))
            {
                //ViewModel.UpdatePages();
                ViewModel.ShowTranslation = !ViewModel.ShowTranslation;
                SettingsUtils.Set(Constants.PREF_SHOW_TRANSLATION, ViewModel.ShowTranslation);
                ViewModel.IsShowMenu = false;
            }
            else
            {
                //Frame.Navigate(new Uri("/Views/TranslationListView.xaml", UriKind.Relative));
            }
        }

        private void Bookmark_Click(object sender, EventArgs e)
        {
            ViewModel.AddPageBookmark();
            ViewModel.IsShowMenu = false;
        }

        private async void AyahContextMenuClick(object sender, RoutedEventArgs e)
        {
            //var menuItem = e.SelectedItem as string;
            //if (menuItem == null)
            //    return;

            //if (sender is RadContextMenuItem)
            //{
            //    var menu = sender as RadContextMenuItem;
            //    var data = menu.DataContext as VerseViewModel;
            //    if (data != null)
            //    {
            //        ViewModel.SelectedAyah = new QuranAyah(data.Surah, data.Ayah) { Translation = data.Text };
            //    }
            //}

            //if (menuItem == AppResources.bookmark_ayah)
            //{
            //    ViewModel.AddAyahBookmark(ViewModel.SelectedAyah);
            //    ViewModel.SelectedAyah = null;                
            //} 
            //else if (menuItem == AppResources.copy)
            //{
            //    ViewModel.CopyAyahToClipboard(ViewModel.SelectedAyah);
            //    ViewModel.SelectedAyah = null;
            //}

            //else if (menuItem == AppResources.share_ayah)
            //{
            //    string ayah = await ViewModel.GetAyahString(ViewModel.SelectedAyah);
            //    ShareAyah(ayah);
            //}
            //else if (menuItem == AppResources.recite_ayah)
            //{
            //    Recite_Click(this, null);
            //}
        }

        private void ShareAyah(string ayah)
        {
            //ShareStatusTask shareTask = new ShareStatusTask();
            //shareTask.Status = ayah;
            //shareTask.Show();
        }
        private void Settings_Click(object sender, EventArgs e)
        {
            ViewModel.IsShowMenu = false;
            //Frame.Navigate(new Uri("/Views/SettingsView.xaml?tab=general", UriKind.Relative));
        }

        private async void Recite_Click(object sender, EventArgs e)
        {
            var reciter = SettingsUtils.Get<string>(Constants.PREF_ACTIVE_QARI);
            if (string.IsNullOrEmpty(reciter))
            {
                //Frame.Navigate(new Uri("/Views/RecitersListView.xaml", UriKind.Relative));
            }
            else
            {
                var selectedAyah = ViewModel.SelectedAyah;
                if (selectedAyah == null)
                {
                    var bounds = QuranUtils.GetPageBounds(ViewModel.CurrentPageNumber);
                    selectedAyah = new QuranAyah
                    {
                        Surah = bounds[0],
                        Ayah = bounds[1]
                    };
                    if (selectedAyah.Ayah == 1 && selectedAyah.Surah != Constants.SURA_TAWBA &&
                        selectedAyah.Surah != Constants.SURA_FIRST)
                    {
                        selectedAyah.Ayah = 0;
                    }
                }
                if (QuranUtils.IsValid(selectedAyah))
                {
                    await ViewModel.PlayFromAyah(selectedAyah.Surah, selectedAyah.Ayah);
                }
            }
        }

        private async void Search_Click(object sender, EventArgs e)
        {
            await ViewModel.DownloadArabicSearchFile();
            ViewModel.IsShowMenu = false;
            //Frame.Navigate(new Uri("/Views/SearchView.xaml", UriKind.Relative));
        }

        private async void ContactUs_Click(object sender, EventArgs e)
        {
            await QuranApp.NativeProvider.ComposeEmail("quran.phone@gmail.com", "Email from QuranPhone");
        }

        private void KeepOrientation_Click(object sender, EventArgs e)
        {
            //var button = sender as ApplicationBarMenuItem;
            //if (button == null)
            //    return;

            //if (this.SupportedOrientations == SupportedPageOrientation.PortraitOrLandscape)
            //{
            //    button.Text = AppResources.auto_orientation;
            //    if (QuranApp.NativeProvider.IsPortaitOrientation)
            //        this.SupportedOrientations = SupportedPageOrientation.Portrait;
            //    else
            //        this.SupportedOrientations = SupportedPageOrientation.Landscape;
            //}
            //else
            //{
            //    button.Text = AppResources.keep_orientation;
            //    this.SupportedOrientations = SupportedPageOrientation.PortraitOrLandscape;
            //}
        }

        private void AyahTapped(object sender, QuranAyahEventArgs e)
        {
            ViewModel.SelectedAyah = e.QuranAyah;
        }

        #endregion Menu Events

        #region AppBar creation
        // Build a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            //// Set the page's ApplicationBar to a new instance of ApplicationBar.
            //ApplicationBar = new ApplicationBar();

            //var reciteButton = new ApplicationBarIconButton(new Uri("/Assets/Images/recite.png", UriKind.Relative)) { Text = AppResources.recite };
            //reciteButton.Click += Recite_Click;
            //ApplicationBar.Buttons.Add(reciteButton);
            //var searchButton = new ApplicationBarIconButton(new Uri("/Assets/Images/search.png", UriKind.Relative)) { Text = AppResources.search };
            //searchButton.Click += Search_Click;
            //ApplicationBar.Buttons.Add(searchButton);
            //var bookmarkButton = new ApplicationBarIconButton(new Uri("/Assets/Images/favorite.png", UriKind.Relative)) { Text = AppResources.bookmark };
            //bookmarkButton.Click += Bookmark_Click;
            //ApplicationBar.Buttons.Add(bookmarkButton);
            //var translationButton = new ApplicationBarIconButton(new Uri("/Assets/Images/appbar.globe.png", UriKind.Relative)) { Text = AppResources.translation };
            //translationButton.Click += Translation_Click;
            //ApplicationBar.Buttons.Add(translationButton);

            //// Create a new menu item with the localized string from AppResources.
            //var settingsButton = new ApplicationBarMenuItem(AppResources.settings);
            //settingsButton.Click += Settings_Click;
            //ApplicationBar.MenuItems.Add(settingsButton);
            //var contactButton = new ApplicationBarMenuItem(AppResources.contact_us);
            //contactButton.Click += ContactUs_Click;
            //ApplicationBar.MenuItems.Add(contactButton);
            //var orientationButton = new ApplicationBarMenuItem(AppResources.keep_orientation);
            //orientationButton.Click += KeepOrientation_Click;
            //ApplicationBar.MenuItems.Add(orientationButton);

            //// Set style
            //ApplicationBar.Opacity = 0.9;
            //ApplicationBar.BackgroundColor = ViewModel.IsNightMode ? Colors.Black : Colors.White;
            //ApplicationBar.ForegroundColor = Color.FromArgb(0xFF, 0x49, 0xA4, 0xC5);
            //ViewModel.IsShowMenu = QuranApp.NativeProvider.IsPortaitOrientation;

            //ApplicationBar.Mode = ApplicationBarMode.Minimized;

            //ViewModel.PropertyChanged += (sender, e) =>
            //{
            //    if (e.PropertyName == "IsShowMenu")
            //    {
            //        ApplicationBar.IsVisible = ViewModel.IsShowMenu;
            //    }
            //    if (e.PropertyName == "IsNightMode")
            //    {
            //        ApplicationBar.BackgroundColor = ViewModel.IsNightMode ? Colors.Black : Colors.White;
            //    }
            //    else if (e.PropertyName == "Orientation")
            //    {
            //        ViewModel.IsShowMenu = QuranApp.NativeProvider.IsPortaitOrientation;
            //    }
            //};
        }

        #endregion

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //base.OnNavigatedFrom(e);
            //NavigationContext.QueryString["page"] = SettingsUtils.Get<int>(Constants.PREF_LAST_PAGE).ToString(CultureInfo.InvariantCulture);
            //foreach (var page in ViewModel.Pages)
            //{
            //    page.ImageSource = null;
            //}
            //ViewModel.CurrentPageIndex = -1;
            //radSlideView.SelectionChanged -= PageFlipped;            
        }

        //private void PageOrientationChanged(object sender, OrientationChangedEventArgs e)
        //{
        //    ViewModel.Orientation = PhoneUtils.PageOrientationConverter(e.Orientation);
        //}

        //protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        //{
        //    // if back key pressed when menu is visible, hide the menu
        //    // somehow, I (kemasdimas) frequently expect "back" key to hide menu,
        //    // instead of going back to previous page.
        //    if (ViewModel.IsShowMenu && !QuranApp.NativeProvider.IsPortaitOrientation)
        //    {
        //        ViewModel.IsShowMenu = false;
        //        e.Cancel = true;
        //    }
        //    else if (ViewModel.AudioPlayerState != AudioState.Stopped)
        //    {
        //        ViewModel.AudioPlayerState = AudioState.Stopped;
        //        QuranApp.NativeProvider.AudioProvider.Stop();
        //        e.Cancel = true;
        //    }
        //    else
        //    {
        //        base.OnBackKeyPress(e);
        //    }
        //}
    }
}