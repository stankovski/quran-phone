using System.Collections.Generic;
using Quran.Core.Utils;

namespace Quran.Core.Interfaces
{
    public interface INativeProvider
    {
        double ActualWidth { get; }
        double ActualHeight { get; }
        bool IsPortaitOrientation { get; }
        IDownloadManager DownloadManager { get; }
        ISettingsProvider SettingsProvider { get; }
        IAudioProvider AudioProvider { get; }
        ICollection<string> SplitLongText(string value, double fontSize, string fontWeight);
        void ExtractZip(string source, string destination);
        void CopyToClipboard(string text);
        void ComposeEmail(string to, string subject);
        void LaunchWebBrowser(string url);
        void ToggleDeviceSleep(bool enable);
        void ShowInfoMessageBox(string text);
        void ShowInfoMessageBox(string text, string title);
        bool ShowQuestionMessageBox(string text);
        bool ShowQuestionMessageBox(string text, string title);
        void ShowErrorMessageBox(string text);
        void Log(string text);
        string NativePath { get; }
    }
}
