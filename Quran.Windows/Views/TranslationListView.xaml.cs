using Quran.Core;
using Quran.Core.Utils;
using Quran.Core.ViewModels;
using Quran.Core.Data;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls;
using System.IO;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.Graphics.Display;

namespace Quran.Windows.Views
{
    public partial class TranslationListView : Page
    {
        public TranslationsListViewModel ViewModel { get; set; }

        public TranslationListView()
        {
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
            ViewModel = QuranApp.TranslationsListViewModel;
            InitializeComponent();            
        }

        // When page is navigated to set data context to selected item in list
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.Initialize();
            TranslationViewSource.Source = ViewModel.Groups;
        }
        
        private void NavigationRequested(object sender, TappedRoutedEventArgs e)
        {
            var list = sender as FrameworkElement;
            if (list == null || list.DataContext == null)
                return;

            var translation = (ObservableTranslationItem)list.DataContext;
            if (translation == null)
            {
                return;
            }

            if (translation.Exists)
            {
                SettingsUtils.Set(Constants.PREF_ACTIVE_TRANSLATION, string.Join("|",
                    Path.GetFileName(translation.LocalPath), translation.Name));
                SettingsUtils.Set(Constants.PREF_SHOW_TRANSLATION, true);
                Frame.GoBack();
            }
        }
    }
}