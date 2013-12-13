using System;
using Microsoft.Phone.BackgroundAudio;
using Quran.Core.Interfaces;

namespace Quran.WindowsPhone.NativeProvider
{
    /// <summary>
    /// Represents a single track of audio.
    /// </summary>
    public class WindowsPhoneAudioTrack : IAudioTrack
    {
        private readonly AudioTrack originalTrack;

        public AudioTrack OriginalTrack
        {
            get { return originalTrack; }
        }

        public WindowsPhoneAudioTrack(AudioTrack source)
        {
            originalTrack = source;
        }
        /// <summary>
        /// The URI path to the track.
        /// </summary>
        public Uri Source { get { return originalTrack.Source; } set { originalTrack.Source = value; } }

        /// <summary>
        /// Text to display as the track's title.
        /// </summary>
        public string Title { get { return originalTrack.Title; } set { originalTrack.Title = value; } }

        /// <summary>
        /// Text to display as the track's artist.
        /// </summary>
        public string Artist { get { return originalTrack.Artist; } set { originalTrack.Artist = value; } }

        /// <summary>
        /// Text to display as the track’s album.
        /// </summary>
        public string Album { get { return originalTrack.Album; } set { originalTrack.Album = value; } }

        /// <summary>
        /// An arbitrary string associated with this track, used to store application-specific state.
        /// </summary>
        public string Tag { get { return originalTrack.Tag; } set { originalTrack.Tag = value; } }

        /// <summary>
        /// The path to the album art for the track.
        /// </summary>
        public Uri AlbumArt { get { return originalTrack.AlbumArt; } set { originalTrack.AlbumArt = value; } }

        /// <summary>
        /// The length of the track.
        /// </summary>
        public TimeSpan Duration
        {
            get { return originalTrack.Duration; }
        }
    }
}
