using System;
using Quran.Core.Common;
using Quran.Core.Utils;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;

namespace Quran.WindowsPhone.NativeProvider
{
    public class UniversalHttpClientTransferRequest : ITransferRequest, IDisposable
    {
        private HttpClient httpClient;
        public UniversalHttpClientTransferRequest(Uri serverUrl, string downloadLocation)
        {
            this.RequestUri = serverUrl;
            this.DownloadLocation = downloadLocation;
        }
        public string RequestId { get; set; }
        public Uri RequestUri { get; set; }
        public string Tag { get; set; }
        public string DownloadLocation { get; }
        public FileTransferStatus TransferStatus { get; private set; }
        public ulong TotalBytesToReceive { get; set; }
        public ulong BytesReceived { get; set; }

        public async Task Download(CancellationToken cancellationToken)
        {
            TransferStatus = FileTransferStatus.Transferring;
            if (TransferProgressChanged != null)
                TransferProgressChanged(this, new TransferEventArgs(this));

            await FileUtils.DownloadFileFromWebAsync(RequestUri.ToString(), DownloadLocation, cancellationToken);

            TransferStatus = FileTransferStatus.Completed;
            if (TransferProgressChanged != null)
                TransferProgressChanged(this, new TransferEventArgs(this));
        }

        public event EventHandler<TransferEventArgs> TransferProgressChanged;

        public void Dispose()
        {
            if (httpClient != null)
                httpClient.Dispose();
        }
    }
}
