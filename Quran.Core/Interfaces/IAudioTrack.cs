using System;

namespace Quran.Core.Interfaces
{
    public interface IAudioTrack
    {
        /// <summary>
        /// The URI path to the track.
        /// </summary>
        Uri Source { get; set; }

        /// <summary>
        /// Text to display as the track's title.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Text to display as the track's artist.
        /// </summary>
        string Artist { get; set; }

        /// <summary>
        /// Text to display as the track’s album.
        /// </summary>
        string Album { get; set; }

        /// <summary>
        /// An arbitrary string associated with this track, used to store application-specific state.
        /// </summary>
        string Tag { get; set; }

        /// <summary>
        /// The path to the album art for the track.
        /// </summary>
        Uri AlbumArt { get; set; }

        /// <summary>
        /// The length of the track.
        /// </summary>
        TimeSpan Duration { get; }
    }
}