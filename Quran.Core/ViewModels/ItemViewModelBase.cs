// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the ItemViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Quran.Core.Common;

namespace Quran.Core.ViewModels
{
    public enum ItemViewModelType
    {
        Unknown,
        Surah,
        Juz,
        Bookmark,
        Header,
        Tag
    }

    /// <summary>
    /// Define the ItemViewModel type.
    /// </summary>
    public abstract class ItemViewModelBase : BaseViewModel
    {
        #region Properties
        private string id;
        public string Id
        {
            get { return id; }
            set
            {
                if (value == id)
                    return;

                id = value;

                base.OnPropertyChanged(() => Id);
            }
        }


        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                if (value == title)
                    return;

                title = value;

                base.OnPropertyChanged(() => Title);
            }
        }

        private string details;
        public string Details
        {
            get { return details; }
            set
            {
                if (value == details)
                    return;

                details = value;

                base.OnPropertyChanged(() => Details);
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

        private ItemViewModelType itemType;
        public ItemViewModelType ItemType
        {
            get { return itemType; }
            set
            {
                if (value == itemType)
                    return;

                itemType = value;

                base.OnPropertyChanged(() => ItemType);
            }
        }        
        #endregion Properties
    }
}
