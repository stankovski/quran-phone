using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using QuranPhone.Resources;
using QuranPhone.Data;
using System.Windows.Controls;
using QuranPhone.Utils;

namespace QuranPhone.ViewModels
{
    public class PageViewModel : ViewModelBase
    {
        public PageViewModel()
        {
            Verses = new ObservableCollection<VerseViewModel>();
        }

        public PageViewModel(int page)
            : this()
        {
            PageNumber = page;
            TextSize = SettingsUtils.Get<int>(Constants.PREF_TRANSLATION_TEXT_SIZE);
        }

        #region Properties
        public ObservableCollection<VerseViewModel> Verses { get; private set; }

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

        private int textSize;
        public int TextSize
        {
            get { return textSize; }
            set
            {
                if (value == textSize)
                    return;

                textSize = value;

                base.OnPropertyChanged(() => TextSize);
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

        #endregion Properties

        protected override void OnDispose()
        {
            base.OnDispose();
            this.ImageSource = null;
            this.Verses.Clear();
        }
    }
}