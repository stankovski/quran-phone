using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using QuranPhone.Utils;
using QuranPhone.ViewModels;
using Telerik.Windows.Controls;

namespace QuranPhone
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
            DataContext = App.MainViewModel;
            Header.NavigationRequest += header_NavigationRequest;
            LittleWatson.CheckForPreviousException();
        }

        private void header_NavigationRequest(object sender, NavigationEventArgs e)
        {
            NavigationService.Navigate(e.Uri);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            while (NavigationService.CanGoBack)
            {
                NavigationService.RemoveBackEntry();
            }

            if (!App.MainViewModel.IsDataLoaded)
            {
                App.MainViewModel.LoadData();
            }
            else
            {
                App.MainViewModel.RefreshData();
            }

            if (!QuranFileUtils.HaveAllImages())
            {
                try
                {
                    App.MainViewModel.Download();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("failed to download quran data: " + ex.Message);
                }
            }
        }

        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as RadDataBoundListBox;
            if (list == null || list.SelectedItem == null)
            {
                return;
            }

            var selectedItem = (ItemViewModel) list.SelectedItem;

            try
            {
                if (selectedItem.SelectedAyah == null)
                {
                    NavigationService.Navigate(new Uri("/DetailsPage.xaml?page=" + selectedItem.PageNumber,
                        UriKind.Relative));
                }
                else
                {
                    NavigationService.Navigate(
                        new Uri(
                            string.Format("/DetailsPage.xaml?page={0}&surah={1}&ayah={2}", selectedItem.PageNumber,
                                selectedItem.SelectedAyah.Sura, selectedItem.SelectedAyah.Ayah), UriKind.Relative));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            list.SelectedItem = null;
        }

        private void DeleteBookmark(object sender, ContextMenuItemSelectedEventArgs e)
        {
            var menuItem = sender as RadContextMenuItem;
            if (menuItem != null)
            {
                if (menuItem.DataContext != null)
                {
                    App.MainViewModel.DeleteBookmark(menuItem.DataContext as ItemViewModel);
                }
            }
        }
    }
}