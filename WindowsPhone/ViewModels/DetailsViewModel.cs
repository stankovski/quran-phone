using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using QuranPhone.Common;
using QuranPhone.Data;
using QuranPhone.Resources;
using QuranPhone.Utils;
using QuranPhone.UI;
using Microsoft.Phone.Controls;

namespace QuranPhone.ViewModels
{
    public class DetailsViewModel : ViewModelBase
    {
        private static SolidColorBrush lightColor;
        private static SolidColorBrush darkColor;

        public DetailsViewModel()
        {
            Pages = new ObservableCollection<PageViewModel>();
            lightColor = new SolidColorBrush(Colors.White);
            darkColor = new SolidColorBrush(Colors.Black);
            BackgroundColor = (LinearGradientBrush)Application.Current.TryFindResource("LightBackground");
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

                CurrentSurahName = QuranInfo.GetSuraNameFromPage(value, false);
                CurrentSurahNumber = QuranInfo.GetSuraNumberFromPage(value);
                var rub = QuranInfo.GetRub3FromPage(value);
                CurrentJuzName = string.Format("{0} {1}{2} {3} {4}", QuranInfo.GetJuzTitle(),
                                               QuranInfo.GetJuzFromPage(value),
                                               getJuzPart(rub), AppResources.quran_rub3, rub);

                currentPageNumber = value;
                base.OnPropertyChanged(() => CurrentPageNumber);
            }
        }

