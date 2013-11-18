using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using System;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Reactive;
using Microsoft.Phone.Shell;
using Quran.Core.Common;
using Quran.Core.Utils;
using Quran.Core.Data;

namespace Quran.WindowsPhone.Utils
{
    public class PhoneUtils
    {
        public static string CurrentMemoryUsage()
        {
            return string.Format("{0}MB", (double)((long)DeviceExtendedProperties.GetValue("ApplicationCurrentMemoryUsage") / 1024 / 1024));
        }

        public static string TotalDeviceMemory()
        {
            return string.Format("{0}MB", (double)((long)DeviceExtendedProperties.GetValue("DeviceTotalMemory") / 1024 / 1024));
        }

        private static bool? designTime;
        public static bool IsDesignMode
        {
            get
            {
                if (!designTime.HasValue)
                {
                    try
                    {
                        var isoStor = IsolatedStorageSettings.ApplicationSettings.Contains("asasdasd");
                        designTime = false;
                    }
                    catch (Exception ex)
                    {
                        designTime = true;
                    }
                }
                return designTime.Value;
            }
        }

        public static void ToggleIdleMode()
        {
            var preventSleep = SettingsUtils.Get<bool>(Constants.PREF_PREVENT_SLEEP);

            if (preventSleep)
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            else
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
        }

        public static bool IsPortaitOrientation
        {
            get
            {
                if (Application.Current == null || Application.Current.RootVisual == null)
                    return true;
                else
                    return ((PhoneApplicationFrame) Application.Current.RootVisual).Orientation == PageOrientation.Portrait ||
                       ((PhoneApplicationFrame) Application.Current.RootVisual).Orientation ==
                       PageOrientation.PortraitUp ||
                       ((PhoneApplicationFrame) Application.Current.RootVisual).Orientation ==
                       PageOrientation.PortraitDown;
            }
        }

        public static bool IsOnWifiNetwork()
        {
            return NetworkInterface.GetIsNetworkAvailable() && NetworkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211;
        }

        public static ScreenOrientation PageOrientationConverter(PageOrientation orientation)
        {
            return (ScreenOrientation)((int) orientation);
        }

        public static FontWeight FontWeightsConverter(string value)
        {
            if (value == "Normal")
                return FontWeights.Normal;
            else if (value == "Bold")
                return FontWeights.Bold;
            else
                return FontWeights.Normal;
        }

        public static void PinPageToStart(int page)
        {
            var standardTileData = new StandardTileData();
            standardTileData.Title = QuranUtils.GetSurahNameFromPage(page, true);
            standardTileData.Count = page;
            var imageUrl = PathHelper.Combine(FileUtils.GetQuranDirectory(false),
                FileUtils.GetPageFileName(page));
            imageUrl = ResizeAndCopyImageToShared(imageUrl, page);
            standardTileData.BackgroundImage = new Uri(imageUrl, UriKind.Relative);
            standardTileData.BackContent = QuranUtils.GetSurahNameFromPage(page) + " page " + page;
            var pageUrl = string.Format("/Views/MainView.xaml?page={0}", page);
            ShellTile tiletopin = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(pageUrl));
            if (tiletopin == null)
            {
                ShellTile.Create(new Uri(pageUrl, UriKind.Relative), standardTileData);
            }
        }

        private static string ResizeAndCopyImageToShared(string imageUrl, int page)
        {
            var newUrl = string.Format("/Shared/ShellContent/{0}.jpg", page);
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isf.FileExists(newUrl))
                {
                    BitmapImage source = new BitmapImage(new Uri(imageUrl, UriKind.Relative));
                    WriteableBitmap bitmap = new WriteableBitmap(source);
                    bitmap.Crop(0, 0, 137, 137);
                    using (var fileStream = isf.CreateFile(newUrl))
                    {
                        bitmap.SaveJpeg(fileStream, 137, 137, 0, 100);
                    }
                }
            }
            return newUrl;
        }
    }
}
