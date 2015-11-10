using System.IO.IsolatedStorage;
using Quran.Core.Interfaces;
using Quran.Core.Utils;

namespace Quran.WindowsPhone.NativeProvider
{
    public class WindowsPhoneSettingsProvider : ISettingsProvider
    {
        public bool Contains(string key)
        {
            return IsolatedStorageSettings.ApplicationSettings.Contains(key);
        }

        public void Save()
        {
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        public void Add(string key, object value)
        {
            IsolatedStorageSettings.ApplicationSettings.Add(key, value);
        }

        public object this[string key]
        {
            get { return IsolatedStorageSettings.ApplicationSettings[key]; }
            set { IsolatedStorageSettings.ApplicationSettings[key] = value; }
        }
    }
}
