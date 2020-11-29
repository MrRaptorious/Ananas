using AnanasCore.DBConnection;
using AnanasCore.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AnanasCore.Wrapping
{
    public class WrappingHandler
    {
        private readonly FieldTypeParser fieldTypeParser;
        public Dictionary<Type, ClassWrapper> classWrapper;

        public FieldWrapper CreateFieldWrapper(ClassWrapper cw, PropertyInfo field)// where T : PersistentObject
        {
            return new FieldWrapper(cw, field, this);
        }

        public ClassWrapper createClassWrapper(Type cls)
        {
            return new ClassWrapper(cls, this);
        }

        public WrappingHandler(FieldTypeParser parser)
        {
            classWrapper = new Dictionary<Type, ClassWrapper>();
            fieldTypeParser = parser;
        }

        public bool registerType(Type cls)
        {

            if (!classWrapper.ContainsKey(cls))
            {
                classWrapper.Put(cls, new ClassWrapper(cls, this));
                return true;
            }

            return false;
        }

        public Dictionary<Type, ClassWrapper> getWrappingMap()
        {
            return classWrapper;
        }

        public List<ClassWrapper> getWrapperList()
        {
            return new List<ClassWrapper>(classWrapper.Values);
        }

        public List<Type> getRegisteredTypes() { return new List<Type>(classWrapper.Keys); }

        public ClassWrapper getClassWrapper(Type cls)
        {
            if (classWrapper.ContainsKey(cls))
                return classWrapper[cls];

            return null;
        }

        public FieldWrapper getFieldWrapper(Type type, string fieldName)
        {
            return classWrapper[type].getFieldWrapper(fieldName);
        }

        public void updateRelations()
        {
            foreach (var entry in classWrapper)
            {
                entry.Value.updateRelations();
            }
        }

        public FieldTypeParser getFieldTypeParser()
        {
            return fieldTypeParser;
        }
    }
}
