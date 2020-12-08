using AnanasCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnanasCore
{
    /// <summary>
    /// Can cache objects extending <see cref="PersistentObject"/>
    /// </summary>
    public class ObjectCache
    {
        private readonly Dictionary<Type, List<PersistentObject>> permanentCache;
        private readonly Dictionary<Type, List<PersistentObject>> tempCache;
        private readonly List<Type> types;

        public ObjectCache(List<Type> types)
        {
            permanentCache = new Dictionary<Type, List<PersistentObject>>();
            tempCache = new Dictionary<Type, List<PersistentObject>>();
            this.types = types;

            // "deep" init caches
            foreach (var type in types)
            {
                permanentCache.Put(type, new List<PersistentObject>());
                tempCache.Put(type, new List<PersistentObject>());
            }
        }

        /// <summary>
        /// Put the newly loaded objects in the permanent cache
        /// </summary>
        public void ApplyLoadedObjectsToCache()
        {
            foreach (var elem in tempCache)
            {
                permanentCache.Put(elem.Key, elem.Value);
            }

            tempCache.Values.Select(x => x = new List<PersistentObject>()).ToList();
        }

        /// <summary>
        /// Empties the tempCache WITHOUT transmitting data to the permanentCache
        /// </summary>
        public void EmptyTempCache()
        {
            foreach (var type in types)
            {
                tempCache.Put(type, new List<PersistentObject>());
            }
        }

        /// <summary>
        /// Gets objects from a specific type from the cache
        /// </summary>
        /// <param name="cls">look for objects from this type/class</param>
        /// <returns>list of cached objects from given type</returns>
        public List<PersistentObject> Get(Type cls)
        {
            return permanentCache[cls];
        }

        /// <summary>
        /// Gets objects from a specific type from the temp cache
        /// </summary>
        /// <param name="cls">look for objects from this type/class</param>
        /// <returns>list of cached objects from given type</returns>
        public List<PersistentObject> GetTemp(Type cls)
        {
            return tempCache[cls];
        }

        /// <summary>
        /// Adds an <see cref="PersistentObject"/> to the cache
        /// </summary>
        /// <param name="obj">the <see cref="PersistentObject"/> to add</param>
        /// <returns></returns>
        public bool Add(PersistentObject obj)
        {
            Type key = obj.GetType();
            if (permanentCache.ContainsKey(key))
            {
                permanentCache[key].Add(obj);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds an <see cref="PersistentObject"/> to the temp cache
        /// </summary>
        /// <param name="obj">the <see cref="PersistentObject"/> to add</param>
        /// <returns></returns>
        public bool AddTemp(PersistentObject obj)
        {
            Type key = obj.GetType();
            if (tempCache.ContainsKey(key))
            {
                tempCache[key].Add(obj);
                return true;
            }

            return false;
        }
    }
}
