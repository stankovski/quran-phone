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
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Windows.Web;
using Windows.Networking.BackgroundTransfer;
using System.Linq;
using Windows.UI.Core;
using Windows.Storage;

namespace Quran.Core.ViewModels
{
    /// <summary>
    /// Define the DownloadableViewModelBase type.
    /// </summary>
    public class DownloadableViewModelBase : BaseViewModel
    {
        private IList<DownloadOperation> activeDownloads;
        private CancellationTokenSource cts;
        private readonly CoreDispatcher _dispatcher;

        public DownloadableViewModelBase()
        {
            IsDownloading = false;
            IsIndeterminate = true;
            cts = new CancellationTokenSource();
            _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
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
                {
                    IsIndeterminate = false;
                }

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

        private BackgroundTransferStatus downloadStatus;
        public BackgroundTransferStatus DownloadStatus
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
        #region Public methods
        /// <summary>
        /// Initialize the view model by enumerating all existing downloads.
        /// </summary>
        public override async Task Initialize()
        {
            if (activeDownloads != null)
            {
                return;
            }

            IsDownloading = false;
            IsIndeterminate = false;
            Description = null;
            InstallationStep = null;
            activeDownloads = new List<DownloadOperation>();

            IEnumerable<DownloadOperation> downloads = null;
            try
            {
                downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
            }
            catch (Exception ex)
            {
                WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult);
                await QuranApp.NativeProvider.ShowErrorMessageBox("Error getting active downloads: " + error.ToString());
                return;
            }

            if (downloads.Any())
            {
                List<Task> tasks = new List<Task>();
                foreach (var download in downloads)
                {
                    // Attach progress and completion handlers.
                    tasks.Add(HandleDownloadAsync(download, false));
                }

                // Don't await HandleDownloadAsync() in the foreach loop since we would attach to the second
                // download only when the first one completed; attach to the third download when the second one
                // completes etc. We want to attach to all downloads immediately.
                // If there are actions that need to be taken once downloads complete, await tasks here, outside
                // the loop.
                await Task.WhenAll(tasks);
            }
        }
        
        public async Task<bool> DownloadSingleFile(string serverUrl, string destinationFile, string description = null)
        {
            Reset();
            this.Description = description;
            return await Download(serverUrl, destinationFile);
        }

        
        public async Task<bool> DownloadMultiple(string[] serverUrls, string destinationFolder, string description = null)
        {
            Reset();
            this.Description = description;
            if (serverUrls == null || serverUrls.Length == 0)
            {
                throw new ArgumentNullException(nameof(serverUrls));
            }
            if (string.IsNullOrWhiteSpace(destinationFolder))
            {
                throw new ArgumentNullException(nameof(destinationFolder));
            }
            foreach (var serverUrl in serverUrls)
            {
                var fileName = Path.GetFileName(serverUrl);
                await Download(serverUrl, Path.Combine(destinationFolder, fileName));
            }
            return true;
        }
        
        private async Task<bool> Download(string serverUrl, string destinationFilePath)
        {
            if (string.IsNullOrWhiteSpace(destinationFilePath))
            {
                throw new ArgumentNullException(nameof(description));
            }
            if (string.IsNullOrWhiteSpace(serverUrl))
            {
                throw new ArgumentNullException(nameof(serverUrl));
            }

            StorageFile destinationFile;
            try
            {
                await FileUtils.EnsureDirectoryExists(Path.GetDirectoryName(destinationFilePath));
                var destinationFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(destinationFilePath));
                destinationFile = await destinationFolder.CreateFileAsync(Path.GetFileName(destinationFilePath), CreationCollisionOption.ReplaceExisting);
            }
            catch (FileNotFoundException ex)
            {
                await QuranApp.NativeProvider.
                    ShowErrorMessageBox("Error while creating file: " + ex.Message);
                return false;
            }

            BackgroundDownloader downloader = new BackgroundDownloader();
            DownloadOperation download = downloader.CreateDownload(new Uri(serverUrl), destinationFile);

            IsDownloading = true;
            InstallationStep = Description ?? AppResources.loading_message;

            // Attach progress and completion handlers.
            await HandleDownloadAsync(download, true);
            return true;
        }

        public async Task FinishActiveDownloads()
        {
            foreach (var download in activeDownloads)
            {
                if (download.Progress.Status == BackgroundTransferStatus.Completed)
                {
                    await FinishDownload(download.ResultFile.Path);
                }
            }
        }

