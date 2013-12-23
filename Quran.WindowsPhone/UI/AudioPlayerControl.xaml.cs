using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Quran.Core;
using Quran.Core.Common;
using System;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using System.Globalization;

namespace Quran.WindowsPhone.UI
{
    public partial class AudioPlayerControl
    {
        public AudioPlayerControl()
        {
            InitializeComponent();
            this.PlayButtonGrid.Visibility = System.Windows.Visibility.Visible;
            this.PauseButtonGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.RepeatButtonGrid.Visibility = System.Windows.Visibility.Visible;
            this.NoRepeatButtonGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.StopButtonGrid.Visibility = System.Windows.Visibility.Visible;
            this.SettingsButtonGrid.Visibility = System.Windows.Visibility.Visible;
            this.Visibility = System.Windows.Visibility.Collapsed;
            //this.SizeChanged += AudioPlayerControl_SizeChanged;
        }

        //void AudioPlayerControl_SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    var converter = new AudioPlayerOffsetConverter();
        //    this.GridStoryboardFrame.Value =
        //        (double) converter.Convert(this.ActualWidth, typeof (double), null, CultureInfo.CurrentUICulture);
        //    this.GridStoryboardReverseFrame.Value =
        //        (double)converter.Convert(this.ActualWidth, typeof(double), null, CultureInfo.CurrentUICulture);
        //    var translateTransform = new TranslateTransform();
        //    translateTransform.X = (double)converter.Convert(this.ActualWidth, typeof(double), null, CultureInfo.CurrentUICulture);
        //    var transformGroup = new TransformGroup();
        //    transformGroup.Children.Add(translateTransform);
        //    this.LayoutRoot.RenderTransform = transformGroup;
        //}

        public AudioState AudioState
        {
            get { return (AudioState)GetValue(AudioStateProperty); }
            set { SetValue(AudioStateProperty, value); }
        }

        public bool RepeatEnabled
        {
            get { return (bool)GetValue(RepeatEnabledProperty); }
            set { SetValue(RepeatEnabledProperty, value); }
        }

        //public bool ControlExpanded
        //{
        //    get { return (bool)GetValue(ControlExpandedProperty); }
        //    set { SetValue(ControlExpandedProperty, value); }
        //}

        public event EventHandler PlayTapped;
        public event EventHandler PauseTapped;
        public event EventHandler StopTapped;
        public event EventHandler SettingsTapped;

        public static readonly DependencyProperty AudioStateProperty = DependencyProperty.Register("AudioState",
            typeof(AudioState), typeof(AudioPlayerControl),
            new PropertyMetadata(ChangeAudioState));

        public static readonly DependencyProperty RepeatEnabledProperty = DependencyProperty.Register("RepeatEnabled",
            typeof(bool), typeof(AudioPlayerControl),
            new PropertyMetadata(ChangeRepeatEnabled));

        //public static readonly DependencyProperty ControlExpandedProperty = DependencyProperty.Register("ControlExpanded",
        //    typeof(bool), typeof(AudioPlayerControl),
        //    new PropertyMetadata(ChangeControlExpanded));

        private static void ChangeAudioState(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var thisControl = source as AudioPlayerControl;
            var state = (AudioState)e.NewValue;
            if (thisControl != null)
            {
                switch (state)
                {
                    case AudioState.Stopped:
                        thisControl.Visibility = Visibility.Collapsed;
                        //thisControl.ControlExpanded = false;
                        break;
                    case AudioState.Playing:
                        thisControl.Visibility = Visibility.Visible;
                        thisControl.PlayButtonGrid.Visibility = Visibility.Collapsed;
                        thisControl.PauseButtonGrid.Visibility = Visibility.Visible;
                        break;
                    case AudioState.Paused:
                        thisControl.Visibility = Visibility.Visible;
                        thisControl.PlayButtonGrid.Visibility = Visibility.Visible;
                        thisControl.PauseButtonGrid.Visibility = Visibility.Collapsed;
                        break;
                }
            }
        }

        private static void ChangeRepeatEnabled(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var thisControl = source as AudioPlayerControl;
            var enabled = (bool)e.NewValue;
            if (enabled)
            {
                thisControl.RepeatButtonGrid.Visibility = Visibility.Visible;
                thisControl.NoRepeatButtonGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                thisControl.RepeatButtonGrid.Visibility = Visibility.Collapsed;
                thisControl.NoRepeatButtonGrid.Visibility = Visibility.Visible;
            }
        }

        //private async static void ChangeControlExpanded(DependencyObject source, DependencyPropertyChangedEventArgs e)
        //{
        //    var thisControl = source as AudioPlayerControl;
        //    var expanded = (bool)e.NewValue;

        //    if (expanded)
        //    {
        //        await thisControl.ExpandCondrol();
        //    }
        //    else
        //    {
        //        await thisControl.CollapseCondrol();
        //    }
        //}

        //private async void OnControlManipulationComplete(object sender, ManipulationCompletedEventArgs e)
        //{
        //    e.Handled = true;
        //    var velocities = e.FinalVelocities;

        //    if (velocities.LinearVelocity.X > 400 && !ControlExpanded)
        //    {
        //        await ExpandCondrol();
        //    }
        //    else if (velocities.LinearVelocity.X < -300 && ControlExpanded)
        //    {
        //        await CollapseCondrol();
        //    }    
        //}

        //private Task ExpandCondrol() 
        //{
        //    GridStoryboardReverse.Stop();
        //    ControlExpanded = true;
        //    return GridStoryboard.PlayAsync();
        //}

        //private Task CollapseCondrol()
        //{
        //    GridStoryboard.Stop();
        //    ControlExpanded = false;
        //    return GridStoryboardReverse.PlayAsync();
        //}

        private void ButtonTap(object sender, GestureEventArgs e)
        {
            if (sender == SettingsButtonGrid)
            {
                if (SettingsTapped != null)
                {
                    SettingsTapped(this, null);
                }
            }
            else if (sender == StopButtonGrid)
            {
                if (StopTapped != null)
                {
                    StopTapped(this, null);
                }
            }
            else if (sender == RepeatButtonGrid || sender == NoRepeatButtonGrid)
            {
                RepeatEnabled = !RepeatEnabled;
            }
            else if (sender == PlayButtonGrid)
            {
                if (PlayTapped != null)
                {
                    PlayTapped(this, null);
                }
            }
            else if (sender == PauseButtonGrid)
            {
                if (PauseTapped != null)
                {
                    PauseTapped(this, null);
                }
            }
        }
    }
}
