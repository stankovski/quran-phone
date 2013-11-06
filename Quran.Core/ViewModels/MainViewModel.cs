// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the MainViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using Quran.Core.Common;
using Quran.Core.Data;
using Quran.Core.Properties;
using Quran.Core.Utils;
using Quran.Core.Common;
using Quran.Core.Data;

namespace Quran.Core.ViewModels
{
    /// <summary>
    /// Define the MainViewModel type.
    /// </summary>
    public class MainViewModel : ViewModelWithDownload
    {
        public MainViewModel()
        {
            this.Surahs = new ObservableCollection<ItemViewModel>();
            this.Juz = new ObservableCollection<ItemViewModel>();
            this.Bookmarks = new ObservableCollection<ItemViewModel>();
            
            this.InstallationStep = AppResources.loading_message;

            this.Tags = new ObservableCollection<ItemViewModel>();
            this.HasAskedToDownload = false;
            this.ActiveDownload.ServerUrl = QuranFileUtils.GetZipFileUrl();
            this.ActiveDownload.LocalUrl = QuranFileUtils.QURAN_BASE;
        }

        #region Properties
        public ObservableCollection<ItemViewModel> Surahs { get; private set; }
        public ObservableCollection<ItemViewModel> Juz { get; private set; }
        public ObservableCollection<ItemViewModel> Bookmarks { get; private set; }
        public ObservableCollection<ItemViewModel> Tags { get; private set; }
        public bool IsDataLoaded { get; set; }
        public bool HasAskedToDownload { get; set; }

        private bool isInstalling;
        public bool IsInstalling
        {
            get { return isInstalling; }
            set
            {
                if (value == isInstalling)
                    return;

                isInstalling = value;

                base.RaisePropertyChanged(() => IsInstalling);
            }
        }

        private string installationStep;
        public string InstallationStep
        {
            get { return installationStep; }
            set
            {
                if (value == installationStep)
                    return;

                installationStep = value;

                base.RaisePropertyChanged(() => InstallationStep);
            }
        }
        #endregion Properties

        #region Public methods

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            // Sample data; replace with real data
            loadSuraList();
            loadJuz2List();
            loadBookmarkList();

            this.IsDataLoaded = true;
        }

        public void RefreshData()
        {
            this.Bookmarks.Clear();
            loadBookmarkList();
        }

        public async void Download()
        {
            if (!this.ActiveDownload.IsDownloading)
            {
                //bool finalizeSucceded = true; // not used?

                // kdimas: How if we include a zipped width_800 images for testing?
                // will save cost for testing, cutting the need to always download it from server.
#if DEBUG
                // MAKE SURE TO EXCLUDE "Assets/Offline" folder and its content before packaging for
                // production. (apply to WP 8 / WP 7.1).
                prepareOfflineZip();
#endif
                // If downloaded offline and stuck in temp storage
                if (this.ActiveDownload.IsInTempStorage && !this.ActiveDownload.IsDownloading)
                {
                    await ActiveDownload.FinishDownload();
                }

                if (!QuranFileUtils.HaveAllImages() && !this.HasAskedToDownload)
                {
                    this.HasAskedToDownload = true;
                    var askingToDownloadResult = QuranApp.NativeProvider.ShowQuestionMessageBox(AppResources.downloadPrompt,
                                                                 AppResources.downloadPrompt_title);

                    if (askingToDownloadResult && ActiveDownload.DownloadCommand.CanExecute(null))
                    {
                        ActiveDownload.DownloadCommand.Execute(null);
                    }
                }
            }
        }

