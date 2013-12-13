using System;
using System.Collections.Generic;
using System.Windows;
using Windows.Storage;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Quran.Core.Interfaces;
using Quran.Core.Utils;
using Quran.WindowsPhone.Utils;

namespace Quran.WindowsPhone.NativeProvider
{
    public class WindowsPhoneNativeProvider : INativeProvider
    {
        public double ActualWidth { get { return Application.Current.Host.Content.ActualWidth; } }
        public double ActualHeight { get { return Application.Current.Host.Content.ActualHeight; } }
        public bool IsPortaitOrientation { get { return PhoneUtils.IsPortaitOrientation; } }

        private IDownloadManager downloadManager;
        public IDownloadManager DownloadManager
        {
            get
            {
                if (downloadManager == null)
                    downloadManager = new WindowsPhoneDownloadManager();
                return downloadManager;
            }
        }

        private ISettingsProvider settingsProvider;
        public ISettingsProvider SettingsProvider
        {
            get
            {
                if (settingsProvider == null)
                    settingsProvider = new WindowsPhoneSettingsProvider();
                return settingsProvider;
            }
        }

        private IAudioProvider audioProvider;
        public IAudioProvider AudioProvider
        {
            get
            {
                if (audioProvider == null)
                    audioProvider = new WindowsPhoneAudioProvider();
                return audioProvider;
            }
        }

        public ICollection<string> SplitLongText(string value, double fontSize, string fontWeight)
        {
            return TextBlockSplitter.Instance.Split(value, fontSize, PhoneUtils.FontWeightsConverter(fontWeight));
        }

        public void ExtractZip(string source, string baseFolder)
        {
            ZipHelper.Unzip(source, baseFolder);
        }

        public void CopyToClipboard(string text)
        {
            Clipboard.SetText(text);
        }

        public void ComposeEmail(string to, string subject)
        {
            var email = new EmailComposeTask {To = to, Subject = subject};
            email.Show();
        }

        public void LaunchWebBrowser(string url)
        {
            var task = new WebBrowserTask() { Uri = new Uri(url) };
            task.Show();
        }

        public void ToggleDeviceSleep(bool enable)
        {
            if (enable)
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
            else
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
        }

        public void ShowInfoMessageBox(string text)
        {
            MessageBox.Show(text);
        }

        public void ShowInfoMessageBox(string text, string title)
        {
            MessageBox.Show(text, title, MessageBoxButton.OK);
        }

        public bool ShowQuestionMessageBox(string text)
        {
            return MessageBox.Show(text, "", MessageBoxButton.OKCancel) == MessageBoxResult.OK;
        }

        public bool ShowQuestionMessageBox(string text, string title)
        {
            return MessageBox.Show(text, title, MessageBoxButton.OKCancel) == MessageBoxResult.OK;
        }

        public void ShowErrorMessageBox(string text)
        {
            MessageBox.Show(text);
        }

        public void Log(string text)
        {
            Console.WriteLine(text);
        }

        public string NativePath { get { return ApplicationData.Current.LocalFolder.Path; } }
    }
}
