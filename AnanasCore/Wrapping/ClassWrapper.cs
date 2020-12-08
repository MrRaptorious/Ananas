using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AnanasCore.Attributes;
using AnanasCore.Extensions;

namespace AnanasCore.Wrapping
{
    /// <summary>
    /// Wraps a type/class at runtime
    /// </summary>
    public class ClassWrapper
    {
        public string Name { get; private set; }
        public Type ClassToWrap { get; private set; }
        private PropertyWrapper PrimaryKey;
        private readonly WrappingHandler WrappingHandler;
        private Dictionary<string, PropertyWrapper> WrappedFields;
        private Dictionary<string, PropertyWrapper> WrappedPersistentFields;
        private Dictionary<string, PropertyWrapper> NonPersistentFields;
        private Dictionary<string, PropertyWrapper> WrappedRelations;
        private Dictionary<string, PropertyWrapper> WrappedValueMember;
        private Dictionary<string, PropertyWrapper> WrappedAnonymousRelations;
        private Dictionary<string, PropertyWrapper> WrappedIdentifiedAssociations;

        public ClassWrapper(Type toWrap, WrappingHandler handler)
        {
            ClassToWrap = toWrap;
            WrappingHandler = handler;
            Initialize();
        }

        private void Initialize()
        {
            WrappedFields = new Dictionary<string, PropertyWrapper>();
            WrappedPersistentFields = new Dictionary<string, PropertyWrapper>();
            NonPersistentFields = new Dictionary<string, PropertyWrapper>();
            WrappedRelations = new Dictionary<string, PropertyWrapper>();
            WrappedValueMember = new Dictionary<string, PropertyWrapper>();
            WrappedAnonymousRelations = new Dictionary<string, PropertyWrapper>();
            WrappedIdentifiedAssociations = new Dictionary<string, PropertyWrapper>();

            Name = CalculateClassName(ClassToWrap);
            CalculateWrappedFields();
        }

        public PropertyWrapper GetPrimaryKeyMember()
        {
            if (PrimaryKey == null)
            {
                foreach (var fieldWrapper in WrappedPersistentFields)
                {
                    if (fieldWrapper.Value.IsPrimaryKey)
                    {
                        PrimaryKey = fieldWrapper.Value;
                        return PrimaryKey;
                    }
                }
            }

            return PrimaryKey;
        }

        public List<PropertyWrapper> GetWrappedFields(bool alsoNonPersistent = false)
        {
            if (alsoNonPersistent)
                return new List<PropertyWrapper>(WrappedFields.Values);
            else
                return new List<PropertyWrapper>(WrappedPersistentFields.Values);
        }

        private void CalculateWrappedFields()
        {
            foreach (PropertyInfo field in ClassToWrap.GetProperties())
            {

                // wrap all member
                PropertyWrapper wrapper = WrappingHandler.CreateFieldWrapper(this, field);
                WrappedFields.Put(field.Name, wrapper);


                // wrap persistent member
                if ((ClassToWrap.GetCustomAttribute<PersistentAttribute>() != null
                    || field.HasCustomAttribute<PersistentAttribute>() || field.HasCustomAttribute<AssociationAttribute>())
                    && !field.HasCustomAttribute<NonPersistentAttribute>() && !wrapper.IsList)
                {
                    WrappedPersistentFields.Put(field.Name, wrapper);

                    // wrap reference member
                    if (typeof(PersistentObject).IsAssignableFrom(field.PropertyType))
                    {

                        // add to all wrappedRelations
                        WrappedRelations.Put(field.Name, wrapper);

                        AssociationAttribute associationAttribute = field.GetCustomAttribute<AssociationAttribute>();

                        // add also to anonymous or identified relations
                        if (associationAttribute == null)
                        {
                            WrappedAnonymousRelations.Put(field.Name, wrapper);
                        }
                        else
                        {
                            WrappedIdentifiedAssociations.Put(associationAttribute.Name, wrapper);
                        }
                    }
                    else
                    { // wrap value Member
                        WrappedValueMember.Put(field.Name, wrapper);
                    }
                }
                else // wrap non persistent member
                {
                    NonPersistentFields.Put(field.Name, wrapper);

                    if (wrapper.IsList)
                    {
                        AssociationAttribute associationAttribute = field.GetCustomAttribute<AssociationAttribute>();

                        if (associationAttribute != null)
                        {
                            WrappedRelations.Put(field.Name, wrapper);
                            WrappedIdentifiedAssociations.Put(associationAttribute.Name, wrapper);
                        }
                    }
                }
            }
        }

        public void UpdateRelations()
        {
            foreach (var entry in WrappedRelations)
            {
                entry.Value.UpdateAssociation();
            }
        }

        public static string CalculateClassName<U>() where U : PersistentObject
        {
            return CalculateClassName(typeof(U));
        }

        public static string CalculateClassName(Type toWrap)
        {
            string name = "";
            PersistentAttribute persistentAnnotation = toWrap.GetCustomAttribute<PersistentAttribute>();

            if (persistentAnnotation != null)
                name = persistentAnnotation.Name;

            if (name == null || name.Equals(""))
                name = toWrap.Name;

            return name;
        }

        public PropertyWrapper GetFieldWrapper(string fieldName, bool alsoNonPersistent = false, bool ignorecase = false)
        {
            Dictionary<string, PropertyWrapper> dicToUse = WrappedPersistentFields;

            if (alsoNonPersistent)
                dicToUse = WrappedFields;

            if (!ignorecase)
            {
                return dicToUse.GetSave(fieldName);
            }
            else
            {
                foreach (var pair in dicToUse)
                {
                    if (pair.Key.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                        return pair.Value;
                }
            }

            return null;
        }

        public PropertyWrapper GetRelationWrapper(string relationName)
        {
            return WrappedRelations.GetSave(relationName);
        }

        public List<PropertyWrapper> GetRelationWrapper()
        {
            return new List<PropertyWrapper>(WrappedRelations.Values);
        }

        public List<PropertyWrapper> GetWrappedValueMemberWrapper()
        {
            return new List<PropertyWrapper>(WrappedValueMember.Values);
        }

        public PropertyWrapper GetWrappedAssociation(string associationName)
        {
            if (WrappedIdentifiedAssociations.ContainsKey(associationName))
                return WrappedIdentifiedAssociations[associationName];

            return null;
        }
    }
}
