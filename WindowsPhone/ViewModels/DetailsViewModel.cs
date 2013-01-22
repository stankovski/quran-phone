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
    public class DetailsViewModel : ViewModelBase
    {
        private const int PAGES_TO_PRELOAD = 3;

        public DetailsViewModel()
        {
            this.Pages = new ObservableCollection<PageViewModel>();
            CurrentPage = 0;
        }
        public ObservableCollection<PageViewModel> Pages { get; private set; }
        private int[] loadedPages = new int[Constants.PAGES_LAST];

        private int currentPage;
        public int CurrentPage
        {
            get { return currentPage; }
            set
            {
                if (value == currentPage)
                    return;

                currentPage = value;
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
            if (Pages.Count == 0)
            {
                for (int i = 1; i <= Constants.PAGES_LAST; i++)
                {
                    Pages.Add(new PageViewModel(i));
                }
            }

            this.IsDataLoaded = true;
        }

        #region Private Methods
        //Load only several pages
        private void loadPages()
        {
            var curPage = 1;
            if (Pages.Count > 0 && Pages.Count <= CurrentPage)
                curPage = Pages[CurrentPage].PageNumber;

            for (int i = curPage - PAGES_TO_PRELOAD; i <= curPage + PAGES_TO_PRELOAD; i++)
            {
                var page = (i <= 0 ? Constants.PAGES_LAST + i : i);
                Pages.Add(new PageViewModel(page));
            }
        }
             
        #endregion
    }
}