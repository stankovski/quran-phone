// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the DetailsViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Quran.Core.Common;
using Quran.Core.Data;
using Quran.Core.Properties;
using Quran.Core.Utils;

namespace Quran.Core.ViewModels
{
    /// <summary>
    /// Define the DetailsViewModel type.
    /// </summary>
    public class DetailsViewModel : ViewModelWithDownload
    {
        private static string lightColor;
        private static string darkColor;

        public DetailsViewModel()
        {
            Pages = new ObservableCollection<PageViewModel>();
            QuranApp.NativeProvider.AudioProvider.StateChanged += AudioProvider_StateChanged;
            lightColor = "White";
            darkColor = "Black";
            BackgroundColor = "Resource:LightBackground";
            ForegroundColor = darkColor;

            // default detail page to full screen mode.
            IsShowMenu = false;
        }

        #region Properties
        public ObservableCollection<PageViewModel> Pages { get; private set; }
        
        private string translationFile;
        public string TranslationFile
        {
            get { return translationFile; }
            set
            {
                if (value == translationFile)
                    return;

                translationFile = value;

                base.RaisePropertyChanged(() => TranslationFile);
            }
        }

        private bool showTranslation;
        public bool ShowTranslation
        {
            get { return showTranslation; }
            set
            {
                if (value == showTranslation)
                    return;

                showTranslation = value;

                base.RaisePropertyChanged(() => ShowTranslation);
            }
        }

        private bool showArabicInTranslation;
        public bool ShowArabicInTranslation
        {
            get { return showArabicInTranslation; }
            set
            {
                if (value == showArabicInTranslation)
                    return;

                showArabicInTranslation = value;

                base.RaisePropertyChanged(() => ShowArabicInTranslation);
            }
        }

        private int currentPageNumber;
        public int CurrentPageNumber
        {
            get { return currentPageNumber; }
            set
            {
                if (value == currentPageNumber)
                    return;

                CurrentSurahName = QuranInfo.GetSuraNameFromPage(value, false);
                CurrentSurahNumber = QuranInfo.GetSuraNumberFromPage(value);
                var rub = QuranInfo.GetRub3FromPage(value);
                CurrentJuzName = string.Format("{0} {1}{2} {3} {4}", QuranInfo.GetJuzTitle(),
                                               QuranInfo.GetJuzFromPage(value),
                                               getJuzPart(rub), AppResources.quran_rub3, rub);

                currentPageNumber = value;
                base.RaisePropertyChanged(() => CurrentPageNumber);
            }
        }

        private string getJuzPart(int rub)
        {
            switch (rub % 8)
            {
                case 0:
                    return "";
                case 1:
                    return "?";
                case 2:
                    return "¼";
                case 3:
                    return "?";
                case 4:
                    return "½";
                case 5:
                    return "?";
                case 6:
                    return "¾";
                case 7:
                    return "?";
                default:
                    return "";
            }
        }

        private string currentSurahName;
        public string CurrentSurahName
        {
            get { return currentSurahName; }
            set
            {
                if (value == currentSurahName)
                    return;

                currentSurahName = value;

                base.RaisePropertyChanged(() => CurrentSurahName);
            }
        }

        private int currentSurahNumber;
        public int CurrentSurahNumber
        {
            get { return currentSurahNumber; }
            set
            {
                if (value == currentSurahNumber)
                    return;

                currentSurahNumber = value;

                base.RaisePropertyChanged(() => CurrentSurahNumber);
            }
        }

        private string currentJuzName;
        public string CurrentJuzName
        {
            get { return currentJuzName; }
            set
            {
                if (value == currentJuzName)
                    return;

                currentJuzName = value;

                base.RaisePropertyChanged(() => CurrentJuzName);
            }
        }

