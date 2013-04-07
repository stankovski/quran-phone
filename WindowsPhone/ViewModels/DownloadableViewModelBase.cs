using Microsoft.Phone.BackgroundTransfer;
using QuranPhone.UI;
using QuranPhone.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QuranPhone.ViewModels
{
    public class DownloadableViewModelBase : ViewModelBase
    {
        protected BackgroundTransferRequest downloadRequest;

        public DownloadableViewModelBase()
        {
            IsDownloading = false;
            IsIndeterminate = true;
        }

        #region Properties
        private string localUrl;
        public string LocalUrl
        {
            get { return localUrl; }
            set
            {
                if (value == localUrl)
                    return;

                localUrl = value;
                FinishPreviousDownload();

                base.OnPropertyChanged(() => LocalUrl);
            }
        }

        private string tempUrl;
        public string TempUrl
        {
            get { return tempUrl; }
            private set
            {
                if (value == tempUrl)
                    return;

                tempUrl = value;

                base.OnPropertyChanged(() => TempUrl);
            }
        }

        private string filename;
        public string FileName
        {
            get { return filename; }
            set
            {
                if (value == filename)
                    return;

                filename = value;
                TempUrl = string.Format("/shared/transfers/{0}", FileName);
                FinishPreviousDownload();

                base.OnPropertyChanged(() => FileName);
            }
        }

        private string serverUrl;
        public string ServerUrl
        {
            get { return serverUrl; }
            set
            {
                if (value == serverUrl)
                    return;

                serverUrl = value;

                if (string.IsNullOrEmpty(serverUrl)) 
                {
                    downloadRequest = DownloadManager.Instance.GetRequest(this.ServerUrl);
                    if (downloadRequest != null)
                    {
                        downloadRequest.TransferProgressChanged += TransferProgressChanged;
                        downloadRequest.TransferStatusChanged += TransferStatusChanged;
                    }
                }

                base.OnPropertyChanged(() => ServerUrl);
            }
        }

        private bool isDownloading;
        public bool IsDownloading
        {
            get { return isDownloading; }
            set
            {
                if (value == isDownloading)
                    return;

                isDownloading = value;

                base.OnPropertyChanged(() => IsDownloading);
            }
        }

        private int progress;
        public int Progress
        {
            get { return progress; }
            set
            {
                if (value == progress)
                    return;

                progress = value;

                if (progress > 0)
                    IsIndeterminate = false;

                base.OnPropertyChanged(() => Progress);
            }
        }

        private bool isIndeterminate;
        public bool IsIndeterminate
        {
            get { return isIndeterminate; }
            set
            {
                if (value == isIndeterminate)
                    return;

                isIndeterminate = value;

                base.OnPropertyChanged(() => IsIndeterminate);
            }
        }

        public bool CanDownload
        {
            get {
                return !IsDownloading;            
            }            
        }

        public bool IsInTempStorage
        {
            get
            {
                if (!IsDownloading && QuranFileUtils.FileExists(this.TempUrl))
                    return true;
                else
                    return false;
            }
        }

        public bool IsInLocalStorage
        {
            get
            {
                if (!IsDownloading && QuranFileUtils.FileExists(this.LocalUrl))
                    return true;
                else
                    return false;
            }
        }

        public string DownloadId
        {
            get
            {
                if (downloadRequest != null)
                    return downloadRequest.RequestId;
                else
                    return null;
            }
        }

        #endregion Properties

        #region Event handlers and commands

        RelayCommand downloadCommand;
        /// <summary>
        /// Returns an download command
        /// </summary>
        public ICommand DownloadCommand
        {
            get
            {
                if (downloadCommand == null)
                {
                    downloadCommand = new RelayCommand(
                        param => this.Download(),
                        param => this.CanDownload
                        );
                }
                return downloadCommand;
            }
        }

        RelayCommand cancelCommand;
        /// <summary>
        /// Returns an cancel command
        /// </summary>
        public ICommand CancelCommand
        {
            get
            {
                if (cancelCommand == null)
                {
                    cancelCommand = new RelayCommand(
                        param => this.Cancel()
                        );
                }
                return cancelCommand;
            }
        }
        
        protected void TransferStatusChanged(object sender, BackgroundTransferEventArgs e)
        {
            if (e.Request.TransferStatus != TransferStatus.Completed && e.Request.TransferStatus == TransferStatus.None)
            {
                IsDownloading = true;
            }
            else
                IsDownloading = false;
            if (e.Request.TransferStatus == TransferStatus.Completed)
            {
                QuranFileUtils.MoveFile(TempUrl, this.LocalUrl);
                DownloadManager.Instance.FinalizeRequest(e.Request);
                if (DownloadComplete != null)
                    DownloadComplete(this, null);
            }
        }

        protected void TransferProgressChanged(object sender, BackgroundTransferEventArgs e)
        {
            this.Progress = (int)(e.Request.BytesReceived * 100 / e.Request.TotalBytesToReceive);
        }

        #endregion Event handlers and commands

        #region Public methods
        public void Download()
        {
            if (QuranFileUtils.FileExists(this.LocalUrl))
                return;
            if (QuranFileUtils.FileExists(TempUrl))
                QuranFileUtils.DeleteFile(TempUrl);
            else
            {
                IsDownloading = true;
                if (downloadRequest != null)
                {
                    downloadRequest.TransferProgressChanged -= TransferProgressChanged;
                    downloadRequest.TransferStatusChanged -= TransferStatusChanged;
                }
                downloadRequest = DownloadManager.Instance.Download(this.ServerUrl, this.TempUrl);
                if (downloadRequest != null)
                {
                    downloadRequest.TransferProgressChanged += TransferProgressChanged;
                    downloadRequest.TransferStatusChanged += TransferStatusChanged;
                    if (downloadRequest.TransferStatus == TransferStatus.Completed)
                        TransferStatusChanged(this, new BackgroundTransferEventArgs(downloadRequest));
                }
            }
        }

        public TransferStatus GetDownloadStatus(string requestId)
        {
            var request = DownloadManager.Instance.GetRequest(requestId);
            if (request == null)
                return TransferStatus.None;
            else
                return request.TransferStatus;
        }

        public void FinishPreviousDownload()
        {
            if (!string.IsNullOrEmpty(this.TempUrl) && !string.IsNullOrEmpty(this.LocalUrl) &&
                QuranFileUtils.FileExists(this.TempUrl))
            {

                if (DownloadComplete != null)
                    DownloadComplete(this, null);
                QuranFileUtils.MoveFile(this.TempUrl, this.LocalUrl);
            }
        }

        public void Cancel()
        {
            if (downloadRequest != null)
            {
                DownloadManager.Instance.Cancel(downloadRequest);
                try
                {
                    QuranFileUtils.DeleteFile(this.TempUrl);
                }
                catch
                {
                    //Ignore
                }
            }
        }
        #endregion Public methods

        public event EventHandler DownloadComplete;
    }
}
