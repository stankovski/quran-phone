using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using QuranPhone.Common;
using QuranPhone.Data;
using QuranPhone.Resources;
using QuranPhone.Utils;

namespace QuranPhone.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {
        private const int MaxPreviewCharacter = 200;

        public SearchViewModel()
        {
            SearchResults = new ObservableCollection<ItemViewModel>();
        }

        #region Properties

        private string _query;
        public ObservableCollection<ItemViewModel> SearchResults { get; private set; }

        public string Query
        {
            get { return _query; }
            set
            {
                _query = value;
                base.OnPropertyChanged(() => Query);
            }
        }

        #endregion Properties

        #region Public methods

        public async void Load(string query)
        {
            if ((string.IsNullOrEmpty(App.DetailsViewModel.TranslationFile) ||
                 !QuranFileUtils.FileExists(Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false),
                     App.DetailsViewModel.TranslationFile))) &&
                !QuranFileUtils.FileExists(Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false),
                    QuranFileUtils.QuranArabicDatabase)))
            {
                MessageBox.Show(AppResources.no_translation_to_search);
            }
            else
            {
                IsLoading = true;
                try
                {
                    var translationVerses = new List<QuranAyah>();
                    var arabicVerses = new List<ArabicAyah>();
                    var taskFactory = new TaskFactory();

                    if (App.DetailsViewModel.TranslationFile != null &&
                        QuranFileUtils.FileExists(Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false),
                            App.DetailsViewModel.TranslationFile)))
                    {
                        using (var db = new DatabaseHandler<QuranAyah>(App.DetailsViewModel.TranslationFile))
                        {
                            translationVerses = await taskFactory.StartNew(() => db.Search(query));
                        }
                    }
                    if (
                        QuranFileUtils.FileExists(Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false),
                            QuranFileUtils.QuranArabicDatabase)))
                    {
                        using (var dbArabic = new DatabaseHandler<ArabicAyah>(QuranFileUtils.QuranArabicDatabase))
                        {
                            arabicVerses = await taskFactory.StartNew(() => dbArabic.Search(query));
                        }
                    }
                    SearchResults.Clear();

                    // Merging 2 results
                    int a = 0;
                    int t = 0;
                    var arabicVerse = new QuranAyah {Sura = int.MaxValue, Ayah = int.MaxValue};
                    var translationVerse = new QuranAyah {Sura = int.MaxValue, Ayah = int.MaxValue};
                    var verseToDisplay = new QuranAyah();
                    var comparer = new AyahComparer();

                    while (a < arabicVerses.Count || t < translationVerses.Count)
                    {
                        if (a < arabicVerses.Count)
                        {
                            arabicVerse = arabicVerses[a];
                        }
                        else
                        {
                            arabicVerse = new QuranAyah {Sura = int.MaxValue, Ayah = int.MaxValue};
                        }

                        if (t < translationVerses.Count)
                        {
                            translationVerse = translationVerses[t];
                        }
                        else
                        {
                            translationVerse = new QuranAyah {Sura = int.MaxValue, Ayah = int.MaxValue};
                        }

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

                        QuranAyah verse = verseToDisplay;
                        string text = TrimText(verse.Text, MaxPreviewCharacter);
                        SearchResults.Add(new ItemViewModel
                        {
                            Id =
                                string.Format("{0} ({1}:{2})", QuranInfo.GetSuraName(verse.Sura, false), verse.Sura,
                                    verse.Ayah),
                            Details = text,
                            PageNumber = QuranInfo.GetPageFromSuraAyah(verse.Sura, verse.Ayah),
                            SelectedAyah = new QuranAyah(verse.Sura, verse.Ayah)
                        });
                    }
                }
                catch
                {
                    SearchResults.Add(new ItemViewModel
                    {
                        Id = "Error",
                        Details = "Error performing translation",
                        PageNumber = 1,
                        SelectedAyah = new QuranAyah()
                    });
                }

                IsLoading = false;
            }
        }

        private string TrimText(string text, int maxPreviewCharacter)
        {
            if (text.Length <= maxPreviewCharacter)
            {
                return text;
            }

            for (int i = maxPreviewCharacter - 1; i >= 0; i--)
            {
                if (text[i] == ' ')
                {
                    return string.Format("{0}...", text.Substring(0, i));
                }
            }

            return string.Format("{0}...", text.Substring(0, maxPreviewCharacter - 1));
        }

        #endregion Public methods
    }
}