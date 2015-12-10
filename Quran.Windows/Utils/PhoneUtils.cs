using Windows.ApplicationModel;
using Windows.UI.Text;

namespace Quran.Windows.Utils
{
    public class PhoneUtils
    {
        public static bool IsDesignMode
        {
            get
            {
                return DesignMode.DesignModeEnabled;
            }
        }

        public static void ToggleIdleMode()
        {
            // TODO: Implement
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