        private int currentPageIndex = -1;
        public int CurrentPageIndex
        {
            get { return currentPageIndex; }
            set
            {
                if (value == currentPageIndex)
                    return;

                currentPageIndex = value;
                if (value >= 0)
                    UpdatePages();
                base.RaisePropertyChanged(() => CurrentPageIndex);
            }
        }

        public PageViewModel CurrentPage
        {
            get
            {
                if (CurrentPageIndex >= 0 && CurrentPageIndex < Pages.Count)
                    return Pages[CurrentPageIndex];
                else
                    return null;
            }
        }

        public bool IsDataLoaded { get; protected set; }

        private ScreenOrientation orientation;
        public ScreenOrientation Orientation
        {
            get { return orientation; }
            set
            {
                if (value == orientation)
                    return;

                orientation = value;

                base.RaisePropertyChanged(() => Orientation);

                // directly affect ShowInfoOverlay
                base.RaisePropertyChanged(() => ShowInfoOverlay);
            }
        }

        private bool isShowMenu;
        public bool IsShowMenu
        {
            get { return isShowMenu; }
            set
            {
                if (value == isShowMenu)
                    return;

                if (!value && QuranApp.NativeProvider.IsPortaitOrientation)
                    return;

                isShowMenu = value;

                base.RaisePropertyChanged(() => IsShowMenu);
            }
        }

        public bool ShowInfoOverlay
        {
            get
            {
                if (Orientation == ScreenOrientation.Landscape ||
                        Orientation == ScreenOrientation.LandscapeLeft ||
                        Orientation == ScreenOrientation.LandscapeRight)
                {
                    return keepInfoOverlay;
                }

                // always show info overlay on portrait
                return true;
            }
        }

        private bool keepInfoOverlay;
        public bool KeepInfoOverlay
        {
            get { return keepInfoOverlay; }
            set
            {
                if (value == keepInfoOverlay)
                    return;

                keepInfoOverlay = value;

                base.RaisePropertyChanged(() => KeepInfoOverlay);
                base.RaisePropertyChanged(() => ShowInfoOverlay);
            }
        }

        private bool isNightMode;
        public bool IsNightMode
        {
            get { return isNightMode; }
            set
            {
                if (value == isNightMode)
                    return;

                isNightMode = value;
                UpdateStyles();

                base.RaisePropertyChanged(() => IsNightMode);
            }
        }

        private AudioState audioPlayerState;
        public AudioState AudioPlayerState
        {
            get { return audioPlayerState; }
            set
            {
                if (value == audioPlayerState)
                    return;

                audioPlayerState = value;
                base.RaisePropertyChanged(() => AudioPlayerState);
            }
        }

        private bool isDownloadingAudio;
        public bool IsDownloadingAudio
        {
            get { return isDownloadingAudio; }
            set
            {
                if (value == isDownloadingAudio)
                    return;

                isDownloadingAudio = value;
                base.RaisePropertyChanged(() => IsDownloadingAudio);
            }
        }

        private int audioDownloadProgress;
        public int AudioDownloadProgress
        {
            get { return audioDownloadProgress; }
            set
            {
                if (value == audioDownloadProgress)
                    return;

                audioDownloadProgress = value;
                base.RaisePropertyChanged(() => AudioDownloadProgress);
            }
        }

        private void UpdateStyles()
        {
            if (IsNightMode)
            {
                BackgroundColor = "Resource:DarkBackground";
                ForegroundColor = lightColor;
            }
            else
            {
                BackgroundColor = "Resource:LightBackground";
                ForegroundColor = darkColor;
            }
        }

        private string backgroundColor;
        public string BackgroundColor
        {
            get { return backgroundColor; }
            set
            {
                if (value == backgroundColor)
                    return;

                backgroundColor = value;

                base.RaisePropertyChanged(() => BackgroundColor);
            }
        }

        private string foregroundColor;
        public string ForegroundColor
        {
            get { return foregroundColor; }
            set
            {
                if (value == foregroundColor)
                    return;

                foregroundColor = value;

                base.RaisePropertyChanged(() => ForegroundColor);
            }
        }

