using System.Windows;
using System.Windows.Input;
using Quran.Core;
using Quran.Core.Common;

namespace Quran.WindowsPhone.UI
{
    public partial class AudioPlayerControl
    {
        public AudioPlayerControl()
        {
            InitializeComponent();
        }

        public AudioState AudioState
        {
            get { return (AudioState)GetValue(AudioStateProperty); }
            set { SetValue(AudioStateProperty, value); }
        }

        public static readonly DependencyProperty AudioStateProperty = DependencyProperty.Register("AudioState",
            typeof(AudioState), typeof(AudioPlayerControl),
            new PropertyMetadata(ChangeAudioState));

        private static void ChangeAudioState(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var thisControl = source as AudioPlayerControl;
            var state = (AudioState)e.NewValue;
            if (thisControl != null)
            {
                switch (state)
                {
                    case AudioState.Stopped:
                        thisControl.PlayButtonGrid.Visibility = Visibility.Collapsed;
                        thisControl.PauseButtonGrid.Visibility = Visibility.Collapsed;
                        break;
                    case AudioState.Playing:
                        thisControl.PlayButtonGrid.Visibility = Visibility.Collapsed;
                        thisControl.PauseButtonGrid.Visibility = Visibility.Visible;
                        break;
                    case AudioState.Paused:
                        thisControl.PlayButtonGrid.Visibility = Visibility.Visible;
                        thisControl.PauseButtonGrid.Visibility = Visibility.Collapsed;
                        break;
                }
            }
        }

        private void OnControlTap(object sender, GestureEventArgs e)
        {
            if (AudioState == AudioState.Playing)
                QuranApp.NativeProvider.AudioProvider.Pause();
            else if (AudioState == AudioState.Paused)
                QuranApp.NativeProvider.AudioProvider.Play();
        }
    }
}
