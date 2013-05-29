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
            IsShowInfoPanel = !IsShowInfoPanel;
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

                changePageShowTranslations();
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

                CurrentSurahName = QuranInfo.GetSuraNameFromPage(value, true);
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

                // directly affect IShowInfoPanel
                base.OnPropertyChanged(() => IsShowInfoPanel);
            }
        }

        private bool isShowInfoPanel;
        public bool IsShowInfoPanel 
        {
            get { return isShowInfoPanel && 
                    (orientation == PageOrientation.PortraitUp || orientation == PageOrientation.PortraitDown); }
            set
            {
                if (value == isShowInfoPanel)
                    return;

                isShowInfoPanel = value;
                base.OnPropertyChanged(() => IsShowInfoPanel);
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
            var textSize = SettingsUtils.Get<int>(Constants.PREF_TRANSLATION_TEXT_SIZE);
            foreach (var pageViewModel in Pages)
            {
                pageViewModel.TextSize = textSize;
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
        private void changePageShowTranslations()
        {
            foreach (var page in Pages)
            {
                page.ShowTranslation = this.ShowTranslation;
            }
        }

        private void cleanPage(int pageIndex)
        {
            var pageModel = Pages[pageIndex];
            pageModel.ImageSource = null;
            pageModel.Translation = string.Empty;
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

                if (!force && !string.IsNullOrEmpty(pageModel.Translation))
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
                StringBuilder translationBuilder = new StringBuilder();
                for (int i = 0; i < verses.Count; i++)
                {
                    var verse = verses[i];
                    if (verse.Sura != tempSurah)
                    {
                        translationBuilder.AppendLine(string.Format("h:{0}",
                                                                    QuranInfo.GetSuraName(verse.Sura, true)));
                        tempSurah = verse.Sura;
                    }
                    var vvm = new VerseViewModel
                        {
                            IsTitle = false,
                            VerseNumber = verse.Ayah,
                            SurahNumber = verse.Sura,
                            Text = verse.Text
                        };

                    translationBuilder.AppendLine(string.Format("b:{0}:{1}", verse.Sura, verse.Ayah));
                    if (versesArabic != null && i < versesArabic.Count)
                        translationBuilder.AppendLine(string.Format("a:{0}", versesArabic[i].Text));
                    
                    translationBuilder.AppendLine(verse.Text);
                }
                pageModel.Translation = translationBuilder.ToString();
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
                pageModel.Translation = "Error loading translation...";
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