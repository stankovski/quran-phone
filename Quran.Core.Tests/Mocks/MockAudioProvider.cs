using System;
using System.Threading.Tasks;
using Quran.Core.Common;
using Quran.Core.Interfaces;
using Windows.Foundation;

namespace Quran.Core.Tests
{
    public class MockAudioProvider : IAudioProvider
    {
        public MockAudioProvider()
        {
            //BackgroundAudioPlayer.Instance.PlayStateChanged += Instance_PlayStateChanged;
        }

        public event TypedEventHandler<IAudioProvider, AudioPlayerPlayState> StateChanged;
        public event TypedEventHandler<IAudioProvider, AudioTrackModel> TrackChanged;

        public void Play()
        {
            //BackgroundAudioPlayer.Instance.Play();
        }

        public void Pause()
        {
            //BackgroundAudioPlayer.Instance.Pause();
        }

        public async Task Stop()
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

        public AudioTrackModel GetTrack()
        {
            //var audioTrack = BackgroundAudioPlayer.Instance.Track;
            //if (WindowsTrack == null || WindowsTrack.OriginalTrack != audioTrack)
            //    WindowsTrack = new WindowsAudioTrack(audioTrack);

            //return WindowsTrack;
            throw new NotImplementedException();
        }

        public void SetTrack(AudioRequest request)
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
