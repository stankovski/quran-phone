using System;
using Quran.Core.Common;
using Quran.Core.Interfaces;

namespace Quran.Core.Tests
{
    public class MockAudioProvider : IAudioProvider
    {
        public MockAudioProvider()
        {
            //BackgroundAudioPlayer.Instance.PlayStateChanged += Instance_PlayStateChanged;
        }

        public event EventHandler StateChanged; 

        public void Play()
        {
            //BackgroundAudioPlayer.Instance.Play();
        }

        public void Pause()
        {
            //BackgroundAudioPlayer.Instance.Pause();
        }

        public void Stop()
        {
            //BackgroundAudioPlayer.Instance.Stop();
        }

        public void Next()
        {
            //BackgroundAudioPlayer.Instance.SkipNext();
        }

        public void Previous()
        {
            //BackgroundAudioPlayer.Instance.SkipPrevious();
        }

        public AudioPlayerPlayState State
        {
            get; set;
        }

        public IAudioTrack GetTrack()
        {
            //var audioTrack = BackgroundAudioPlayer.Instance.Track;
            //if (WindowsTrack == null || WindowsTrack.OriginalTrack != audioTrack)
            //    WindowsTrack = new WindowsAudioTrack(audioTrack);

            //return WindowsTrack;
            throw new NotImplementedException();
        }

        public void SetTrack(Uri source, string title, string artist, string album, Uri albumArt, string tag)
        {
            //BackgroundAudioPlayer.Instance.Track = new AudioTrack(source, title, artist, album, albumArt, tag,
            //    EnabledPlayerControls.All);
            throw new NotImplementedException();
        }

        public TimeSpan Position
        {
            get; set;
        }
    }
}
