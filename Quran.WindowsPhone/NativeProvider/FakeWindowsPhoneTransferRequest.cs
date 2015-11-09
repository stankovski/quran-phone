using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quran.Core.Common;

namespace Quran.UniversalApp.NativeProvider
{
    public class FakeWindowsPhoneTransferRequest : ITransferRequest
    {
        public string RequestId { get; set; }
        public Uri RequestUri { get; set; }
        public string Tag { get; set; }
        public Uri DownloadLocation { get; set; }
        public Uri UploadLocation { get; set; }
        public FileTransferStatus TransferStatus { get; set; }
        public Exception TransferError { get; set; }
        public long TotalBytesToReceive { get; set; }
        public long TotalBytesToSend { get; set; }
        public long BytesReceived { get; set; }
        public long BytesSent { get; set; }
        public bool IsCancelled { get; private set; }

        public void ChangeProgress(long bytes, FileTransferStatus status)
        {
            var oldStatus = TransferStatus;
            TransferStatus = status;
            BytesReceived = bytes;
            if (oldStatus != status)
                TransferStatusChanged(this, new TransferEventArgs(this));

            TransferProgressChanged(this, new TransferEventArgs(this));
        }

        public void Cancel()
        {
            IsCancelled = true;
        }

        public event EventHandler<TransferEventArgs> TransferStatusChanged;
        public event EventHandler<TransferEventArgs> TransferProgressChanged;
    }
}
