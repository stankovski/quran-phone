// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the SearchViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Quran.Core.Data;
using Quran.Core.Properties;
using Quran.Core.Utils;
using Quran.Core.Common;
using System.IO;

namespace Quran.Core.ViewModels
{
    /// <summary>
    /// Define the SearchViewModel type.
    /// </summary>
    public class SearchViewModel : ViewModelWithDownload
    {
        private const int MaxPreviewCharacter = 200;
        public SearchViewModel()
        {
            this.SearchResults = new ObservableCollection<ItemViewModel>();
        }

        #region Properties
        public ObservableCollection<ItemViewModel> SearchResults { get; private set; }

        private string query;
        public string Query
        {
            get { return query; }
            set
            {
                if (value == query)
                    return;

                query = value;

                base.OnPropertyChanged(() => Query);
            }
        }
        #endregion Properties

        #region Public methods
        public override Task Initialize()
        {
            return Refresh();
        }

        public override Task Refresh()
        {
            return Task.FromResult(0);
        }

        public async void Load(string query)
        {
            if (this.IsLoading)
            {
                return;
            }

            this.IsLoading = true;
            var translation = SettingsUtils.Get<string>(Constants.PREF_ACTIVE_TRANSLATION);
            if (!string.IsNullOrEmpty(translation))
            {
                var translationFile = translation.Split('|')[0];

                try
                {
                    var translationVerses = new List<QuranAyah>();
                    var arabicVerses = new List<ArabicAyah>();
                    var taskFactory = new TaskFactory();

                    if (await FileUtils.FileExists(FileUtils.DatabaseFolder, translationFile))
                    {
                        using (var db = new QuranDatabaseHandler<QuranAyah>(Path.Combine(FileUtils.GetQuranDatabaseDirectory(), translationFile)))
                        {
                            translationVerses = await taskFactory.StartNew(() => db.Search(query));
                        }
                    }
                    using (var dbArabic = new QuranDatabaseHandler<ArabicAyah>(FileUtils.ArabicDatabase))
                    {
                        arabicVerses = await taskFactory.StartNew(() => dbArabic.Search(query));
                    }
                    this.SearchResults.Clear();

                    // Merging 2 results
                    int a = 0;
                    int t = 0;
                    var arabicVerse = new QuranAyah { Surah = int.MaxValue, Ayah = int.MaxValue };
                    var translationVerse = new QuranAyah { Surah = int.MaxValue, Ayah = int.MaxValue };
                    var verseToDisplay = new QuranAyah();
                    var comparer = new AyahComparer();

                    while (a < arabicVerses.Count || t < translationVerses.Count)
                    {
                        if (a < arabicVerses.Count)
                            arabicVerse = arabicVerses[a];
                        else
                            arabicVerse = new QuranAyah { Surah = int.MaxValue, Ayah = int.MaxValue };

                        if (t < translationVerses.Count)
                            translationVerse = translationVerses[t];
                        else
                            translationVerse = new QuranAyah { Surah = int.MaxValue, Ayah = int.MaxValue };

                        if (comparer.Compare(arabicVerse, translationVerse) > 0)
                        {
                            verseToDisplay = translationVerse;
                            t++;
                        }
                        else if (comparer.Compare(arabicVerse, translationVerse) < 0)
                        {
                            verseToDisplay = arabicVerse;
                            a++;
                        }
                        else if (comparer.Compare(arabicVerse, translationVerse) == 0)
                        {
                            verseToDisplay = arabicVerse;
                            a++;
                            t++;
                        }

                        var verse = verseToDisplay;
                        var text = TrimText(verse.Text, MaxPreviewCharacter);
                        this.SearchResults.Add(new ItemViewModel
                        {
                            Id =
                                string.Format("{0} ({1}:{2})", QuranUtils.GetSurahName(verse.Surah, false), verse.Surah,
                                              verse.Ayah),
                            Details = text,
                            PageNumber = QuranUtils.GetPageFromAyah(verse.Surah, verse.Ayah),
                            SelectedAyah = new QuranAyah(verse.Surah, verse.Ayah)
                        });
                    }
                    return;
                }
                catch (Exception ex)
                {
                    telemetry.TrackException(ex, new Dictionary<string, string> { { "Scenario", "LoadingTranslations" } });
                    this.SearchResults.Add(new ItemViewModel
                    {
                        Id = "Error",
                        Details = "Error performing translation",
                        PageNumber = 1,
                        SelectedAyah = new QuranAyah()
                    });
                }
                finally
                {
                    IsLoading = false;
                }
            }
            await QuranApp.NativeProvider.ShowInfoMessageBox(Resources.no_translation_to_search);
        }

        private string TrimText(string text, int maxPreviewCharacter)
        {
            if (text.Length <= MaxPreviewCharacter)
            {
                return text;
            }
            else
            {
                for (int i = MaxPreviewCharacter - 1; i >= 0; i--)
                {
                    if (text[i] == ' ')
                    {
                        return string.Format("{0}...", text.Substring(0, i));
                    }
                }
                return string.Format("{0}...", text.Substring(0, maxPreviewCharacter - 1));
            }
        }

        #endregion Public methods

        #region Private methods

        #endregion
    }
}
