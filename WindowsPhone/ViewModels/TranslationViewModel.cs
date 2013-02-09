using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using QuranPhone.Resources;
using QuranPhone.Data;
using System.Windows.Controls;
using QuranPhone.Utils;
using QuranPhone.UI;
using QuranPhone.Common;
using System.Collections.Generic;

namespace QuranPhone.ViewModels
{
    public class TranslationViewModel : DetailsViewModel
    {
        private Dictionary<string, DatabaseHandler> translationDatabases = new Dictionary<string, DatabaseHandler>();

        private string translationFile;
        public string TranslationFile
        {
            get { return translationFile; }
            set
            {
                if (value == translationFile)
                    return;

                translationFile = value;
                if (!translationDatabases.ContainsKey(translationFile))
                    translationDatabases[translationFile] = new DatabaseHandler(translationFile);
                base.OnPropertyChanged(() => TranslationFile);
            }
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public override void LoadData()
        {
            this.CurrentPageIndex = PAGES_TO_PRELOAD;
            this.IsDataLoaded = true;
        }

        #region Private Methods
        //Load only several pages
        protected override void UpdatePages()
        {
            if (Pages.Count == 0)
            {
                for (int i = CurrentPageNumber - PAGES_TO_PRELOAD; i <= CurrentPageNumber + PAGES_TO_PRELOAD; i++)
                {
                    var page = (i <= 0 ? Constants.PAGES_LAST + i : i);
                    Pages.Add(new PageViewModel(page));
                }
            }

            CurrentPageNumber = Pages[CurrentPageIndex].PageNumber;

            if (CurrentPageIndex == PAGES_TO_PRELOAD - 1)
            {
                var firstPage = Pages[0].PageNumber;
                var newPage = (firstPage - 1 <= 0 ? Constants.PAGES_LAST + firstPage - 1 : firstPage - 1);
                Pages.Insert(0, new PageViewModel(newPage));
                CurrentPageIndex++;
            }
            else if (CurrentPageIndex == Pages.Count - PAGES_TO_PRELOAD)
            {
                var lastPage = Pages[Pages.Count - 1].PageNumber;
                var newPage = (lastPage + 1 >= Constants.PAGES_LAST ? Constants.PAGES_LAST - lastPage - 1 : lastPage + 1);
                Pages.Add(new PageViewModel(newPage));
            }

            populatePage(CurrentPageIndex, false);
            populatePage(CurrentPageIndex + 1, false);
            populatePage(CurrentPageIndex - 1, false);
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            foreach (var db in translationDatabases.Keys)
            {
                translationDatabases[db].Dispose();
            }
            translationDatabases.Clear();
        }

        private void populatePage(int pageIndex, bool force)
        {
            var pageModel = Pages[pageIndex];
            if (!force && pageModel.Verses.Count > 0)
                return;

            pageModel.Verses.Clear();
            var db = translationDatabases[this.TranslationFile];
            List<QuranAyah> verses = db.GetVerses(pageModel.PageNumber);
            List<QuranAyah> versesArabic = null;

            //if (SettingsUtils.Get<bool>(Constants.PREF_SHOW_ARABIC_IN_TRANSLATION)) 
            //{
            //    try
            //    {
            //        DatabaseHandler dbArabic = new ArabicDatabaseHandler();
            //        versesArabic = dbArabic.GetVerses(pageModel.PageNumber);
            //    }
            //    catch
            //    {
            //        //Not able to get Arabic text - skipping
            //    }
            //}

            int tempSurah = -1;
            for (int i =0; i < verses.Count; i++)
            {
                var verse = verses[i];
                if (verse.Sura != tempSurah)
                {
                    pageModel.Verses.Add(new VerseViewModel { IsTitle = true, Text = QuranInfo.GetSuraName(verse.Sura, true) });
                    tempSurah = verse.Sura;
                }
                var vvm = new VerseViewModel { IsTitle = false, VerseNumber = verse.Ayah, SurahNumber = verse.Sura, Text = verse.Text };
                if (versesArabic != null && i < versesArabic.Count)
                    vvm.QuranText = versesArabic[i].Text;
                pageModel.Verses.Add(new VerseViewModel { IsTitle = false, VerseNumber = verse.Ayah, SurahNumber = verse.Sura, Text = verse.Text });
            }
        }

        #endregion
    }
}