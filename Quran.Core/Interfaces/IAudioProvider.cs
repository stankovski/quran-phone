using System;
using Quran.Core.Common;
using Quran.Core.Utils;

namespace Quran.Core.Interfaces
{
    public interface IAudioProvider
    {
        event EventHandler StateChanged; 

        /// <summary>
        /// Plays or resumes the current <see cref="T:Microsoft.Phone.BackgroundAudio.AudioTrack"/> at its current position.
        /// </summary>
        void Play();

        /// <summary>
        /// Pauses playback at the current position.
        /// </summary>
        void Pause();

        /// <summary>
        /// Stops and resets media to be played from the beginning.
        /// </summary>
        void Stop();

        /// <summary>
        /// Starts fast-forwarding through the current <see cref="T:Microsoft.Phone.BackgroundAudio.AudioTrack"/>.
        /// </summary>
        void Next();

        /// <summary>
        /// Starts rewinding through the current <see cref="T:Microsoft.Phone.BackgroundAudio.AudioTrack"/>.
        /// </summary>
        void Previous();

        AudioPlayerPlayState State { get; }

        /// <summary>
        /// Gets the current track for this application, whether the application is currently playing or not.
        /// </summary>
        IAudioTrack GetTrack();

        /// <summary>
        /// Sets the current track for this application, whether the application is currently playing or not.
        /// </summary>
        void SetTrack(Uri source, string title, string artist, string album, Uri albumArt, string tag);

        /// <summary>
        /// Gets or sets the current position within the current <see cref="P:Microsoft.Phone.BackgroundAudio.BackgroundAudioPlayer.Track"/>.
        /// </summary>
        TimeSpan Position { get; set; }
    }
}
