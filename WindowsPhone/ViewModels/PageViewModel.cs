using System;
using System.Collections.ObjectModel;
using System.Windows;
using QuranPhone.Data;

namespace QuranPhone.ViewModels
{
    public class PageViewModel : ViewModelBase
    {
        public PageViewModel()
        {
            Translations = new ObservableCollection<VerseViewModel>();
        }

        public PageViewModel(int page) : this()
        {
            PageNumber = page;
        }

        #region Properties

        private Uri _imageSource;
        private int _pageNumber;
        private bool _showTranslation;
        private string _translation;
        public ObservableCollection<VerseViewModel> Translations { get; set; }

        public string Translation
        {
            get { return _translation; }
            set
            {
                _translation = value;
                base.OnPropertyChanged(() => Translation);
            }
        }

        public int PageNumber
        {
            get { return _pageNumber; }
            set
            {
                _pageNumber = value;
                base.OnPropertyChanged(() => PageNumber);
            }
        }

        public bool ShowTranslation
        {
            get { return _showTranslation; }
            set
            {
                _showTranslation = value;
                base.OnPropertyChanged(() => ShowTranslation);
            }
        }

        public Uri ImageSource
        {
            get { return _imageSource; }
            set
            {
                _imageSource = value;
                base.OnPropertyChanged(() => ImageSource);
            }
        }

        public double ScreenWidth
        {
            get { return Application.Current.Host.Content.ActualWidth - 20; }
        }

        public String SuraName
        {
            get { return QuranInfo.GetSuraNameFromPage(PageNumber); }
        }

        public String JuzName
        {
            get { return QuranInfo.GetJuzString(PageNumber); }
        }

        #endregion Properties

        protected override void OnDispose()
        {
            base.OnDispose();
            ImageSource = null;
        }
    }
}