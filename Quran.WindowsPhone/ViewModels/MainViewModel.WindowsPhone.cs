// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the MainViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.ObjectModel;
using Telerik.Windows.Data;

namespace Quran.Core.ViewModels
{
    /// <summary>
    /// Define the MainViewModel type.
    /// </summary>
    public class MainViewModelWindowsPhone : MainViewModel
    {
        public MainViewModelWindowsPhone() : base()
        {
            this.BookmarksGroup = new ObservableCollection<GenericGroupDescriptor<ItemViewModel, string>>();
            this.BookmarksGroupSort = new ObservableCollection<GenericSortDescriptor<ItemViewModel, string>>();
            // Load group
            var group = new GenericGroupDescriptor<ItemViewModel, string>();
            group.KeySelector = (item) => item.Group;
            group.SortMode = ListSortMode.None;
            BookmarksGroup.Add(group);
        }
        public ObservableCollection<GenericGroupDescriptor<ItemViewModel, string>> BookmarksGroup { get; private set; }
        public ObservableCollection<GenericSortDescriptor<ItemViewModel, string>> BookmarksGroupSort { get; private set; }
    }
}
