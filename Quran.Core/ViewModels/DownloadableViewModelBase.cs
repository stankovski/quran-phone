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
using Microsoft.ApplicationInsights;

namespace Quran.Core.ViewModels
{
    /// <summary>
    /// Define the DownloadableViewModelBase type.
    /// </summary>
    public class DownloadableViewModelBase : BaseViewModel
    {
        private IList<DownloadOperation> _activeDownloads;
        private CancellationTokenSource _cts;
        private readonly CoreDispatcher _dispatcher;
        public const string DownloadExtension = ".download";
        private TelemetryClient telemetry = new TelemetryClient();

        public DownloadableViewModelBase()
        {
            IsDownloading = false;
            IsIndeterminate = true;
            _cts = new CancellationTokenSource();
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
            if (_activeDownloads != null)
            {
                return;
            }

            IsDownloading = false;
            IsIndeterminate = false;
            Description = null;
            InstallationStep = null;
            _activeDownloads = new List<DownloadOperation>();

            IEnumerable<DownloadOperation> downloads = null;
            try
            {
                downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex, new Dictionary<string, string> { { "Scenario", "InitializeDownloads" } });
                WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult);
                await QuranApp.NativeProvider.ShowErrorMessageBox("Error getting active downloads: " + error.ToString());
                return;
            }

