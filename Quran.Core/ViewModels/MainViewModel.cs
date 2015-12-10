// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the MainViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Quran.Core.Common;
using Quran.Core.Data;
using Quran.Core.Properties;
using Quran.Core.Utils;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Quran.Core.ViewModels
{
    /// <summary>
    /// Define the MainViewModel type.
    /// </summary>
    public class MainViewModel : ViewModelWithDownload
    {
        private string _zipFileServerUrl;
        private string _zipFileLocalPath;
        public MainViewModel()
        {
            this.Surahs = new ObservableCollection<ItemViewModel>();
            this.Juz = new ObservableCollection<ItemViewModel>();
            this.Bookmarks = new ObservableCollection<ItemViewModel>();
            
            this.InstallationStep = Resources.loading_message;

            this.Tags = new ObservableCollection<ItemViewModel>();
            this.HasAskedToDownload = false;
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

                base.OnPropertyChanged(() => IsInstalling);
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

                base.OnPropertyChanged(() => InstallationStep);
            }
        }

        #endregion Properties

        #region Public methods

        public override async Task Initialize()
        {
            _zipFileServerUrl = FileUtils.GetZipFileUrl();
            _zipFileLocalPath = Path.Combine(await FileUtils.GetQuranBaseDirectory(), 
                Path.GetFileName(_zipFileServerUrl));

            if (!this.IsDataLoaded)
            {
                LoadSuraList();
                LoadJuz2List();
                this.IsDataLoaded = true;
            }

            await LoadBookmarkList();
            await ActiveDownload.Initialize();
        }

        public override async Task Refresh()
        {
            this.Bookmarks.Clear();
            await LoadBookmarkList();
            await base.Refresh();
        }
        public IEnumerable<IGrouping<KeyValuePair<string, string>, ItemViewModel>> GetGrouppedSurahItems()
        {
            return from b in Surahs
                   group b by new KeyValuePair<string, string>(b.Group.Substring(QuranUtils.GetJuzTitle().Length + 1), b.Group) into g
                   select g;
        }

        public IEnumerable<IGrouping<KeyValuePair<string, string>, ItemViewModel>> GetGrouppedBookmarks()
        {
            return  from b in Bookmarks
                    group b by new KeyValuePair<string, string>(b.Group, b.Group) into g
                    select g;
        }

        public IEnumerable<IGrouping<KeyValuePair<string, string>, ItemViewModel>> GetGrouppedJuzItems()
        {
            return from b in Juz
                   group b by new KeyValuePair<string, string>(b.Id, b.Group) into g
                   select g;
        }

        public async Task Download()
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
                if (await FileUtils.FileExists(_zipFileLocalPath))
                {
                    await ActiveDownload.FinishDownload(_zipFileLocalPath);
                }

                if (!await FileUtils.HaveAllImages() && !this.HasAskedToDownload)
                {
                    this.HasAskedToDownload = true;
                    var askingToDownloadResult = await QuranApp.NativeProvider.ShowQuestionMessageBox(Resources.downloadPrompt,
                                                                 Resources.downloadPrompt_title);

                    if (askingToDownloadResult)
                    {
                        await ActiveDownload.DownloadSingleFile(_zipFileServerUrl, _zipFileLocalPath);
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
                using (var bookmarksAdapter = new BookmarksDatabaseHandler())
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
        /// Reference: http://msdn.microsoft.com/en-us/library/Windows/develop/hh286411%28v=vs.105%29.aspx
        /// </summary>
        private void prepareOfflineZip()
        {
            //// TODO: still ERROR in device

            // Move images_800.zip from Assets/Offline to temporary storage
            Uri offlineZipUri = new Uri("Assets/Offline/images_800.zip", UriKind.Relative);

            // Obtain the virtual store for the application.
            //IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();

            // Create a stream for the file in the installation folder.
            //var streamInfo = string.Join(",", Application.Current.Resources.Keys);
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

        
        private void LoadSuraList()
        {
            for (int surah = 1; surah <= Constants.SURAS_COUNT; surah++)
            {
                string title = QuranUtils.GetSurahName(surah, true);
                Surahs.Add(new ItemViewModel
                {
                    Id = surah.ToString(CultureInfo.InvariantCulture),
                    Title = title,
                    Details = QuranUtils.GetSuraListMetaString(surah),
                    PageNumber = QuranUtils.SURA_PAGE_START[surah - 1],
                    ItemType = ItemViewModelType.Surah,
                    Group = QuranUtils.GetJuzTitle() + " " + QuranUtils.GetJuzFromAyah(surah, 1)
                });
            }

            //int surah = 1;
            //int next = 1;
            //for (int juz = 1; juz <= Constants.JUZ2_COUNT; juz++)
            //{
            //    Surahs.Add(new ItemViewModel
            //    {
            //        Id = QuranUtils.GetJuzTitle() + " " + juz,
            //        Title = QuranUtils.GetJuzTitle() + " " + juz,
            //        PageNumber = QuranUtils.JUZ_PAGE_START[juz - 1],
            //        ItemType = ItemViewModelType.Header
            //    });
            //    next = (juz == Constants.JUZ2_COUNT) ? Constants.PAGES_LAST + 1 : QuranUtils.JUZ_PAGE_START[juz];

            //    while ((surah <= Constants.SURAS_COUNT) && (QuranUtils.SURA_PAGE_START[surah - 1] < next))
            //    {
            //        string title = QuranUtils.GetSurahName(surah, true);
            //        Surahs.Add(new ItemViewModel
            //        {
            //            Id = surah.ToString(CultureInfo.InvariantCulture),
            //            Title = title,
            //            Details = QuranUtils.GetSuraListMetaString(surah),
            //            PageNumber = QuranUtils.SURA_PAGE_START[surah - 1],
            //            ItemType = ItemViewModelType.Surah
            //        });
            //        surah++;
            //    }
            //}
        }

        private void LoadJuz2List()
        {
            Uri[] images = new Uri[] {
                new Uri("ms-appx:///Assets/Images/hizb_full.png"),
                new Uri("ms-appx:///Assets/Images/hizb_quarter.png"),
                new Uri("ms-appx:///Assets/Images/hizb_half.png"),
                new Uri("ms-appx:///Assets/Images/hizb_threequarters.png")
            };
            string[] quarters = QuranUtils.GetSurahQuarters();
            int juz = 0;
            for (int i = 0; i < (8 * Constants.JUZ2_COUNT); i++)
            {
                if (i % 8 == 0)
                {
                    juz++;
                }

                int[] pos = QuranUtils.QUARTERS[i];
                int page = QuranUtils.GetPageFromAyah(pos[0], pos[1]);
                string verseString = Resources.quran_ayah + " " + pos[1];
                Juz.Add(new ItemViewModel
                {
                    Id = juz.ToString(CultureInfo.InvariantCulture),
                    Group = string.Format("{0} {1}", Resources.quran_juz2, juz),
                    Title = quarters[i],
                    Details = QuranUtils.GetSurahName(pos[0], true) + ", " + verseString,
                    PageNumber = page,
                    Image = new BitmapImage(images[i % 4]),
                    ItemType = ItemViewModelType.Surah
                });
            }
        }

        private async Task LoadBookmarkList()
        {
            Bookmarks.Clear();
            var lastPage = SettingsUtils.Get<int>(Constants.PREF_LAST_PAGE);
            if (lastPage > 0)
            {
                var lastPageItem = new ItemViewModel();
                lastPageItem.Title = QuranUtils.GetSurahNameFromPage(lastPage, true);
                lastPageItem.Details = string.Format("{0} {1}, {2} {3}", Resources.quran_page, lastPage,
                                                 QuranUtils.GetJuzTitle(),
                                                 QuranUtils.GetJuzFromPage(lastPage));
                lastPageItem.PageNumber = lastPage;
                lastPageItem.Image = new BitmapImage(new Uri("ms-appx:///Assets/Images/favorite.png"));
                lastPageItem.ItemType = ItemViewModelType.Bookmark;
                lastPageItem.Group = Resources.bookmarks_current_page;
                lastPageItem.Id = lastPageItem.Group;
                Bookmarks.Add(lastPageItem);
            }

            using (var bookmarksAdapter = new BookmarksDatabaseHandler())
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
                                Bookmarks.Add(await CreateBookmarkModel(bookmark));
                            }
                        }

                        //Load tagged
                        foreach (var bookmark in bookmarks)
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
                    QuranApp.NativeProvider.Log("failed to load bookmarks: " + ex.Message);
                }
            }
        }

        private const int maxBookmarkTitle = 40;
        private static async Task<ItemViewModel> CreateBookmarkModel(Bookmarks bookmark)
        {
            var group = Resources.bookmarks;
            if (bookmark.Tags != null && bookmark.Tags.Count > 0)
                group = bookmark.Tags[0].Name;

            if (bookmark.Ayah != null && bookmark.Surah != null)
            {
                string title = QuranUtils.GetSurahNameFromPage(bookmark.Page, true);
                string details = "";

                if (await FileUtils.HaveArabicSearchFile())
                {
                    try
                    {
                        using (var dbArabic = new QuranDatabaseHandler<ArabicAyah>(FileUtils.QURAN_ARABIC_DATABASE))
                        {
                            var ayahSurah =
                                await
                                new TaskFactory().StartNew(
                                    () => dbArabic.GetVerse(bookmark.Surah.Value, bookmark.Ayah.Value));
                            title = ayahSurah.Text;
                        }
                    }
                    catch
                    {
                        //Not able to get Arabic text - skipping
                    }

                    details = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}, {3} {4}",
                                               QuranUtils.GetSurahName(bookmark.Surah.Value, true),
                                               Resources.verse,
                                               bookmark.Ayah.Value,
                                               QuranUtils.GetJuzTitle(),
                                               QuranUtils.GetJuzFromPage(bookmark.Page));
                }
                else
                {
                    details = string.Format(CultureInfo.InvariantCulture, "{0} {1}, {2} {3}, {4} {5}",
                                               Resources.quran_page, bookmark.Page,
                                               Resources.verse,
                                               bookmark.Ayah.Value,
                                               QuranUtils.GetJuzTitle(),
                                               QuranUtils.GetJuzFromPage(bookmark.Page));
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
                    SelectedAyah = new QuranAyah(bookmark.Surah.Value, bookmark.Ayah.Value),
                    Image = new BitmapImage(new Uri("ms-appx:///Assets/Images/favorite.png")),
                    ItemType = ItemViewModelType.Bookmark,
                    Group = group
                };
            }
            else
            {
                return new ItemViewModel
                {
                    Id = bookmark.Id.ToString(CultureInfo.InvariantCulture),
                    Title = QuranUtils.GetSurahNameFromPage(bookmark.Page, true),
                    Details = string.Format(CultureInfo.InvariantCulture, "{0} {1}, {2} {3}", Resources.quran_page, bookmark.Page,
                                            QuranUtils.GetJuzTitle(),
                                            QuranUtils.GetJuzFromPage(bookmark.Page)),
                    PageNumber = bookmark.Page,
                    Image = new BitmapImage(new Uri("ms-appx:///Assets/Images/favorite.png")),
                    ItemType = ItemViewModelType.Bookmark,
                    Group = group
                };
            }
        }

        #endregion
    }
}
