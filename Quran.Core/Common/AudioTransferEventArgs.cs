using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quran.Core.Common
{
    public class AudioTransferEventArgs : EventArgs
    {
        public long TotalDataToReceive { get; set; }
        public long ReceivedData { get; set; }
        public bool IsAllComplete { get; set; }

        public AudioTransferEventArgs(long total, long complete, bool isComplete)
        {
            this.TotalDataToReceive = total;
            this.ReceivedData = complete;
            this.IsAllComplete = isComplete;
        }
    }
}
