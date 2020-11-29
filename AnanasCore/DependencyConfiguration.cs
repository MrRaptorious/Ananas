using AnanasCore.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AnanasCore
{
    public abstract class DependencyConfiguration
    {

        private readonly Dictionary<Type, Type> typeMapping;
        private readonly Dictionary<Type, object> objectMapping;

        public DependencyConfiguration()
        {
            typeMapping = new Dictionary<Type, Type>();
            objectMapping = new Dictionary<Type, object>();

            configureTypes();
        }

        /**
         * Creates an instance of a type
         * @param cls class to create instance from
         * @param <T> type of class to create instance from
         * @return new instance of type T
         */
        public object resolve(Type t)
        {

            if (!typeMapping.ContainsKey(t))
                throw new ArgumentException("The type \"" + t.Name + "\" is not registered");

            if (objectMapping.ContainsKey(t))
            {
                if (objectMapping[t] == null)
                    objectMapping.Put(t, createInstance(typeMapping[t]));

                return objectMapping[t];
            }

            return createInstance(typeMapping[t]);
        }

        /**
         *  creates an instance of a given type
         * @param cls class of type T to create a instance from
         * @param <T> type of class to create an instance from
         */
        private object createInstance(Type t)
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
                    argumentArray[i] = resolve(currentConstructor.GetParameters()[i].ParameterType);
                }
                catch (Exception e)
                {
                    // TODO
                    throw e;
                }
            }

            return Activator.CreateInstance(t, argumentArray);
        }

        /**
         * Adds a mapping to the type mapping list
         * @param src base class
         * @param target extending class
         */
        protected void addMapping(Type src, Type target)
        {
            typeMapping.Put(src, target);
        }

        /**
         * Adds a mapping to the type mapping list
         * @param src base class
         * @param target extending class
         * @param isSingle determines if there can be multiple instances of a class
         */
        protected void addMapping(Type src, Type target, bool isSingle)
        {
            typeMapping.Put(src, target);

            if (isSingle)
            {
                objectMapping.Put(src, null);
            }
        }

        protected abstract void configureTypes();
    }
}
