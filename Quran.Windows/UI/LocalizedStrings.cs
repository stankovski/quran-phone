using Quran.Core.Properties;

namespace Quran.Windows.UI
{
    /// <summary>
    /// Provides access to string resources.
    /// </summary>
    public class LocalizedStrings
    {
        private static Resources _localizedResources = new Resources();

        public Resources LocalizedResources { get { return _localizedResources; } }
    }
}
