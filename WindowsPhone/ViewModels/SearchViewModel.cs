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

        public async void Load(string query)
        {
            // Set translation
            if (string.IsNullOrEmpty(App.DetailsViewModel.TranslationFile) ||
                !QuranFileUtils.FileExists(Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false),
                                                        App.DetailsViewModel.TranslationFile)))
            {
                MessageBox.Show(AppResources.no_translation_to_search);
            }
            else
            {
                IsLoading = true;

                List<QuranAyah> verses = null;
                using (var db = new DatabaseHandler(App.DetailsViewModel.TranslationFile))
                {
                    verses = await new TaskFactory().StartNew(() => db.Search(query));
                }
                this.SearchResults.Clear();
                foreach (var verse in verses)
                {
                    var text = TrimText(verse.Text, MaxPreviewCharacter);
                    this.SearchResults.Add(new ItemViewModel
                        {
                            Id = string.Format("{0} ({1}:{2})", QuranInfo.GetSuraName(verse.Sura, false), verse.Sura, verse.Ayah),
                            Details = text,
                            PageNumber = QuranInfo.GetPageFromSuraAyah(verse.Sura, verse.Ayah)
                        });
                }

                IsLoading = false;
            }
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