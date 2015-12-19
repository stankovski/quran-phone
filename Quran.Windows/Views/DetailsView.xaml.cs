using Windows.UI.Xaml.Controls;
using Quran.Core;
using Quran.Core.Common;
using Quran.Core.Data;
using Quran.Core.Utils;
using Quran.Core.ViewModels;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Input;
using Quran.Windows.UI;
using Windows.UI.Xaml.Media.Animation;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using System.Threading.Tasks;
using Windows.UI.Input;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using System;
using Windows.ApplicationModel.DataTransfer;

namespace Quran.Windows.Views
{
    public partial class DetailsView : Page
    {
        public DetailsViewModel ViewModel { get; set; }
        public ObservableCollection<NavigationLink> NavigationLinks = new ObservableCollection<NavigationLink>();
        private DataTransferManager _dataTransferManager;

        public DetailsView()
        {
            ViewModel = QuranApp.DetailsViewModel;
            InitializeComponent();
        }

        // When page is navigated to set data context to selected item in list
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.Initialize();
            BuildLocalizedMenu();
            BuildContextMenu();

            NavigationData parameters = e.Parameter as NavigationData;
            if (parameters == null)
            {
                parameters = new NavigationData();
            }

            ViewModel.CurrentPageNumber = SettingsUtils.Get<int>(Constants.PREF_LAST_PAGE);

            //Monitor property changes
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

            // set KeepInfoOverlay according to setting
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

            // Listen to share requests
            _dataTransferManager = DataTransferManager.GetForCurrentView();
            _dataTransferManager.DataRequested += DataShareRequested;
        }

        private void ImageTap(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedAyah = null;
        }

