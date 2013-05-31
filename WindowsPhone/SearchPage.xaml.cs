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
using Telerik.Windows.Controls;

namespace QuranPhone
{
    public partial class SearchPage : PhoneApplicationPage
    {
        public SearchPage()
        {
            InitializeComponent();

            // Set the data context of the LongListSelector control to the sample data
            DataContext = App.SearchViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string query = string.Empty;
            if (NavigationContext.QueryString.TryGetValue("query", out query))
            {
                if (!string.IsNullOrEmpty(query))
                {
                    App.SearchViewModel.Load(query);
                }
            }
        }
        
        private void SearchKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                App.SearchViewModel.Load(SearchBox.Text);
                ResultList.Focus();
            }
        }

        private void SearchResultsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If selected item is null (no selection) do nothing
            var list = sender as RadDataBoundListBox;
            if (list == null || list.SelectedItem == null)
                return;

            // Navigate to the new page
            NavigationService.Navigate(new Uri("/DetailsPage.xaml?page=" + (list.SelectedItem as ItemViewModel).PageNumber, UriKind.Relative));

            // Reset selected item to null (no selection)
            list.SelectedItem = null;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(App.SearchViewModel.Query))
                SearchBox.Focus();
            else
                ResultList.Focus();
        }
    }
}