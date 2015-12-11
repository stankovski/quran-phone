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

namespace Quran.Windows.Views
{
    public partial class DetailsView : Page
    {
        public DetailsViewModel ViewModel { get; set; }
        public ObservableCollection<NavigationLink> NavigationLinks = new ObservableCollection<NavigationLink>();

        public DetailsView()
        {
            ViewModel = QuranApp.DetailsViewModel;
            InitializeComponent();
        }

        // When page is navigated to set data context to selected item in list
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.Initialize();
            BuildLocalizedApplicationBar();

            //ViewModel.Orientation = QuranApp.NativeProvider.IsPortaitOrientation ? 
            //    ScreenOrientation.Portrait : 
            //    ScreenOrientation.Landscape;

            //DataContext = ViewModel;

            //ayahContextMenu.Items.Add(new RadContextMenuItem() { Content = Resources.bookmark_ayah });
            //if (FileUtils.HaveArabicSearchFile())
            //{
            //    ayahContextMenu.Items.Add(new RadContextMenuItem() {Content = Resources.copy});
            //}
            //ayahContextMenu.Items.Add(new RadContextMenuItem() { Content = Resources.recite_ayah });
            //ayahContextMenu.Items.Add(new RadContextMenuItem() { Content = Resources.share_ayah });
            //ayahContextMenu.ItemTapped += AyahContextMenuClick;
            //ayahContextMenu.Closed += (obj, e) => ViewModel.SelectedAyah = null;

            NavigationData parameters = e.Parameter as NavigationData;
            if (parameters == null)
            {
                parameters = new NavigationData();
            }
            
            ViewModel.CurrentPageNumber = SettingsUtils.Get<int>(Constants.PREF_LAST_PAGE);
                
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

            //if (menuItem == Resources.bookmark_ayah)
            //{
            //    ViewModel.AddAyahBookmark(ViewModel.SelectedAyah);
            //    ViewModel.SelectedAyah = null;                
            //} 
            //else if (menuItem == Resources.copy)
            //{
            //    ViewModel.CopyAyahToClipboard(ViewModel.SelectedAyah);
            //    ViewModel.SelectedAyah = null;
            //}

            //else if (menuItem == Resources.share_ayah)
            //{
            //    string ayah = await ViewModel.GetAyahString(ViewModel.SelectedAyah);
            //    ShareAyah(ayah);
            //}
            //else if (menuItem == Resources.recite_ayah)
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
                Label = Quran.Core.Properties.Resources.home,
                Symbol = Symbol.Home,
                Action = () => { Frame.Navigate(typeof(MainView)); }
            });
            //NavigationLinks.Add(new NavigationLink
            //{
            //    Label = Quran.Core.Properties.Resources.recite,
            //    Symbol = Symbol.Volume,
            //    Action = ReciteClick
            //});
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
    }
}