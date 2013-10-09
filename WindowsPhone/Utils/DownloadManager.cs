using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Phone.BackgroundTransfer;

namespace QuranPhone.Utils
{
    public class DownloadManager : IDisposable
    {
        private static DownloadManager _instance;

        private DownloadManager() {}

        public static DownloadManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DownloadManager();
                    // Cleanup old requests
                    foreach (BackgroundTransferRequest request in BackgroundTransferService.Requests)
                    {
                        if (request.TransferStatus == TransferStatus.Completed)
                        {
                            BackgroundTransferService.Remove(request);
                        }
                    }
                }
                return _instance;
            }
        }

        public void Dispose()
        {
            foreach (BackgroundTransferRequest request in BackgroundTransferService.Requests)
            {
                if (request.TransferStatus == TransferStatus.Completed)
                {
                    BackgroundTransferService.Remove(request);
                }
            }
        }

        public BackgroundTransferRequest Download(string from, string to, bool allowCellular = true)
        {
            var serverUri = new Uri(from, UriKind.Absolute);
            var phoneUri = new Uri(to, UriKind.Relative);

            try
            {
                var request = new BackgroundTransferRequest(serverUri, phoneUri);
                request.Tag = from;

                if (allowCellular)
                {
                    request.TransferPreferences = TransferPreferences.AllowCellularAndBattery;
                }

                int count = 0;
                foreach (BackgroundTransferRequest r in BackgroundTransferService.Requests)
                {
                    count++;
                    if (r.RequestUri == serverUri)
                    {
                        return r;
                    }
                    if (r.TransferStatus == TransferStatus.Completed)
                    {
                        BackgroundTransferService.Remove(r);
                        count--;
                    }
                    // Max 5 downloads
                    if (count >= 5)
                    {
                        return null;
                    }
                }
                BackgroundTransferService.Add(request);
                PersistRequestToStorage(request);
                return request;
            }
            catch (InvalidOperationException)
            {
                return GetRequest(from);
            }
        }

        private void PersistRequestToStorage(BackgroundTransferRequest request)
        {
            Uri requestUri = request.RequestUri;
            string requestUriHash = GetHash(requestUri.ToString());
            string trackerDir = QuranFileUtils.GetDowloadTrackerDirectory(false, true);
            QuranFileUtils.WriteFile(string.Format("{0}\\{1}", trackerDir, requestUriHash), request.RequestId);
        }

        private void DeleteRequestFromStorage(BackgroundTransferRequest request)
        {
            Uri requestUri = request.RequestUri;
            string requestUriHash = GetHash(requestUri.ToString());
            string trackerDir = QuranFileUtils.GetDowloadTrackerDirectory(false, true);
            QuranFileUtils.DeleteFile(string.Format("{0}\\{1}", trackerDir, requestUriHash));
        }

        public BackgroundTransferRequest GetRequest(string serverUri)
        {
            string requestUriHash = GetHash(serverUri);
            string trackerDir = QuranFileUtils.GetDowloadTrackerDirectory(false, true);
            string requestId = QuranFileUtils.ReadFile(string.Format("{0}\\{1}", trackerDir, requestUriHash));
            if (!string.IsNullOrEmpty(requestId))
            {
                return BackgroundTransferService.Find(requestId);
            }
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
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return isf.GetFileNames("/shared/transfers/*");
            }
        }

        internal void FinalizeRequests()
        {
            foreach (BackgroundTransferRequest request in BackgroundTransferService.Requests)
            {
                if (request.TransferStatus == TransferStatus.Completed)
                {
                    BackgroundTransferService.Remove(request);
                }
            }
        }

        public static string GetHash(string value)
        {
            var sha1 = new SHA1Managed();
            byte[] unencryptedByteArray = Encoding.Unicode.GetBytes(value);
            using (var stream = new MemoryStream(unencryptedByteArray))
            {
                byte[] encryptedByteArray = sha1.ComputeHash(stream);
                var encryptedStringBuilder = new StringBuilder();
                foreach (byte b in encryptedByteArray)
                {
                    encryptedStringBuilder.AppendFormat("{0:x2}", b);
                }
                return encryptedStringBuilder.ToString();
            }
        }
    }
}