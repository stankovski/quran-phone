using System;
using Microsoft.Phone.BackgroundTransfer;
using Quran.Core.Common;
using Quran.Core.Utils;
using System.Net;
using System.Threading.Tasks;

namespace Quran.WindowsPhone.NativeProvider
{
    public class WindowsPhoneWebClientTransferRequest : ITransferRequest, IDisposable
    {
        private WebClient webClient;
        public WindowsPhoneWebClientTransferRequest(Uri serverUrl, Uri downloadLocation)
        {
            this.RequestUri = serverUrl;
            this.DownloadLocation = downloadLocation;
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
            webClient.CancelAsync();
        }

        public async Task<bool> Download()
        {
            TransferStatus = FileTransferStatus.Transferring;

            var localFilePath = DownloadLocation.ToString();
            if (FileUtils.FileExists(localFilePath))
            {
                if (FileUtils.IsFileEmpty(localFilePath))
                    FileUtils.DeleteFile(localFilePath);
                else
                    return true;
            }

            webClient = new WebClient();
            var taskComplete = new TaskCompletionSource<bool>();
            webClient.DownloadProgressChanged += (sender, e) =>
            {
                BytesReceived = e.BytesReceived;
                TotalBytesToReceive = e.TotalBytesToReceive;
                if (TransferProgressChanged != null)
                    TransferProgressChanged(this, new TransferEventArgs(this));
            };
            webClient.OpenReadCompleted += (seder, e) =>
            {
                if (e.Cancelled || e.Error != null)
                {
                    taskComplete.SetResult(false);
                }
                else
                {
                    FileUtils.WriteFile(localFilePath, stream => e.Result.CopyTo(stream));
                    taskComplete.SetResult(true);
                }                
            };
            webClient.OpenReadAsync(RequestUri);
            var result = await taskComplete.Task;
            TransferStatus = FileTransferStatus.Completed;
            if (TransferStatusChanged != null)
                TransferStatusChanged(this, new TransferEventArgs(this));
            return result;
        }

        public event EventHandler<TransferEventArgs> TransferStatusChanged;
        public event EventHandler<TransferEventArgs> TransferProgressChanged;

        public void Dispose()
        {
            if (webClient != null)
                webClient.CancelAsync();
        }
    }
}
