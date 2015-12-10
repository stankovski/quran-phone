using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Quran.Core.Common;
using Quran.Core.Utils;

namespace Quran.Windows.NativeProvider
{
    public class UniversalMultifileTransferRequest : ITransferRequest
    {
        private string[] serverUrls;
        public UniversalMultifileTransferRequest(string[] serverUrls, string downloadLocation)
        {
            this.serverUrls = serverUrls;
            this.DownloadLocation = downloadLocation;
            this.TotalBytesToReceive = (ulong)(serverUrls.Length*1000);
        }
        public string RequestId { get; set; }
        public Uri RequestUri { get; set; }
        public string DownloadLocation { get; set; }
        public FileTransferStatus TransferStatus { get; set; }
        public ulong TotalBytesToReceive { get; set; }
        public ulong BytesReceived { get; set; }
        
        public async Task Download(CancellationToken token)
        {
            int successfulDownloads = 0;
            TransferStatus = FileTransferStatus.Transferring;
            if (TransferStatusChanged != null)
                TransferStatusChanged(this, new TransferEventArgs(this));

            foreach (var serverPath in serverUrls)
            {
                successfulDownloads++;

                if (token.IsCancellationRequested)
                {
                    TransferStatus = FileTransferStatus.Cancelled;
                    break;
                }

                var localFilePath = Path.Combine(DownloadLocation.ToString(), Path.GetFileName(serverPath));
                if (await FileUtils.FileExists(localFilePath))
                {
                    if (await FileUtils.IsFileEmpty(localFilePath))
                        await FileUtils.DeleteFile(localFilePath);
                    else
                        continue;
                }
                
                // Retry loop
                for (int i = 0; i < 5; i++)
                {
                    var result = await FileUtils.DownloadFileFromWebAsync(serverPath, localFilePath, token);
                    if (result)
                    {
                        BytesReceived = (ulong)(successfulDownloads*1000);
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
