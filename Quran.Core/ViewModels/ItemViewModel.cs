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
    /// <summary>
    /// Define the ItemViewModel type.
    /// </summary>
    public class ItemViewModel : ItemViewModelBase
    {
        #region Properties
        private Uri image;
        public Uri Image
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
        #endregion Properties
    }
}
