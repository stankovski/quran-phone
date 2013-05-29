using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using QuranPhone.ViewModels;
using QuranPhone.Common;
using QuranPhone.UI;
using QuranPhone.Utils;
using QuranPhone.Data;
using System.Globalization;
using Telerik.Windows.Controls.JumpList;
using Telerik.Windows.Data;
using QuranPhone.Resources;

namespace QuranPhone
{
    public partial class TranslationListPage : PhoneApplicationPage
    {
        public TranslationListPage()
        {
            InitializeComponent();

            GenericGroupDescriptor<ObservableTranslationItem, string> grouping 
                = new GenericGroupDescriptor<ObservableTranslationItem, string>((item) =>
            {
                if (item.Exists)
                {
                    return AppResources.downloaded_translations;
                }
                else
                {
                    return AppResources.available_translations;
                }
            });
            grouping.SortMode = ListSortMode.Descending;

            GenericSortDescriptor<ObservableTranslationItem, string> sorting
                = new GenericSortDescriptor<ObservableTranslationItem, string>(item => item.Name.Substring(0, 1));

            jmpTranslation.GroupDescriptors.Add(grouping);
            jmpTranslation.SortDescriptors.Add(sorting);
        }

        // When page is navigated to set data context to selected item in list
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (DataContext == null)
            {
                DataContext = App.TranslationsListViewModel;                
            }

            if (!App.TranslationsListViewModel.IsDataLoaded)
                App.TranslationsListViewModel.LoadData();
            
            App.TranslationsListViewModel.NavigateRequested += viewModel_NavigateRequested;
            App.TranslationsListViewModel.AvailableTranslations.CollectionChanged += AvailableTranslations_CollectionChanged;
        }

        void AvailableTranslations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                jmpTranslation.RefreshData();
            }
        }

        void viewModel_NavigateRequested(object sender, EventArgs e)
        {
            var translation = sender as ObservableTranslationItem;
            if (translation == null)
                return;

            SettingsUtils.Set(Constants.PREF_ACTIVE_TRANSLATION, string.Join("|", translation.FileName, translation.Name));
            SettingsUtils.Set(Constants.PREF_SHOW_TRANSLATION, true);
            NavigationService.GoBack();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            App.TranslationsListViewModel.NavigateRequested -= viewModel_NavigateRequested;
        }
    }
}