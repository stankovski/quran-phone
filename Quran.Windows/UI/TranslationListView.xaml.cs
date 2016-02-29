using System;
using System.Linq;
using Quran.Core.Common;
using Quran.Core.Utils;
using Quran.Core.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Quran.Windows.UI
{
    public sealed partial class TranslationListView : UserControl, IDisposable
    {
        private AsyncManualResetEvent _listLoaded = new AsyncManualResetEvent();

        public TranslationListView()
        {
            this.InitializeComponent();
            this.LayoutUpdated += TranslationListViewLayoutUpdated;
            this.TranslationListBox.DataContextChanged += TranslationListBoxDataContextChanged;
        }

        private void TranslationListBoxDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            _listLoaded.Reset();
        }

        private void TranslationListViewLayoutUpdated(object sender, object e)
        {
            if (this.Visibility == Visibility.Visible && !_listLoaded.IsComplete && TranslationListBox.ItemsPanelRoot != null)
            {
                _listLoaded.Set();
            }
        }

        public static readonly DependencyProperty TranslationsProperty = DependencyProperty.Register("Page",
            typeof(PageViewModel), typeof(TranslationListView), new PropertyMetadata(new PageViewModel()));

        public QuranAyah SelectedAyah
        {
            get { return (QuranAyah)GetValue(SelectedAyahProperty); }
            set { SetValue(SelectedAyahProperty, value); }
        }

        public static readonly DependencyProperty SelectedAyahProperty = DependencyProperty.Register("SelectedAyah",
            typeof(QuranAyah), typeof(TranslationListView), new PropertyMetadata(null, ChangeSelectedAyah));

        public QuranAyah SelectedAyahDelayed
        {
            get { return (QuranAyah)GetValue(SelectedAyahDelayedProperty); }
            set { SetValue(SelectedAyahDelayedProperty, value); }
        }

        public static readonly DependencyProperty SelectedAyahDelayedProperty = DependencyProperty.Register("SelectedAyahDelayed",
            typeof(QuranAyah), typeof(TranslationListView), new PropertyMetadata(new QuranAyah()));

        private static void ChangeSelectedAyah(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as TranslationListView).UpdateSelectedAyah(e.NewValue as QuranAyah);
        }

        private async void UpdateSelectedAyah(QuranAyah ayahInfo)
        {
            PageViewModel page = DataContext as PageViewModel;
            if (ayahInfo != null && page != null)
            {
                // Wait for translations to load
                await _listLoaded.WaitAsync();

                VerseViewModel selectedTranslation = page.Translations.FirstOrDefault(t => t.Surah == SelectedAyah.Surah && t.Ayah == SelectedAyah.Ayah);
                if (selectedTranslation != null)
                {
                    TranslationListBox.ScrollIntoView(selectedTranslation);
                    SelectedAyahDelayed = ayahInfo;
                }
            }
        }

        public void Dispose()
        {
            this.LayoutUpdated -= TranslationListViewLayoutUpdated;
            this.TranslationListBox.DataContextChanged -= TranslationListBoxDataContextChanged;
        }
    }
}
