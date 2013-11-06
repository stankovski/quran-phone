using System;
using Microsoft.Phone.BackgroundTransfer;
using Quran.Core.Common;

namespace Quran.WindowsPhone.NativeProvider
{
    public class WindowsPhoneTransferRequest : ITransferRequest
    {
        private readonly BackgroundTransferRequest request;
        public WindowsPhoneTransferRequest(BackgroundTransferRequest request)
        {
            this.request = request;
            if (this.request != null)
            {
                this.request.TransferProgressChanged += request_TransferProgressChanged;
                this.request.TransferStatusChanged += request_TransferStatusChanged;
            }
        }

        public BackgroundTransferRequest OriginalRequest
        {
            get { return request; }
        }

        public string RequestId
        {
            get { return request.RequestId; }
        }

        public Uri RequestUri
        {
            get { return request.RequestUri; }
        }

        public string Tag
        {
            get { return request.Tag; }
            set { request.Tag = value; }
        }

        public Uri DownloadLocation
        {
            get { return request.DownloadLocation; }
            set { request.DownloadLocation = value; }
        }

        public Uri UploadLocation
        {
            get { return request.UploadLocation; }
            set { request.UploadLocation = value; }
        }

        public FileTransferStatus TransferStatus
        {
            get { return (FileTransferStatus)((int)request.TransferStatus); }
        }

        public Exception TransferError
        {
            get { return request.TransferError; }
        }

        public long TotalBytesToReceive
        {
            get { return request.TotalBytesToReceive; }
        }

        public long TotalBytesToSend
        {
            get { return request.TotalBytesToSend; }
        }

        public long BytesReceived
        {
            get { return request.BytesReceived; }
        }

        public long BytesSent
        {
            get { return request.BytesSent; }
        }

        public event EventHandler<TransferEventArgs> TransferStatusChanged;

        void request_TransferStatusChanged(object sender, BackgroundTransferEventArgs e)
        {
            TransferStatusChanged(sender, new TransferEventArgs(this));
        }

        public event EventHandler<TransferEventArgs> TransferProgressChanged;

        void request_TransferProgressChanged(object sender, BackgroundTransferEventArgs e)
        {
            TransferProgressChanged(sender, new TransferEventArgs(this));
        }
    }
}
