// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the PageViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Quran.Core.Utils;

namespace Quran.Core.ViewModels
{
    /// <summary>
    /// Define the PageViewModel type.
    /// </summary>
    public class PageViewModel : BaseViewModel
    {
        public PageViewModel()
        {
            Translations = new ObservableCollection<VerseViewModel>();
        }

        public PageViewModel(int page, DetailsViewModel parent)
            : this()
        {
            PageNumber = page;
            Parent = parent;
        }

        #region Properties

        public DetailsViewModel Parent { get; set; }

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

                base.OnPropertyChanged(() => Translation);
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

                base.OnPropertyChanged(() => PageNumber);
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

                base.OnPropertyChanged(() => ShowTranslation);
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

                base.OnPropertyChanged(() => ImageSource);
            }
        }

        public double ScreenWidth
        {
            get { return ScreenUtils.Instance.Width - 20; }
        }

        // QuranUtils Properties
        public String SuraName
        {
            get { return QuranUtils.GetSurahNameFromPage(PageNumber); }
        }

        public String JuzName
        {
            get { return QuranUtils.GetJuzString(PageNumber); }
        }

        #endregion Properties
        public override Task Initialize()
        {
            return Task.FromResult(0);
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            ImageSource = null;
        }
    }
}
