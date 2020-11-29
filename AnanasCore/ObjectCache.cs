using AnanasCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnanasCore
{
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

        /**
         * Put the newly loaded objects in the permanent cache
         */
        public void applyLoadedObjectsToCache()
        {
            foreach (var elem in tempCache)
            {
                permanentCache.Put(elem.Key, elem.Value);
            }

            tempCache.Values.Select(x => x = new List<PersistentObject>()).ToList();
        }

        /**
         * Empties the tempCache WITHOUT transmitting data to the permanentCache
         */
        public void emptyTempCache()
        {
            foreach (var type in types)
            {
                tempCache.Put(type, new List<PersistentObject>());
            }
        }

        /**
         * Gets objects from a specific type from the cache
         *
         * @param cls look for objects from this type/class
         * @return list of cached objects from given type
         */
        public List<PersistentObject> get(Type cls)
        {
            return permanentCache[cls];
        }

        /**
         * Gets objects from a specific type from the temp cache
         *
         * @param cls look for objects from this type/class
         * @return list of cached objects from given type
         */
        public List<PersistentObject> getTemp(Type cls)
        {
            return tempCache[cls];
        }

        /**
         * Adds an object to the cache
         *
         * @param obj the object to add
         */
        public bool add(PersistentObject obj)
        {
            Type key = obj.GetType();
            if (permanentCache.ContainsKey(key))
            {
                permanentCache[key].Add(obj);
                return true;
            }

            return false;
        }

        /**
         * Adds an object to the temp cache
         *
         * @param obj the object to add
         */
        public bool addTemp(PersistentObject obj)
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
