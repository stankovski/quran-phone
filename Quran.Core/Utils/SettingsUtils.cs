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
                if (NeedsToStringSerialization<T>())
                {
                    value = ConvertToType<T>(provider[key]);
                }
                else
                {
                    value = provider[key];
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

        private static T ConvertToType<T>(object v) where T : IConvertible
        {
            if (v is T)
            {
                return (T)v;
            }

            if (typeof(T).GetTypeInfo().IsEnum)
            {
                if (v is int)
                {
                    return CastTo<T>.From((int)v);
                }
                if (v is string)
                {
                    return (T)Enum.Parse(typeof(T), v.ToString());
                }
            }

            return (T)Convert.ChangeType(v, typeof(T));
        }

        public static bool Contains(string key)
        {
            if (provider == null)
                provider = QuranApp.NativeProvider.SettingsProvider;

            if (cache.ContainsKey(key))
                return true;
            else
                return provider.Contains(key);
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

            if (NeedsToStringSerialization<T>())
            {
                provider[key] = Convert.ChangeType(value, typeof(string));
            }
            else
            {
                provider[key] = value;
            }

            cache[key] = value;
            Save();
        }

        private static bool NeedsToStringSerialization<T>()
        {
            T value = default(T);
            if (value is int || value is double || value is byte || value is string || value is bool)
            {
                return false;
            }
            return true;
        }

        public static void Save()
        {
            if (provider == null)
                provider = QuranApp.NativeProvider.SettingsProvider;

            provider.Save();
        }
    }

    /// <summary>
    /// Class to cast to type <see cref="T"/>
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    public static class CastTo<T>
    {
        /// <summary>
        /// Casts <see cref="S"/> to <see cref="T"/>. 
        /// This does not cause boxing for value types. 
        /// Useful in generic methods
        /// </summary>
        /// <typeparam name="S">Source type to cast from. Usually a generic type.</typeparam>
        public static T From<S>(S s)
        {
            return Cache<S>.caster(s);
        }

        static class Cache<S>
        {
            internal static readonly Func<S, T> caster = Get();

            static Func<S, T> Get()
            {
                var p = Expression.Parameter(typeof(S));
                var c = Expression.ConvertChecked(p, typeof(T));
                return Expression.Lambda<Func<S, T>>(c, p).Compile();
            }
        }
    }
}
