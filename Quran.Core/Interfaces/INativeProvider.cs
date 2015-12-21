using System.Collections.Generic;
using System.Threading.Tasks;
using Quran.Core.Utils;

namespace Quran.Core.Interfaces
{
    public interface INativeProvider
    {
        double ActualWidth { get; }
        double ActualHeight { get; }
        double ScaleFactor { get; }
        IDownloadManager DownloadManager { get; }
        ISettingsProvider SettingsProvider { get; }
        IAudioProvider AudioProvider { get; }
        Task ExtractZip(string source, string baseFolder);
        void CopyToClipboard(string text);
        Task ComposeEmail(string to, string subject, string body = null);
        Task LaunchWebBrowser(string url);
        void ToggleDeviceSleep(bool enable);
        Task ShowInfoMessageBox(string text);
        Task ShowInfoMessageBox(string text, string title);
        Task<bool> ShowQuestionMessageBox(string text);
        Task<bool> ShowQuestionMessageBox(string text, string title);
        Task ShowErrorMessageBox(string text);
        void Log(string text);
    }
}