        public async Task FinishDownload(string destinationFile)
        {
            if (destinationFile != null)
            {
                if (await FileUtils.FileExists(destinationFile))
                {
                    if (destinationFile.EndsWith(".zip"))
                    {
                        IsDownloading = true;
                        IsIndeterminate = true;
                        InstallationStep = AppResources.extracting_message;

                        await QuranApp.NativeProvider.ExtractZip(destinationFile,
                            Path.GetDirectoryName(destinationFile));
                        await FileUtils.DeleteFile(destinationFile);
                    }
                    IsIndeterminate = false;
                    IsDownloading = false;
                    IsIndeterminate = false;
                }
            }
        }
        public async Task Cancel()
        {
            if (activeDownloads.Any())
            {
                if (await QuranApp.NativeProvider.ShowQuestionMessageBox(AppResources.download_cancel_confirmation))
                {
                    Reset();
                }
            }
        }

        public void Reset()
        {
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
            IsDownloading = false;
            IsIndeterminate = true;
            Description = null;
            InstallationStep = null;
            activeDownloads.Clear();
        }
        #endregion Public methods
        private async Task HandleDownloadAsync(DownloadOperation download, bool start)
        {
            try
            {
                // Store the download so we can pause/resume.
                activeDownloads.Add(download);

                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
                if (start)
                {
                    // Start the download and attach a progress handler.
                    await download.StartAsync().AsTask(cts.Token, progressCallback);
                }
                else
                {
                    // The download was already running when the application started, re-attach the progress handler.
                    await download.AttachAsync().AsTask(cts.Token, progressCallback);
                }

                await FinishActiveDownloads();
            }
            catch (TaskCanceledException)
            {
                InstallationStep = "Cancelled";
            }
            catch (Exception ex)
            {
                WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult);
                await QuranApp.NativeProvider.ShowErrorMessageBox("Error getting active downloads: " + error.ToString());
            }
            finally
            {
                activeDownloads.Remove(download);
            }
        }

        private void DownloadProgress(DownloadOperation download)
        {
            var ignore = _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UpdateStatus();
            });
        }

        private void UpdateStatus()
        {
            var activeDownload = activeDownloads.FirstOrDefault();
            var downloadsSnapshot = new List<DownloadOperation>(activeDownloads);
            if (activeDownloads.Count > 0)
            {
                if (downloadsSnapshot.Any(o => o.Progress.Status == BackgroundTransferStatus.Running))
                {
                    UpdateInstallationStep(BackgroundTransferStatus.Running);
                }
                else if (downloadsSnapshot.All(o => o.Progress.Status == BackgroundTransferStatus.Completed))
                {
                    UpdateInstallationStep(BackgroundTransferStatus.Completed);
                }
                else
                {
                    UpdateInstallationStep(BackgroundTransferStatus.Idle);
                }
                double percent = 100;
                var totalBytesToReceive = downloadsSnapshot.Sum(o => (long)o.Progress.TotalBytesToReceive);
                var totalBytesReceived = downloadsSnapshot.Sum(o => (long)o.Progress.BytesReceived);
                if (totalBytesToReceive > 0)
                {
                    percent = totalBytesReceived * 100 / totalBytesToReceive;
                }
                Progress = (int)percent;
            }
            else if (activeDownload != null)
            {
                UpdateInstallationStep(activeDownload.Progress.Status);
                double percent = 100;
                if (activeDownload.Progress.TotalBytesToReceive > 0)
                {
                    percent = activeDownload.Progress.BytesReceived * 100 / activeDownload.Progress.TotalBytesToReceive;
                }
                Progress = (int)percent;
            }
        }

        private void UpdateInstallationStep(BackgroundTransferStatus status)
        {
            switch (status)
            {
                case BackgroundTransferStatus.PausedByApplication:
                case BackgroundTransferStatus.PausedSystemPolicy:
                case BackgroundTransferStatus.Idle:
                    InstallationStep = AppResources.waiting;
                    IsDownloading = true;
                    IsIndeterminate = true;
                    break;
                case BackgroundTransferStatus.PausedNoNetwork:
                case BackgroundTransferStatus.PausedCostedNetwork:
                    InstallationStep = AppResources.waiting_for_wifi;
                    IsDownloading = true;
                    IsIndeterminate = true;
                    break;
                case BackgroundTransferStatus.Running:
                    InstallationStep = Description ?? AppResources.loading_message;
                    IsDownloading = true;
                    IsIndeterminate = false;
                    break;
                case BackgroundTransferStatus.Completed:
                    InstallationStep = null;
                    IsDownloading = false;
                    IsIndeterminate = false;
                    if (DownloadComplete != null)
                    {
                        DownloadComplete(this, null);
                    }
                    break;
                case BackgroundTransferStatus.Canceled:
                    InstallationStep = null;
                    IsDownloading = false;
                    IsIndeterminate = false;
                    if (DownloadCancelled != null)
                    {
                        DownloadCancelled(this, null);
                    }
                    break;
            }
        }

        public override void Dispose()
        {
            if (cts != null)
            {
                cts.Dispose();
                cts = null;
            }
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        public event EventHandler DownloadComplete;
        public event EventHandler DownloadCancelled;
    }
}
