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
using System.Collections.Concurrent;
using System.Net.Http;

namespace Quran.Core.ViewModels
{
    /// <summary>
    /// Define the DownloadableViewModelBase type.
    /// </summary>
    public class DownloadableViewModelBase : BaseViewModel
    {
        private ConcurrentDictionary<string, DownloadOperation> _activeDownloads;
        private int _totalDownloads = 0;
        private CancellationTokenSource _cts;
        private readonly CoreDispatcher _dispatcher;
        public const string DownloadExtension = ".download";

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
            _activeDownloads = new ConcurrentDictionary<string, DownloadOperation>();

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

        public async Task<bool> DownloadMultipleViaHttpClient(string[] serverUrls, StorageFolder destinationFolder, string description = null)
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

            int successfulDownloads = 0;
            foreach (var serverPath in serverUrls)
            {
                successfulDownloads++;

                if (_cts.Token.IsCancellationRequested)
                {
                    break;
                }

                var fileName = Path.GetFileName(serverPath);
                if (await FileUtils.FileExists(destinationFolder, fileName))
                {
                    continue;
                }

                var destinationFile = await destinationFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

                // Retry loop
                for (int i = 0; i < 5; i++)
                {
                    var result = await FileUtils.DownloadFileFromWebAsync(serverPath, destinationFile.Path, _cts.Token);
                    if (result)
                    {
                        InstallationStep = Description ?? Resources.loading_message;
                        IsDownloading = true;
                        IsIndeterminate = false;
                        double percent = 100;
                        var totalFilesToReceive = serverUrls.Length;
                        var totalFilesReceived = successfulDownloads;
                        if (totalFilesToReceive > 0)
                        {
                            percent = totalFilesReceived * 100 / totalFilesToReceive;
                        }
                        Progress = (int)percent;
                        break;
                    }
                }
            }
            Reset();
            return true;
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
            foreach (var key in _activeDownloads.Keys)
            {
                await FinishDownload(key);
            }
        }

        public async Task FinishDownload(string downloadUrl)
        {
            DownloadOperation download;
            if (_activeDownloads.TryRemove(downloadUrl, out download))
            {
                if (download.Progress.Status == BackgroundTransferStatus.Completed)
                {
                    await FinishDownload(download.ResultFile as StorageFile);
                }
                else
                {
                    await FileUtils.SafeFileDelete(download.ResultFile.Path);
                }
            }
        }

        public async Task FinishDownload(StorageFile destinationFile)
        {
            if (destinationFile != null)
            {
                StorageFolder parentFolder = await destinationFile.GetParentAsync();

                if (destinationFile.FileType.EndsWith(DownloadExtension, StringComparison.OrdinalIgnoreCase))
                {
                    var realDestinationFileName = destinationFile.Name.Substring(0,
                        destinationFile.Name.IndexOf(DownloadExtension, StringComparison.OrdinalIgnoreCase));
                    await FileUtils.MoveFile(destinationFile, parentFolder, realDestinationFileName);
                    destinationFile = await FileUtils.GetFile(parentFolder, realDestinationFileName);
                    if (destinationFile == null)
                    {
                        throw new InvalidOperationException("File move while finalizing file failed.");
                    }
                }

                if (destinationFile.FileType.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    IsDownloading = true;
                    IsIndeterminate = true;
                    InstallationStep = Resources.extracting_message;

                    await QuranApp.NativeProvider.ExtractZip(destinationFile, parentFolder.Path);
                    await FileUtils.SafeFileDelete(destinationFile);
                }
            }
        }
        public async Task Cancel()
        {
            if (await QuranApp.NativeProvider.ShowQuestionMessageBox(Resources.download_cancel_confirmation))
            {
                _cts.Cancel();
            }
        }

        public void Reset()
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();
            IsDownloading = false;
            IsIndeterminate = false;
            Description = null;
            InstallationStep = null;
            // Empty downloads
            _activeDownloads.Clear();
            _totalDownloads = 0;
        }
        #endregion Public methods
        private async Task<bool> HandleDownloadsAsync(List<DownloadOperation> downloads, bool start)
        {
            UnconstrainedTransferRequestResult result;
            try
            {
                result = await BackgroundDownloader.RequestUnconstrainedDownloadsAsync(downloads);
            }
            catch (NotImplementedException ex)
            {
                telemetry.TrackException(ex);
            }

            try
            {
                List<DownloadOperation> successfullyAddedDownloads = new List<DownloadOperation>();
                foreach (var download in downloads)
                {
                    if (_activeDownloads.TryAdd(download.RequestedUri.ToString(), download))
                    {
                        successfullyAddedDownloads.Add(download);
                        _totalDownloads++;
                    }
                }

                // Store the download so we can pause/resume.
                foreach (var download in successfullyAddedDownloads)
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
                catch (Exception ex)
                {
                    telemetry.TrackException(ex, new Dictionary<string, string> { { "Scenario", "GettingActiveDownloads" } });
                }
            }
        }

        private async void DownloadProgress(DownloadOperation download)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UpdateStatus();              
            });
        }

        private bool IsTerminalStatus(BackgroundTransferStatus status)
        {
            return status == BackgroundTransferStatus.Completed || 
                status == BackgroundTransferStatus.Error || 
                status == BackgroundTransferStatus.Canceled;
        }

        private void UpdateStatus()
        {
            var activeDownload = _activeDownloads.Values.FirstOrDefault();
            var downloadsSnapshot = _activeDownloads.Values.ToArray();
            if (downloadsSnapshot.Length > 1)
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
                var totalFilesToReceive = _totalDownloads > downloadsSnapshot.Length ? _totalDownloads : downloadsSnapshot.Length;
                var totalFilesReceived = downloadsSnapshot.Count(d => IsTerminalStatus(d.Progress.Status));
                if (totalFilesToReceive > 0)
                {
                    percent = totalFilesReceived * 100 / totalFilesToReceive;
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
