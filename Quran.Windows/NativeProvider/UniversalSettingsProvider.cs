using System.IO.IsolatedStorage;
using Quran.Core.Interfaces;
using Quran.Core.Utils;
using Windows.Storage;

namespace Quran.Windows.NativeProvider
{
    public class UniversalSettingsProvider : ISettingsProvider
    {
        public bool Contains(string key)
        {
            return ApplicationData.Current.RoamingSettings.Values.ContainsKey(key);
        }

        public void Save()
        {
            // Do nothing
        }

        public object this[string key]
        {
            get { return ApplicationData.Current.RoamingSettings.Values[key]; }
            set { ApplicationData.Current.RoamingSettings.Values[key] = value; }
        }
    }
}
