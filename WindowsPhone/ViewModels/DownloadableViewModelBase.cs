using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Phone.BackgroundTransfer;
using QuranPhone.Resources;
using QuranPhone.UI;
using QuranPhone.Utils;

namespace QuranPhone.ViewModels
{
    public class DownloadableViewModelBase : ViewModelBase
    {
        protected BackgroundTransferRequest DownloadRequest;

        public DownloadableViewModelBase()
        {
            IsDownloading = false;
            IsIndeterminate = true;
        }

        #region Properties

        private TransferStatus _downloadStatus;
        private string _filename;
        private bool _isCompressed;
        private bool _isDownloading;
        private bool _isIndeterminate;
        private string _localUrl;
        private int _progress;
        private string _serverUrl;
        private string _tempUrl;

        public string LocalUrl
        {
            get { return _localUrl; }
            set
            {
                _localUrl = value;
                base.OnPropertyChanged(() => LocalUrl);
            }
        }

        public string TempUrl
        {
            get { return _tempUrl; }
            private set
            {
                _tempUrl = value;
                base.OnPropertyChanged(() => TempUrl);
            }
        }

        public string FileName
        {
            get { return _filename; }
            set
            {
                _filename = value;
                TempUrl = string.Format("/shared/transfers/{0}", FileName);
                FinishPreviousDownload();
                base.OnPropertyChanged(() => FileName);
            }
        }

        public bool IsCompressed
        {
            get { return _isCompressed; }
            set
            {
                _isCompressed = value;
                base.OnPropertyChanged(() => IsCompressed);
            }
        }

        public string ServerUrl
        {
            get { return _serverUrl; }
            set
            {
                _serverUrl = value;

                if (!string.IsNullOrEmpty(_serverUrl))
                {
                    DownloadRequest = DownloadManager.Instance.GetRequest(ServerUrl);
                    if (DownloadRequest != null)
                    {
                        UpdateDownloadStatus(DownloadRequest.TransferStatus);
                        DownloadRequest.TransferProgressChanged += TransferProgressChanged;
                        DownloadRequest.TransferStatusChanged += TransferStatusChanged;
                    }
                }
                base.OnPropertyChanged(() => ServerUrl);
            }
        }

