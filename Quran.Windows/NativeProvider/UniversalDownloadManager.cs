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

namespace Quran.Windows.NativeProvider
{
    public class UniversalDownloadManager : IDownloadManager
    {
        private Dictionary<string, ITransferRequest> customTransferRequests =
            new Dictionary<string, ITransferRequest>();

        public async Task<ITransferRequest> DownloadAsync(string from, string to, CancellationToken token = default(CancellationToken))
        {
            var serverUri = new Uri(from, UriKind.Absolute);

            if (SettingsUtils.Get<bool>(Constants.PREF_ALT_DOWNLOAD))
                return await DownloadAsyncViaWebClient(serverUri, to, token);
            else
                return await DownloadAsyncViaBackgroundTranfer(serverUri, to, token);   
        }

        private async Task<ITransferRequest> DownloadAsyncViaBackgroundTranfer(Uri serverUri, string resultFile, CancellationToken token)
        {
            try
            {
                var backgroundDownloader = new BackgroundDownloader();
                var folder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(resultFile));
                var newFile = await folder.CreateFileAsync(Path.GetFileName(resultFile), CreationCollisionOption.OpenIfExists);
                var request = backgroundDownloader.CreateDownload(serverUri, newFile);
                request.CostPolicy = BackgroundTransferCostPolicy.Always;

                var progressCallback = new Progress<DownloadOperation>();
                await request.StartAsync().AsTask(token, progressCallback);
                return new UniversalTransferRequest(request, progressCallback);
            }
            catch (InvalidOperationException)
            {
                return await GetRequest(serverUri.ToString());
            }
        }

        private async Task<ITransferRequest> DownloadAsyncViaWebClient(Uri serverUri, string to, CancellationToken token)
        {
            var request = new UniversalHttpClientTransferRequest(serverUri, to)
            {
                RequestId = Guid.NewGuid().ToString()
            };
            await request.Download(token);
            customTransferRequests[request.RequestId] = request;
            return request; 
        }

        public async Task<ITransferRequest> DownloadMultipleAsync(string[] from, string to, CancellationToken token = default(CancellationToken))
        {
            var request = new UniversalMultifileTransferRequest(from, to)
            {
                RequestId = Guid.NewGuid().ToString()
            };
            await request.Download(token);
            customTransferRequests[request.RequestId] = request;
            return request; 
        }

        private async Task PersistRequestToStorage(DownloadOperation request)
        {
            var requestUri = request.RequestedUri;
            var requestUriHash = CryptoUtils.GetHash(requestUri.ToString());
            var trackerDir = await FileUtils.GetDowloadTrackerDirectory();
            await FileUtils.WriteFile(string.Format("{0}\\{1}", trackerDir, requestUriHash), request.RequestedUri.ToString());
        }

        private async Task DeleteRequestFromStorage(DownloadOperation request)
        {
            var requestUri = request.RequestedUri;
            var requestUriHash = CryptoUtils.GetHash(requestUri.ToString());
            var trackerDir = await FileUtils.GetDowloadTrackerDirectory();
            await FileUtils.DeleteFile(string.Format("{0}\\{1}", trackerDir, requestUriHash));
        }

        public async Task<ITransferRequest> GetRequest(string serverUri)
        {
            var requestUriHash = CryptoUtils.GetHash(serverUri);
            var trackerDir = await FileUtils.GetDowloadTrackerDirectory();
            var requestId = await FileUtils.ReadFile(string.Format("{0}\\{1}", trackerDir, requestUriHash));
            if (!string.IsNullOrEmpty(requestId))
            {
                var downloads = await BackgroundDownloader.GetCurrentDownloadsAsync().AsTask();
                var request = downloads.FirstOrDefault(d => d.Guid.ToString().Equals(requestId, StringComparison.OrdinalIgnoreCase));
                if (request == null)
                    return null;
                else
                    return new UniversalTransferRequest(request, null);
            }
            else
                return null;
        }

        public void FinalizeRequest(ITransferRequest request)
        {
            //if (request.RequestId != null)
            //{
            //    if (BackgroundTransferService.Find(request.RequestId) != null)
            //    {
            //        BackgroundTransferService.Remove(((UniversalTransferRequest)request).OriginalRequest);
            //        DeleteRequestFromStorage(((UniversalTransferRequest)request).OriginalRequest);
            //    }
            //    if (customTransferRequests.ContainsKey(request.RequestId))
            //    {
            //        customTransferRequests[request.RequestId].Cancel();
            //        customTransferRequests.Remove(request.RequestId);
            //    }
            //}
        }

        public async Task<IEnumerable<ITransferRequest>> GetAllRequests(CancellationToken token = default(CancellationToken))
        {
            List<ITransferRequest> activeDownloads = new List<ITransferRequest>();
            var downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
            foreach (var download in downloads)
            {
                var progressCallback = new Progress<DownloadOperation>();
                activeDownloads.Add(new UniversalTransferRequest(
                    await download.AttachAsync().AsTask(token, progressCallback), progressCallback));
            }
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
