using System.Collections.Generic;
using System;

namespace Quran.Core.Utils
{
    public class TempSettingsUtils
    {
        private static IDictionary<string, object> cache = new Dictionary<string, object>();
        /// <summary>
        /// Gets value of object in dictionary and deserializes it to specified type
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize from</typeparam>
        /// <param name="key">Key</param>
        /// <returns></returns>
        public static T Get<T>(string key) where T : IConvertible
        {
            if (cache.ContainsKey(key))
            {
                try
                {
                    return (T)cache[key];
                }
                catch
                {
                    return SettingsUtils.GetDefaultValue<T>(key);
                }
            }
            return SettingsUtils.GetDefaultValue<T>(key);
        }
        
        /// <summary>
        /// Sets value of object in dictionary and serializes it to specified type
        /// </summary>
        /// <typeparam name="T">Type of object to serialize into</typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set<T>(string key, T value) where T : IConvertible
        {            
            cache[key] = value;
        }
    }
}
