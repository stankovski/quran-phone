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
    public partial class DetailsViewModel : ViewModelWithDownload
    {
        private static string lightColor;
        private static string darkColor;

        public DetailsViewModel()
        {
            Pages = new ObservableCollection<PageViewModel>();
            CreatePages();
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

        public int CurrentPageNumber
        {
            get
            {
                return GetPageNumberFromIndex(CurrentPageIndex);
            }
            set
            {
                CurrentPageIndex = GetIndexFromPageNumber(value);
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

        private int currentPageIndex = Constants.PAGES_LAST - 1;
        public int CurrentPageIndex
        {
            get { return currentPageIndex; }
            set
            {
                if (value == currentPageIndex)
                    return;

                currentPageIndex = value;
                
                var pageNumber = GetPageNumberFromIndex(value);

                SettingsUtils.Set<int>(Constants.PREF_LAST_PAGE, pageNumber);

                CurrentSurahName = QuranUtils.GetSurahNameFromPage(pageNumber, false);
                CurrentSurahNumber = QuranUtils.GetSurahNumberFromPage(pageNumber);
                var rub = QuranUtils.GetRub3FromPage(pageNumber);
                CurrentJuzName = string.Format("{0} {1}{2} {3} {4}", QuranUtils.GetJuzTitle(),
                                               QuranUtils.GetJuzFromPage(pageNumber),
                                               getJuzPart(rub), AppResources.quran_rub3, rub);

                if (value >= 0)
                {
                    RefreshCurrentPage();
                }

                base.RaisePropertyChanged(() => CurrentPageIndex);
                base.RaisePropertyChanged(() => CurrentPageNumber);
                base.RaisePropertyChanged(() => CurrentPage);
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
            set
            {
                if (value != null)
                {
                    CurrentPageIndex = GetIndexFromPageNumber(value.PageNumber);
                }
            }
        }

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

        #endregion Properties

        #region Public methods

        public async Task RefreshCurrentPage()
        {
            await LoadPage(CurrentPage, false);
        }

        public void ClearPages()
        {
            foreach (var page in Pages)
            {
                CleanPage(page);
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
                if (FileUtils.HaveArabicSearchFile())
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

        public async Task<bool> DownloadAyahPositionFile()
        {
            if (!FileUtils.HaveAyaPositionFile())
            {
                string url = FileUtils.GetAyaPositionFileUrl();
                string destination = FileUtils.GetQuranDatabaseDirectory(false);
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
            if (!FileUtils.HaveArabicSearchFile())
            {
                string url = FileUtils.GetArabicSearchUrl();
                string destination = FileUtils.GetQuranDatabaseDirectory(false);
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
            ClearPages();
        }

        public override void SyncViewModelWithSettings()
        {
            RepeatAudio = SettingsUtils.Get<bool>(Constants.PREF_AUDIO_REPEAT);
            IsNightMode = SettingsUtils.Get<bool>(Constants.PREF_NIGHT_MODE);
            KeepInfoOverlay = SettingsUtils.Get<bool>(Constants.PREF_KEEP_INFO_OVERLAY);
            //Update translations
            var translation = SettingsUtils.Get<string>(Constants.PREF_ACTIVE_TRANSLATION);
            if (!string.IsNullOrEmpty(translation))
            {
                if (Pages != null &&
                    (TranslationFile != translation.Split('|')[0] ||
                    ShowTranslation != SettingsUtils.Get<bool>(Constants.PREF_SHOW_TRANSLATION) ||
                    ShowArabicInTranslation != SettingsUtils.Get<bool>(Constants.PREF_SHOW_ARABIC_IN_TRANSLATION)))
                {
                    Pages.Clear();
                }
                TranslationFile = translation.Split('|')[0];
                ShowTranslation = SettingsUtils.Get<bool>(Constants.PREF_SHOW_TRANSLATION);
                ShowArabicInTranslation = SettingsUtils.Get<bool>(Constants.PREF_SHOW_ARABIC_IN_TRANSLATION);
            }
            else
            {
                TranslationFile = null;
                ShowTranslation = false;
                ShowArabicInTranslation = false;
            }
        }
        #endregion

        #region Private helper methods
        /// <summary>
        ///     Creates and adds ItemViewModel objects into the Items collection.
        /// </summary>
        private void CreatePages()
        {
            if (Pages.Count == 0)
            {
                for (int page = Constants.PAGES_LAST; page >= Constants.PAGES_FIRST; page--)
                {
                    Pages.Add(new PageViewModel(page) { ShowTranslation = this.ShowTranslation });
                }
            }
        }

        private void CleanPage(PageViewModel pageModel)
        {
            pageModel.ImageSource = null;
            pageModel.Translations.Clear();
        }

        private async Task LoadPage(PageViewModel pageModel, bool force)
        {
            if (pageModel == null)
            {
                return;
            }

            // Set image
            pageModel.ImageSource = FileUtils.GetImageFromWeb(FileUtils.GetPageFileName(pageModel.PageNumber));

            try
            {
                // Set translation
                if (string.IsNullOrEmpty(this.TranslationFile) ||
                    !FileUtils.FileExists(PathHelper.Combine(FileUtils.GetQuranDatabaseDirectory(false),
                                                            this.TranslationFile)))
                    return;

                if (!force && pageModel.Translations.Count > 0)
                    return;

                List<QuranAyah> verses = null;
                using (var db = new QuranDatabaseHandler<QuranAyah>(this.TranslationFile))
                {
                    verses = await new TaskFactory().StartNew(() => db.GetVerses(pageModel.PageNumber));
                }

                List<ArabicAyah> versesArabic = null;
                if (this.ShowArabicInTranslation && FileUtils.HaveArabicSearchFile())
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

                    var versesSplit = QuranApp.NativeProvider.SplitLongText(verse.Text, SettingsUtils.Get<int>(Constants.PREF_TRANSLATION_TEXT_SIZE), "Normal");

                    foreach (var vs in versesSplit)
                    {
                        pageModel.Translations.Add(new VerseViewModel(vs, null, verse.Surah, verse.Ayah));
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
            return;
        }

        private int GetIndexFromPageNumber(int number)
        {
            var index = Constants.PAGES_LAST - number;
            if (index < 0 || index > Constants.PAGES_LAST - 1)
                return Constants.PAGES_LAST - 1;
            else
                return index;
        }

        private int GetPageNumberFromIndex(int index)
        {
            var page = Constants.PAGES_LAST - index;
            if (page < Constants.PAGES_FIRST || page > Constants.PAGES_LAST)
                return 0;
            else
                return page;
        }
        #endregion Private helper methods
    }
}