        private QuranAyah selectedAyah;
        public QuranAyah SelectedAyah
        {
            get { return selectedAyah; }
            set
            {
                if (value == selectedAyah)
                    return;

                selectedAyah = value;

                base.RaisePropertyChanged(() => SelectedAyah);
            }
        }

        public bool AyahDetailsExist
        {
            get
            {
                string path = PathHelper.Combine(FileUtils.GetQuranDatabaseDirectory(false, true), FileUtils.GetAyaPositionFileName());
                if (FileUtils.FileExists(path))
                    return true;
                else
                    return false;
            }
        }

        #endregion Properties

        #region Public methods

        /// <summary>
        ///     Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            if (Pages.Count == 0)
            {
                for (int page = Constants.PAGES_LAST; page >= Constants.PAGES_FIRST; page--)
                {
                    Pages.Add(new PageViewModel(page) { ShowTranslation = this.ShowTranslation });
                }
            }

            this.CurrentPageIndex = getIndexFromPageNumber(this.CurrentPageNumber);
            this.IsDataLoaded = true;
        }

        public async void UpdatePages()
        {
            CurrentPageNumber = Pages[CurrentPageIndex].PageNumber;
            SettingsUtils.Set<int>(Constants.PREF_LAST_PAGE, CurrentPageNumber);

            await loadPage(CurrentPageIndex, false);
        }

        public void ClearPages()
        {
            foreach (var page in Pages)
            {
                page.Translations.Clear();
                page.ImageSource = null;
            }
        }

        public bool AddPageBookmark()
        {
            return AddBookmark(null);
        }

        public bool AddAyahBookmark(QuranAyah ayah)
        {
            return AddBookmark(ayah);
        }

        private bool AddBookmark(QuranAyah ayah)
        {
            try
            {
                using (var adapter = new BookmarksDatabaseHandler())
                {
                    if (ayah == null)
                        adapter.AddBookmarkIfNotExists(null, null, CurrentPageNumber);
                    else
                        adapter.AddBookmarkIfNotExists(ayah.Sura, ayah.Ayah, CurrentPageNumber);
                }
                return true;
            }
            catch (Exception)
            {
                QuranApp.NativeProvider.Log("error creating bookmark");
                return false;
            }
        }

        public async void CopyAyahToClipboard(QuranAyah ayah)
        {
            if (ayah == null)
                return;

            if (ayah.Translation != null)
            {
                QuranApp.NativeProvider.CopyToClipboard(ayah.Translation);
            }
            else if (ayah.Text != null)
            {
                QuranApp.NativeProvider.CopyToClipboard(ayah.Text);
            }
            else
            {
                if (FileUtils.FileExists(PathHelper.Combine(FileUtils.GetQuranDatabaseDirectory(false),
                                                           FileUtils.QURAN_ARABIC_DATABASE)))
                {
                    try
                    {
                        using (var dbArabic = new QuranDatabaseHandler<ArabicAyah>(FileUtils.QURAN_ARABIC_DATABASE))
                        {
                            var ayahSurah =
                                await new TaskFactory().StartNew(() => dbArabic.GetVerse(ayah.Sura, ayah.Ayah));
                            QuranApp.NativeProvider.CopyToClipboard(ayahSurah.Text);
                        }
                    }
                    catch
                    {
                        //Not able to get Arabic text - skipping
                    }
                }
            }
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            foreach (var page in Pages)
            {
                cleanPage(Pages.IndexOf(page));
            }
        }

        #endregion

        #region Audio

