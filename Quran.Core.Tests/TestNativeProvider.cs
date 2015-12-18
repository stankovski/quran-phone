using System;
using System.Collections.Generic;
using Windows.Storage;
using Quran.Core.Interfaces;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.ApplicationModel.Email;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using System.IO.Compression;
using Windows.System.Display;
using Windows.Graphics.Display;
using Windows.Storage.Streams;
using System.IO;
using Quran.Core.Utils;

namespace Quran.Core.Tests
{
    public class TestNativeProvider : INativeProvider
    {
        DisplayRequest _keepScreenOnRequest = null;

        public double ActualWidth
        {
            get
            {
                return 800;
            }
        }

        public double ActualHeight
        {
            get
            {
                return 400;
            }
        }

        public double ScaleFactor
        {
            get
            {
                return 1;
            }
        }

        private IDownloadManager downloadManager;
        public IDownloadManager DownloadManager
        {
            get
            {
                if (downloadManager == null)
                    downloadManager = new MockDownloadManager();
                return downloadManager;
            }
        }

        private ISettingsProvider settingsProvider;
        public ISettingsProvider SettingsProvider
        {
            get
            {
                if (settingsProvider == null)
                    settingsProvider = new MockSettingsProvider();
                return settingsProvider;
            }
        }

        private IAudioProvider audioProvider;
        public IAudioProvider AudioProvider
        {
            get
            {
                if (audioProvider == null)
                    audioProvider = new MockAudioProvider();
                return audioProvider;
            }
        }

        public Task ExtractZip(string source, string baseFolder)
        {
            return Task.Run(() =>
            {
                //ZipFile.ExtractToDirectory(source, baseFolder);
                using (var fileStream = File.OpenRead(source))
                {
                    ZipArchive archive = new ZipArchive(File.OpenRead(source));
                    foreach (ZipArchiveEntry file in archive.Entries)
                    {
                        string completeFileName = Path.Combine(baseFolder, file.FullName);

                        if (file.Name == "")
                        {
                            // Assuming Empty for Directory
                            Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                            continue;
                        }

                        file.ExtractToFile(completeFileName, true);
                    }
                }
            });
        }

        public void CopyToClipboard(string text)
        {
            DataPackage dp = new DataPackage();
            dp.SetText(text);
            Clipboard.SetContent(dp);
        }

        public async Task ComposeEmail(string to, string subject, string body = "")
        {
            var dialog = new MessageDialog(subject);
            await dialog.ShowAsync();
        }

        public async Task LaunchWebBrowser(string url)
        {
            await Launcher.LaunchUriAsync(new Uri(url));
        }

        public void ToggleDeviceSleep(bool enable)
        {
            if (!enable)
            {
                _keepScreenOnRequest = new DisplayRequest();
                _keepScreenOnRequest.RequestActive();
            }
            else
            {
                if (_keepScreenOnRequest != null)
                {
                    _keepScreenOnRequest.RequestRelease();
                    _keepScreenOnRequest = null;
                }
            }
        }

        public async Task ShowInfoMessageBox(string text)
        {
            var dialog = new MessageDialog(text);
            await dialog.ShowAsync();
        }

        public async Task ShowInfoMessageBox(string text, string title)
        {
            var dialog = new MessageDialog(text, title);
            dialog.Commands.Add(new UICommand { Label = "OK", Id = 0 });
            await dialog.ShowAsync();
        }

        public async Task<bool> ShowQuestionMessageBox(string text)
        {
            var dialog = new MessageDialog(text);
            dialog.Commands.Add(new UICommand { Label = "OK", Id = 0 });
            dialog.Commands.Add(new UICommand { Label = "Cancel", Id = 1 });
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;
            IUICommand result = await dialog.ShowAsync();
            return (int)result.Id == 0;
        }

        public async Task<bool> ShowQuestionMessageBox(string text, string title)
        {
            var dialog = new MessageDialog(text, title);
            dialog.Commands.Add(new UICommand { Label = "OK", Id = 0 });
            dialog.Commands.Add(new UICommand { Label = "Cancel", Id = 1 });
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;
            IUICommand result = await dialog.ShowAsync();
            return (int)result.Id == 0;
        }

        public Task ShowErrorMessageBox(string text)
        {
            return ShowInfoMessageBox(text);
        }

        public void Log(string text)
        {
            // TODO: Implement
        }

        public string NativePath { get { return ApplicationData.Current.LocalFolder.Path; } }        
    }
}