            if (downloads.Any())
            {
                await HandleDownloadsAsync(downloads.ToList(), false);
            }
        }

        public override Task Refresh()
        {
            return Task.FromResult(0);
        }

        public async Task<bool> DownloadSingleFile(string serverUrl, string destinationFile, string description = null)
        {
            Reset();
            Description = description;
            IsDownloading = true;
            InstallationStep = description ?? Resources.loading_message;

            var destinationFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(destinationFile));
            var download = await GetDownloadOperation(serverUrl, destinationFolder, destinationFile);
            // Attach progress and completion handlers.
            return await HandleDownloadsAsync(new[] { download }.ToList(), true);
        }

        
        public async Task<bool> DownloadMultiple(string[] serverUrls, StorageFolder destinationFolder, string description = null)
        {
            Reset();
            Description = description;
            IsDownloading = true;
            InstallationStep = description ?? Resources.loading_message;

            if (serverUrls == null || serverUrls.Length == 0)
            {
                throw new ArgumentNullException(nameof(serverUrls));
            }
            if (destinationFolder == null)
            {
                throw new ArgumentNullException(nameof(destinationFolder));
            }
            List<DownloadOperation> downloads = new List<DownloadOperation>();
            foreach (var serverUrl in serverUrls)
            {
                var fileName = Path.GetFileName(serverUrl);
                downloads.Add(await GetDownloadOperation(serverUrl, destinationFolder, fileName));
            }
            // Attach progress and completion handlers.
            return await HandleDownloadsAsync(downloads, true);
        }
        
        private async Task<DownloadOperation> GetDownloadOperation(string serverUrl, StorageFolder destinationFolder, string destinationFilePath)
        {
            if (string.IsNullOrWhiteSpace(destinationFilePath))
            {
                throw new ArgumentNullException(nameof(description));
            }
            if (string.IsNullOrWhiteSpace(serverUrl))
            {
                throw new ArgumentNullException(nameof(serverUrl));
            }
            if (destinationFolder == null)
            {
                throw new ArgumentNullException(nameof(destinationFolder));
            }

            destinationFilePath = destinationFilePath + DownloadExtension;

            StorageFile destinationFile;
            try
            {
                destinationFile = await destinationFolder.CreateFileAsync(Path.GetFileName(destinationFilePath), 
                    CreationCollisionOption.ReplaceExisting);
            }
            catch (FileNotFoundException ex)
            {
                telemetry.TrackException(ex, new Dictionary<string, string> { { "Scenario", "CreatingFileToWriteDownload" } });
                await QuranApp.NativeProvider.
                    ShowErrorMessageBox("Error while creating file: " + ex.Message);
                return null;
            }

            BackgroundDownloader downloader = new BackgroundDownloader();
            DownloadOperation download = downloader.CreateDownload(new Uri(serverUrl), destinationFile);
            
            return download;
        }

        public async Task FinishActiveDownloads()
        {
            foreach (var download in _activeDownloads)
            {
                if (download.Progress.Status == BackgroundTransferStatus.Completed)
                {
                    DownloadProgress(download);
                    await FinishDownload(download.ResultFile.Path);
                }
                else if (download.Progress.Status == BackgroundTransferStatus.Error)
                {
                    DownloadProgress(download);
                    await FileUtils.SafeFileDelete(download.ResultFile.Path);
                }
            }
        }

        public async Task FinishDownload(string destinationFile)
        {
            if (string.IsNullOrEmpty(destinationFile))
            {
                throw new ArgumentNullException(nameof(destinationFile));
            }

            if (await FileUtils.FileExists(destinationFile))
            {
                if (destinationFile.EndsWith(DownloadExtension, StringComparison.OrdinalIgnoreCase))
                {
                    var realDestinationFile = destinationFile.Substring(0, 
                        destinationFile.IndexOf(DownloadExtension, StringComparison.OrdinalIgnoreCase));
                    await FileUtils.MoveFile(destinationFile, realDestinationFile);
                    destinationFile = realDestinationFile;
                }
                if (destinationFile.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    IsDownloading = true;
                    IsIndeterminate = true;
                    InstallationStep = Resources.extracting_message;

                    await QuranApp.NativeProvider.ExtractZip(destinationFile,
                        Path.GetDirectoryName(destinationFile));
                    await FileUtils.SafeFileDelete(destinationFile);
                }
                IsIndeterminate = false;
                IsDownloading = false;
                IsIndeterminate = false;
            }
        }
        public async Task Cancel()
        {
            if (_activeDownloads.Any())
            {
                if (await QuranApp.NativeProvider.ShowQuestionMessageBox(Resources.download_cancel_confirmation))
                {
                    Reset();
                }
            }
        }

        public void Reset()
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();
            IsDownloading = false;
            IsIndeterminate = true;
            Description = null;
            InstallationStep = null;
            _activeDownloads.Clear();
        }
        #endregion Public methods
        private async Task<bool> HandleDownloadsAsync(List<DownloadOperation> downloads, bool start)
        {
            try
            {
                foreach (var download in downloads)
                {
                    _activeDownloads.Add(download);
                }

                // Store the download so we can pause/resume.
                foreach (var download in downloads)
                {
                    Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
                    if (start)
                    {
                        // Start the download and attach a progress handler.
                        await download.StartAsync().AsTask(_cts.Token, progressCallback);
                    }
                    else
                    {
                        // The download was already running when the application started, re-attach the progress handler.
                        await download.AttachAsync().AsTask(_cts.Token, progressCallback);
                    }
                }

                return true;
            }
            catch (TaskCanceledException)
            {
                InstallationStep = "Cancelled";
                return false;
            }
            catch (Exception ex)
            {
                WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult);
                telemetry.TrackException(ex, new Dictionary<string, string> { { "Scenario", "GettingActiveDownloads" } });
                await QuranApp.NativeProvider.ShowErrorMessageBox("Error getting active downloads: " + error.ToString());
                return false;
            }
            finally
            {
                try
                {
                    await FinishActiveDownloads();
                }
                finally
                {
                    foreach (var download in downloads)
                    {
                        _activeDownloads.Remove(download);
                    }
                }
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
            var activeDownload = _activeDownloads.FirstOrDefault();
            var downloadsSnapshot = new List<DownloadOperation>(_activeDownloads);
            if (_activeDownloads.Count > 0)
            {
                if (downloadsSnapshot.Any(o => o.Progress.Status == BackgroundTransferStatus.Running))
                {
                    UpdateInstallationStep(BackgroundTransferStatus.Running);
                }
                else if (downloadsSnapshot.Any(o => o.Progress.Status == BackgroundTransferStatus.Error))
                {
                    UpdateInstallationStep(BackgroundTransferStatus.Error);
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
                    InstallationStep = Resources.waiting;
                    IsDownloading = true;
                    IsIndeterminate = true;
                    break;
                case BackgroundTransferStatus.PausedNoNetwork:
                case BackgroundTransferStatus.PausedCostedNetwork:
                    InstallationStep = Resources.waiting_for_wifi;
                    IsDownloading = true;
                    IsIndeterminate = true;
                    break;
                case BackgroundTransferStatus.Running:
                    InstallationStep = Description ?? Resources.loading_message;
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
                case BackgroundTransferStatus.Error:
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
            if (_cts != null)
            {
                _cts.Dispose();
                _cts = null;
            }
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        public event EventHandler DownloadComplete;
        public event EventHandler DownloadCancelled;
    }
}
