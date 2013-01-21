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
            QuranScreenInfo screen = QuranScreenInfo.GetInstance();
        }

        public PageViewModel(int page)
            : this()
        {
            PageNumber = page;
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
    }
}