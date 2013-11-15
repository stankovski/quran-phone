// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the PageViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using Quran.Core.Data;
using Quran.Core.Utils;
using Quran.Core.Data;

namespace Quran.Core.ViewModels
{
    using System.Windows.Input;

    using Cirrious.MvvmCross.ViewModels;

    /// <summary>
    /// Define the PageViewModel type.
    /// </summary>
    public class PageViewModel : BaseViewModel
    {
        public PageViewModel()
        {
            Translations = new ObservableCollection<VerseViewModel>();
        }

        public PageViewModel(int page)
            : this()
        {
            PageNumber = page;
        }

        #region Properties

        public ObservableCollection<VerseViewModel> Translations { get; set; }

        private string translation;
        public string Translation
        {
            get { return translation; }
            set
            {
                if (value == translation)
                    return;

                translation = value;

                base.RaisePropertyChanged(() => Translation);
            }
        }

        private int pageNumber;
        public int PageNumber
        {
            get { return pageNumber; }
            set
            {
                if (value == pageNumber)
                    return;

                pageNumber = value;

                base.RaisePropertyChanged(() => PageNumber);
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

                base.RaisePropertyChanged(() => ShowTranslation);
            }
        }

        private Uri imageSource;
        public Uri ImageSource
        {
            get { return imageSource; }
            set
            {
                if (value == imageSource)
                    return;

                imageSource = value;

                base.RaisePropertyChanged(() => ImageSource);
            }
        }

        public double ScreenWidth
        {
            get { return ScreenUtils.Instance.Width - 20; }
        }

        // QuranUtils Properties
        public String SuraName
        {
            get { return QuranUtils.GetSuraNameFromPage(PageNumber); }
        }

        public String JuzName
        {
            get { return QuranUtils.GetJuzString(PageNumber); }
        }

        #endregion Properties

        protected override void OnDispose()
        {
            base.OnDispose();
            ImageSource = null;
        }
    }
}
