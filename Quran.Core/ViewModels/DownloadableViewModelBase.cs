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
using System.Windows.Input;
using System.IO;

namespace Quran.Core.ViewModels
{

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

                base.OnPropertyChanged(() => LocalUrl);
            }
        }

        public string TempUrl
        {
            get
            {
                if (FileName == null)
                    return null;
                else
                    return Path.Combine(FileUtils.GetTempDirectory(), FileName);
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

                base.OnPropertyChanged(() => FileName);
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

                base.OnPropertyChanged(() => Description);
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

                base.OnPropertyChanged(() => IsCompressed);
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
                    FileName = Path.GetFileName(serverUrl);

                    // Set IsCompressed
                    IsCompressed = FileName != null && FileName.EndsWith(".zip");

                    // Finish any stuck files
                    FinishDownload().Wait();

                    // Check existing downloads
                    downloadRequest = FileUtils.RunSync(() => QuranApp.NativeProvider.DownloadManager.GetRequest(this.ServerUrl));
                    if (downloadRequest != null)
                    {
                        UpdateDownloadStatus(downloadRequest.TransferStatus);
                        downloadRequest.TransferProgressChanged += TransferProgressChanged;
                    }
                }

                base.OnPropertyChanged(() => ServerUrl);
                base.OnPropertyChanged(() => FileName);
                base.OnPropertyChanged(() => IsCompressed);
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

        private FileTransferStatus downloadStatus;
        public FileTransferStatus DownloadStatus
        {
            get { return downloadStatus; }
            set
            {
                if (value == downloadStatus)
                    return;

                downloadStatus = value;

                base.OnPropertyChanged(() => DownloadStatus);
            }
        }

        public bool CanDownload
        {
            get
            {
                return !IsDownloading;
            }
        }

        public async Task<bool> IsInTempStorage()
        {
            if (!IsDownloading && await FileUtils.FileExists(this.TempUrl))
                return true;
            else
                return false;
        }

        public bool IsInLocalStorage
        {
            get
            {
                if (!IsDownloading && FileUtils.RunSync(()=> FileUtils.FileExists(this.LocalUrl)))
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

                base.OnPropertyChanged(() => InstallationStep);
            }
        }

        #endregion Properties

        #region Event handlers and commands

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
            }
            downloadRequest = await QuranApp.NativeProvider.DownloadManager.DownloadMultipleAsync(serverUrls, this.LocalUrl);
            if (downloadRequest != null)
            {
                downloadRequest.TransferProgressChanged += TransferProgressChanged;
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
            if (await FileUtils.FileExists(this.LocalUrl))
                return true;
            if (await FileUtils.FileExists(TempUrl))
                await FileUtils.DeleteFile(TempUrl);

            IsDownloading = true;
            InstallationStep = Description ?? AppResources.loading_message;
            if (downloadRequest != null)
            {
                downloadRequest.TransferProgressChanged -= TransferProgressChanged;
            }
            downloadRequest = await QuranApp.NativeProvider.DownloadManager.DownloadAsync(this.ServerUrl, this.TempUrl);
            if (downloadRequest != null)
            {
                downloadRequest.TransferProgressChanged += TransferProgressChanged;
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
                await FileUtils.FileExists(this.TempUrl))
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
                        await FinalizeFile();
                        return true;
                    }
                    catch
                    {
                        await QuranApp.NativeProvider.ShowErrorMessageBox(
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

        private async Task FinalizeFile()
        {
            IsDownloading = true;
            IsIndeterminate = true;
            await FileUtils.MoveFile(TempUrl, this.LocalUrl);
            IsDownloading = false;
            IsIndeterminate = false;
        }

        public async Task<bool> ExtractZipAndFinalize()
        {
            if (await FileUtils.FileExists(TempUrl))
            {
                IsIndeterminate = true;
                InstallationStep = AppResources.extracting_message;

                IsIndeterminate = true;

                var folderToExtractInto = LocalUrl;
                // Check if LocalUrl is a file or a folder
                if (Path.HasExtension(LocalUrl))
                {
                    folderToExtractInto = Path.GetDirectoryName(LocalUrl);
                }

                await QuranApp.NativeProvider.ExtractZip(TempUrl, folderToExtractInto);
                await FileUtils.DeleteFile(TempUrl);
                IsIndeterminate = false;
            }
            return true;
        }

        public async Task Cancel()
        {
            if (downloadRequest != null)
            {
                if (await QuranApp.NativeProvider.ShowQuestionMessageBox(AppResources.download_cancel_confirmation))
                {
                    await QuranApp.NativeProvider.DownloadManager.Cancel(downloadRequest);
                    IsDownloading = false;
                    try
                    {
                        await FileUtils.DeleteFile(this.TempUrl);
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
