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
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void Put<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, value);
            else
                dictionary[key] = value;
        }

        public static TValue GetSave<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            if (!dictionary.ContainsKey(key))
                return dictionary[key];

            return default;
        }
    }
}