        private async void ImageDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (sender != null && !string.IsNullOrEmpty(ViewModel.TranslationFile))
            {
                if (!await FileUtils.HaveAyaPositionFile())
                {
                    await ViewModel.DownloadAyahPositionFile();
                }

                var cachedImage = sender as CachedImage;
                if (cachedImage == null)
                    return;

                var ayah = await CachedImage.GetAyahFromGesture(e.GetPosition(cachedImage.Image),
                                                          ViewModel.CurrentPageNumber,
                                                          radSlideView.ActualWidth);
                var currentPage = ViewModel.CurrentPage;
                if (currentPage != null)
                {
                    ViewModel.SelectedAyah = ayah;
                    ViewModel.ShowTranslation = !ViewModel.ShowTranslation;
                    SettingsUtils.Set(Constants.PREF_SHOW_TRANSLATION, ViewModel.ShowTranslation);
                }
            }
        }

        private async void ListViewDoubleTap(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource != null && e.OriginalSource is FrameworkElement)
            {
                if (!await FileUtils.HaveAyaPositionFile())
                {
                    await ViewModel.DownloadAyahPositionFile();
                }

                var selectedVerse = ((FrameworkElement)e.OriginalSource).DataContext as VerseViewModel;
                if (selectedVerse != null)
                {
                    ViewModel.SelectedAyah = new QuranAyah(selectedVerse.Surah, selectedVerse.Ayah);
                }
                ViewModel.ShowTranslation = !ViewModel.ShowTranslation;
                SettingsUtils.Set(Constants.PREF_SHOW_TRANSLATION, ViewModel.ShowTranslation);
            }
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

        private void ImageTapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.SelectedAyah = null;
        }


        private void AyahContextMenuClick(object sender, RoutedEventArgs e)
        {
            var menuFlyoutItem = sender as MenuFlyoutItem;
            if (menuFlyoutItem == null)
            {
                return;
            }

            QuranAyah selectedAyah = menuFlyoutItem.DataContext as QuranAyah;
            if (selectedAyah == null)
            {
                return;
            }

            var menuItem = menuFlyoutItem.Text;

            if (menuItem == Core.Properties.Resources.bookmark_ayah)
            {
                ViewModel.AddAyahBookmark(selectedAyah);
            }
            else if (menuItem == Core.Properties.Resources.copy)
            {
                ViewModel.CopyAyahToClipboard(selectedAyah);
            }
            else if (menuItem == Core.Properties.Resources.share_ayah)
            {
                _ayahToShare = selectedAyah;
                DataTransferManager.ShowShareUI();
            }

            ViewModel.SelectedAyah = null;

        }

        private QuranAyah _ayahToShare;
        private async void DataShareRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            if (_ayahToShare != null)
            {
                string ayah = await ViewModel.GetAyahString(_ayahToShare);
                args.Request.Data.Properties.Title = Core.Properties.Resources.share_ayah;
                args.Request.Data.SetText(ayah);
                _ayahToShare = null;
            }
        }

        #endregion Menu Events

        private void BuildContextMenu()
        {
            var ayahContextMenu = this.Resources["AyahContextMenu"] as MenuFlyout;
            ayahContextMenu.Items.Add(new MenuFlyoutItem() { Text = Core.Properties.Resources.bookmark_ayah });
            ayahContextMenu.Items.Add(new MenuFlyoutItem() { Text = Core.Properties.Resources.copy });
            //ayahContextMenu.Items.Add(new MenuFlyoutItem() { Text = Core.Properties.Resources.recite_ayah });
            ayahContextMenu.Items.Add(new MenuFlyoutItem() { Text = Core.Properties.Resources.share_ayah });
            foreach (MenuFlyoutItem item in ayahContextMenu.Items)
            {
                item.Click += AyahContextMenuClick;
            }
            ayahContextMenu.Closed += (obj, ev) =>
            {
                _isShowingContextMenu = false;
                ViewModel.SelectedAyah = null;
            };
        }

        #region Context menu events
        private bool _isShowingContextMenu = false;
        private async void ImageHolding(object sender, HoldingRoutedEventArgs e)
        {
            if (!_isShowingContextMenu)
            {
                _isShowingContextMenu = true;
                await ImageHoldingOrRightTapped(sender, (ui) => { return e.GetPosition(ui); });
            }
        }

        private async void ImageRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (!_isShowingContextMenu)
            {
                _isShowingContextMenu = true;
                await ImageHoldingOrRightTapped(sender, (ui) => { return e.GetPosition(ui); });
            }
        }

        private async Task ImageHoldingOrRightTapped(object sender, Func<UIElement, Point> getPosition)
        {
            if (sender != null)
            {
                if (!await FileUtils.HaveAyaPositionFile())
                {
                    await ViewModel.DownloadAyahPositionFile();
                }

                var cachedImage = sender as CachedImage;
                if (cachedImage == null)
                    return;

                QuranAyah ayah = await CachedImage.GetAyahFromGesture(getPosition(cachedImage.Image),
                                                          ViewModel.CurrentPageNumber,
                                                          radSlideView.ActualWidth);
                ShowContextMenu(ayah, null, getPosition(ThisPage));
            }
        }

        private void TranslationItemHolding(object sender, HoldingRoutedEventArgs e)
        {
            if (!_isShowingContextMenu)
            {
                _isShowingContextMenu = true;
                TranslationItemHoldingOrRightTapped(sender, e.GetPosition(null), e.OriginalSource);
            }
        }

        private void TranslationItemRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (!_isShowingContextMenu)
            {
                _isShowingContextMenu = true;
                TranslationItemHoldingOrRightTapped(sender, e.GetPosition(null), e.OriginalSource);
            }
        }

        private void TranslationItemHoldingOrRightTapped(object sender, Point pointerPosition, object originalSource)
        {
            if (sender != null)
            {
                var verseViewModel = (originalSource as FrameworkElement).DataContext as VerseViewModel;
                if (verseViewModel != null)
                {
                    ShowContextMenu(new QuranAyah(verseViewModel.Surah, verseViewModel.Ayah) { Translation = verseViewModel.Text }, null, pointerPosition);
                }
            }
        }

        private void ShowContextMenu(QuranAyah data, UIElement target, Point offset)
        {
            ViewModel.SelectedAyah = data;
            var ayahContextMenu = this.Resources["AyahContextMenu"] as MenuFlyout;
            foreach (MenuFlyoutItem item in ayahContextMenu.Items)
            {
                item.DataContext = data;
            }
            ayahContextMenu.ShowAt(target, offset);
        }
        #endregion


        // Build a localized Menu
        private void BuildLocalizedMenu()
        {
            NavigationLinks.Add(new NavigationLink
            {
                Label = Quran.Core.Properties.Resources.home,
                Symbol = Symbol.Home,
                Action = () => { Frame.Navigate(typeof(MainView)); }
            });
            NavigationLinks.Add(new NavigationLink
            {
                Label = Quran.Core.Properties.Resources.translation,
                Symbol = Symbol.Globe,
                Action = TranslationClick
            });
            NavigationLinks.Add(new NavigationLink
            {
                Label = Quran.Core.Properties.Resources.bookmark,
                Symbol = Symbol.SolidStar,
                Action = () => { ViewModel.AddPageBookmark(); }
            });
            NavigationLinks.Add(new NavigationLink
            {
                Label = Quran.Core.Properties.Resources.contact_us,
                Symbol = Symbol.MailForward,
                Action = () => { QuranApp.NativeProvider.ComposeEmail("quran.phone@gmail.com", "Email from QuranPhone"); }
            });
            var keepOrientationLink = new NavigationLink
            {
                Label = Quran.Core.Properties.Resources.keep_orientation,
                Symbol = Symbol.Orientation,
            };
            keepOrientationLink.Action = () => { KeepOrientationClick(keepOrientationLink); };
            NavigationLinks.Add(keepOrientationLink);
        }

        private void TranslationClick()
        {
            int pageNumber = ViewModel.CurrentPageNumber;
            if (!string.IsNullOrEmpty(ViewModel.TranslationFile))
            {
                ViewModel.ShowTranslation = !ViewModel.ShowTranslation;
                SettingsUtils.Set(Constants.PREF_SHOW_TRANSLATION, ViewModel.ShowTranslation);
            }
            else
            {
                Frame.Navigate(typeof(TranslationListView), null, new DrillInNavigationTransitionInfo());
            }
        }

        private void GoToSettings(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsView), "general");
        }

        private void KeepOrientationClick(NavigationLink link)
        {
            if (DisplayInformation.AutoRotationPreferences == DisplayOrientations.None)
            {
                link.Label = Quran.Core.Properties.Resources.auto_orientation;
                DisplayInformation.AutoRotationPreferences = DisplayInformation.GetForCurrentView().CurrentOrientation;                
            }
            else
            {
                link.Label = Quran.Core.Properties.Resources.keep_orientation;
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
            }
        }

        private void WindowsSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewModel.Orientation = DisplayInformation.GetForCurrentView().CurrentOrientation;
        }

        #region Audio controls

        private async void AudioSetRepeat(object sender, RoutedEventArgs e)
        {
            // TODO: Implement
        }

        private async void AudioSkipBackward(object sender, RoutedEventArgs e)
        {
            await ViewModel.PreviousTrack();
        }

        private async void AudioPlay(object sender, RoutedEventArgs e)
        {
            await ViewModel.Play();
        }

        private async void AudioSkipForward(object sender, RoutedEventArgs e)
        {
            await ViewModel.NextTrack();
        }
        #endregion
    }
}