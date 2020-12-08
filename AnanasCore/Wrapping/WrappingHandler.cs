using AnanasCore.DBConnection;
using AnanasCore.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AnanasCore.Wrapping
{
    /// <summary>
    /// Handels all wrapping
    /// </summary>
    public class WrappingHandler
    {
        public TypeParser PropertyTypeParser { get; private set; }
        public Dictionary<Type, ClassWrapper> ClassWrapper { get; private set; }

        public PropertyWrapper CreateFieldWrapper(ClassWrapper cw, PropertyInfo field)
        {
            return new PropertyWrapper(cw, field, this);
        }

        public ClassWrapper CreateClassWrapper(Type cls)
        {
            return new ClassWrapper(cls, this);
        }

        public WrappingHandler(TypeParser parser)
        {
            ClassWrapper = new Dictionary<Type, ClassWrapper>();
            PropertyTypeParser = parser;
        }

        public bool RegisterType(Type cls)
        {
            if (!typeof(PersistentObject).IsAssignableFrom(cls))
            {
                throw new ArgumentException($"The type \"{cls.Name}\" is not a decend of PersistentObject!");
            }

            if (!ClassWrapper.ContainsKey(cls))
            {
                ClassWrapper.Put(cls, new ClassWrapper(cls, this));
                return true;
            }

            return false;
        }

        public List<ClassWrapper> GetWrapperList()
        {
            return new List<ClassWrapper>(ClassWrapper.Values);
        }

        public List<Type> GetRegisteredTypes() { return new List<Type>(ClassWrapper.Keys); }

        public ClassWrapper GetClassWrapper(Type cls)
        {
            if (ClassWrapper.ContainsKey(cls))
                return ClassWrapper[cls];

            return null;
        }

        public PropertyWrapper GetFieldWrapper(Type type, string fieldName)
        {
            return ClassWrapper[type].GetFieldWrapper(fieldName);
        }

        public void UpdateRelations()
        {
            foreach (var entry in ClassWrapper)
            {
                entry.Value.UpdateRelations();
            }
        }
    }
}