        public void PlayFromAyah(int startSura, int startAyah)
        {
            int currentQari = AudioUtils.GetReciterIdByName(SettingsUtils.Get<string>(Constants.PREF_ACTIVE_QARI));
            if (currentQari == -1)
                return;

            var lookaheadAmount = SettingsUtils.Get<LookAheadAmount>(Constants.PREF_DOWNLOAD_AMOUNT);
            var ayah = new QuranAyah(startSura, startAyah);
            var request = new AudioRequest(currentQari, ayah, lookaheadAmount);

            if (SettingsUtils.Get<bool>(Constants.PREF_PREFER_STREAMING))
            {
                PlayStreaming(request);
            }
            else
            {
                DownloadAndPlayAudioRequest(request);
            }
        }

        private void PlayStreaming(AudioRequest request)
        {
            //TODO: download database

            //TODO: play audio
        }

        private async void DownloadAndPlayAudioRequest(AudioRequest request)
        {
            if (request == null || this.ActiveDownload.IsDownloading)
            {
                return;
            }

            var result = await DownloadAudioRequest(request);

            if (!result)
            {
                QuranApp.NativeProvider.ShowErrorMessageBox("Something went wrong. Unable to download audio.");
            }
            else
            {
                var uri = AudioUtils.GetLocalPathForAyah(request.CurrentAyah, request.Reciter);
                QuranApp.NativeProvider.AudioProvider.SetTrack(new Uri(uri, UriKind.Relative), null, null, null, null,
                    request.ToString());
            }
        }

        private async Task<bool> DownloadAudioRequest(AudioRequest request)
        {
            bool result = true;
            // checking if there is aya position file
            if (!FileUtils.HaveAyaPositionFile())
            {
                string url = FileUtils.GetAyaPositionFileUrl();
                string destination = FileUtils.GetQuranDatabaseDirectory(false);
                // start the download
                result = await this.ActiveDownload.Download(url, destination, AppResources.loading_data);
            }

            // checking if need to download gapless database file
            if (result && AudioUtils.ShouldDownloadGaplessDatabase(request))
            {
                string url = request.Reciter.GaplessDatabasePath;
                string destination = request.Reciter.LocalPath;
                // start the download
                result = await this.ActiveDownload.Download(url, destination, AppResources.loading_data);
            }

            // checking if need to download mp3
            if (result && !AudioUtils.HaveAllFiles(request))
            {
                string url = request.Reciter.ServerUrl;
                string destination = request.Reciter.LocalPath;
                FileUtils.MakeDirectory(destination);

                if (request.Reciter.IsGapless)
                    result = await AudioUtils.DownloadGaplessRange(url, destination, request.MinAyah, request.MaxAyah);
                else
                    result = await AudioUtils.DownloadRange(request);
            }
            return result;
        }

        private async void AudioProvider_StateChanged(object sender, EventArgs e)
        {
            if (QuranApp.NativeProvider.AudioProvider.State == AudioPlayerPlayState.Stopped)
            {
                // Wait to make sure the audio is really stopped and is not being changed to a different track
                await Task.Delay(500);

                if (QuranApp.NativeProvider.AudioProvider.State == AudioPlayerPlayState.Stopped ||
                    QuranApp.NativeProvider.AudioProvider.State == AudioPlayerPlayState.Unknown)
                    AudioPlayerState = AudioState.Stopped;

                //TODO: download next batch if needed
            }
            else if (QuranApp.NativeProvider.AudioProvider.State == AudioPlayerPlayState.Paused)
            {
                AudioPlayerState = AudioState.Paused;
            }
            else if (QuranApp.NativeProvider.AudioProvider.State == AudioPlayerPlayState.Playing)
            {
                AudioPlayerState = AudioState.Playing;

                var track = QuranApp.NativeProvider.AudioProvider.GetTrack();
                if (track != null && track.Tag != null)
                {
                    try
                    {
                        var request = new AudioRequest(track.Tag);
                        var pageNumber = QuranInfo.GetPageFromSuraAyah(request.CurrentAyah);
                        CurrentPageIndex = getIndexFromPageNumber(pageNumber);
                        SelectedAyah = request.CurrentAyah;
                    }
                    catch
                    {
                        // Bad track
                    }
                }
            }
        }

