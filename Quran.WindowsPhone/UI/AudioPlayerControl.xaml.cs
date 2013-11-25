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
            this.PlayButtonGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.PauseButtonGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.RepeatButtonGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.NoRepeatButtonGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.StopButtonGrid.Visibility = System.Windows.Visibility.Collapsed;
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
                        thisControl.RepeatButtonGrid.Visibility = Visibility.Collapsed;
                        thisControl.NoRepeatButtonGrid.Visibility = Visibility.Collapsed;
                        thisControl.StopButtonGrid.Visibility = Visibility.Collapsed;
                        break;
                    case AudioState.Playing:
                        thisControl.PlayButtonGrid.Visibility = Visibility.Collapsed;
                        thisControl.PauseButtonGrid.Visibility = Visibility.Visible;
                        thisControl.RepeatButtonGrid.Visibility = Visibility.Visible;
                        thisControl.NoRepeatButtonGrid.Visibility = Visibility.Collapsed;
                        thisControl.StopButtonGrid.Visibility = Visibility.Visible;
                        break;
                    case AudioState.Paused:
                        thisControl.PlayButtonGrid.Visibility = Visibility.Visible;
                        thisControl.PauseButtonGrid.Visibility = Visibility.Collapsed;
                        thisControl.StopButtonGrid.Visibility = Visibility.Visible;
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

        private void OnControlManipulationComplete(object sender, ManipulationCompletedEventArgs e)
        {
            e.Handled = true;
            var velocities = e.FinalVelocities;
            if (velocities.LinearVelocity.X > 200)
                PanelStoryboard.Begin();
        }
    }
}
