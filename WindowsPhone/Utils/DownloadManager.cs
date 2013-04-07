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
                return request;
            }
            catch (InvalidOperationException)
            {
                return BackgroundTransferService.Find(request.Tag);
            }            
        }

        public BackgroundTransferRequest GetRequest(string requestId)
        {
            return BackgroundTransferService.Find(requestId);
        }

        public void FinalizeRequest(BackgroundTransferRequest request)
        {
            BackgroundTransferService.Remove(request);
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