        public bool IsDownloading
        {
            get { return _isDownloading; }
            set
            {
                _isDownloading = value;
                base.OnPropertyChanged(() => IsDownloading);
            }
        }

        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                if (_progress > 0)
                {
                    IsIndeterminate = false;
                }
                base.OnPropertyChanged(() => Progress);
            }
        }

        public bool IsIndeterminate
        {
            get { return _isIndeterminate; }
            set
            {
                _isIndeterminate = value;
                base.OnPropertyChanged(() => IsIndeterminate);
            }
        }

        public TransferStatus DownloadStatus
        {
            get { return _downloadStatus; }
            set
            {
                _downloadStatus = value;
                base.OnPropertyChanged(() => DownloadStatus);
            }
        }

        public bool CanDownload
        {
            get { return !IsDownloading; }
        }

        public bool IsInTempStorage
        {
            get
            {
                if (!IsDownloading && QuranFileUtils.FileExists(TempUrl))
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsInLocalStorage
        {
            get
            {
                if (!IsDownloading && QuranFileUtils.FileExists(LocalUrl))
                {
                    return true;
                }
                return false;
            }
        }

        public string DownloadId
        {
            get
            {
                if (DownloadRequest != null)
                {
                    return DownloadRequest.RequestId;
                }
                return null;
            }
        }

        #endregion Properties

        #region Event handlers and commands

        private RelayCommand cancelCommand;
        private RelayCommand downloadCommand;

        /// <summary>
        ///     Returns an download command
        /// </summary>
        public ICommand DownloadCommand
        {
            get
            {
                if (downloadCommand == null)
                {
                    downloadCommand = new RelayCommand(param => Download(), param => CanDownload);
                }
                return downloadCommand;
            }
        }

        /// <summary>
        ///     Returns an cancel command
        /// </summary>
        public ICommand CancelCommand
        {
            get
            {
                if (cancelCommand == null)
                {
                    cancelCommand = new RelayCommand(param => Cancel());
                }
                return cancelCommand;
            }
        }

        protected void TransferStatusChanged(object sender, BackgroundTransferEventArgs e)
        {
            UpdateDownloadStatus(e.Request.TransferStatus);
            if (e.Request.TransferStatus == TransferStatus.Completed)
            {
                if (IsCompressed)
                {
                    IsIndeterminate = true;
                    QuranFileUtils.ExtractZipFile(TempUrl, QuranFileUtils.GetQuranDatabaseDirectory(false, true));
                    IsIndeterminate = false;
                }
                else
                {
                    try
                    {
                        QuranFileUtils.MoveFile(TempUrl, LocalUrl);
                    }
                    catch
                    {
                        MessageBox.Show("Something went wrong with the download. Please try again.", "Error",
                            MessageBoxButton.OK);
                    }
                }
                DownloadManager.Instance.FinalizeRequest(e.Request);
                if (DownloadComplete != null)
                {
                    DownloadComplete(this, null);
                }
            }
        }

        protected void TransferProgressChanged(object sender, BackgroundTransferEventArgs e)
        {
            Progress = (int) (e.Request.BytesReceived*100/e.Request.TotalBytesToReceive);
        }

        #endregion Event handlers and commands

        #region Public methods

        public void Download()
        {
            if (QuranFileUtils.FileExists(LocalUrl))
            {
                return;
            }
            if (QuranFileUtils.FileExists(TempUrl))
            {
                QuranFileUtils.DeleteFile(TempUrl);
            }

            IsDownloading = true;
            if (DownloadRequest != null)
            {
                DownloadRequest.TransferProgressChanged -= TransferProgressChanged;
                DownloadRequest.TransferStatusChanged -= TransferStatusChanged;
            }
            DownloadRequest = DownloadManager.Instance.Download(ServerUrl, TempUrl);
            if (DownloadRequest != null)
            {
                DownloadRequest.TransferProgressChanged += TransferProgressChanged;
                DownloadRequest.TransferStatusChanged += TransferStatusChanged;
                if (DownloadRequest.TransferStatus == TransferStatus.Completed)
                {
                    TransferStatusChanged(this, new BackgroundTransferEventArgs(DownloadRequest));
                }
            }
        }

        public void FinishPreviousDownload()
        {
            if (!string.IsNullOrEmpty(TempUrl) && !string.IsNullOrEmpty(LocalUrl) && QuranFileUtils.FileExists(TempUrl))
            {
                if (DownloadComplete != null)
                {
                    DownloadComplete(this, null);
                }
                try
                {
                    QuranFileUtils.MoveFile(TempUrl, LocalUrl);
                }
                catch
                {
                    MessageBox.Show("Something went wrong with the download. Please try again.", "Error",
                        MessageBoxButton.OK);
                }
            }
        }

        public void Cancel()
        {
            if (DownloadRequest != null)
            {
                if (
                    MessageBox.Show(AppResources.download_cancel_confirmation, "Cancel download",
                        MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    DownloadManager.Instance.Cancel(DownloadRequest);
                    try
                    {
                        QuranFileUtils.DeleteFile(TempUrl);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }
            }
        }

        #endregion Public methods

        private void UpdateDownloadStatus(TransferStatus status)
        {
            DownloadStatus = status;
            switch (status)
            {
                case TransferStatus.Paused:
                case TransferStatus.Transferring:
                case TransferStatus.Waiting:
                case TransferStatus.WaitingForExternalPower:
                case TransferStatus.WaitingForExternalPowerDueToBatterySaverMode:
                case TransferStatus.WaitingForNonVoiceBlockingNetwork:
                case TransferStatus.WaitingForWiFi:
                    IsDownloading = true;
                    break;
                default:
                    IsDownloading = false;
                    break;
            }
        }

        public event EventHandler DownloadComplete;
    }
}