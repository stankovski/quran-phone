using System;
using Microsoft.Phone.BackgroundAudio;
using Quran.Core.Common;
using Quran.Core.Interfaces;
using Quran.Core.Utils;

namespace Quran.WindowsPhone.NativeProvider
{
    public class WindowsPhoneAudioProvider : IAudioProvider
    {
        public WindowsPhoneAudioProvider()
        {
            BackgroundAudioPlayer.Instance.PlayStateChanged += Instance_PlayStateChanged;
        }

        void Instance_PlayStateChanged(object sender, EventArgs e)
        {
            if (StateChanged != null)
                StateChanged(sender, e);
        }

        public event EventHandler StateChanged; 

        public void Play()
        {
            BackgroundAudioPlayer.Instance.Play();
        }

        public void Pause()
        {
            BackgroundAudioPlayer.Instance.Pause();
        }

        public void Stop()
        {
            BackgroundAudioPlayer.Instance.Stop();
        }

        public void Next()
        {
            BackgroundAudioPlayer.Instance.SkipNext();
        }

        public void Previous()
        {
            BackgroundAudioPlayer.Instance.SkipPrevious();
        }

        public AudioPlayerPlayState State
        {
            get { return (AudioPlayerPlayState) (int)BackgroundAudioPlayer.Instance.PlayerState; }
        }

        private WindowsPhoneAudioTrack windowsPhoneTrack;
        public IAudioTrack GetTrack()
        {
            var audioTrack = BackgroundAudioPlayer.Instance.Track;
            if (windowsPhoneTrack == null || windowsPhoneTrack.OriginalTrack != audioTrack)
                windowsPhoneTrack = new WindowsPhoneAudioTrack(audioTrack);

            return windowsPhoneTrack;
        }

        public void SetTrack(Uri source, string title, string artist, string album, Uri albumArt, string tag)
        {
            BackgroundAudioPlayer.Instance.Track = new AudioTrack(source, title, artist, album, albumArt, tag,
                EnabledPlayerControls.All);
        }

        public TimeSpan Position
        {
            get { return BackgroundAudioPlayer.Instance.Position; }
            set { BackgroundAudioPlayer.Instance.Position = value; }
        }
    }
}
