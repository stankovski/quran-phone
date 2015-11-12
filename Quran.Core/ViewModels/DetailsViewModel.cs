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
using System.Diagnostics;

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

                base.OnPropertyChanged(() => TranslationFile);
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

                base.OnPropertyChanged(() => ShowTranslation);
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

                base.OnPropertyChanged(() => ShowArabicInTranslation);
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

                CurrentSurahName = QuranUtils.GetSurahNameFromPage(value, false);
                CurrentSurahNumber = QuranUtils.GetSurahNumberFromPage(value);
                var rub = QuranUtils.GetRub3FromPage(value);
                CurrentJuzName = string.Format("{0} {1}{2} {3} {4}", QuranUtils.GetJuzTitle(),
                                               QuranUtils.GetJuzFromPage(value),
                                               getJuzPart(rub), AppResources.quran_rub3, rub);

                currentPageNumber = value;
                base.OnPropertyChanged(() => CurrentPageNumber);
            }
        }

        private string getJuzPart(int rub)
        {
            switch (rub % 8)
            {
                case 0:
                    return "";
                case 1:
                    return "⅛";
                case 2:
                    return "¼";
                case 3:
                    return "⅜";
                case 4:
                    return "½";
                case 5:
                    return "⅝";
                case 6:
                    return "¾";
                case 7:
                    return "⅞";
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

                base.OnPropertyChanged(() => CurrentSurahName);
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

                base.OnPropertyChanged(() => CurrentSurahNumber);
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

                base.OnPropertyChanged(() => CurrentJuzName);
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
                base.OnPropertyChanged(() => CurrentPageIndex);
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

                base.OnPropertyChanged(() => Orientation);

                // directly affect ShowInfoOverlay
                base.OnPropertyChanged(() => ShowInfoOverlay);
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

                base.OnPropertyChanged(() => IsShowMenu);
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

                base.OnPropertyChanged(() => KeepInfoOverlay);
                base.OnPropertyChanged(() => ShowInfoOverlay);
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

                base.OnPropertyChanged(() => IsNightMode);
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
                base.OnPropertyChanged(() => AudioPlayerState);
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
                base.OnPropertyChanged(() => IsDownloadingAudio);
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
                base.OnPropertyChanged(() => AudioDownloadProgress);
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

                base.OnPropertyChanged(() => BackgroundColor);
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

                base.OnPropertyChanged(() => ForegroundColor);
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

                base.OnPropertyChanged(() => SelectedAyah);
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
                        adapter.AddBookmarkIfNotExists(ayah.Surah, ayah.Ayah, CurrentPageNumber);
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
                await DownloadArabicSearchFile();
                if (await FileUtils.HaveArabicSearchFile())
                {
                    try
                    {
                        using (var dbArabic = new QuranDatabaseHandler<ArabicAyah>(FileUtils.QURAN_ARABIC_DATABASE))
                        {
                            var ayahSurah =
                                await new TaskFactory().StartNew(() => dbArabic.GetVerse(ayah.Surah, ayah.Ayah));
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

        public async Task<string> GetAyahString(QuranAyah ayah)
        {
            if (ayah == null)
            {
                return null;
            }

            else if (ayah.Text != null)
            {
                return ayah.Text;
            }
            else
            {
                await DownloadArabicSearchFile();
                if (await FileUtils.HaveArabicSearchFile())
                {
                    try
                    {
                        using (var dbArabic = new QuranDatabaseHandler<ArabicAyah>(FileUtils.QURAN_ARABIC_DATABASE))
                        {
                            var ayahSurah = await new TaskFactory().StartNew(() => dbArabic.GetVerse(ayah.Surah, ayah.Ayah));
                            string ayahText = ayahSurah.Text;
                            return ayahText;
                        }
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine("Ayah Text isn't found");
                        
                    }
                }
            }
            return null;
        }
        public async Task<bool> DownloadAyahPositionFile()
        {
            if (!await FileUtils.HaveAyaPositionFile())
            {
                string url = FileUtils.GetAyaPositionFileUrl();
                string destination = await FileUtils.GetQuranDatabaseDirectory();
                // start the download
                return await this.ActiveDownload.Download(url, destination, AppResources.loading_data);
            }
            else
            {
                return true;
            }
        }

        public async Task<bool> DownloadArabicSearchFile()
        {
            if (!await FileUtils.HaveArabicSearchFile())
            {
                string url = FileUtils.GetArabicSearchUrl();
                string destination = await FileUtils.GetQuranDatabaseDirectory();
                // start the download
                return await this.ActiveDownload.Download(url, destination, AppResources.loading_data);
            }
            else
            {
                return true;
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

        public async Task PlayFromAyah(int startSura, int startAyah)
        {
            int currentQari = AudioUtils.GetReciterIdByName(SettingsUtils.Get<string>(Constants.PREF_ACTIVE_QARI));
            if (currentQari == -1)
                return;

            var lookaheadAmount = SettingsUtils.Get<AudioDownloadAmount>(Constants.PREF_DOWNLOAD_AMOUNT);
            var ayah = new QuranAyah(startSura, startAyah);
            var request = new AudioRequest(currentQari, ayah, lookaheadAmount);

            if (SettingsUtils.Get<bool>(Constants.PREF_PREFER_STREAMING))
            {
                PlayStreaming(request);
            }
            else
            {
                await DownloadAndPlayAudioRequest(request);
            }
        }

        private void PlayStreaming(AudioRequest request)
        {
            //TODO: download database

            //TODO: play audio
        }

        private async Task DownloadAndPlayAudioRequest(AudioRequest request)
        {
            if (request == null || this.ActiveDownload.IsDownloading)
            {
                return;
            }

            var result = await DownloadAudioRequest(request);

            if (!result)
            {
                await QuranApp.NativeProvider.ShowErrorMessageBox("Something went wrong. Unable to download audio.");
            }
            else
            {
                var path = AudioUtils.GetLocalPathForAyah(request.CurrentAyah.Ayah == 0 ? new QuranAyah(1, 1) : request.CurrentAyah, request.Reciter);
                var title = request.CurrentAyah.Ayah == 0 ? "Bismillah" : QuranUtils.GetSurahAyahString(request.CurrentAyah);
                QuranApp.NativeProvider.AudioProvider.SetTrack(new Uri(path, UriKind.Relative), title, request.Reciter.Name, "Quran", null,
                    request.ToString());
            }
        }

        private async Task<bool> DownloadAudioRequest(AudioRequest request)
        {
            bool result = true;
            // checking if there is aya position file
            if (!await FileUtils.HaveAyaPositionFile())
            {
                result = await DownloadAyahPositionFile();
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
                await FileUtils.EnsureDirectoryExists(destination);

                if (request.Reciter.IsGapless)
                    result = await AudioUtils.DownloadGaplessRange(url, destination, request.FromAyah, request.ToAyah);
                else
                    result = await AudioUtils.DownloadRange(request);
            }
            return result;
        }

        private async void AudioProvider_StateChanged(object sender, EventArgs e)
        {
            if (QuranApp.NativeProvider.AudioProvider.State == AudioPlayerPlayState.Stopped ||
                    QuranApp.NativeProvider.AudioProvider.State == AudioPlayerPlayState.Unknown ||
                QuranApp.NativeProvider.AudioProvider.State == AudioPlayerPlayState.Error)
            {
                await Task.Delay(500);
                // Check if still stopped
                if (QuranApp.NativeProvider.AudioProvider.State == AudioPlayerPlayState.Stopped ||
                    QuranApp.NativeProvider.AudioProvider.State == AudioPlayerPlayState.Unknown ||
                QuranApp.NativeProvider.AudioProvider.State == AudioPlayerPlayState.Error)
                {
                    AudioPlayerState = AudioState.Stopped;
                }
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
                        var pageNumber = QuranUtils.GetPageFromSurahAyah(request.CurrentAyah);
                        var oldPageIndex = CurrentPageIndex;
                        var newPageIndex = getIndexFromPageNumber(pageNumber);

                        CurrentPageIndex = newPageIndex;
                        if (oldPageIndex != newPageIndex)
                        {
                            await Task.Delay(500);
                        }
                        // If bismillah set to first ayah
                        if (request.CurrentAyah.Ayah == 0)
                            request.CurrentAyah.Ayah = 1;
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
            pageModel.ImageSource = await FileUtils.GetImageFromWeb(FileUtils.GetPageFileName(pageModel.PageNumber));

            try
            {
                // Set translation
                if (string.IsNullOrEmpty(this.TranslationFile) ||
                    !await FileUtils.FileExists(PathHelper.Combine(await FileUtils.GetQuranDatabaseDirectory(),
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
                if (this.ShowArabicInTranslation && await FileUtils.HaveArabicSearchFile())
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
                    if (verse.Surah != tempSurah)
                    {
                        pageModel.Translations.Add(new VerseViewModel
                        {
                            StyleName = "TranslationViewHeader",
                            Text = QuranUtils.GetSurahName(verse.Surah, true)
                        });

                        tempSurah = verse.Surah;
                    }

                    pageModel.Translations.Add(new VerseViewModel(string.Format("{0}:{1}", verse.Surah, verse.Ayah), "BoldText"));

                    if (versesArabic != null)
                    {
                        pageModel.Translations.Add(new VerseViewModel(versesArabic[i].Text, "ArabicText", verse.Surah, verse.Ayah));
                    }

                    pageModel.Translations.Add(new VerseViewModel(verse.Text, null, verse.Surah, verse.Ayah));
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
                        await FileUtils.DeleteFile(PathHelper.Combine(await FileUtils.GetQuranDatabaseDirectory(),
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
