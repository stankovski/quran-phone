// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the ItemViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Quran.Core.Common;
using Windows.UI.Xaml.Media.Imaging;

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

        private BitmapImage image;
        public BitmapImage Image
        {
            get { return image; }
            set
            {
                if (value == image)
                    return;

                image = value;

                base.OnPropertyChanged(() => Image);
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

                base.OnPropertyChanged(() => Group);
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

        public override Task Initialize()
        {
            return Refresh();
        }

        public override Task Refresh()
        {
            return Task.FromResult(0);
        }
        #endregion Properties
    }
}
