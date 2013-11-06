using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quran.Core.Utils
{
    public enum AudioPlayerPlayState
    {
        Unknown,
        Stopped,
        Paused,
        Playing,
        BufferingStarted,
        BufferingStopped,
        TrackReady,
        TrackEnded,
        Rewinding,
        FastForwarding,
        Shutdown,
        Error,
    }
}
