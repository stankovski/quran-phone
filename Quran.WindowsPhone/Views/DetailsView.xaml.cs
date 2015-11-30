using Windows.UI.Xaml.Controls;
using Quran.Core;
using Quran.Core.Common;
using Quran.Core.Data;
using Quran.Core.Properties;
using Quran.Core.Utils;
using Quran.Core.ViewModels;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;
using System.Collections.ObjectModel;

namespace Quran.WindowsPhone.Views
{
    public partial class DetailsView : Page
    {
        public DetailsViewModel ViewModel { get; set; }
        public ObservableCollection<NavigationLink> NavigationLinks = new ObservableCollection<NavigationLink>();

        // When page is navigated to set data context to selected item in list
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel = QuranApp.DetailsViewModel;
            await ViewModel.Initialize();
            InitializeComponent();
            BuildLocalizedApplicationBar();

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

            NavigationData parameters = e.Parameter as NavigationData;
            if (parameters == null)
            {
                parameters = new NavigationData();
            }

            //NavigationContext.QueryString.TryGetValue("page", out selectedPage);
            //NavigationContext.QueryString.TryGetValue("surah", out selectedSurah);
            //NavigationContext.QueryString.TryGetValue("ayah", out selectedAyah);

            if (parameters.Page != null)
            {
                ViewModel.CurrentPageNumber = parameters.Page.Value;
                
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

            // set keepinfooverlay according to setting
            ViewModel.KeepInfoOverlay = SettingsUtils.Get<bool>(Constants.PREF_KEEP_INFO_OVERLAY);

            //Select ayah
            if (parameters.Surah != null && parameters.Ayah != null)
            {
                ViewModel.SelectedAyah = new QuranAyah(parameters.Surah.Value, parameters.Ayah.Value);
            }
            else
            {
                ViewModel.SelectedAyah = null;
            }
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
        private void HamburgerButtonClick(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
        }

        private void NavLinkItemClick(object sender, ItemClickEventArgs e)
        {
            MainSplitView.IsPaneOpen = false;
            var item = e.ClickedItem as NavigationLink;
            if (item != null)
            {
                item.Action();
            }
        }

        private void TranslationClick()
        {
            int pageNumber = ((DetailsViewModel)DataContext).CurrentPageNumber;
            if (!string.IsNullOrEmpty(ViewModel.TranslationFile))
            {
                //ViewModel.UpdatePages();
                ViewModel.ShowTranslation = !ViewModel.ShowTranslation;
                SettingsUtils.Set(Constants.PREF_SHOW_TRANSLATION, ViewModel.ShowTranslation);
                //ViewModel.IsShowMenu = false;
            }
            else
            {
                //Frame.Navigate(new Uri("/Views/TranslationListView.xaml", UriKind.Relative));
            }
        }

        private void BookmarkClick()
        {
            ViewModel.AddPageBookmark();
        }
        
        private async void ReciteClick()
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

        private async void ContactUsClick()
        {
            await QuranApp.NativeProvider.ComposeEmail("quran.phone@gmail.com", "Email from QuranPhone");
        }

        private void KeepOrientationClick()
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

        private void ShareClick(string ayah)
        {
            //ShareStatusTask shareTask = new ShareStatusTask();
            //shareTask.Status = ayah;
            //shareTask.Show();
        }

        #endregion Menu Events

        // Build a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            NavigationLinks.Add(new NavigationLink
            {
                Label = AppResources.recite,
                Symbol = Symbol.Volume,
                Action = ReciteClick
            });
            NavigationLinks.Add(new NavigationLink
            {
                Label = AppResources.translation,
                Symbol = Symbol.Globe,
                Action = TranslationClick
            });
            NavigationLinks.Add(new NavigationLink
            {
                Label = AppResources.bookmark,
                Symbol = Symbol.SolidStar,
                Action = BookmarkClick
            });
            NavigationLinks.Add(new NavigationLink
            {
                Label = AppResources.contact_us,
                Symbol = Symbol.MailForward,
                Action = ContactUsClick
            });
            NavigationLinks.Add(new NavigationLink
            {
                Label = AppResources.keep_orientation,
                Symbol = Symbol.Orientation,
                Action = KeepOrientationClick
            });
            NavigationLinks.Add(new NavigationLink
            {
                Label = AppResources.settings,
                Symbol = Symbol.Setting,
                Action = () => { Frame.Navigate(typeof(SettingsView), "general"); }
            });
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            foreach (var page in ViewModel.Pages)
            {
                page.ImageSource = null;
            }
            ViewModel.CurrentPageIndex = -1;
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