﻿using QuranPhone.Common;
using QuranPhone.Data;
using System;
using System.Collections.ObjectModel;

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

        private QuranAyah selectedAyah;
        public QuranAyah SelectedAyah
        {
            get { return selectedAyah; }
            set
            {
                if (value == selectedAyah)
                    return;

                selectedAyah = value;

                base.OnPropertyChanged(() => SelectedAyah);
            }
        }
        
        public double ScreenWidth
        {
            get { return System.Windows.Application.Current.Host.Content.ActualWidth - 20; }
        }

        // QuranInfo Properties
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