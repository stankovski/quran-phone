using System;
using System.Windows.Navigation;
using Quran.Core;
using Quran.Core.Properties;
using Quran.Core.Utils;
using Quran.Core.ViewModels;
using Quran.Windows.UI;
using Quran.Core.Data;
using Telerik.Windows.Data;

namespace Quran.Windows.Views
{
    public partial class RecitersListView
    {
        public RecitersListView()
        {
            InitializeComponent();

            var grouping = new GenericGroupDescriptor<ObservableReciterItem, string>((item) =>
            {
                if (item.Exists)
                {
                    return AppResources.downloaded_reciters;
                }
                else
                {
                    return AppResources.available_reciters;
                }
            });
            grouping.SortMode = ListSortMode.Descending;

            var sorting = new GenericSortDescriptor<ObservableReciterItem, string>(item => item.Name.Substring(0, 1));

            jmpRecitations.GroupDescriptors.Add(grouping);
            jmpRecitations.SortDescriptors.Add(sorting);
        }

        // When page is navigated to set data context to selected item in list
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (DataContext == null)
            {
                DataContext = QuranApp.RecitersListViewModel;                
            }

            if (!QuranApp.RecitersListViewModel.IsDataLoaded)
                QuranApp.RecitersListViewModel.LoadData();

            QuranApp.RecitersListViewModel.NavigateRequested += viewModel_NavigateRequested;
            QuranApp.RecitersListViewModel.AvailableReciters.CollectionChanged += AvailableTranslations_CollectionChanged;
        }

        void AvailableTranslations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                jmpRecitations.RefreshData();
            }
        }

        void viewModel_NavigateRequested(object sender, EventArgs e)
        {
            var qari = sender as ObservableReciterItem;
            if (qari == null)
                return;

            SettingsUtils.Set(Constants.PREF_ACTIVE_QARI, qari.Name);
            NavigationService.GoBack();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            QuranApp.RecitersListViewModel.NavigateRequested -= viewModel_NavigateRequested;
        }
    }
}