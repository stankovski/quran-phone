using System;
using QuranPhone.Common;

namespace QuranPhone.ViewModels
{
    public enum ItemViewModelType
    {
        Unknown,
        Sura,
        Juz,
        Bookmark,
        Header,
        Tag
    }

    public class ItemViewModel : ViewModelBase
    {
        #region Properties

        private string _details;
        private string _group;
        private string _id;
        private Uri _image;
        private ItemViewModelType _itemType;
        private int _pageNumber;
        private QuranAyah _selectedAyah;
        private string _style;
        private string _title;

        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                base.OnPropertyChanged(() => Id);
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                base.OnPropertyChanged(() => Title);
            }
        }

        public string Details
        {
            get { return _details; }
            set
            {
                _details = value;
                base.OnPropertyChanged(() => Details);
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

        public QuranAyah SelectedAyah
        {
            get { return _selectedAyah; }
            set
            {
                _selectedAyah = value;
                base.OnPropertyChanged(() => SelectedAyah);
            }
        }

        public Uri Image
        {
            get { return _image; }
            set
            {
                _image = value;
                base.OnPropertyChanged(() => Image);
            }
        }

        public ItemViewModelType ItemType
        {
            get { return _itemType; }
            set
            {
                _itemType = value;
                Style = value.ToString();
                base.OnPropertyChanged(() => ItemType);
            }
        }

        public string Style
        {
            get { return _style; }
            set
            {
                value = string.Format("ItemViewStyle{0}", value);
                _style = value;
                base.OnPropertyChanged(() => Style);
            }
        }

        public string Group
        {
            get { return _group; }
            set
            {
                _group = value;
                base.OnPropertyChanged(() => Group);
            }
        }

        #endregion Properties
    }
}