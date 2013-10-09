using System;
using System.Collections.Specialized;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using QuranPhone.Data;
using QuranPhone.Resources;
using QuranPhone.UI;
using QuranPhone.Utils;
using Telerik.Windows.Data;

namespace QuranPhone
{
    public partial class TranslationListPage : PhoneApplicationPage
    {
        public TranslationListPage()
        {
            InitializeComponent();

            var grouping = new GenericGroupDescriptor<ObservableTranslationItem, string>(item =>
            {
                if (item.Exists)
                {
                    return AppResources.downloaded_translations;
                }
                return AppResources.available_translations;
            });
            grouping.SortMode = ListSortMode.Descending;

            var sorting = new GenericSortDescriptor<ObservableTranslationItem, string>(item => item.Name.Substring(0, 1));

            JmpTranslation.GroupDescriptors.Add(grouping);
            JmpTranslation.SortDescriptors.Add(sorting);
        }

        // When page is navigated to set data context to selected item in list
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (DataContext == null)
            {
                DataContext = App.TranslationsListViewModel;
            }

            if (!App.TranslationsListViewModel.IsDataLoaded)
            {
                App.TranslationsListViewModel.LoadData();
            }

            App.TranslationsListViewModel.NavigateRequested += viewModel_NavigateRequested;
            App.TranslationsListViewModel.AvailableTranslations.CollectionChanged +=
                AvailableTranslations_CollectionChanged;
        }

        private void AvailableTranslations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace ||
                e.Action == NotifyCollectionChangedAction.Reset)
            {
                JmpTranslation.RefreshData();
            }
        }

        private void viewModel_NavigateRequested(object sender, EventArgs e)
        {
            var translation = sender as ObservableTranslationItem;
            if (translation == null)
            {
                return;
            }

            SettingsUtils.Set(Constants.PrefActiveTranslation, string.Join("|", translation.FileName, translation.Name));
            SettingsUtils.Set(Constants.PrefShowTranslation, true);
            NavigationService.GoBack();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            App.TranslationsListViewModel.NavigateRequested -= viewModel_NavigateRequested;
        }
    }
}