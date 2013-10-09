using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using QuranPhone.Common;
using QuranPhone.Data;
using QuranPhone.Resources;
using QuranPhone.UI;
using QuranPhone.Utils;

namespace QuranPhone.ViewModels
{
    public class DetailsViewModel : ViewModelBase
    {
        private static readonly SolidColorBrush _lightColor = new SolidColorBrush(Colors.White);
        private static readonly SolidColorBrush _darkColor = new SolidColorBrush(Colors.Black);

        public DetailsViewModel()
        {
            Pages = new ObservableCollection<PageViewModel>();
            BackgroundColor = (LinearGradientBrush)Application.Current.Resources["LightBackground"];
            ForegroundColor = _darkColor;
            IsShowMenu = false;
        }

        #region Properties

        private LinearGradientBrush backgroundColor;
        private string currentJuzName;
        private int currentPageIndex = -1;
        private int currentPageNumber;
        private string currentSurahName;
        private int currentSurahNumber;
        private SolidColorBrush foregroundColor;
        private bool isNightMode;
        private bool isShowMenu;
        private bool keepInfoOverlay;
        private PageOrientation orientation;
        private QuranAyah selectedAyah;
        private bool showArabicInTranslation;
        private bool showTranslation;
        private string translationFile;
        public ObservableCollection<PageViewModel> Pages { get; private set; }

        public string TranslationFile
        {
            get { return translationFile; }
            set
            {
                if (value == translationFile)
                {
                    return;
                }

                translationFile = value;

                base.OnPropertyChanged(() => TranslationFile);
            }
        }

        public bool ShowTranslation
        {
            get { return showTranslation; }
            set
            {
                if (value == showTranslation)
                {
                    return;
                }

                showTranslation = value;

                base.OnPropertyChanged(() => ShowTranslation);
            }
        }

        public bool ShowArabicInTranslation
        {
            get { return showArabicInTranslation; }
            set
            {
                if (value == showArabicInTranslation)
                {
                    return;
                }

                showArabicInTranslation = value;

                base.OnPropertyChanged(() => ShowArabicInTranslation);
            }
        }

        public int CurrentPageNumber
        {
            get { return currentPageNumber; }
            set
            {
                if (value == currentPageNumber)
                {
                    return;
                }

                CurrentSurahName = QuranInfo.GetSuraNameFromPage(value, false);
                CurrentSurahNumber = QuranInfo.GetSuraNumberFromPage(value);
                int rub = QuranInfo.GetRub3FromPage(value);
                CurrentJuzName = string.Format("{0} {1}{2} {3} {4}", AppResources.quran_juz2,
                    QuranInfo.GetJuzFromPage(value), GetJuzPart(rub), AppResources.quran_rub3, rub);

                currentPageNumber = value;
                base.OnPropertyChanged(() => CurrentPageNumber);
            }
        }

        public string CurrentSurahName
        {
            get { return currentSurahName; }
            set
            {
                if (value == currentSurahName)
                {
                    return;
                }

                currentSurahName = value;

                base.OnPropertyChanged(() => CurrentSurahName);
            }
        }

        public int CurrentSurahNumber
        {
            get { return currentSurahNumber; }
            set
            {
                if (value == currentSurahNumber)
                {
                    return;
                }

                currentSurahNumber = value;

                base.OnPropertyChanged(() => CurrentSurahNumber);
            }
        }

        public string CurrentJuzName
        {
            get { return currentJuzName; }
            set
            {
                if (value == currentJuzName)
                {
                    return;
                }

                currentJuzName = value;

                base.OnPropertyChanged(() => CurrentJuzName);
            }
        }

        public int CurrentPageIndex
        {
            get { return currentPageIndex; }
            set
            {
                if (value == currentPageIndex)
                {
                    return;
                }

                currentPageIndex = value;
                if (value >= 0)
                {
                    UpdatePages();
                }
                base.OnPropertyChanged(() => CurrentPageIndex);
            }
        }

        public PageViewModel CurrentPage
        {
            get
            {
                if (CurrentPageIndex >= 0 && CurrentPageIndex < Pages.Count)
                {
                    return Pages[CurrentPageIndex];
                }
                return null;
            }
        }

