using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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
            ThreadSafeAction(() => MessageBox.Show(text));
        }

        public void ShowInfoMessageBox(string text, string title)
        {
            ThreadSafeAction(() => MessageBox.Show(text, title, MessageBoxButton.OK));
        }

        public bool ShowQuestionMessageBox(string text)
        {
            var result = ThreadSafeFunction(() => MessageBox.Show(text, "", MessageBoxButton.OKCancel));
            return result == MessageBoxResult.OK;
        }

        public bool ShowQuestionMessageBox(string text, string title)
        {
            var result = ThreadSafeFunction(() => MessageBox.Show(text, title, MessageBoxButton.OKCancel));
            return result == MessageBoxResult.OK;
        }

        public void ShowErrorMessageBox(string text)
        {
            ThreadSafeAction(() => MessageBox.Show(text));
        }

        public void Log(string text)
        {
            Console.WriteLine(text);
        }

        private void ThreadSafeAction(Action action)
        {
            UISynchronizationContext.Instance.InvokeSynchronously(action);
        }

        private T ThreadSafeFunction<T>(Func<T> func)
        {
            T result = default(T);
            UISynchronizationContext.Instance.InvokeSynchronously(() => result = func());
            return result;
        }

        public string NativePath { get { return ApplicationData.Current.LocalFolder.Path; } }
    }
}
