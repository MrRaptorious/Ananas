using System;
using System.Collections.Generic;
using System.Text;

namespace AnanasCore.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Replaces or Create <paramref name="key"/> with value <paramref name="value"/>
        /// </summary>
        public static void Put<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, value);
            else
                dictionary[key] = value;
        }

        /// <summary>
        /// Gets a value from a dictionary or the default value
        /// </summary>
        /// <returns>value from key or default</returns>
        public static TValue GetSave<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            
            if (dictionary.ContainsKey(key))
                return dictionary[key];

            return default;
        }
    }
}
