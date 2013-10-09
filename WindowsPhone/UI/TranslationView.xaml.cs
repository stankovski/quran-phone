using System.Windows;
using System.Windows.Controls;
using QuranPhone.Common;
using QuranPhone.ViewModels;

namespace QuranPhone.UI
{
    public partial class TranslationView : UserControl
    {
        public static readonly DependencyProperty SelectedAyahProperty = DependencyProperty.Register("SelectedAyah",
            typeof (QuranAyah), typeof (TranslationView), new PropertyMetadata(changeSelectedAyah));

        public TranslationView()
        {
            InitializeComponent();
        }

        public QuranAyah SelectedAyah
        {
            get { return (QuranAyah) GetValue(SelectedAyahProperty); }
            set { SetValue(SelectedAyahProperty, value); }
        }

        private static void changeSelectedAyah(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as TranslationView).UpdateSelectedAyah(e.NewValue as QuranAyah);
        }

        private void UpdateSelectedAyah(QuranAyah ayahInfo)
        {
            if (ayahInfo == null)
            {
                canvas.Opacity = 0;
            }
            else
            {
                var currentModel = DataContext as VerseViewModel;
                if (currentModel != null && currentModel.Surah == ayahInfo.Sura && currentModel.Ayah == ayahInfo.Ayah)
                {
                    canvas.Opacity = 1.0;
                    canvasStoryboard.Begin();
                }
            }
        }
    }
}