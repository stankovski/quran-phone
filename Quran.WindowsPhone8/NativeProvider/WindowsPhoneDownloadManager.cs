using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using Microsoft.Phone.BackgroundTransfer;
using Quran.Core.Common;
using Quran.Core.Interfaces;
using Quran.Core.Utils;
using Quran.Core.Data;

namespace Quran.WindowsPhone.NativeProvider
{
    public class WindowsPhoneDownloadManager : IDownloadManager
    {
        private Dictionary<string, ITransferRequest> customTransferRequests =
            new Dictionary<string, ITransferRequest>();

        public ITransferRequest DownloadAsync(string from, string to)
        {
            var serverUri = new Uri(from, UriKind.Absolute);
            var phoneUri = new Uri(to, UriKind.Relative);

            if (SettingsUtils.Get<bool>(Constants.PREF_ALT_DOWNLOAD))
                return DownloadAsyncViaWebClient(serverUri, phoneUri);
            else
                return DownloadAsyncViaBackgroundTranfer(serverUri, phoneUri);   
        }

        private ITransferRequest DownloadAsyncViaBackgroundTranfer(Uri serverUri, Uri phoneUri)
        {
            try
            {
                var request = new BackgroundTransferRequest(serverUri, phoneUri);
                request.Tag = serverUri.ToString();
                request.TransferPreferences = TransferPreferences.AllowCellularAndBattery;

                int count = 0;
                foreach (var r in BackgroundTransferService.Requests)
                {
                    count++;
                    if (r.RequestUri == serverUri)
                        return new WindowsPhoneTransferRequest(r);
                    if (r.TransferStatus == TransferStatus.Completed)
                    {
                        BackgroundTransferService.Remove(r);
                        count--;
                    }
                    // Max 5 downloads
                    if (count >= 5)
                        return null;
                }
                BackgroundTransferService.Add(request);
                PersistRequestToStorage(request);
                return new WindowsPhoneTransferRequest(request);
            }
            catch (InvalidOperationException)
            {
                return GetRequest(serverUri.ToString());
            }
        }

        private ITransferRequest DownloadAsyncViaWebClient(Uri serverUri, Uri phoneUri)
        {
            var request = new WindowsPhoneWebClientTransferRequest(serverUri, phoneUri)
            {
                RequestId = Guid.NewGuid().ToString()
            };
            request.Download();
            customTransferRequests[request.RequestId] = request;
            return request; 
        }

        public ITransferRequest DownloadMultipleAsync(string[] from, string to)
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
                    return new WindowsPhoneTransferRequest(request);
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
                    BackgroundTransferService.Remove(((WindowsPhoneTransferRequest) request).OriginalRequest);
                    DeleteRequestFromStorage(((WindowsPhoneTransferRequest) request).OriginalRequest);
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
            return BackgroundTransferService.Requests.Select(r => new WindowsPhoneTransferRequest(r));
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
