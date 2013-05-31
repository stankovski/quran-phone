using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using QuranPhone.Common;
using QuranPhone.Resources;
using QuranPhone.Data;
using QuranPhone.Utils;
using System.IO.IsolatedStorage;

namespace QuranPhone.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {
        public SearchViewModel()
        {
            this.SearchResults = new ObservableCollection<ItemViewModel>();
        }

        #region Properties
        public ObservableCollection<ItemViewModel> SearchResults { get; private set; }
        #endregion Properties

        #region Public methods

        public async void Load(string query)
        {
            // Set translation
            if (string.IsNullOrEmpty(App.DetailsViewModel.TranslationFile) ||
                !QuranFileUtils.FileExists(Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false),
                                                        App.DetailsViewModel.TranslationFile)))
            {
                // Tell users to select translation
            }
            else
            {
                List<QuranAyah> verses = null;
                using (var db = new DatabaseHandler(App.DetailsViewModel.TranslationFile))
                {
                    verses = await new TaskFactory().StartNew(() => db.Search(query));
                }
                this.SearchResults.Clear();
                foreach (var verse in verses)
                {
                    this.SearchResults.Add(new ItemViewModel
                        {
                            Id = QuranInfo.GetSuraName(verse.Sura, true),
                            Details = verse.Text,
                            PageNumber = QuranInfo.GetPageFromSuraAyah(verse.Sura, verse.Ayah)
                        });
                }
            }
        }

        #endregion Public methods

        #region Private methods
        
        #endregion
    }
}