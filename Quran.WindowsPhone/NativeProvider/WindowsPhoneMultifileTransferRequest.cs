using System;
using Quran.Core.Common;
using Quran.Core.Utils;

namespace Quran.UniversalApp.NativeProvider
{
    public class WindowsPhoneMultifileTransferRequest : ITransferRequest
    {
        private string[] serverUrls;
        public WindowsPhoneMultifileTransferRequest(string[] serverUrls, Uri downloadLocation)
        {
            this.serverUrls = serverUrls;
            this.DownloadLocation = downloadLocation;
            this.TotalBytesToReceive = serverUrls.Length*1000;
        }
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
        public void Cancel()
        {
            IsCancelled = true;
        }

        public async void Download()
        {
            int successfulDownloads = 0;
            TransferStatus = FileTransferStatus.Transferring;
            if (TransferStatusChanged != null)
                TransferStatusChanged(this, new TransferEventArgs(this));

            foreach (var serverPath in serverUrls)
            {
                successfulDownloads++;

                if (IsCancelled)
                {
                    TransferStatus = FileTransferStatus.Cancelled;
                    break;
                }

                var localFilePath = PathHelper.Combine(DownloadLocation.ToString(), PathHelper.GetFileName(serverPath));
                if (FileUtils.FileExists(localFilePath))
                {
                    if (FileUtils.IsFileEmpty(localFilePath))
                        FileUtils.DeleteFile(localFilePath);
                    else
                        continue;
                }
                
                // Retry loop
                for (int i = 0; i < 5; i++)
                {
                    var result = await FileUtils.DownloadFileFromWebAsync(serverPath, localFilePath);
                    if (result)
                    {
                        BytesReceived = successfulDownloads*1000;
                        if (TransferProgressChanged != null)
                        {
                            TransferProgressChanged(this, new TransferEventArgs(this));
                        }
                        break;
                    }
                }
            }

            TransferStatus = FileTransferStatus.Completed;
            if (TransferStatusChanged != null)
                TransferStatusChanged(this, new TransferEventArgs(this));
        }

        public event EventHandler<TransferEventArgs> TransferStatusChanged;
        public event EventHandler<TransferEventArgs> TransferProgressChanged;
    }
}
