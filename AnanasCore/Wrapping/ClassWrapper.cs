using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AnanasCore.Attributes;
using AnanasCore.Extensions;

namespace AnanasCore.Wrapping
{
    public class ClassWrapper//<T> where T : PersistentObject
    {
        public string Name { get; private set; }
        //private final Class<? extends PersistentObject> classToWrap;
        public Type ClassToWrap { get; private set; }
        private FieldWrapper PrimaryKey;
        private readonly WrappingHandler WrappingHandler;
        private Dictionary<string, FieldWrapper> WrappedFields;
        private Dictionary<string, FieldWrapper> WrappedPersistentFields;
        private Dictionary<string, FieldWrapper> NonPersistentFields;
        private Dictionary<string, FieldWrapper> WrappedRelations;
        private Dictionary<string, FieldWrapper> WrappedValueMember;
        private Dictionary<string, FieldWrapper> WrappedAnonymousRelations;
        private Dictionary<string, FieldWrapper> WrappedIdentifiedAssociations;

        public ClassWrapper(Type toWrap, WrappingHandler handler)
        {
            ClassToWrap = toWrap;
            WrappingHandler = handler;
            Initialize();
        }

        private void Initialize()
        {
            WrappedFields = new Dictionary<string, FieldWrapper>();
            WrappedPersistentFields = new Dictionary<string, FieldWrapper>();
            NonPersistentFields = new Dictionary<string, FieldWrapper>();
            WrappedRelations = new Dictionary<string, FieldWrapper>();
            WrappedValueMember = new Dictionary<string, FieldWrapper>();
            WrappedAnonymousRelations = new Dictionary<string, FieldWrapper>();
            WrappedIdentifiedAssociations = new Dictionary<string, FieldWrapper>();

            Name = CalculateClassName(ClassToWrap);
            CalculateWrappedFields();
        }

        public FieldWrapper GetPrimaryKeyMember()
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

        public List<FieldWrapper> GetWrappedFields(bool alsoNonPersistent = false)
        {
            if (alsoNonPersistent)
                return new List<FieldWrapper>(WrappedFields.Values);
            else
                return new List<FieldWrapper>(WrappedPersistentFields.Values);
        }

        private void CalculateWrappedFields()
        {
            foreach (PropertyInfo field in ClassToWrap.GetProperties())
            {

                // wrap all member
                FieldWrapper wrapper = WrappingHandler.CreateFieldWrapper(this, field);
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

        public FieldWrapper GetFieldWrapper(string fieldName, bool alsoNonPersistent = false, bool ignorecase = false)
        {
            Dictionary<string, FieldWrapper> dicToUse = WrappedPersistentFields;

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

        public FieldWrapper GetRelationWrapper(string relationName)
        {
            return WrappedRelations.GetSave(relationName);
        }

        public List<FieldWrapper> GetRelationWrapper()
        {
            return new List<FieldWrapper>(WrappedRelations.Values);
        }

        public List<FieldWrapper> GetWrappedValueMemberWrapper()
        {
            return new List<FieldWrapper>(WrappedValueMember.Values);
        }

        public FieldWrapper GetWrappedAssociation(string associationName)
        {
            if (WrappedIdentifiedAssociations.ContainsKey(associationName))
                return WrappedIdentifiedAssociations[associationName];

            //if(wrappedAnonymousRelations.ContainsKey(associ)

            return null;
        }

        public override string ToString()
        {
            return "Wrappes: " + ClassToWrap?.Name;
        }
    }
}
