using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Quran.Core;
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

        public ObservableCollection<VerseViewModel> Translations
        {
            get { return (ObservableCollection<VerseViewModel>)GetValue(TranslationsProperty); }
            set { SetValue(TranslationsProperty, value); }
        }

        public static readonly DependencyProperty TranslationsProperty = DependencyProperty.Register("Translations",
            typeof(ObservableCollection<VerseViewModel>), typeof(TranslationListView), new PropertyMetadata(new ObservableCollection<VerseViewModel>()));

        public QuranAyah SelectedAyah
        {
            get { return (QuranAyah)GetValue(SelectedAyahProperty); }
            set { SetValue(SelectedAyahProperty, value); }
        }

        public static readonly DependencyProperty SelectedAyahProperty = DependencyProperty.Register("SelectedAyah",
            typeof(QuranAyah), typeof(TranslationListView), new PropertyMetadata(null, ChangeSelectedAyah));

        private static void ChangeSelectedAyah(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as TranslationListView).UpdateSelectedAyah(e.NewValue as QuranAyah);
        }

        private async void UpdateSelectedAyah(QuranAyah ayahInfo)
        {
            if (ayahInfo != null && Translations != null)
            {
                // Wait for image to load
                await _listLoaded.WaitAsync();

                VerseViewModel selectedTranslation = Translations.FirstOrDefault(t => t.Surah == SelectedAyah.Surah && t.Ayah == SelectedAyah.Ayah);
                if (selectedTranslation != null)
                {
                    TranslationListBox.ScrollIntoView(selectedTranslation);
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
