using System;
using Windows.UI.Xaml.Controls;
using Quran.Core;
using Quran.Core.Common;
using Quran.Core.ViewModels;
using Windows.UI.Xaml;

namespace Quran.Windows.UI
{
    public partial class TranslationView : UserControl
    {
        public VerseViewModel ViewModel {
            get
            {
                return DataContext as VerseViewModel;
            }
        }

        public TranslationView()
        {
            InitializeComponent();
        }

        public QuranAyah SelectedAyah
        {
            get { return (QuranAyah)GetValue(SelectedAyahProperty); }
            set { SetValue(SelectedAyahProperty, value); }
        }

        public static readonly DependencyProperty SelectedAyahProperty = DependencyProperty.Register("SelectedAyah",
            typeof(QuranAyah), typeof(TranslationView), new PropertyMetadata(null, changeSelectedAyah));

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
                var currentModel = this.DataContext as VerseViewModel;
                if (currentModel != null && currentModel.Surah == ayahInfo.Surah &&
                    currentModel.Ayah == ayahInfo.Ayah)
                {
                    canvas.Opacity = 1.0;
                    if (QuranApp.DetailsViewModel.AudioPlayerState == AudioState.Playing)
                    {
                        canvasStoryboard.Seek(new TimeSpan(1));
                        canvasStoryboard.Stop();
                    }
                    else
                    {
                        canvasStoryboard.Begin();
                    }
                }
                else
                {
                    canvas.Opacity = 0;
                }
            }
        }
    }
}
