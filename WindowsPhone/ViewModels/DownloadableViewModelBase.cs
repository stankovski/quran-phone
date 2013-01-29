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
        private string tempPath;

        public DownloadableViewModelBase()
        {
            CanDownload = true;
            Downloading = false;
        }

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

        private string filename;
        public string FileName
        {
            get { return filename; }
            set
            {
                if (value == filename)
                    return;

                filename = value;
                tempPath = string.Format("/shared/transfers/{0}", FileName);
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

        private bool downloading;
        public bool Downloading
        {
            get { return downloading; }
            set
            {
                if (value == downloading)
                    return;

                downloading = value;

                base.OnPropertyChanged(() => Downloading);
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

                base.OnPropertyChanged(() => Progress);
            }
        }

        private bool canDownload;
        public bool CanDownload
        {
            get { return canDownload; }
            set
            {
                if (value == canDownload)
                    return;

                canDownload = value;

                base.OnPropertyChanged(() => CanDownload);
            }
        }

        RelayCommand downloadCommand;
        /// <summary>
        /// Returns an undo command
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

        protected void TransferStatusChanged(object sender, BackgroundTransferEventArgs e)
        {
            if (e.Request.TransferStatus != TransferStatus.Completed && e.Request.TransferStatus == TransferStatus.None)
            {
                Downloading = true;
                CanDownload = false;
            }
            else
                Downloading = false;
            if (e.Request.TransferStatus == TransferStatus.Completed)
            {
                if (DownloadComplete != null)
                    DownloadComplete(this, null);
                QuranFileUtils.MoveFile(tempPath, this.LocalUrl);
                CanDownload = false;
                DownloadManager.Instance.FinalizeRequest(e.Request);
            }
        }

        protected void TransferProgressChanged(object sender, BackgroundTransferEventArgs e)
        {
            this.Progress = (int)(e.Request.BytesReceived * 100 / e.Request.TotalBytesToReceive);
        }

        public void Download()
        {
            if (QuranFileUtils.FileExists(this.LocalUrl))
                return;
            if (QuranFileUtils.FileExists(tempPath))
                QuranFileUtils.DeleteFile(tempPath);
            else
            {
                Downloading = true;
                if (downloadRequest != null)
                {
                    downloadRequest.TransferProgressChanged -= TransferProgressChanged;
                    downloadRequest.TransferStatusChanged -= TransferStatusChanged;
                }
                downloadRequest = DownloadManager.Instance.Download(this.ServerUrl, this.tempPath);
                downloadRequest.TransferProgressChanged += TransferProgressChanged;
                downloadRequest.TransferStatusChanged += TransferStatusChanged;
            }
        }

        public void FinishPreviousDownload()
        {
            if (!string.IsNullOrEmpty(tempPath) && !string.IsNullOrEmpty(this.LocalUrl) &&
                QuranFileUtils.FileExists(tempPath))
            {

                if (DownloadComplete != null)
                    DownloadComplete(this, null);
                QuranFileUtils.MoveFile(tempPath, this.LocalUrl);
                CanDownload = false; 
            }
        }

        public event EventHandler DownloadComplete;
    }
}
