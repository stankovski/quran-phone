using System.IO.IsolatedStorage;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using System;
using Microsoft.Phone.Net.NetworkInformation;
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
    }
}
