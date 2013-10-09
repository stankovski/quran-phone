using System.Collections.Generic;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using QuranPhone.Data;

namespace QuranPhone.Utils
{
    public static class SettingsUtils
    {
        private static List<FieldInfo> _constantKeys;

        public static T Get<T>(string key)
        {
            object value = null;

            if (Contains(key))
            {
                value = IsolatedStorageSettings.ApplicationSettings[key];
            }

            if (value == null)
            {
                value = GetDefaultValue<T>(key);
            }

            return (T) value;
        }

        private static T GetDefaultValue<T>(string key)
        {
            if (_constantKeys == null)
            {
                _constantKeys = new List<FieldInfo>();
                FieldInfo[] thisObjectProperties = typeof (Constants).GetFields();
                foreach (FieldInfo fi in thisObjectProperties.Where(fi => fi.IsLiteral)) {
                    _constantKeys.Add(fi);
                }
            }

            FieldInfo info =
                _constantKeys.Find(fi => fi.FieldType == typeof (string) && fi.GetRawConstantValue() as string == key);
            if (info == null)
            {
                return default(T);
            }

            var attr = info.GetCustomAttributes(typeof (DefaultValueAttribute), false) as DefaultValueAttribute[];
            if (attr != null && attr.Length == 1)
            {
                return (T) attr[0].Value;
            }

            return default(T);
        }

        public static void Set<T>(string key, T value)
        {
            IsolatedStorageSettings.ApplicationSettings[key] = value;
            Save();
        }

        public static bool Contains(string key)
        {
            return IsolatedStorageSettings.ApplicationSettings.Contains(key);
        }

        public static void Save()
        {
            IsolatedStorageSettings.ApplicationSettings.Save();
        }
    }
}