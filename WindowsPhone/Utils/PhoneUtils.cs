using Microsoft.Phone.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
