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
using QuranPhone.Utils;
using QuranPhone.Data;

namespace QuranPhone
{
    public partial class TranslationPage : PhoneApplicationPage
    {
        //QuranScreenInfo screenInfo;
        // Constructor
        public TranslationPage()
        {
            InitializeComponent();
            //screenInfo = QuranScreenInfo.GetInstance();
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        // When page is navigated to set data context to selected item in list
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (DataContext == null)
            {
                string selectedPage = "";
                string selectedTranslation = "";
                if (NavigationContext.QueryString.TryGetValue("page", out selectedPage))
                {
                    NavigationContext.QueryString.TryGetValue("translation", out selectedTranslation);
                    if (string.IsNullOrEmpty(selectedTranslation))
                        selectedTranslation = SettingsUtils.Get<string>(Constants.PREF_ACTIVE_TRANSLATION);
                    int index = int.Parse(selectedPage);
                    var viewModel = new TranslationViewModel(index, selectedTranslation);
                    viewModel.LoadData();
                    DataContext = viewModel;                    
                }
            }
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            var viewModel = (TranslationViewModel)DataContext;
            NavigationService.Navigate(new Uri("/DetailsPage.xaml?noBack=true&page=" + viewModel.CurrentPageNumer, UriKind.Relative));
        }

        //private void Pivot_LoadingItem(object sender, PivotItemEventArgs e)
        //{
        //    var pageModel = (PageViewModel)e.Item.DataContext;
        //    DatabaseHandler db = new DatabaseHandler(((TranslationViewModel)DataContext).TranslationFile);
        //    var verses = db.GetVerses(pageModel.PageNumber);
        //    pageModel.Verses.Clear();
        //    foreach (var verse in verses)
        //    {
        //        pageModel.Verses.Add(new VerseViewModel { IsTitle = false, VerseNumber = verse.Ayah, SurahNumber = verse.Sura, Text = verse.Text });
        //    }
        //}

        //private void Translation_Click(object sender, EventArgs e)
        //{
        //    // Navigate to the translation page
        //    NavigationService.Navigate(new Uri("/TranslationListPage.xaml", UriKind.Relative));
        //}
    }
}