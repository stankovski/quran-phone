using Quran.Core.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Quran.Core.Interfaces;
using System;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace Quran.Core.Utils
{
    public class SettingsUtils
    {
        private static ISettingsProvider provider;

        private static IDictionary<string, object> cache = new Dictionary<string, object>();
        /// <summary>
        /// Gets value of object in dictionary and deserializes it to specified type
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize from</typeparam>
        /// <param name="key">Key</param>
        /// <returns></returns>
        public static T Get<T>(string key) where T : IConvertible
        {
            if (provider == null)
                provider = QuranApp.NativeProvider.SettingsProvider;

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
            if (provider.Contains(key))
            {
                try
                {
                    value = JsonConvert.DeserializeObject<T>(provider[key].ToString());
                }
                catch
                {
                    //Ignore
                }
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

        private static List<FieldInfo> constantKeys;
        private static T getDefaultValue<T>(string key)
        {
            if (constantKeys == null)
            {
                constantKeys = new List<FieldInfo>();
                var thisObjectProperties = typeof(Constants).GetRuntimeFields();
                foreach (FieldInfo fi in thisObjectProperties)
                {
                    if (fi.IsLiteral)
                    {
                        constantKeys.Add(fi);
                    }
                }
            }
            
            FieldInfo info = null;
            info = constantKeys.FirstOrDefault(fi => fi.FieldType == typeof(string) && fi.GetValue(null) as string == key);
            if (info == null)
                return default(T);

            var attr = info.GetCustomAttributes(typeof(DefaultValueAttribute), false) as DefaultValueAttribute[];
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
        public static void Set<T>(string key, T value) where T : IConvertible
        {
            if (provider == null)
                provider = QuranApp.NativeProvider.SettingsProvider;

            if (value == null)
            {
                return;
            }

            provider[key] = JsonConvert.SerializeObject(value);
            cache[key] = value;
        }
    }
}