        public void DeleteBookmark(ItemViewModel item)
        {
            int id = 0;
            if (item != null && item.Id != null && int.TryParse(item.Id, NumberStyles.None, CultureInfo.InvariantCulture, out id))
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
        /// <summary>
        /// Prepare offline zip for debugging purpose, this function will only be called
        /// on debug configuration.
        /// 
        /// Reference: http://msdn.microsoft.com/en-us/library/windowsphone/develop/hh286411%28v=vs.105%29.aspx
        /// </summary>
        private void prepareOfflineZip()
        {
            //// TODO: still ERROR in device

            //// Move images_800.zip from Assets/Offline to temporary storage
            //Uri offlineZipUri = new Uri("Assets/Offline/images_800.zip", UriKind.Relative);

            //// Obtain the virtual store for the application.
            //IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();

            //// Create a stream for the file in the installation folder.
            //var streamInfo = Application.GetResourceStream(offlineZipUri);
            //if (streamInfo != null)
            //{
            //    using (Stream input = streamInfo.Stream)
            //    {
            //        // Create a stream for the new file in the local folder.
            //        using (IsolatedStorageFileStream output = iso.CreateFile(this.QuranData.LocalUrl))
            //        {
            //            // Initialize the buffer.
            //            byte[] readBuffer = new byte[4096];
            //            int bytesRead = -1;

            //            // Copy the file from the installation folder to the local folder. 
            //            while ((bytesRead = input.Read(readBuffer, 0, readBuffer.Length)) > 0)
            //            {
            //                output.Write(readBuffer, 0, bytesRead);
            //            }
            //        }

            //        this.HasAskedToDownload = true;
            //    }
            //}
        }

        
        private void loadSuraList()
        {
            int sura = 1;
            int next = 1;

            for (int juz = 1; juz <= Constants.JUZ2_COUNT; juz++)
            {
                Surahs.Add(new ItemViewModel
                {
                    Id = QuranInfo.GetJuzTitle() + " " + juz,
                    PageNumber = QuranInfo.JUZ_PAGE_START[juz - 1],
                    ItemType = ItemViewModelType.Header
                });
                next = (juz == Constants.JUZ2_COUNT) ? Constants.PAGES_LAST + 1 : QuranInfo.JUZ_PAGE_START[juz];

                while ((sura <= Constants.SURAS_COUNT) && (QuranInfo.SURA_PAGE_START[sura - 1] < next))
                {
                    string title = QuranInfo.GetSuraName(sura, true);
                    Surahs.Add(new ItemViewModel
                    {
                        Id = sura.ToString(CultureInfo.InvariantCulture),
                        Title = title,
                        Details = QuranInfo.GetSuraListMetaString(sura),
                        PageNumber = QuranInfo.SURA_PAGE_START[sura - 1],
                        ItemType = ItemViewModelType.Sura
                    });
                    sura++;
                }
            }
        }

        private void loadJuz2List()
        {
            Uri[] images = new Uri[] {
                new Uri("/Assets/Images/hizb_full.png", UriKind.Relative),
                new Uri("/Assets/Images/hizb_quarter.png", UriKind.Relative),
                new Uri("/Assets/Images/hizb_half.png", UriKind.Relative),
                new Uri("/Assets/Images/hizb_threequarters.png", UriKind.Relative)
            };
            string[] quarters = QuranInfo.GetSuraQuarters();
            for (int i = 0; i < (8 * Constants.JUZ2_COUNT); i++)
            {
                int[] pos = QuranInfo.QUARTERS[i];
                int page = QuranInfo.GetPageFromSuraAyah(pos[0], pos[1]);

                if (i % 8 == 0)
                {
                    int juz = 1 + (i / 8);
                    Juz.Add(new ItemViewModel
                    {
                        Id = QuranInfo.GetJuzTitle() + " " + juz,
                        PageNumber = QuranInfo.JUZ_PAGE_START[juz - 1],
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
                    Juz[Juz.Count - 1].Id = (1 + (i / 4)).ToString("0", CultureInfo.InvariantCulture);
            }
        }

        private async void loadBookmarkList()
        {
            var lastPage = SettingsUtils.Get<int>(Constants.PREF_LAST_PAGE);
            if (lastPage > 0)
            {
                var lastPageItem = new ItemViewModel();
                lastPageItem.Title = QuranInfo.GetSuraNameFromPage(lastPage, true);
                lastPageItem.Details = string.Format("{0} {1}, {2} {3}", AppResources.quran_page, lastPage,
                                                 QuranInfo.GetJuzTitle(),
                                                 QuranInfo.GetJuzFromPage(lastPage));
                lastPageItem.PageNumber = lastPage;
                lastPageItem.Image = new Uri("/Assets/Images/favorite.png", UriKind.Relative);
                lastPageItem.ItemType = ItemViewModelType.Bookmark;
                lastPageItem.Group = AppResources.bookmarks_current_page;
                Bookmarks.Add(lastPageItem);
            }

            using (var bookmarksAdapter = new BookmarksDBAdapter())
            {
                try
                {
                    var bookmarks = bookmarksAdapter.GetBookmarks(true, BoomarkSortOrder.Location);
                    if (bookmarks.Count > 0)
                    {
                        //Load untagged first
                        foreach (var bookmark in bookmarks)
                        {
                            if (bookmark.Tags == null)
                            {
                                Bookmarks.Add(await createBookmarkModel(bookmark));
                            }
                        }

                        //Load tagged
                        foreach (var bookmark in bookmarks)
                        {
                            if (bookmark.Tags != null)
                            {
                                Bookmarks.Add(await createBookmarkModel(bookmark));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    QuranApp.NativeProvider.Log("failed to load bookmarks: " + ex.Message);
                }
            }
        }

        private const int maxBookmarkTitle = 40;
        private static async Task<ItemViewModel> createBookmarkModel(Bookmarks bookmark)
        {
            var group = AppResources.bookmarks;
            if (bookmark.Tags != null && bookmark.Tags.Count > 0)
                group = bookmark.Tags[0].Name;

            if (bookmark.Ayah != null && bookmark.Sura != null)
            {
                string title = QuranInfo.GetSuraNameFromPage(bookmark.Page, true);
                string details = "";

                if (
                    QuranFileUtils.FileExists(PathHelper.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false),
                                                           QuranFileUtils.QURAN_ARABIC_DATABASE)))
                {
                    try
                    {
                        using (var dbArabic = new DatabaseHandler<ArabicAyah>(QuranFileUtils.QURAN_ARABIC_DATABASE))
                        {
                            var ayahSurah =
                                await
                                new TaskFactory().StartNew(
                                    () => dbArabic.GetVerse(bookmark.Sura.Value, bookmark.Ayah.Value));
                            title = ayahSurah.Text;
                        }
                    }
                    catch
                    {
                        //Not able to get Arabic text - skipping
                    }

                    details = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}, {3} {4}",
                                               QuranInfo.GetSuraName(bookmark.Sura.Value, true),
                                               AppResources.verse,
                                               bookmark.Ayah.Value,
                                               QuranInfo.GetJuzTitle(),
                                               QuranInfo.GetJuzFromPage(bookmark.Page));
                }
                else
                {
                    details = string.Format(CultureInfo.InvariantCulture, "{0} {1}, {2} {3}, {4} {5}",
                                               AppResources.quran_page, bookmark.Page,
                                               AppResources.verse,
                                               bookmark.Ayah.Value,
                                               QuranInfo.GetJuzTitle(),
                                               QuranInfo.GetJuzFromPage(bookmark.Page));
                }

                if (title.Length > maxBookmarkTitle)
                {
                    for (int i = maxBookmarkTitle; i > 1; i--)
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
            else
            {
                return new ItemViewModel
                {
                    Id = bookmark.Id.ToString(CultureInfo.InvariantCulture),
                    Title = QuranInfo.GetSuraNameFromPage(bookmark.Page, true),
                    Details = string.Format(CultureInfo.InvariantCulture, "{0} {1}, {2} {3}", AppResources.quran_page, bookmark.Page,
                                            QuranInfo.GetJuzTitle(),
                                            QuranInfo.GetJuzFromPage(bookmark.Page)),
                    PageNumber = bookmark.Page,
                    Image = new Uri("/Assets/Images/favorite.png", UriKind.Relative),
                    ItemType = ItemViewModelType.Bookmark,
                    Group = group
                };
            }
        }

        #endregion
    }
}
