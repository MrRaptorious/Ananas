using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AnanasCore.Attributes;
using AnanasCore.Extensions;

namespace AnanasCore.Wrapping
{
    public class FieldWrapper//<T,U> where T : PersistentObject
    {
        private readonly ClassWrapper declaringClassWrapper;
        private AssociationWrapper association;
        private PropertyInfo fieldToWrap { get; }
        public string name { get; }
        public string dbType { get; }
        public bool isPrimaryKey { get; }
        public bool canNotBeNull { get; }
        public bool autoincrement { get; }
        public bool isList { get; }
        public int size { get; }
        public WrappingHandler wrappingHandler { get; }

        public FieldWrapper(ClassWrapper cw, PropertyInfo field, WrappingHandler handler)
        {
            wrappingHandler = handler;
            fieldToWrap = field;
            name = calculateFieldName(field);
            size = field.HasCustomAttribute<SizeAttribute>() ? field.GetCustomAttribute<SizeAttribute>().Length : -1;
            dbType = handler.getFieldTypeParser().ParseFieldType(field.PropertyType, size);
            isPrimaryKey = field.HasCustomAttribute<PrimaryKeyAttribute>();
            canNotBeNull = field.HasCustomAttribute<CanNotBeNullAttribute>();
            autoincrement = field.HasCustomAttribute<AutoincrementAttribute>();
            isList = typeof(GenericList).IsAssignableFrom(field.PropertyType);
            declaringClassWrapper = cw;
            association = null;
        }

        public bool IsForeignKey()
        {
            return association != null;
        }

        public AssociationWrapper GetForeignKey()
        {
            return association;
        }

        public ClassWrapper getClassWrapper()
        {
            return declaringClassWrapper;
        }

        public void updateAssociation()
        {
            if (typeof(PersistentObject).IsAssignableFrom(fieldToWrap.PropertyType) || isList)
            {
                string name = null;

                if (fieldToWrap.HasCustomAttribute<AssociationAttribute>())
                    name = fieldToWrap.GetCustomAttribute<AssociationAttribute>().Name;

                ClassWrapper foreignClassWrapper = null;

                if (!isList)
                {
                    //foreignClassWrapper = wrappingHandler
                    //.getClassWrapper((Class <? extends PersistentObject >) fieldToWrap.getType
                    foreignClassWrapper = wrappingHandler.getClassWrapper(fieldToWrap.PropertyType);
                }
                else
                {
                    // find generic parameter
                    var foreignClass = fieldToWrap.PropertyType.GetGenericArguments()[0];

                    // find classWrapper
                    foreignClassWrapper = wrappingHandler.getClassWrapper(foreignClass);
                }

                association = new AssociationWrapper(foreignClassWrapper, name);
            }
        }

        public static string calculateFieldName(PropertyInfo field)
        {
            string name = "";
            PersistentAttribute persistentAnnotation = field.GetCustomAttribute<PersistentAttribute>();

            if (persistentAnnotation != null)
                name = persistentAnnotation.Name;

            if (name == null || name.Equals(""))
                name = field.Name;

            return name;
        }

        public PropertyInfo getOriginalField()
        {
            return fieldToWrap;
        }

        public T getAnnotation<T>() where T : Attribute
        {
            return fieldToWrap.GetCustomAttribute<T>();
        }

        public override string ToString()
        {
            return "Wrappes : " + this.fieldToWrap?.Name;
        }
    }
}
