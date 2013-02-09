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
        protected const int PAGES_TO_PRELOAD = 2;

        public DetailsViewModel()
        {
            this.Pages = new ObservableCollection<PageViewModel>();
        }

        public ObservableCollection<PageViewModel> Pages { get; private set; }

        private int currentPageNumber;
        public int CurrentPageNumber
        {
            get { return currentPageNumber; }
            set
            {
                if (value == currentPageNumber)
                    return;

                currentPageNumber = value;
                base.OnPropertyChanged(() => CurrentPageNumber);
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

        public bool IsDataLoaded
        {
            get;
            protected set;
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public virtual void LoadData()
        {
            this.CurrentPageIndex = PAGES_TO_PRELOAD;
            this.IsDataLoaded = true;
        }

        #region Private Methods
        //Load only several pages
        protected virtual void UpdatePages()
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
            SettingsUtils.Set<int>(Constants.PREF_LAST_PAGE, CurrentPageNumber);

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
            
            Pages[CurrentPageIndex].ImageSource = QuranFileUtils.GetImageFromWeb(QuranFileUtils.GetPageFileName(Pages[CurrentPageIndex].PageNumber));
            Pages[CurrentPageIndex + 1].ImageSource = QuranFileUtils.GetImageFromWeb(QuranFileUtils.GetPageFileName(Pages[CurrentPageIndex + 1].PageNumber));
            Pages[CurrentPageIndex - 1].ImageSource = QuranFileUtils.GetImageFromWeb(QuranFileUtils.GetPageFileName(Pages[CurrentPageIndex - 1].PageNumber));

            Pages[CurrentPageIndex + PAGES_TO_PRELOAD].ImageSource = null;
            Pages[CurrentPageIndex - PAGES_TO_PRELOAD].ImageSource = null;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            foreach (var page in Pages)
            {
                page.Dispose();
            }
            Pages.Clear();
        }
             
        #endregion
    }
}