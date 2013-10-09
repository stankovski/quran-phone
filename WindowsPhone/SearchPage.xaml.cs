using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using QuranPhone.Resources;
using QuranPhone.Utils;
using QuranPhone.ViewModels;
using Telerik.Windows.Controls;

namespace QuranPhone
{
    public partial class SearchPage : PhoneApplicationPage
    {
        public SearchPage()
        {
            InitializeComponent();
            DataContext = App.SearchViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string query;
            if (NavigationContext.QueryString.TryGetValue("query", out query))
            {
                if (!string.IsNullOrEmpty(query))
                {
                    App.SearchViewModel.Load(query);
                }
            }
        }

        private void SearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if ((string.IsNullOrEmpty(App.DetailsViewModel.TranslationFile) ||
                     !QuranFileUtils.FileExists(Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false),
                         App.DetailsViewModel.TranslationFile))) &&
                    !QuranFileUtils.FileExists(Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false),
                        QuranFileUtils.QuranArabicDatabase)))
                {
                    MessageBox.Show(AppResources.no_translation_to_search);
                    NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
                }
                else
                {
                    App.SearchViewModel.Load(SearchBox.Text);
                    ResultList.Focus();
                }
            }
        }

        private void SearchResultsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as RadDataBoundListBox;
            if (list == null || list.SelectedItem == null)
            {
                return;
            }

            var item = list.SelectedItem as ItemViewModel;

            if (item == null || item.SelectedAyah == null)
            {
                return;
            }

            NavigationService.Navigate(
                new Uri(
                    string.Format(CultureInfo.InvariantCulture, "/DetailsPage.xaml?page={0}&surah={1}&ayah={2}",
                        item.PageNumber, item.SelectedAyah.Sura, item.SelectedAyah.Ayah), UriKind.Relative));

            list.SelectedItem = null;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(App.SearchViewModel.Query))
            {
                SearchBox.Focus();
            }
            else
            {
                ResultList.Focus();
            }
        }
    }
}