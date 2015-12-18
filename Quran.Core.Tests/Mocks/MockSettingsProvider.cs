using System.Collections.Generic;
using System.IO.IsolatedStorage;
using Quran.Core.Interfaces;
using Quran.Core.Utils;
using Windows.Storage;

namespace Quran.Core.Tests
{
    public class MockSettingsProvider : ISettingsProvider
    {
        Dictionary<string, object> settingsStore = new Dictionary<string, object>();

        public bool Contains(string key)
        {
            return settingsStore.ContainsKey(key);
        }

        public void Save()
        {
            // Do nothing
        }

        public object this[string key]
        {
            get { return settingsStore[key]; }
            set { settingsStore[key] = value; }
        }
    }
}
