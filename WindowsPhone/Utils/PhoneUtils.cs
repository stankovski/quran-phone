using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Windows;
using Microsoft.Phone.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Shell;

namespace QuranPhone.Utils
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

        public static void DisableIdleDetection()
        {
            try
            {
                PhoneApplicationService.Current.ApplicationIdleDetectionMode = IdleDetectionMode.Disabled;
            }
            catch (InvalidOperationException ex)
            {
                // This exception is expected in the current release.
            }
        }

        public static void EnableIdleDetection()
        {
            try
            {
                PhoneApplicationService.Current.ApplicationIdleDetectionMode = IdleDetectionMode.Enabled;
            }
            catch (InvalidOperationException ex)
            {
                // This exception is expected in the current release.
            }
        }
    }
}