        #endregion

        #region Private helper methods
        private void cleanPage(int pageIndex)
        {
            var pageModel = Pages[pageIndex];
            pageModel.ImageSource = null;
            pageModel.Translations.Clear();
        }

        private async Task<bool> loadPage(int pageIndex, bool force)
        {
            var pageModel = Pages[pageIndex];
            // Set image
            pageModel.ImageSource = FileUtils.GetImageFromWeb(FileUtils.GetPageFileName(pageModel.PageNumber));

            try
            {
                // Set translation
                if (string.IsNullOrEmpty(this.TranslationFile) ||
                    !FileUtils.FileExists(PathHelper.Combine(FileUtils.GetQuranDatabaseDirectory(false),
                                                            this.TranslationFile)))
                    return false;

                if (!force && pageModel.Translations.Count > 0)
                    return false;

                List<QuranAyah> verses = null;
                using (var db = new QuranDatabaseHandler<QuranAyah>(this.TranslationFile))
                {
                    verses = await new TaskFactory().StartNew(() => db.GetVerses(pageModel.PageNumber));
                }

                List<ArabicAyah> versesArabic = null;
                if (this.ShowArabicInTranslation && FileUtils.FileExists(PathHelper.Combine(FileUtils.GetQuranDatabaseDirectory(false),
                                                        FileUtils.QURAN_ARABIC_DATABASE)))
                {
                    try
                    {
                        using (var dbArabic = new QuranDatabaseHandler<ArabicAyah>(FileUtils.QURAN_ARABIC_DATABASE))
                        {
                            versesArabic = await new TaskFactory().StartNew(() => dbArabic.GetVerses(pageModel.PageNumber));
                        }
                    }
                    catch
                    {
                        //Not able to get Arabic text - skipping
                    }
                }

                int tempSurah = -1;
                pageModel.Translations.Clear();
                for (int i = 0; i < verses.Count; i++)
                {
                    var verse = verses[i];
                    if (verse.Sura != tempSurah)
                    {
                        pageModel.Translations.Add(new VerseViewModel
                        {
                            StyleName = "TranslationViewHeader",
                            Text = QuranInfo.GetSuraName(verse.Sura, true)
                        });

                        tempSurah = verse.Sura;
                    }

                    pageModel.Translations.Add(new VerseViewModel(string.Format("{0}:{1}", verse.Sura, verse.Ayah), "BoldText"));

                    if (versesArabic != null)
                    {
                        pageModel.Translations.Add(new VerseViewModel(versesArabic[i].Text, "ArabicText", verse.Sura, verse.Ayah));
                    }

                    var versesSplit = QuranApp.NativeProvider.SplitLongText(verse.Text, SettingsUtils.Get<int>(Constants.PREF_TRANSLATION_TEXT_SIZE), "Normal");

                    foreach (var vs in versesSplit)
                    {
                        pageModel.Translations.Add(new VerseViewModel(vs, null, verse.Sura, verse.Ayah));
                    }
                }
                // Adding padding
                pageModel.Translations.Add(new VerseViewModel(" "));
            }
            catch (Exception e)
            {
                // Try delete bad translation file if error is "no such table: verses"
                try
                {
                    if (e.Message.StartsWith("no such table:", StringComparison.OrdinalIgnoreCase))
                    {
                        FileUtils.DeleteFile(PathHelper.Combine(FileUtils.GetQuranDatabaseDirectory(false, false),
                                                               this.TranslationFile));
                    }
                }
                catch
                {
                    // Do nothing
                }
                pageModel.Translations.Add(new VerseViewModel { Text = "Error loading translation..." });
            }
            return true;
        }

        private int getIndexFromPageNumber(int number)
        {
            return Constants.PAGES_LAST - number;
        }
        #endregion Private helper methods
    }
}
