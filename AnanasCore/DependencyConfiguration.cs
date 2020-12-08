using AnanasCore.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AnanasCore
{
    /// <summary>
    /// Abstract class to collect all database specific classes
    /// </summary>
    public abstract class DependencyConfiguration
    {
        private readonly Dictionary<Type, Type> typeMapping;
        private readonly Dictionary<Type, object> objectMapping;

        public DependencyConfiguration()
        {
            typeMapping = new Dictionary<Type, Type>();
            objectMapping = new Dictionary<Type, object>();

            ConfigureTypes();
        }

        /// <summary>
        /// Creates an instance of a type
        /// </summary>
        /// <param name="t">type of class to create instance from</param>
        /// <returns>new instance of type T</returns>
        public object Resolve(Type t)
        {

            if (!typeMapping.ContainsKey(t))
                throw new ArgumentException("The type \"" + t.Name + "\" is not registered");

            if (objectMapping.ContainsKey(t))
            {
                if (objectMapping[t] == null)
                    objectMapping.Put(t, CreateInstance(typeMapping[t]));

                return objectMapping[t];
            }

            return CreateInstance(typeMapping[t]);
        }

        /// <summary>
        /// Creates an instance of a given type
        /// </summary>
        /// <param name="t">type of class to create an instance from</param>
        /// <returns>new instance of type <paramref name="t"/></returns>
        private object CreateInstance(Type t)
        {
            var constructors = t.GetConstructors();
            List<ConstructorInfo> validConstructors = new List<ConstructorInfo>();

            foreach (var constructor in constructors)
            {
                bool skipConstructor = false;
                var parameter = constructor.GetParameters();

                foreach (var param in parameter)
                {
                    if (!typeMapping.ContainsKey(param.ParameterType))
                    {
                        skipConstructor = true;
                        break;
                    }
                }

                if (skipConstructor)
                    continue;
                else
                    validConstructors.Add(constructor);
            }

            if (validConstructors.Count == 0 || validConstructors.Count > 1)
                throw new TypeInitializationException(t.FullName, new ArgumentException("There is no definitive Constructor for the type " + t.Name));

            ConstructorInfo currentConstructor = validConstructors[0];
            object[] argumentArray = new object[currentConstructor.GetParameters().Length];

            for (int i = 0; i < currentConstructor.GetParameters().Length; i++)
            {
                try
                {
                    argumentArray[i] = Resolve(currentConstructor.GetParameters()[i].ParameterType);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            return Activator.CreateInstance(t, argumentArray);
        }

        /// <summary>
        /// Adds a mapping to the type mapping list
        /// </summary>
        /// <param name="src">base class</param>
        /// <param name="target">extending class</param>
        protected void AddMapping(Type src, Type target)
        {
            typeMapping.Put(src, target);
        }

        /// <summary>
        /// Adds a mapping to the type mapping list
        /// </summary>
        /// <param name="src">base class</param>
        /// <param name="target">extending class</param>
        /// <param name="isSingle">determines if there can be multiple instances of a class</param>
        protected void AddMapping(Type src, Type target, bool isSingle)
        {
            typeMapping.Put(src, target);

            if (isSingle)
            {
                objectMapping.Put(src, null);
            }
        }

        /// <summary>
        /// Adds all mappings
        /// </summary>
        protected abstract void ConfigureTypes();
    }
}
