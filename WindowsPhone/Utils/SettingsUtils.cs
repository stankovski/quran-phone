using QuranPhone.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuranPhone.Utils
{
    public class SettingsUtils
    {
        /// <summary>
        /// Gets value of object in dictionary and deserializes it to specified type
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize from</typeparam>
        /// <param name="key">Key</param>
        /// <returns></returns>
        public static T Get<T>(string key)
        {
            object value = null;
            if (IsolatedStorageSettings.ApplicationSettings.Contains(key))
            {
                value = IsolatedStorageSettings.ApplicationSettings[key];
            }

            if (value == null)
                return getDefaultValue<T>(key);

            try
            {
                return (T)value;
            }
            catch
            {
                return getDefaultValue<T>(key);
            }
        }

        private static List<FieldInfo> constantKeys;
        private static T getDefaultValue<T>(string key)
        {
            if (constantKeys == null)
            {
                constantKeys = new List<FieldInfo>();
                FieldInfo[] thisObjectProperties = typeof(Constants).GetFields();
                foreach (FieldInfo fi in thisObjectProperties)
                {
                    if (fi.IsLiteral)
                    {
                        constantKeys.Add(fi);
                    }
                }
            }
            FieldInfo info = constantKeys.Find(fi => fi.Name == key);
            if (info == null)
                return default(T);

            DefaultValueAttribute[] attr = info.GetCustomAttributes(typeof(DefaultValueAttribute), false) as DefaultValueAttribute[];
            if (attr != null && attr.Length == 1)
                return (T)attr[0].Value;

            return default(T);
        }

        /// <summary>
        /// Sets value of object in dictionary and serializes it to specified type
        /// </summary>
        /// <typeparam name="T">Type of object to serialize into</typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set<T>(string key, T value)
        {
            if (value == null)
                return;

            bool keyExists = IsolatedStorageSettings.ApplicationSettings.Contains(key);

            if (!keyExists)
                IsolatedStorageSettings.ApplicationSettings.Add(key, value);
            else
                IsolatedStorageSettings.ApplicationSettings[key] = value;
        }

        public static void Save()
        {
            IsolatedStorageSettings.ApplicationSettings.Save();
        }
    }
}