        private string getJuzPart(int rub)
        {
            switch (rub%8)
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

        private PageOrientation orientation;
        public PageOrientation Orientation
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

                if (!value && PhoneUtils.IsPortaitOrientation)
                    return;

                isShowMenu = value;

                base.OnPropertyChanged(() => IsShowMenu);
            }
        }

        public bool ShowInfoOverlay
        {
            get 
            {
                if (Orientation == PageOrientation.Landscape || 
                        Orientation == PageOrientation.LandscapeLeft || 
                        Orientation == PageOrientation.LandscapeRight)
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

        private bool isPlayingAudio;
        public bool IsPlayingAudio
        {
            get { return isPlayingAudio; }
            set
            {
                if (value == isPlayingAudio)
                    return;

                isPlayingAudio = value;
                base.OnPropertyChanged(() => IsPlayingAudio);
            }
        }

        private bool isAudioControlVisible;
        public bool IsAudioControlVisible
        {
            get { return isAudioControlVisible; }
            set
            {
                if (value == isAudioControlVisible)
                    return;

                isAudioControlVisible = value;
                base.OnPropertyChanged(() => IsAudioControlVisible);
            }
        }

        private void UpdateStyles()
        {
            if (IsNightMode)
            {
                BackgroundColor = (LinearGradientBrush)Application.Current.TryFindResource("DarkBackground");
                ForegroundColor = lightColor;
            }
            else
            {
                BackgroundColor = (LinearGradientBrush)Application.Current.TryFindResource("LightBackground");
                ForegroundColor = darkColor;
            }
        }

        private LinearGradientBrush backgroundColor;
        public LinearGradientBrush BackgroundColor
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

        private SolidColorBrush foregroundColor;
        public SolidColorBrush ForegroundColor
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

        public bool AyahDetailsExist
        {
            get
            {
                string path = Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false, true), QuranFileUtils.GetAyaPositionFileName());
                if (QuranFileUtils.FileExists(path))
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
                    Pages.Add(new PageViewModel(page) {ShowTranslation = this.ShowTranslation});
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
                using (var adapter = new BookmarksDBAdapter())
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
                Console.WriteLine("error creating bookmark");
                return false;
            }
        }

        public async void CopyAyahToClipboard(QuranAyah ayah)
        {
            if (ayah == null)
                return;

            if (ayah.Translation != null)
            {
                Clipboard.SetText(ayah.Translation);
            }
            else if (ayah.Text != null)
            {
                Clipboard.SetText(ayah.Text);
            }
            else
            {
                if (QuranFileUtils.FileExists(Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false),
                                                           QuranFileUtils.QURAN_ARABIC_DATABASE)))
                {
                    try
                    {
                        using (var dbArabic = new DatabaseHandler<ArabicAyah>(QuranFileUtils.QURAN_ARABIC_DATABASE))
                        {
                            var ayahSurah =
                                await new TaskFactory().StartNew(() => dbArabic.GetVerse(ayah.Sura, ayah.Ayah));
                            Clipboard.SetText(ayahSurah.Text);
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

        private bool mShouldOverridePlaying = false;
        private AudioRequest mLastAudioDownloadRequest = null;

        public void PlayFromAyah(int page, int sura, int ayah)
        {
            PlayFromAyah(page, sura, ayah, true);
        }

        private void PlayFromAyah(int page, int startSura,
                                  int startAyah, bool force)
        {
            if (force)
            {
                mShouldOverridePlaying = true;
            }
            int currentQari = AudioUtils.GetQariPositionByName(App.SettingsViewModel.ActiveQari);

            QuranAyah ayah = new QuranAyah(startSura, startAyah);
            if (SettingsUtils.Get<bool>(Constants.PREF_PREFER_STREAMING))
            {
                PlayStreaming(ayah, page, currentQari);
            }
            else
            {
                DownloadAndPlayAudio(ayah, page, currentQari);
            }
        }

        private void PlayStreaming(QuranAyah ayah, int page, int qari)
        {
            string qariUrl = AudioUtils.GetQariUrl(qari, true);
            string dbFile = AudioUtils.GetQariDatabasePathIfGapless(qari);
            if (!string.IsNullOrEmpty(dbFile))
            {
                // gapless audio is "download only"
                DownloadAndPlayAudio(ayah, page, qari);
                return;
            }

            var request = new AudioRequest(qariUrl, ayah);
            request.SetGaplessDatabaseFilePath(dbFile);
            Play(request);

            IsPlayingAudio = true;
        }

        private void DownloadAndPlayAudio(QuranAyah ayah, int page, int qari)
        {
            QuranAyah endAyah = AudioUtils.GetLastAyahToPlay(ayah, page,
                                                             SettingsUtils.Get<LookAheadAmount>(
                                                                 Constants.PREF_DOWNLOAD_AMOUNT));
            string baseUri = AudioUtils.GetLocalQariUrl(qari);
            if (endAyah == null || baseUri == null)
            {
                return;
            }
            string dbFile = AudioUtils.GetQariDatabasePathIfGapless(qari);

            string fileUrl = "";
            if (string.IsNullOrEmpty(dbFile))
            {
                fileUrl = baseUri + "/" + "%d" + "/" +
                          "%d" + AudioUtils.AUDIO_EXTENSION;
            }
            else
            {
                fileUrl = baseUri + "/" + "%03d" +
                          AudioUtils.AUDIO_EXTENSION;
            }

            var request = new AudioRequest(fileUrl, ayah, qari, baseUri);
            request.SetGaplessDatabaseFilePath(dbFile);
            request.SetPlayBounds(ayah, endAyah);
            mLastAudioDownloadRequest = request;
            PlayAudioRequest(request);
        }

        private void PlayAudioRequest(AudioRequest request)
        {
            if (request == null)
            {
                IsPlayingAudio = false;
                return;
            }

            // seeing if we can play audio request...
            if (!QuranFileUtils.HaveAyaPositionFile())
            {
                string url = QuranFileUtils.GetAyaPositionFileUrl();
                string destination = QuranFileUtils.GetQuranDatabaseDirectory(false);
                // start the download

                // DO THE DOWNLOAD HERE
            }
            else if (AudioUtils.ShouldDownloadGaplessDatabase(request))
            {
                string url = AudioUtils.GetGaplessDatabaseUrl(request);
                string destination = request.GetLocalPath();
                // start the download

                // DO THE DOWNLOAD HERE
            }
            else if (AudioUtils.HaveAllFiles(request))
            {
                if (!AudioUtils.ShouldDownloadBasmallah(request))
                {
                    request.RemovePlayBounds();
                    Play(request);
                    mLastAudioDownloadRequest = null;
                }
                else
                {
                    //should download basmalla...
                    QuranAyah firstAyah = new QuranAyah(1, 1);
                    string url = AudioUtils.GetQariUrl(request.GetQariId(), true);
                    string destination = request.GetLocalPath();

                    //intent.putExtra(QuranDownloadService.EXTRA_START_VERSE, firstAyah);
                    //intent.putExtra(QuranDownloadService.EXTRA_END_VERSE, firstAyah);

                    // DO THE DOWNLOAD HERE
                }
            }
            else
            {
                string qariUrl = AudioUtils.GetQariUrl(request.GetQariId(), true);
                string destination = request.GetLocalPath();

                // start service
                //intent.putExtra(QuranDownloadService.EXTRA_START_VERSE,
                //        request.getMinAyah());
                //intent.putExtra(QuranDownloadService.EXTRA_END_VERSE,
                //        request.getMaxAyah());
                //intent.putExtra(QuranDownloadService.EXTRA_IS_GAPLESS,
                //        request.isGapless());

                // DO THE DOWNLOAD HERE
            }
        }

        private void Play(AudioRequest request)
        {
            // DO THE PLAYBACK

            if (mShouldOverridePlaying)
            {
                // force the current audio to stop and start playing new request
                // STOP PLAYING
                mShouldOverridePlaying = false;
            }
                // just a playback request, so tell audio service to just continue
                // playing (and don't store new audio data) if it was already playing
            else
            {
                // ADD REQUEST TO QUEUE
            }
        }

        public void onPausePressed()
        {
        }

        public void onNextPressed()
        {
        }

        public void onPreviousPressed()
        {
        }

        public void setRepeatCount(int repeatCount)
        {
        }

        public void onStopPressed()
        {
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
            pageModel.ImageSource = QuranFileUtils.GetImageFromWeb(QuranFileUtils.GetPageFileName(pageModel.PageNumber));

            try
            {
                // Set translation
                if (string.IsNullOrEmpty(this.TranslationFile) ||
                    !QuranFileUtils.FileExists(Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false),
                                                            this.TranslationFile)))
                    return false;

                if (!force && pageModel.Translations.Count > 0)
                    return false;

                List<QuranAyah> verses = null;
                using (var db = new DatabaseHandler<QuranAyah>(this.TranslationFile))
                {
                    verses = await new TaskFactory().StartNew(() => db.GetVerses(pageModel.PageNumber));
                }

                List<ArabicAyah> versesArabic = null;
                if (this.ShowArabicInTranslation && QuranFileUtils.FileExists(Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false),
                                                        QuranFileUtils.QURAN_ARABIC_DATABASE)))
                {
                    try
                    {
                        using (var dbArabic = new DatabaseHandler<ArabicAyah>(QuranFileUtils.QURAN_ARABIC_DATABASE))
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

                    var versesSplit = TextBlockSplitter.Instance.Split(verse.Text, SettingsUtils.Get<int>(Constants.PREF_TRANSLATION_TEXT_SIZE), FontWeights.Normal);

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
                    if (e.Message.StartsWith("no such table:", StringComparison.InvariantCultureIgnoreCase))
                    {
                        QuranFileUtils.DeleteFile(Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false, false),
                                                               this.TranslationFile));
                    }
                }
                catch
                {
                    // Do nothing
                }
                pageModel.Translations.Add(new VerseViewModel {Text = "Error loading translation..."});
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