using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Phone.BackgroundTransfer;
using QuranPhone.Common;
using QuranPhone.Data;
using QuranPhone.Resources;
using QuranPhone.Utils;
using Telerik.Windows.Data;

namespace QuranPhone.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            Surahs = new ObservableCollection<ItemViewModel>();
            Juz = new ObservableCollection<ItemViewModel>();
            Bookmarks = new ObservableCollection<ItemViewModel>();
            BookmarksGroup = new ObservableCollection<GenericGroupDescriptor<ItemViewModel, string>>();
            BookmarksGroupSort = new ObservableCollection<GenericSortDescriptor<ItemViewModel, string>>();

            var group = new GenericGroupDescriptor<ItemViewModel, string>();
            group.KeySelector = item => item.Group;
            group.SortMode = ListSortMode.None;
            BookmarksGroup.Add(group);

            InstallationStep = AppResources.loading_message;
            Tags = new ObservableCollection<ItemViewModel>();
            HasAskedToDownload = false;

            QuranData = new DownloadableViewModelBase();
            QuranData.DownloadComplete += DownloadComplete;
            QuranData.PropertyChanged += QuranDataPropertyChanged;
            QuranData.ServerUrl = QuranFileUtils.GetZipFileUrl();
            QuranData.FileName = Path.GetFileName(QuranData.ServerUrl);
            QuranData.LocalUrl = QuranData.FileName;
        }

        #region Properties

        private string _installationStep;
        private bool _isInstalling;

        public ObservableCollection<ItemViewModel> Surahs { get; private set; }
        public ObservableCollection<ItemViewModel> Juz { get; private set; }
        public ObservableCollection<ItemViewModel> Bookmarks { get; private set; }
        public ObservableCollection<GenericGroupDescriptor<ItemViewModel, string>> BookmarksGroup { get; private set; }
        public ObservableCollection<GenericSortDescriptor<ItemViewModel, string>> BookmarksGroupSort { get; private set; }
        public ObservableCollection<ItemViewModel> Tags { get; private set; }
        public bool IsDataLoaded { get; set; }
        public bool HasAskedToDownload { get; set; }
        public DownloadableViewModelBase QuranData { get; private set; }

        public bool IsInstalling
        {
            get { return _isInstalling; }
            set
            {
                _isInstalling = value;
                base.OnPropertyChanged(() => IsInstalling);
            }
        }

        public string InstallationStep
        {
            get { return _installationStep; }
            set
            {
                _installationStep = value;
                base.OnPropertyChanged(() => InstallationStep);
            }
        }

        #endregion Properties

        #region Public methods

        public void LoadData()
        {
            LoadSuraList();
            LoadJuzList();
            LoadBookmarkList();
            IsDataLoaded = true;
        }

        public void RefreshData()
        {
            Bookmarks.Clear();
            LoadBookmarkList();
        }

        public async void Download()
        {
            if (!QuranData.IsDownloading)
            {
                if (QuranData.IsInTempStorage)
                {
                    QuranData.FinishPreviousDownload();
                    await ExtractZipAndFinalize();
                }
                else if (QuranData.IsInLocalStorage)
                {
                    await ExtractZipAndFinalize();
                }

                if (!QuranFileUtils.HaveAllImages() && !HasAskedToDownload)
                {
                    HasAskedToDownload = true;
                    MessageBoxResult askingToDownloadResult = MessageBox.Show(AppResources.downloadPrompt,
                        AppResources.downloadPrompt_title, MessageBoxButton.OKCancel);

                    if (askingToDownloadResult == MessageBoxResult.OK && QuranData.DownloadCommand.CanExecute(null))
                    {
                        QuranData.DownloadCommand.Execute(null);
                    }
                }
            }
        }

        public async Task<bool> ExtractZipAndFinalize()
        {
            IsInstalling = true;

            if (QuranFileUtils.FileExists(QuranData.LocalUrl))
            {
                InstallationStep = AppResources.extracting_message;
                QuranData.IsIndeterminate = true;

                bool result =
                    await
                        new TaskFactory().StartNew(
                            () => QuranFileUtils.ExtractZipFile(QuranData.LocalUrl, QuranFileUtils.QuranBase));

                if (!result)
                {
                    return false;
                }

                QuranFileUtils.DeleteFile(QuranData.LocalUrl);
            }

            IsInstalling = false;

            if (QuranFileUtils.HaveAllImages())
            {
                return true;
            }
            InstallationStep = AppResources.loading_message;
            QuranData.Progress = 0;
            return false;
        }

        public void DeleteBookmark(ItemViewModel item)
        {
            int id;
            if (item != null && item.Id != null &&
                int.TryParse(item.Id, NumberStyles.None, CultureInfo.InvariantCulture, out id))
            {
                Bookmarks.Remove(item);
                using (var bookmarksAdapter = new BookmarksDBAdapter())
                {
                    bookmarksAdapter.RemoveBookmark(id);
                }
            }
        }

        #endregion Public methods

        #region Private methods

        private const int MaxBookmarkTitle = 40;

        private async void DownloadComplete(object sender, EventArgs e)
        {
            await ExtractZipAndFinalize();
        }

        private void QuranDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsDownloading")
            {
                IsInstalling = QuranData.IsDownloading;
            }
            if (e.PropertyName == "DownloadStatus")
            {
                switch (QuranData.DownloadStatus)
                {
                    case TransferStatus.Paused:
                    case TransferStatus.Waiting:
                    case TransferStatus.WaitingForNonVoiceBlockingNetwork:
                        InstallationStep = AppResources.waiting;
                        break;
                    case TransferStatus.WaitingForExternalPower:
                    case TransferStatus.WaitingForExternalPowerDueToBatterySaverMode:
                        InstallationStep = AppResources.waiting_for_power;
                        break;
                    case TransferStatus.WaitingForWiFi:
                        InstallationStep = AppResources.waiting_for_wifi;
                        break;
                    case TransferStatus.Completed:
                    case TransferStatus.Transferring:
                        InstallationStep = AppResources.loading_message;
                        break;
                }
            }
        }

        private void LoadSuraList()
        {
            int sura = 1;

            for (int juz = 1; juz <= Constants.JuzConut; juz++)
            {
                Surahs.Add(new ItemViewModel
                {
                    Id = AppResources.quran_juz2 + " " + juz,
                    PageNumber = QuranInfo.JuzNumberToPage[juz - 1],
                    ItemType = ItemViewModelType.Header
                });
                int next = (juz == Constants.JuzConut) ? Constants.LastPage + 1 : QuranInfo.JuzNumberToPage[juz];

                while ((sura <= Constants.SuraCount) && (QuranInfo.SuraNumberToPage[sura - 1] < next))
                {
                    string title = QuranInfo.GetSuraName(sura, true);
                    Surahs.Add(new ItemViewModel
                    {
                        Id = sura.ToString(CultureInfo.InvariantCulture),
                        Title = title,
                        Details = QuranInfo.GetSuraListMetaString(sura),
                        PageNumber = QuranInfo.SuraNumberToPage[sura - 1],
                        ItemType = ItemViewModelType.Sura
                    });
                    sura++;
                }
            }
        }

        private void LoadJuzList()
        {
            Uri[] images =
            {
                new Uri("/Assets/Images/hizb_full.png", UriKind.Relative),
                new Uri("/Assets/Images/hizb_quarter.png", UriKind.Relative),
                new Uri("/Assets/Images/hizb_half.png", UriKind.Relative),
                new Uri("/Assets/Images/hizb_threequarters.png", UriKind.Relative)
            };
            string[] quarters = QuranInfo.GetSuraQuarters();
            for (int i = 0; i < (8 * Constants.JuzConut); i++)
            {
                int[] pos = QuranInfo.QUARTERS[i];
                int page = QuranInfo.GetPageFromSuraAyah(pos[0], pos[1]);

                if (i % 8 == 0)
                {
                    int juz = 1 + (i / 8);
                    Juz.Add(new ItemViewModel
                    {
                        Id = AppResources.quran_juz2 + " " + juz,
                        PageNumber = QuranInfo.JuzNumberToPage[juz - 1],
                        ItemType = ItemViewModelType.Header
                    });
                }
                string verseString = AppResources.quran_ayah + " " + pos[1];
                Juz.Add(new ItemViewModel
                {
                    Title = quarters[i],
                    Details = QuranInfo.GetSuraName(pos[0], true) + ", " + verseString,
                    PageNumber = page,
                    Image = images[i % 4],
                    ItemType = ItemViewModelType.Sura
                });

                if (i % 4 == 0)
                {
                    Juz[Juz.Count - 1].Id = (1 + (i / 4)).ToString("0", CultureInfo.InvariantCulture);
                }
            }
        }

        private async void LoadBookmarkList()
        {
            var lastPage = SettingsUtils.Get<int>(Constants.PrefLastPage);
            if (lastPage > 0)
            {
                var lastPageItem = new ItemViewModel
                {
                    Title = QuranInfo.GetSuraNameFromPage(lastPage, true),
                    Details =
                        string.Format("{0} {1}, {2} {3}", AppResources.quran_page, lastPage, AppResources.quran_juz2,
                            QuranInfo.GetJuzFromPage(lastPage)),
                    PageNumber = lastPage,
                    Image = new Uri("/Assets/Images/favorite.png", UriKind.Relative),
                    ItemType = ItemViewModelType.Bookmark,
                    Group = AppResources.bookmarks_current_page
                };
                Bookmarks.Add(lastPageItem);
            }

            using (var bookmarksAdapter = new BookmarksDBAdapter())
            {
                try
                {
                    List<Bookmarks> bookmarks = bookmarksAdapter.GetBookmarks(true, BoomarkSortOrder.Location);
                    if (bookmarks.Count > 0)
                    {
                        //Load untagged first
                        foreach (Bookmarks bookmark in bookmarks)
                        {
                            if (bookmark.Tags == null)
                            {
                                Bookmarks.Add(await CreateBookmarkModel(bookmark));
                            }
                        }

                        //Load tagged
                        foreach (Bookmarks bookmark in bookmarks)
                        {
                            if (bookmark.Tags != null)
                            {
                                Bookmarks.Add(await CreateBookmarkModel(bookmark));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("failed to load bookmarks: " + ex.Message);
                }
            }
        }

        private static async Task<ItemViewModel> CreateBookmarkModel(Bookmarks bookmark)
        {
            string group = AppResources.bookmarks;
            if (bookmark.Tags != null && bookmark.Tags.Count > 0)
            {
                group = bookmark.Tags[0].Name;
            }

            if (bookmark.Ayah != null && bookmark.Sura != null)
            {
                string title = QuranInfo.GetSuraNameFromPage(bookmark.Page, true);
                string details;

                if (
                    QuranFileUtils.FileExists(Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false),
                        QuranFileUtils.QuranArabicDatabase)))
                {
                    try
                    {
                        using (var dbArabic = new DatabaseHandler<ArabicAyah>(QuranFileUtils.QuranArabicDatabase))
                        {
                            ArabicAyah ayahSurah =
                                await
                                    new TaskFactory().StartNew(
                                        () => dbArabic.GetVerse(bookmark.Sura.Value, bookmark.Ayah.Value));
                            title = ayahSurah.Text;
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Not able to get Arabic text");
                    }

                    details = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}, {3} {4}",
                        QuranInfo.GetSuraName(bookmark.Sura.Value, true), AppResources.verse, bookmark.Ayah.Value,
                        AppResources.quran_juz2, QuranInfo.GetJuzFromPage(bookmark.Page));
                }
                else
                {
                    details = string.Format(CultureInfo.InvariantCulture, "{0} {1}, {2} {3}, {4} {5}",
                        AppResources.quran_page, bookmark.Page, AppResources.verse, bookmark.Ayah.Value,
                        AppResources.quran_juz2, QuranInfo.GetJuzFromPage(bookmark.Page));
                }

                if (title.Length > MaxBookmarkTitle)
                {
                    for (int i = MaxBookmarkTitle; i > 1; i--)
                    {
                        if (title[i] == ' ')
                        {
                            title = title.Substring(0, i - 1);
                            break;
                        }
                    }
                }

                return new ItemViewModel
                {
                    Id = bookmark.Id.ToString(CultureInfo.InvariantCulture),
                    Title = title,
                    Details = details,
                    PageNumber = bookmark.Page,
                    SelectedAyah = new QuranAyah(bookmark.Sura.Value, bookmark.Ayah.Value),
                    Image = new Uri("/Assets/Images/favorite.png", UriKind.Relative),
                    ItemType = ItemViewModelType.Bookmark,
                    Group = group
                };
            }
            return new ItemViewModel
            {
                Id = bookmark.Id.ToString(CultureInfo.InvariantCulture),
                Title = QuranInfo.GetSuraNameFromPage(bookmark.Page, true),
                Details =
                    string.Format(CultureInfo.InvariantCulture, "{0} {1}, {2} {3}", AppResources.quran_page,
                        bookmark.Page, AppResources.quran_juz2, QuranInfo.GetJuzFromPage(bookmark.Page)),
                PageNumber = bookmark.Page,
                Image = new Uri("/Assets/Images/favorite.png", UriKind.Relative),
                ItemType = ItemViewModelType.Bookmark,
                Group = @group
            };
        }

        #endregion
    }
}