        public bool IsDataLoaded { get; protected set; }

        public PageOrientation Orientation
        {
            get { return orientation; }
            set
            {
                if (value == orientation)
                {
                    return;
                }

                orientation = value;

                base.OnPropertyChanged(() => Orientation);

                // directly affect ShowInfoOverlay
                base.OnPropertyChanged(() => ShowInfoOverlay);
            }
        }

        public bool IsShowMenu
        {
            get { return isShowMenu; }
            set
            {
                if (value == isShowMenu)
                {
                    return;
                }

                if (!value && PhoneUtils.IsPortaitOrientation)
                {
                    return;
                }

                isShowMenu = value;

                base.OnPropertyChanged(() => IsShowMenu);
            }
        }

        public bool ShowInfoOverlay
        {
            get
            {
                if (Orientation == PageOrientation.Landscape || Orientation == PageOrientation.LandscapeLeft ||
                    Orientation == PageOrientation.LandscapeRight)
                {
                    return keepInfoOverlay;
                }

                // always show info overlay on portrait
                return true;
            }
        }

        public bool KeepInfoOverlay
        {
            get { return keepInfoOverlay; }
            set
            {
                if (value == keepInfoOverlay)
                {
                    return;
                }

                keepInfoOverlay = value;

                base.OnPropertyChanged(() => KeepInfoOverlay);
                base.OnPropertyChanged(() => ShowInfoOverlay);
            }
        }

        public bool IsNightMode
        {
            get { return isNightMode; }
            set
            {
                if (value == isNightMode)
                {
                    return;
                }

                isNightMode = value;
                UpdateStyles();

                base.OnPropertyChanged(() => IsNightMode);
            }
        }

        public LinearGradientBrush BackgroundColor
        {
            get { return backgroundColor; }
            set
            {
                if (value == backgroundColor)
                {
                    return;
                }

                backgroundColor = value;

                base.OnPropertyChanged(() => BackgroundColor);
            }
        }

        public SolidColorBrush ForegroundColor
        {
            get { return foregroundColor; }
            set
            {
                if (value == foregroundColor)
                {
                    return;
                }

                foregroundColor = value;

                base.OnPropertyChanged(() => ForegroundColor);
            }
        }

        public QuranAyah SelectedAyah
        {
            get { return selectedAyah; }
            set
            {
                if (value == selectedAyah)
                {
                    return;
                }

                selectedAyah = value;

                base.OnPropertyChanged(() => SelectedAyah);
            }
        }

