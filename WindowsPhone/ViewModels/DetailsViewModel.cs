using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using QuranPhone.Common;
using QuranPhone.Data;
using QuranPhone.Utils;
using QuranPhone.UI;
using Microsoft.Phone.Controls;

namespace QuranPhone.ViewModels
{
    public class DetailsViewModel : ViewModelBase
    {
        public DetailsViewModel()
        {
            Pages = new ObservableCollection<PageViewModel>();

            // default detail page to full screen mode.
            IsShowMenu = false;

            cmdToggleMenu = new RelayCommand(ToggleMenu);
        }

        #region Commands
        /// <summary>
        /// Toggle whether to show menu / hide it in detail page
        /// </summary>
        public void ToggleMenu()
        {
            ToggleMenu(null);
        }

        public void ToggleMenu(object obj)
        {
            IsShowMenu = !IsShowMenu;
        }
        public ICommand CommandToggleMenu { get { return cmdToggleMenu; } }
        private RelayCommand cmdToggleMenu;
        #endregion Commands

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
                CurrentJuzName = QuranInfo.GetJuzTitle() + " " + QuranInfo.GetJuzFromPage(value);

                currentPageNumber = value;
                base.OnPropertyChanged(() => CurrentPageNumber);
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

        private int currentPageIndex;
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

        private int textSize;
        public int TextSize
        {
            get { return textSize; }
            set
            {
                if (value == textSize)
                    return;

                textSize = value;

                base.OnPropertyChanged(() => TextSize);
            }
        }

        private int arabicTextSize;
        public int ArabicTextSize
        {
            get { return arabicTextSize; }
            set
            {
                if (value == arabicTextSize)
                    return;

                arabicTextSize = value;

                base.OnPropertyChanged(() => ArabicTextSize);
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

            //Set text size
            this.TextSize = SettingsUtils.Get<int>(Constants.PREF_TRANSLATION_TEXT_SIZE);
            this.ArabicTextSize = SettingsUtils.Get<int>(Constants.PREF_ARABIC_TEXT_SIZE);
            
            this.CurrentPageIndex = getIndexFromPageNumber(this.CurrentPageNumber);
            this.IsDataLoaded = true;
        }

        public async void UpdatePages()
        {
            CurrentPageNumber = Pages[CurrentPageIndex].PageNumber;
            SettingsUtils.Set<int>(Constants.PREF_LAST_PAGE, CurrentPageNumber);

            await loadPage(CurrentPageIndex, false);
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
                using (var db = new DatabaseHandler(this.TranslationFile))
                {
                    verses = await new TaskFactory().StartNew(() => db.GetVerses(pageModel.PageNumber));
                }

                List<QuranAyah> versesArabic = null;
                if (this.ShowArabicInTranslation && QuranFileUtils.FileExists(Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false),
                                                        QuranFileUtils.QURAN_ARABIC_DATABASE)))
                {
                    try
                    {
                        using (var dbArabic = new DatabaseHandler(QuranFileUtils.QURAN_ARABIC_DATABASE))
                        {
                            versesArabic = await new TaskFactory().StartNew(() => dbArabic.GetVerses(pageModel.PageNumber, "arabic_text"));
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
                            IsTitle = true,
                            Text = QuranInfo.GetSuraName(verse.Sura, true)
                        });

                        tempSurah = verse.Sura;
                    }

                    var vvm = new VerseViewModel
                        {
                            IsTitle = false,
                            VerseNumber = verse.Ayah,
                            SurahNumber = verse.Sura,
                            Text = verse.Text
                        };

                    if (versesArabic != null)
                    {
                        vvm.QuranText = versesArabic[i].Text;
                        vvm.QuranTextExists = true;
                    }

                    pageModel.Translations.Add(vvm);
                }
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