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
        private static IDictionary<string, object> cache = new Dictionary<string, object>();
        /// <summary>
        /// Gets value of object in dictionary and deserializes it to specified type
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize from</typeparam>
        /// <param name="key">Key</param>
        /// <returns></returns>
        public static T Get<T>(string key)
        {
            if (cache.ContainsKey(key))
            {
                try
                {
                    return (T)cache[key];
                }
                catch
                {
                    return getDefaultValue<T>(key);
                }
            }

            object value = null;
            if (IsolatedStorageSettings.ApplicationSettings.Contains(key))
            {
                value = IsolatedStorageSettings.ApplicationSettings[key];
            }

            if (value == null)
            {
                value = getDefaultValue<T>(key);
            }
            
            try
            {
                cache[key] = value;
                return (T) value;
            }
            catch
            {
                cache[key] = getDefaultValue<T>(key);
                return getDefaultValue<T>(key);
            }
        }

        public static bool Contains(string key)
        {
            if (cache.ContainsKey(key))
                return true;
            else
                return IsolatedStorageSettings.ApplicationSettings.Contains(key);
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
            FieldInfo info = constantKeys.Find(fi => fi.FieldType == typeof(string) && fi.GetRawConstantValue() as string == key);
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

            cache[key] = value;
            Save();
        }

        public static void Save()
        {
            IsolatedStorageSettings.ApplicationSettings.Save();
        }
    }
}
