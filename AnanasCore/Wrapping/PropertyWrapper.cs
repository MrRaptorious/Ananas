using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AnanasCore.Attributes;
using AnanasCore.Extensions;

namespace AnanasCore.Wrapping
{
    /// <summary>
    /// Wraps a property of a class
    /// </summary>
    public class PropertyWrapper
    {
        public ClassWrapper DeclaringClassWrapper { get; }
        private AssociationWrapper Association;
        public PropertyInfo OriginalField { get; }
        public string Name { get; }
        public string DBType { get; }
        public bool IsPrimaryKey { get; }
        public bool CanNotBeNull { get; }
        public bool Autoincrement { get; }
        public bool IsList { get; }
        public int Size { get; }
        public WrappingHandler WrappingHandler { get; }

        public PropertyWrapper(ClassWrapper cw, PropertyInfo field, WrappingHandler handler)
        {
            WrappingHandler = handler;
            OriginalField = field;
            Name = CalculateFieldName(field);
            Size = field.HasCustomAttribute<SizeAttribute>() ? field.GetCustomAttribute<SizeAttribute>().Length : -1;
            DBType = handler.PropertyTypeParser.ParseFieldType(field.PropertyType, Size);
            IsPrimaryKey = field.HasCustomAttribute<PrimaryKeyAttribute>();
            CanNotBeNull = field.HasCustomAttribute<CanNotBeNullAttribute>();
            Autoincrement = field.HasCustomAttribute<AutoincrementAttribute>();
            IsList = typeof(GenericList).IsAssignableFrom(field.PropertyType);
            DeclaringClassWrapper = cw;
            Association = null;
        }

        public bool IsForeignKey()
        {
            return Association != null;
        }

        public AssociationWrapper GetForeignKey()
        {
            return Association;
        }

        public void UpdateAssociation()
        {
            if (typeof(PersistentObject).IsAssignableFrom(OriginalField.PropertyType) || IsList)
            {
                string name = null;

                if (OriginalField.HasCustomAttribute<AssociationAttribute>())
                    name = OriginalField.GetCustomAttribute<AssociationAttribute>().Name;

                ClassWrapper foreignClassWrapper;

                if (!IsList)
                {
                    //foreignClassWrapper = wrappingHandler
                    //.getClassWrapper((Class <? extends PersistentObject >) fieldToWrap.getType
                    foreignClassWrapper = WrappingHandler.GetClassWrapper(OriginalField.PropertyType);
                }
                else
                {
                    // find generic parameter
                    var foreignClass = OriginalField.PropertyType.GetGenericArguments()[0];

                    // find classWrapper
                    foreignClassWrapper = WrappingHandler.GetClassWrapper(foreignClass);
                }

                Association = new AssociationWrapper(foreignClassWrapper, name);
            }
        }

        public static string CalculateFieldName(PropertyInfo field)
        {
            string name = "";
            PersistentAttribute persistentAnnotation = field.GetCustomAttribute<PersistentAttribute>();

            if (persistentAnnotation != null)
                name = persistentAnnotation.Name;

            if (name == null || name.Equals(""))
                name = field.Name;

            return name;
        }

        public T GetAttribute<T>() where T : Attribute
        {
            return OriginalField.GetCustomAttribute<T>();
        }
    }
}
