using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using Quran.Core.Common;
using Quran.Core.Interfaces;
using Quran.Core.Utils;
using Quran.Core.Data;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Quran.Core.Tests
{
    public class MockDownloadManager : IDownloadManager
    {
        private Dictionary<string, ITransferRequest> customTransferRequests =
            new Dictionary<string, ITransferRequest>();

        public async Task<ITransferRequest> DownloadAsync(string from, string to, CancellationToken token = default(CancellationToken))
        {
            return new MockTransferRequest();
        }

        private async Task<ITransferRequest> DownloadAsyncViaBackgroundTranfer(Uri serverUri, string resultFile, CancellationToken token)
        {
            return new MockTransferRequest();
        }

        private async Task<ITransferRequest> DownloadAsyncViaWebClient(Uri serverUri, string to, CancellationToken token)
        {
            return new MockTransferRequest();
        }

        public async Task<ITransferRequest> DownloadMultipleAsync(string[] from, string to, CancellationToken token = default(CancellationToken))
        {
            return new MockTransferRequest();
        }

        private async Task PersistRequestToStorage(DownloadOperation request)
        {
            var requestUri = request.RequestedUri;
            var requestUriHash = CryptoUtils.GetHash(requestUri.ToString());
            var trackerDir = FileUtils.GetDowloadTrackerDirectory();
            await FileUtils.WriteFile(string.Format("{0}\\{1}", trackerDir, requestUriHash), request.RequestedUri.ToString());
        }

        private async Task DeleteRequestFromStorage(DownloadOperation request)
        {
            var requestUri = request.RequestedUri;
            var requestUriHash = CryptoUtils.GetHash(requestUri.ToString());
            var trackerDir = FileUtils.GetDowloadTrackerDirectory();
            await FileUtils.SafeFileDelete(string.Format("{0}\\{1}", trackerDir, requestUriHash));
        }

        public async Task<ITransferRequest> GetRequest(string serverUri)
        {
            return null;
        }

        public void FinalizeRequest(ITransferRequest request)
        {
            //Do nothing
        }

        public async Task<IEnumerable<ITransferRequest>> GetAllRequests(CancellationToken token = default(CancellationToken))
        {
            List<ITransferRequest> activeDownloads = new List<ITransferRequest>();
            return activeDownloads;
        }

        public IEnumerable<string> GetAllStuckFiles()
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return isf.GetFileNames("/shared/transfers/*");
            }
        }

        public async Task Cancel(ITransferRequest request)
        {
            var downloads = await BackgroundDownloader.GetCurrentDownloadsAsync().AsTask();
            var requestToCancel = downloads.FirstOrDefault(d => d.Guid.ToString().Equals(request.RequestId, StringComparison.OrdinalIgnoreCase));
            if (requestToCancel != null)
            {
                requestToCancel.AttachAsync().Cancel();
            }
        }

        public void Dispose()
        {
            var downloads = BackgroundDownloader.GetCurrentDownloadsAsync().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
            foreach (var download in downloads)
            {
                download.AttachAsync().Cancel();
            }
        }
    }
}
