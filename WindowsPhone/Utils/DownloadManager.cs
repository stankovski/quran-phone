using Microsoft.Phone.BackgroundTransfer;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuranPhone.Utils
{
    public class DownloadManager : IDisposable
    {
        private DownloadManager() { }

        private static DownloadManager instance;
        public static DownloadManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DownloadManager();
                    // Cleanup old requests
                    foreach (var request in BackgroundTransferService.Requests)
                    {
                        if (request.TransferStatus == TransferStatus.Completed)
                            BackgroundTransferService.Remove(request);
                    }
                }
                return instance;
            }
        }

        public BackgroundTransferRequest Download(string from, string to, bool allowCellular = true)
        {
            var serverUri = new Uri(from, UriKind.Absolute);
            var phoneUri = new Uri(to, UriKind.Relative);
            var request = new BackgroundTransferRequest(serverUri, phoneUri);
            request.Tag = from;
            if (allowCellular)
                request.TransferPreferences = TransferPreferences.AllowCellularAndBattery;
            try
            {
                int count = 0;
                foreach (var r in BackgroundTransferService.Requests)
                {
                    count++;
                    if (r.RequestUri == serverUri)
                        return r;
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
                return request;
            }
            catch (InvalidOperationException)
            {
                return BackgroundTransferService.Find(request.Tag);
            }            
        }

        private void PersistRequestToStorage(BackgroundTransferRequest request)
        {
            var requestUri = request.RequestUri;
            var requestUriHash = CryptoUtils.GetHash(requestUri.ToString());
            var trackerDir = QuranFileUtils.GetDowloadTrackerDirectory(false, true);
            QuranFileUtils.WriteFile(string.Format("{0}\\{1}", trackerDir, requestUriHash), request.RequestId);
        }

        private void DeleteRequestFromStorage(BackgroundTransferRequest request)
        {
            var requestUri = request.RequestUri;
            var requestUriHash = CryptoUtils.GetHash(requestUri.ToString());
            var trackerDir = QuranFileUtils.GetDowloadTrackerDirectory(false, true);
            QuranFileUtils.DeleteFile(string.Format("{0}\\{1}", trackerDir, requestUriHash));
        }

        public BackgroundTransferRequest GetRequest(string serverUri)
        {
            var requestUriHash = CryptoUtils.GetHash(serverUri);
            var trackerDir = QuranFileUtils.GetDowloadTrackerDirectory(false, true);
            var requestId = QuranFileUtils.ReadFile(string.Format("{0}\\{1}", trackerDir, requestUriHash));
            if (!string.IsNullOrEmpty(requestId))
                return BackgroundTransferService.Find(requestId);
            else
                return null;
        }

        public void Cancel(BackgroundTransferRequest request)
        {
            FinalizeRequest(request);
        }

        public void FinalizeRequest(BackgroundTransferRequest request)
        {
            if (BackgroundTransferService.Find(request.RequestId) != null)
            {
                BackgroundTransferService.Remove(request);
                DeleteRequestFromStorage(request);
            }
        }

        public IEnumerable<BackgroundTransferRequest> GetAllRequests()
        {
            return BackgroundTransferService.Requests;
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
