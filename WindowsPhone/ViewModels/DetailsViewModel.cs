using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using QuranPhone.Resources;
using QuranPhone.Data;
using System.Windows.Controls;
using QuranPhone.Utils;
using QuranPhone.UI;

namespace QuranPhone.ViewModels
{
    public class ObservablePages : ObservableDictionary<int, PageViewModel>
    {
    }

    public class DetailsViewModel : ViewModelBase
    {
        private const int PAGES_TO_PRELOAD = 3;

        public DetailsViewModel()
        {
            this.Pages = new ObservablePages();
            CurrentPage = 1;
        }
        public ObservablePages Pages { get; private set; }

        private int currentPage;
        public int CurrentPage
        {
            get { return currentPage; }
            set
            {
                if (value == currentPage)
                    return;

                currentPage = value;
                loadPages();
                base.OnPropertyChanged(() => CurrentPage);
            }
        }

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
        //Load only several pages
        private void loadPages()
        {
            var curPage = CurrentPage;

            for (int i = curPage - PAGES_TO_PRELOAD; i <= curPage + PAGES_TO_PRELOAD; i++)
            {
                var page = (i <= 0 ? Constants.PAGES_LAST + i : i);
                Pages[page] = new PageViewModel(page);
            }
        }
             
        #endregion
    }
}