using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Quran.Core;
using Quran.Core.Properties;
using Quran.Core.Utils;
using Quran.Core.ViewModels;
using Telerik.Windows.Controls;

namespace Quran.WindowsPhone.Views
{
    public partial class SearchView
    {
        public SearchView()
        {
            InitializeComponent();

            // Set the data context of the LongListSelector control to the sample data
            DataContext = QuranApp.SearchViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string query = string.Empty;
            if (NavigationContext.QueryString.TryGetValue("query", out query))
            {
                if (!string.IsNullOrEmpty(query))
                {
                    QuranApp.SearchViewModel.Load(query);
                }
            }
        }
        
        private void SearchKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if ((string.IsNullOrEmpty(QuranApp.DetailsViewModel.TranslationFile) || 
                    !FileUtils.FileExists(Path.Combine(FileUtils.GetQuranDatabaseDirectory(false), QuranApp.DetailsViewModel.TranslationFile))) 
                    && !FileUtils.FileExists(Path.Combine(FileUtils.GetQuranDatabaseDirectory(false), FileUtils.QURAN_ARABIC_DATABASE)))
                {
                    MessageBox.Show(AppResources.no_translation_to_search);
                    NavigationService.Navigate(new Uri("/Views/SettingsView.xaml", UriKind.Relative));
                }
                else
                {
                    QuranApp.SearchViewModel.Load(SearchBox.Text);
                    ResultList.Focus();
                }
            }
        }

        private void SearchResultsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If selected item is null (no selection) do nothing
            var list = sender as RadDataBoundListBox;
            if (list == null || list.SelectedItem == null)
                return;

            var item = list.SelectedItem as ItemViewModel;

            if (item == null || item.SelectedAyah == null)
                return;

            // Navigate to the new page
            NavigationService.Navigate(new Uri(
                string.Format(CultureInfo.InvariantCulture, "/Views/DetailsView.xaml?page={0}&surah={1}&ayah={2}",
                                          item.PageNumber,
                                          item.SelectedAyah.Sura,
                                          item.SelectedAyah.Ayah), UriKind.Relative));

            // Reset selected item to null (no selection)
            list.SelectedItem = null;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(QuranApp.SearchViewModel.Query))
                SearchBox.Focus();
            else
                ResultList.Focus();
        }
    }
}