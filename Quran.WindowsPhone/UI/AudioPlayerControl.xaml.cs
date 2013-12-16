using System.Windows;
using System.Windows.Input;
using Quran.Core;
using Quran.Core.Common;
using System;
using System.Windows.Media.Animation;
using System.Threading.Tasks;

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

        public bool ControlExpanded
        {
            get { return (bool)GetValue(ControlExpandedProperty); }
            set { SetValue(ControlExpandedProperty, value); }
        }

        public static readonly DependencyProperty AudioStateProperty = DependencyProperty.Register("AudioState",
            typeof(AudioState), typeof(AudioPlayerControl),
            new PropertyMetadata(ChangeAudioState));

        public static readonly DependencyProperty ControlExpandedProperty = DependencyProperty.Register("ControlExpanded",
            typeof(bool), typeof(AudioPlayerControl),
            new PropertyMetadata(ChangeControlExpanded));

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

        private static void ChangeControlExpanded(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
        }

        private void OnControlTap(object sender, GestureEventArgs e)
        {
            if (AudioState == AudioState.Playing)
                QuranApp.NativeProvider.AudioProvider.Pause();
            else if (AudioState == AudioState.Paused)
                QuranApp.NativeProvider.AudioProvider.Play();
        }

        private async void OnControlManipulationComplete(object sender, ManipulationCompletedEventArgs e)
        {
            e.Handled = true;
            var velocities = e.FinalVelocities;

            if (velocities.LinearVelocity.X > 400 && !ControlExpanded)
            {
                await ExpandCondrol();
            }
            else if (velocities.LinearVelocity.X < -300 && ControlExpanded)
            {
                await CollapseCondrol();
            }    
        }

        private Task ExpandCondrol() 
        {
            GridStoryboardReverse.Stop();
            ControlExpanded = true;
            return GridStoryboard.PlayAsync();
        }

        private Task CollapseCondrol()
        {
            GridStoryboard.Stop();
            ControlExpanded = false;
            return GridStoryboardReverse.PlayAsync();
        }
    }
}
