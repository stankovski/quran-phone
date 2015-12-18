using System;
using System.Threading;
using Quran.Core.Common;
using Windows.Foundation;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace Quran.Core.Tests
{
    public class MockTransferRequest : ITransferRequest
    {
        private readonly DownloadOperation request;
        private readonly Progress<DownloadOperation> callback;

        public MockTransferRequest()
        {
        }

        public DownloadOperation OriginalRequest
        {
            get { return request; }
        }

        public string RequestId
        {
            get { return request.Guid.ToString(); }
        }

        public Uri RequestUri
        {
            get { return request.RequestedUri; }
        }

        public string DownloadLocation
        {
            get { return request.ResultFile.Path; }
        }

        public FileTransferStatus TransferStatus
        {
            get
            {
                return (FileTransferStatus)((int)request.Progress.Status);
            }
        }

        public ulong TotalBytesToReceive
        {
            get { return request.Progress.TotalBytesToReceive; }
        }

        public ulong BytesReceived
        {
            get { return request.Progress.BytesReceived; }
        }

        public bool IsCompressed
        {
            get
            {
                return request.ResultFile.Path != null && request.ResultFile.Path.EndsWith(".zip");
            }
        }

        public event EventHandler<TransferEventArgs> TransferProgressChanged;

        private void Request_ProgressChanged(object sender, DownloadOperation e)
        {
            if (TransferProgressChanged != null)
                TransferProgressChanged(sender, new TransferEventArgs(this));
        }
    }
}
