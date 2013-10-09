using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using QuranPhone.Data;

namespace QuranPhone.Utils
{
    public static class PhoneUtils
    {
        public static bool IsPortaitOrientation
        {
            get
            {
                return ((PhoneApplicationFrame)Application.Current.RootVisual).Orientation == PageOrientation.Portrait || ((PhoneApplicationFrame)Application.Current.RootVisual).Orientation == PageOrientation.PortraitUp || ((PhoneApplicationFrame)Application.Current.RootVisual).Orientation == PageOrientation.PortraitDown;
            }
        }

        public static bool IsLandscapeOrientation()
        {
            return ((PhoneApplicationFrame)Application.Current.RootVisual).Orientation == PageOrientation.Landscape || ((PhoneApplicationFrame)Application.Current.RootVisual).Orientation == PageOrientation.LandscapeLeft || ((PhoneApplicationFrame)Application.Current.RootVisual).Orientation == PageOrientation.LandscapeRight;
        }

        public static void ToggleIdleMode()
        {
            var preventSleep = SettingsUtils.Get<bool>(Constants.PrefPreventSleep);
            PhoneApplicationService.Current.UserIdleDetectionMode = preventSleep ? IdleDetectionMode.Disabled : IdleDetectionMode.Enabled;
        }
    }
}