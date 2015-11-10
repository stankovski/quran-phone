using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Phone.BackgroundTransfer;
using Quran.Core.Common;
using Quran.Core.Interfaces;
using Quran.Core.Utils;

namespace Quran.WindowsPhone.NativeProvider
{
    public class FakeWindowsPhoneDownloadManager : IDownloadManager
    {
        private readonly Dictionary<string, ITransferRequest> transfers =
            new Dictionary<string, ITransferRequest>();
        public ITransferRequest DownloadAsync(string from, string to)
        {
            var serverUri = new Uri(from, UriKind.Absolute);
            var phoneUri = new Uri(to, UriKind.Relative);

            try
            {
                var request = new FakeWindowsPhoneTransferRequest {RequestId = serverUri.ToString(), RequestUri = serverUri, DownloadLocation = phoneUri, TotalBytesToReceive = 100, BytesReceived = 0, TransferStatus = FileTransferStatus.Transferring };
                PersistRequestToStorage(request);
                transfers[request.RequestId] = request;
                new TaskFactory().StartNew(() =>
                {
                    for (int i = 0; i < 100; i++)
                    {
                        request.ChangeProgress(i, FileTransferStatus.Transferring);
                        Thread.Sleep(500);
                    }
                    request.ChangeProgress(100, FileTransferStatus.Completed);
                });
                return request;
            }
            catch (InvalidOperationException)
            {
                return GetRequest(from);
            }   
        }

        public ITransferRequest DownloadMultipleAsync(string[] from, string to)
        {
            return null;
        }

        private void PersistRequestToStorage(ITransferRequest request)
        {
            var requestUri = request.RequestUri;
            var requestUriHash = CryptoUtils.GetHash(requestUri.ToString());
            var trackerDir = FileUtils.GetDowloadTrackerDirectory(false, true);
            FileUtils.WriteFile(string.Format("{0}\\{1}", trackerDir, requestUriHash), request.RequestId);
        }

        private void DeleteRequestFromStorage(ITransferRequest request)
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
            if (!string.IsNullOrEmpty(requestId) && transfers.ContainsKey(requestId))
                return transfers[requestId];
            else
                return null;
        }

        public void Cancel(ITransferRequest request)
        {
            FinalizeRequest(request);
        }

        public void FinalizeRequest(ITransferRequest request)
        {
            if (transfers.ContainsKey(request.RequestId))
            {
                DeleteRequestFromStorage(request);
            }
        }

        public IEnumerable<ITransferRequest> GetAllRequests()
        {
            return transfers.Values;
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
            transfers.Clear();
        }

        internal void FinalizeRequests()
        {
            transfers.Clear();
        }
    }
}
