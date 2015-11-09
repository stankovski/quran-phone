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

namespace Quran.WindowsPhone.NativeProvider
{
    public class UniversalDownloadManager : IDownloadManager
    {
        private Dictionary<string, ITransferRequest> customTransferRequests =
            new Dictionary<string, ITransferRequest>();

        public async Task<ITransferRequest> DownloadAsync(string from, FolderTypes folderType, string to, CancellationToken token = default(CancellationToken))
        {
            var serverUri = new Uri(from, UriKind.Absolute);
            var localFolder = ApplicationData.Current.LocalFolder;
            if (folderType == FolderTypes.Roaming)
            {
                localFolder = ApplicationData.Current.RoamingFolder;
            } else
            {
                localFolder = ApplicationData.Current.TemporaryFolder;
            }
            var localFile = await localFolder.CreateFileAsync(to, CreationCollisionOption.ReplaceExisting);

            if (SettingsUtils.Get<bool>(Constants.PREF_ALT_DOWNLOAD))
                return DownloadAsyncViaWebClient(serverUri, localFile);
            else
                return await DownloadAsyncViaBackgroundTranfer(serverUri, localFile, token);   
        }

        private async Task<ITransferRequest> DownloadAsyncViaBackgroundTranfer(Uri serverUri, IStorageFile resultFile, CancellationToken token)
        {
            try
            {
                var backgroundDownloader = new BackgroundDownloader();
                var request = backgroundDownloader.CreateDownload(serverUri, resultFile);
                request.CostPolicy = BackgroundTransferCostPolicy.Always;

                var progressCallback = new Progress<DownloadOperation>();
                await request.StartAsync().AsTask(token, progressCallback);
                return new UniversalTransferRequest(request, progressCallback);
            }
            catch (InvalidOperationException)
            {
                return GetRequest(serverUri.ToString());
            }
        }

        private ITransferRequest DownloadAsyncViaWebClient(Uri serverUri, IStorageFile phoneUri)
        {
            var request = new WindowsPhoneWebClientTransferRequest(serverUri, phoneUri)
            {
                RequestId = Guid.NewGuid().ToString()
            };
            request.Download();
            customTransferRequests[request.RequestId] = request;
            return request; 
        }

        public async Task<ITransferRequest> DownloadMultipleAsync(string[] from, string to)
        {
            var phoneUri = new Uri(to, UriKind.Relative);

            var request = new WindowsPhoneMultifileTransferRequest(from, phoneUri)
            {
                RequestId = Guid.NewGuid().ToString()
            };
            request.Download();
            customTransferRequests[request.RequestId] = request;
            return request; 
        }

        private void PersistRequestToStorage(BackgroundTransferRequest request)
        {
            var requestUri = request.RequestUri;
            var requestUriHash = CryptoUtils.GetHash(requestUri.ToString());
            var trackerDir = FileUtils.GetDowloadTrackerDirectory(false, true);
            FileUtils.WriteFile(string.Format("{0}\\{1}", trackerDir, requestUriHash), request.RequestId);
        }

        private void DeleteRequestFromStorage(BackgroundTransferRequest request)
        {
            var requestUri = request.RequestUri;
            var requestUriHash = CryptoUtils.GetHash(requestUri.ToString());
            var trackerDir = FileUtils.GetDowloadTrackerDirectory(false, true);
            FileUtils.DeleteFile(string.Format("{0}\\{1}", trackerDir, requestUriHash));
        }

        public ITransferRequest GetRequest(string serverUri)
        {
            var requestUriHash = CryptoUtils.GetHash(serverUri);
            var trackerDir = FileUtils.GetDowloadTrackerDirectory(false, true);
            var requestId = FileUtils.ReadFile(string.Format("{0}\\{1}", trackerDir, requestUriHash));
            if (!string.IsNullOrEmpty(requestId))
            {
                var request = BackgroundTransferService.Find(requestId);
                if (request == null)
                    return null;
                else
                    return new UniversalTransferRequest(request);
            }
            else
                return null;
        }

        public void Cancel(ITransferRequest request)
        {
            request.Cancel();
            FinalizeRequest(request);
        }

        public void FinalizeRequest(ITransferRequest request)
        {
            if (request.RequestId != null)
            {
                if (BackgroundTransferService.Find(request.RequestId) != null)
                {
                    BackgroundTransferService.Remove(((UniversalTransferRequest) request).OriginalRequest);
                    DeleteRequestFromStorage(((UniversalTransferRequest) request).OriginalRequest);
                }
                if (customTransferRequests.ContainsKey(request.RequestId))
                {
                    customTransferRequests[request.RequestId].Cancel();
                    customTransferRequests.Remove(request.RequestId);
                }
            }
        }

        public IEnumerable<ITransferRequest> GetAllRequests()
        {
            return BackgroundTransferService.Requests.Select(r => new UniversalTransferRequest(r));
        }

        public IEnumerable<string> GetAllStuckFiles()
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return isf.GetFileNames("/shared/transfers/*");
            }
        }

        public void Dispose()
        {
            foreach (var request in BackgroundTransferService.Requests)
            {
                if (request.TransferStatus == TransferStatus.Completed)
                    BackgroundTransferService.Remove(request);
            }
        }

        internal void FinalizeRequests()
        {
            foreach (var request in BackgroundTransferService.Requests)
            {
                if (request.TransferStatus == TransferStatus.Completed)
                {
                    BackgroundTransferService.Remove(request);
                }
            }
        }
    }
}
