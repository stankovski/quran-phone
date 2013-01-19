using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using QuranPhone.Resources;
using QuranPhone.Data;
using System.Windows.Controls;
using QuranPhone.Utils;

namespace QuranPhone.ViewModels
{
    public class DetailsViewModel : ViewModelBase
    {
        public DetailsViewModel()
        {
            this.Pages = new ObservableCollection<PageViewModel>();
        }
        public ObservableCollection<PageViewModel> Pages { get; private set; }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            // Sample data; replace with real data
            loadPages();

            this.IsDataLoaded = true;
        }

        #region Private Methods
        private void loadPages()
        {
            for (int i = 1; i < 10; i++)
            {
                Pages.Add(new PageViewModel
                {
                    ImageSource =
                        QuranFileUtils.GetImageFromWeb(QuranFileUtils.GetPageFileName(i))
                });
            }
        }
                
        #endregion
    }
}