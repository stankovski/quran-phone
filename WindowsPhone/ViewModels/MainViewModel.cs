using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using QuranPhone.Common;
using QuranPhone.Resources;
using QuranPhone.Data;
using QuranPhone.Utils;
using System.IO.IsolatedStorage;

namespace QuranPhone.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            this.Surahs = new ObservableCollection<ItemViewModel>();
            this.Juz = new ObservableCollection<ItemViewModel>();
            this.Bookmarks = new ObservableCollection<ItemViewModel>();
            this.LastPage = new ObservableCollection<ItemViewModel>();
            this.Tags = new ObservableCollection<ItemViewModel>();
            this.HasAskedToDownload = false;
            this.QuranData = new DownloadableViewModelBase();
            this.QuranData.DownloadComplete += downloadComplete;
            this.QuranData.PropertyChanged += quranDataPropertyChanged;
            this.QuranData.ServerUrl = QuranFileUtils.GetZipFileUrl();
            this.QuranData.FileName = Path.GetFileName(QuranData.ServerUrl);
            this.QuranData.LocalUrl = QuranData.FileName;

            this.InstallationStep = Resources.AppResources.loading_message;
        }

        #region Properties
        public ObservableCollection<ItemViewModel> Surahs { get; private set; }
        public ObservableCollection<ItemViewModel> Juz { get; private set; }
        public ObservableCollection<ItemViewModel> Bookmarks { get; private set; }
        public ObservableCollection<ItemViewModel> LastPage { get; private set; }
        public ObservableCollection<ItemViewModel> Tags { get; private set; }
        public bool IsDataLoaded { get; set; }
        public bool HasAskedToDownload { get; set; }

        public DownloadableViewModelBase QuranData { get; private set; }

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

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            // Sample data; replace with real data
            loadSuraList();
            loadJuz2List();
            loadBookmarlList();

            this.IsDataLoaded = true;
        }

        public void RefreshData()
        {
            this.Bookmarks.Clear();
            this.LastPage.Clear();
            loadBookmarlList();
        }
        
        public async void Download()
        {
            if (!this.QuranData.IsDownloading)
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
                if (this.QuranData.IsInTempStorage && !this.QuranData.IsDownloading)
                {
                    this.QuranData.FinishPreviousDownload();
                    await ExtractZipAndFinalize();
                }
                // If downloaded offline and stuck in temp storage
                else if (this.QuranData.IsInLocalStorage && !this.QuranData.IsDownloading)
                {
                    await ExtractZipAndFinalize();
                }

                if (!QuranFileUtils.HaveAllImages() && !this.HasAskedToDownload)
                {
                    this.HasAskedToDownload = true;
                    var askingToDownloadResult = MessageBox.Show(AppResources.downloadPrompt,
                                                                 AppResources.downloadPrompt_title,
                                                                 MessageBoxButton.OKCancel);

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

                // showing indeterminate progress instead off (100%), 
                // using (100%) is kind of confusing, since the extraction is quite long
                QuranData.IsIndeterminate = true;
                //QuranData.Progress = 100;

                bool result = await new TaskFactory().StartNew(() => QuranFileUtils.ExtractZipFile(QuranData.LocalUrl, QuranFileUtils.QURAN_BASE));
                
                //QuranData.Progress = 100;
                //bool result = await Task.Run(() => QuranFileUtils.ExtractZipFile(QuranData.LocalUrl, QuranFileUtils.QURAN_BASE)).ConfigureAwait(false);

                if (!result)
                    return false;

                QuranFileUtils.DeleteFile(QuranData.LocalUrl);
            }
            
            IsInstalling = false;

            if (QuranFileUtils.HaveAllImages())
                return true;
            else
            {
                InstallationStep = AppResources.loading_message;
                QuranData.Progress = 0;
                return false;
            }
        }

        public void DeleteBookmark(ItemViewModel item)
        {
             if (item != null)
             {
                 Bookmarks.Remove(item);
                 using (var bookmarksAdapter = new BookmarksDBAdapter())
                 {
                     bookmarksAdapter.RemoveBookmark(int.Parse(item.Id));
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
            // TODO: still ERROR in device

            // Move images_800.zip from Assets/Offline to temporary storage
            Uri offlineZipUri = new Uri("Assets/Offline/images_800.zip", UriKind.Relative);

            // Obtain the virtual store for the application.
            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();

            // Create a stream for the file in the installation folder.
            using (Stream input = Application.GetResourceStream(offlineZipUri).Stream)
            {
                // Create a stream for the new file in the local folder.
                using (IsolatedStorageFileStream output = iso.CreateFile(this.QuranData.LocalUrl))
                {
                    // Initialize the buffer.
                    byte[] readBuffer = new byte[4096];
                    int bytesRead = -1;

                    // Copy the file from the installation folder to the local folder. 
                    while ((bytesRead = input.Read(readBuffer, 0, readBuffer.Length)) > 0)
                    {
                        output.Write(readBuffer, 0, bytesRead);
                    }
                }

                this.HasAskedToDownload = true;
            }
        }

        private void downloadComplete(object sender, EventArgs e)
        {
            ExtractZipAndFinalize();
        }

        void quranDataPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsDownloading")
            {
                IsInstalling = this.QuranData.IsDownloading;
            }
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
                        Id = sura.ToString(),
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
                    Juz[Juz.Count - 1].Id = (1 + (i / 4)).ToString("0");
            }
        }

        private void loadBookmarlList()
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
                LastPage.Add(lastPageItem);
            }

            using (var bookmarksAdapter = new BookmarksDBAdapter())
            {
                try
                {
                    var bookmarks = bookmarksAdapter.GetBookmarks(false, BoomarkSortOrder.Location);
                    if (bookmarks.Count > 0)
                    {
                        foreach (var bookmark in bookmarks)
                        {
                            Bookmarks.Add(createBookmarkModel(bookmark));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("failed to load bookmarks: " + ex.Message);
                }
            }
        }

        private static ItemViewModel createBookmarkModel(Bookmarks bookmark)
        {
            if (bookmark.Ayah != null && bookmark.Sura != null)
            {
                return new ItemViewModel
                {
                    Id = bookmark.Id.ToString(CultureInfo.InvariantCulture),
                    Title = QuranInfo.GetSuraNameFromPage(bookmark.Page, true),
                    Details = string.Format(CultureInfo.InvariantCulture, "{0} {1}, {2} {3} {4}, {5} {6}", 
                                            AppResources.quran_page, bookmark.Page,
                                            QuranInfo.GetSuraName(bookmark.Sura.Value, true),
                                            AppResources.verse,
                                            bookmark.Ayah.Value,
                                            QuranInfo.GetJuzTitle(),
                                            QuranInfo.GetJuzFromPage(bookmark.Page)),
                    PageNumber = bookmark.Page,
                    SelectedAyah = new QuranAyah(bookmark.Sura.Value, bookmark.Ayah.Value),
                    Image = new Uri("/Assets/Images/favorite.png", UriKind.Relative),
                    ItemType = ItemViewModelType.Bookmark
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
                        ItemType = ItemViewModelType.Bookmark
                    };
            }
        }

        #endregion
    }
}