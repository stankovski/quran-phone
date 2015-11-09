using Windows.UI.Text;

namespace Quran.WindowsPhone.Utils
{
    public class PhoneUtils
    {
        public static bool IsDesignMode
        {
            get
            {
                return true;
            }
        }

        public static void ToggleIdleMode()
        {
            // TODO: Implement
        }

        public static bool IsPortaitOrientation
        {
            get
            {
                // TODO: Implement
                return true;
            }
        }

        public static bool IsOnWifiNetwork()
        {
            // TODO: Implement
            return true;
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
