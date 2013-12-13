using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quran.Core.Common
{
    public class TransferEventArgs : EventArgs
    {
        public ITransferRequest Request { get; private set; }

        public TransferEventArgs(ITransferRequest request)
        {
            Request = request;
        }
    }
}