        public bool AyahDetailsExist
        {
            get
            {
                string path = Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false, true),
                    QuranFileUtils.GetAyaPositionFileName());
                if (QuranFileUtils.FileExists(path))
                {
                    return true;
                }
                return false;
            }
        }

        private string GetJuzPart(int rub)
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

        private void UpdateStyles()
        {
            if (IsNightMode)
            {
                BackgroundColor = (LinearGradientBrush)Application.Current.Resources["DarkBackground"];
                ForegroundColor = _lightColor;
            }
            else
            {
                BackgroundColor = (LinearGradientBrush)Application.Current.Resources["LightBackground"];
                ForegroundColor = _darkColor;
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
                for (int page = Constants.LastPage; page >= Constants.FirstPage; page--)
                {
                    Pages.Add(new PageViewModel(page) { ShowTranslation = ShowTranslation });
                }
            }

            CurrentPageIndex = GetIndexFromPageNumber(CurrentPageNumber);
            IsDataLoaded = true;
        }

        public async void UpdatePages()
        {
            CurrentPageNumber = Pages[CurrentPageIndex].PageNumber;
            SettingsUtils.Set(Constants.PrefLastPage, CurrentPageNumber);

            await LoadPage(CurrentPageIndex, false);
        }

        public void ClearPages()
        {
            foreach (PageViewModel page in Pages)
            {
                page.Translations.Clear();
                page.ImageSource = null;
            }
        }

        public bool AddPageBookmark()
        {
            return AddBookmark(null);
        }

        public void AddAyahBookmark(QuranAyah ayah)
        {
            AddBookmark(ayah);
        }

        private bool AddBookmark(QuranAyah ayah)
        {
            try
            {
                using (var adapter = new BookmarksDBAdapter())
                {
                    if (ayah == null)
                    {
                        adapter.AddBookmarkIfNotExists(null, null, CurrentPageNumber);
                    }
                    else
                    {
                        adapter.AddBookmarkIfNotExists(ayah.Sura, ayah.Ayah, CurrentPageNumber);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                MessageBox.Show("error creating bookmark");
                return false;
            }
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            foreach (PageViewModel page in Pages)
            {
                CleanPage(Pages.IndexOf(page));
            }
        }

        #endregion

        #region Private methods

        private void CleanPage(int pageIndex)
        {
            PageViewModel pageModel = Pages[pageIndex];
            pageModel.ImageSource = null;
            pageModel.Translations.Clear();
        }

        private async Task<bool> LoadPage(int pageIndex, bool force)
        {
            PageViewModel pageModel = Pages[pageIndex];
            // Set image
            pageModel.ImageSource = QuranFileUtils.GetImageFromWeb(QuranFileUtils.GetPageFileName(pageModel.PageNumber));

            try
            {
                // Set translation
                if (string.IsNullOrEmpty(TranslationFile) ||
                    !QuranFileUtils.FileExists(Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false),
                        TranslationFile)))
                {
                    return false;
                }

                if (!force && pageModel.Translations.Count > 0)
                {
                    return false;
                }

                List<QuranAyah> verses;
                using (var db = new DatabaseHandler<QuranAyah>(TranslationFile))
                {
                    verses = await new TaskFactory().StartNew(() => db.GetVerses(pageModel.PageNumber));
                }

                List<ArabicAyah> versesArabic = null;
                if (ShowArabicInTranslation &&
                    QuranFileUtils.FileExists(Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false),
                        QuranFileUtils.QuranArabicDatabase)))
                {
                    try
                    {
                        using (var dbArabic = new DatabaseHandler<ArabicAyah>(QuranFileUtils.QuranArabicDatabase))
                        {
                            versesArabic =
                                await new TaskFactory().StartNew(() => dbArabic.GetVerses(pageModel.PageNumber));
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }

                int tempSurah = -1;
                pageModel.Translations.Clear();
                for (int i = 0; i < verses.Count; i++)
                {
                    QuranAyah verse = verses[i];
                    if (verse.Sura != tempSurah)
                    {
                        pageModel.Translations.Add(new VerseViewModel(QuranInfo.GetSuraName(verse.Sura, true),
                            "TranslationViewHeader"));
                        tempSurah = verse.Sura;
                    }

                    pageModel.Translations.Add(new VerseViewModel(string.Format("{0}:{1}", verse.Sura, verse.Ayah),
                        "BoldText"));

                    if (versesArabic != null)
                    {
                        pageModel.Translations.Add(new VerseViewModel(versesArabic[i].Text, "ArabicText", verse.Sura,
                            verse.Ayah));
                    }

                    IList<string> versesSplit = TextBlockSplitter.Instance.Split(verse.Text,
                        SettingsUtils.Get<int>(Constants.PrefTranslationTextSize), FontWeights.Normal);

                    foreach (string vs in versesSplit)
                    {
                        pageModel.Translations.Add(new VerseViewModel(vs, null, verse.Sura, verse.Ayah));
                    }
                }
                pageModel.Translations.Add(new VerseViewModel(" "));
            }
            catch (Exception e)
            {
                try
                {
                    if (e.Message.StartsWith("no such table:", StringComparison.InvariantCultureIgnoreCase))
                    {
                        QuranFileUtils.DeleteFile(Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false, false),
                            TranslationFile));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                pageModel.Translations.Add(new VerseViewModel { Text = "Error loading translation..." });
            }
            return true;
        }

        private int GetIndexFromPageNumber(int number)
        {
            return Constants.LastPage - number;
        }

        #endregion Private helper methods
    }
}