// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the ItemViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Globalization;
using Quran.Core.Common;

namespace Quran.Core.ViewModels
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

    /// <summary>
    /// Define the ItemViewModel type.
    /// </summary>
    public class ItemViewModel : BaseViewModel
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

                base.RaisePropertyChanged(() => Id);
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

                base.RaisePropertyChanged(() => Title);
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

                base.RaisePropertyChanged(() => Details);
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

        private QuranAyah selectedAyah;
        public QuranAyah SelectedAyah
        {
            get { return selectedAyah; }
            set
            {
                if (value == selectedAyah)
                    return;

                selectedAyah = value;

                base.RaisePropertyChanged(() => SelectedAyah);
            }
        }

        private Uri image;
        public Uri Image
        {
            get { return image; }
            set
            {
                if (value == image)
                    return;

                image = value;

                base.RaisePropertyChanged(() => Image);
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
                Style = value.ToString();

                base.RaisePropertyChanged(() => ItemType);
            }
        }

        private string style;
        public string Style
        {
            get { return style; }
            set
            {
                value = string.Format("ItemViewStyle{0}", value);
                if (value == style)
                    return;

                style = value;

                base.RaisePropertyChanged(() => Style);
            }
        }

        private string group;
        public string Group
        {
            get { return group; }
            set
            {
                if (value == group)
                    return;

                group = value;

                base.RaisePropertyChanged(() => Group);
            }
        }
        #endregion Properties
    }
}
