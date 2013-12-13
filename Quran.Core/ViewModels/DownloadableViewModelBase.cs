// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the DownloadableViewModelBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Quran.Core.Common;
using Quran.Core.Properties;
using Quran.Core.Utils;

namespace Quran.Core.ViewModels
{
    using System.Windows.Input;

    using Cirrious.MvvmCross.ViewModels;

    /// <summary>
    /// Define the DownloadableViewModelBase type.
    /// </summary>
    public class DownloadableViewModelBase : BaseViewModel
    {
        protected ITransferRequest downloadRequest;

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

                base.RaisePropertyChanged(() => LocalUrl);
            }
        }

        public string TempUrl
        {
            get
            {
                if (FileName == null)
                    return null;
                else
                    return string.Format("/shared/transfers/{0}", FileName);
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

                base.RaisePropertyChanged(() => FileName);
            }
        }

        private string description;
        public string Description
        {
            get { return description; }
            set
            {
                if (value == description)
                    return;

                description = value;

                base.RaisePropertyChanged(() => Description);
            }
        }

        private bool isCompressed;
        public bool IsCompressed
        {
            get { return isCompressed; }
            set
            {
                if (value == isCompressed)
                    return;

                isCompressed = value;

                base.RaisePropertyChanged(() => IsCompressed);
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

                if (!string.IsNullOrEmpty(serverUrl))
                {
                    // Set FileName
                    FileName = PathHelper.GetFileName(serverUrl);

                    // Set IsCompressed
                    IsCompressed = FileName != null && FileName.EndsWith(".zip");

                    // Finish any stuck files
                    FinishDownload().Wait();

                    // Check existing downloads
                    downloadRequest = QuranApp.NativeProvider.DownloadManager.GetRequest(this.ServerUrl);
                    if (downloadRequest != null)
                    {
                        UpdateDownloadStatus(downloadRequest.TransferStatus);
                        downloadRequest.TransferProgressChanged += TransferProgressChanged;
                        downloadRequest.TransferStatusChanged += TransferStatusChanged;
                    }
                }

                base.RaisePropertyChanged(() => ServerUrl);
                base.RaisePropertyChanged(() => FileName);
                base.RaisePropertyChanged(() => IsCompressed);
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

                base.RaisePropertyChanged(() => IsDownloading);
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

                base.RaisePropertyChanged(() => Progress);
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

                base.RaisePropertyChanged(() => IsIndeterminate);
            }
        }

        private FileTransferStatus downloadStatus;
        public FileTransferStatus DownloadStatus
        {
            get { return downloadStatus; }
            set
            {
                if (value == downloadStatus)
                    return;

                downloadStatus = value;

                base.RaisePropertyChanged(() => DownloadStatus);
            }
        }

        public bool CanDownload
        {
            get
            {
                return !IsDownloading;
            }
        }

        public bool IsInTempStorage
        {
            get
            {
                if (!IsDownloading && FileUtils.FileExists(this.TempUrl))
                    return true;
                else
                    return false;
            }
        }

        public bool IsInLocalStorage
        {
            get
            {
                if (!IsDownloading && FileUtils.FileExists(this.LocalUrl))
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

        private string installationStep;
        public string InstallationStep
        {
            get { return installationStep; }
            set
            {
                if (value == installationStep)
                    return;

                installationStep = value;

                base.RaisePropertyChanged(() => InstallationStep);
            }
        }

        #endregion Properties

        #region Event handlers and commands

        MvxCommand downloadCommand;
        /// <summary>
        /// Returns an download command
        /// </summary>
        public ICommand DownloadCommand
        {
            get
            {
                if (downloadCommand == null)
                {
                    downloadCommand = new MvxCommand(() => Download(), ()=> this.CanDownload);
                }
                return downloadCommand;
            }
        }

        MvxCommand cancelCommand;
        /// <summary>
        /// Returns an cancel command
        /// </summary>
        public ICommand CancelCommand
        {
            get
            {
                if (cancelCommand == null)
                {
                    cancelCommand = new MvxCommand(Cancel);
                }
                return cancelCommand;
            }
        }

        protected async void TransferStatusChanged(object sender, TransferEventArgs e)
        {
            UpdateDownloadStatus(e.Request.TransferStatus);
            if (e.Request.TransferStatus == FileTransferStatus.Completed)
            {
                await FinishDownload();
                QuranApp.NativeProvider.DownloadManager.FinalizeRequest(e.Request);
                if (DownloadComplete != null)
                    DownloadComplete(this, null);
            }
            if (e.Request.TransferStatus == FileTransferStatus.Cancelled)
            {
                await FinishDownload();
                QuranApp.NativeProvider.DownloadManager.FinalizeRequest(e.Request);
                if (DownloadCancelled != null)
                    DownloadCancelled(this, null);
            }
        }

        protected void TransferProgressChanged(object sender, TransferEventArgs e)
        {
            this.Progress = (int)(e.Request.BytesReceived * 100 / e.Request.TotalBytesToReceive);
        }

        #endregion Event handlers and commands

        #region Public methods
        public async Task<bool> Download(string serverUrl, string localUrl)
        {
            Reset();
            this.ServerUrl = serverUrl;
            this.LocalUrl = localUrl;
            return await Download();
        }

        public async Task<bool> Download(string serverUrl, string localUrl, string description)
        {
            Reset();
            this.ServerUrl = serverUrl;
            this.LocalUrl = localUrl;
            this.Description = description;
            return await DownloadOneFile();
        }

        public async Task<bool> DownloadMultiple(string[] serverUrls, string localUrl, string description)
        {
            Reset();
            this.LocalUrl = localUrl;
            this.Description = description;
            return await DownloadMultipleFile(serverUrls);
        }

        public async Task<bool> Download()
        {
            return await DownloadOneFile();
        }

        private async Task<bool> DownloadMultipleFile(string[] serverUrls)
        {
            IsDownloading = true;
            InstallationStep = Description ?? AppResources.loading_message;
            if (downloadRequest != null)
            {
                downloadRequest.TransferProgressChanged -= TransferProgressChanged;
                downloadRequest.TransferStatusChanged -= TransferStatusChanged;
            }
            downloadRequest = QuranApp.NativeProvider.DownloadManager.DownloadMultipleAsync(serverUrls, this.LocalUrl);
            if (downloadRequest != null)
            {
                downloadRequest.TransferProgressChanged += TransferProgressChanged;
                downloadRequest.TransferStatusChanged += TransferStatusChanged;
                if (downloadRequest.TransferStatus == FileTransferStatus.Completed)
                {
                    TransferStatusChanged(this, new TransferEventArgs(downloadRequest));
                }
            }
            var tcs = new TaskCompletionSource<bool>();
            DownloadComplete += (s, e) => tcs.TrySetResult(true);
            DownloadCancelled += (s, e) => tcs.TrySetResult(false);
            var result = await tcs.Task;
            IsDownloading = false;
            return result;
        }

        private async Task<bool> DownloadOneFile()
        {
            if (FileUtils.FileExists(this.LocalUrl))
                return true;
            if (FileUtils.FileExists(TempUrl))
                FileUtils.DeleteFile(TempUrl);

            IsDownloading = true;
            InstallationStep = Description ?? AppResources.loading_message;
            if (downloadRequest != null)
            {
                downloadRequest.TransferProgressChanged -= TransferProgressChanged;
                downloadRequest.TransferStatusChanged -= TransferStatusChanged;
            }
            downloadRequest = QuranApp.NativeProvider.DownloadManager.DownloadAsync(this.ServerUrl, this.TempUrl);
            if (downloadRequest != null)
            {
                downloadRequest.TransferProgressChanged += TransferProgressChanged;
                downloadRequest.TransferStatusChanged += TransferStatusChanged;
                if (downloadRequest.TransferStatus == FileTransferStatus.Completed)
                {
                    TransferStatusChanged(this, new TransferEventArgs(downloadRequest));
                }
            }
            var tcs = new TaskCompletionSource<bool>();
            DownloadComplete += (s, e) => tcs.TrySetResult(true);
            DownloadCancelled += (s, e) => tcs.TrySetResult(false);
            return await tcs.Task;
        }

        public async Task<bool> FinishDownload()
        {
            if (!string.IsNullOrEmpty(this.TempUrl) && !string.IsNullOrEmpty(this.LocalUrl) &&
                FileUtils.FileExists(this.TempUrl))
            {
                if (IsCompressed)
                {
                    IsDownloading = true;
                    IsIndeterminate = true;
                    var result = await ExtractZipAndFinalize();
                    IsDownloading = false;
                    IsIndeterminate = false;
                    return result;
                }
                else
                {
                    try
                    {
                        FinalizeFile();
                        return true;
                    }
                    catch
                    {
                        QuranApp.NativeProvider.ShowErrorMessageBox(
                            "Something went wrong with the download. Please try again.");
                        return false;
                    }
                    finally
                    {
                        IsIndeterminate = false;
                        IsDownloading = false;
                    }
                }
            }
            return true;
        }

        private void FinalizeFile()
        {
            IsDownloading = true;
            IsIndeterminate = true;
            FileUtils.MoveFile(TempUrl, this.LocalUrl);
            IsDownloading = false;
            IsIndeterminate = false;
        }

        public async Task<bool> ExtractZipAndFinalize()
        {
            if (FileUtils.FileExists(TempUrl))
            {
                IsIndeterminate = true;
                InstallationStep = AppResources.extracting_message;

                IsIndeterminate = true;

                var folderToExtractInto = LocalUrl;
                // Check if LocalUrl is a file or a folder
                if (System.IO.Path.HasExtension(LocalUrl))
                {
                    folderToExtractInto = System.IO.Path.GetDirectoryName(LocalUrl);
                }

                bool result = await new TaskFactory().StartNew(() => FileUtils.ExtractZipFile(TempUrl, folderToExtractInto));

                if (!result)
                    return false;

                FileUtils.DeleteFile(TempUrl);
                IsIndeterminate = false;
            }
            return true;
        }

        public void Cancel()
        {
            if (downloadRequest != null)
            {
                if (QuranApp.NativeProvider.ShowQuestionMessageBox(AppResources.download_cancel_confirmation))
                {
                    QuranApp.NativeProvider.DownloadManager.Cancel(downloadRequest);
                    IsDownloading = false;
                    try
                    {
                        FileUtils.DeleteFile(this.TempUrl);
                    }
                    catch
                    {
                        //Ignore
                    }
                }
            }
        }

        public void Reset()
        {
            if (downloadRequest != null)
            {
                downloadRequest.TransferProgressChanged -= TransferProgressChanged;
                downloadRequest.TransferStatusChanged -= TransferStatusChanged;
            }
            downloadRequest = null;
            IsDownloading = true;
            IsIndeterminate = true;
            Description = null;
            FileName = null;
            InstallationStep = null;
            IsCompressed = false;
            LocalUrl = null;
            FileName = null;
            ServerUrl = null;
        }
        #endregion Public methods

        private void UpdateDownloadStatus(FileTransferStatus status)
        {
            DownloadStatus = status;
            switch (status)
            {
                case FileTransferStatus.Paused:
                case FileTransferStatus.Transferring:
                case FileTransferStatus.Waiting:
                case FileTransferStatus.WaitingForExternalPower:
                case FileTransferStatus.WaitingForExternalPowerDueToBatterySaverMode:
                case FileTransferStatus.WaitingForNonVoiceBlockingNetwork:
                case FileTransferStatus.WaitingForWiFi:
                    IsDownloading = true;
                    break;
                default:
                    IsDownloading = false;
                    break;
            }
            UpdateInstallationStep(status);
        }

        private void UpdateInstallationStep(FileTransferStatus status)
        {
            switch (status)
            {
                case FileTransferStatus.Paused:
                case FileTransferStatus.Waiting:
                case FileTransferStatus.WaitingForNonVoiceBlockingNetwork:
                    InstallationStep = AppResources.waiting;
                    break;
                case FileTransferStatus.WaitingForExternalPower:
                case FileTransferStatus.WaitingForExternalPowerDueToBatterySaverMode:
                    InstallationStep = AppResources.waiting_for_power;
                    break;
                case FileTransferStatus.WaitingForWiFi:
                    InstallationStep = AppResources.waiting_for_wifi;
                    break;
                case FileTransferStatus.Completed:
                case FileTransferStatus.Transferring:
                    InstallationStep = Description ?? AppResources.loading_message;
                    break;
            }
        }

        public event EventHandler DownloadComplete;
        public event EventHandler DownloadCancelled;
    }
}
