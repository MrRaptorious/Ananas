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
        private string name;
        //private final Class<? extends PersistentObject> classToWrap;
        private readonly Type classToWrap;
        private FieldWrapper primaryKey;
        private readonly WrappingHandler wrappingHandler;
        private Dictionary<string, FieldWrapper> wrappedFields;
        private Dictionary<string, FieldWrapper> wrappedPersistentFields;
        private Dictionary<string, FieldWrapper> nonPersistentFields;
        private Dictionary<string, FieldWrapper> wrappedRelations;
        private Dictionary<string, FieldWrapper> wrappedValueMember;
        private Dictionary<string, FieldWrapper> wrappedAnonymousRelations;
        private Dictionary<string, FieldWrapper> wrappedIdentifiedAssociations;

        public ClassWrapper(Type toWrap, WrappingHandler handler)
        {
            classToWrap = toWrap;
            wrappingHandler = handler;
            initialize();
        }

        private void initialize()
        {
            wrappedFields = new Dictionary<string, FieldWrapper>();
            wrappedPersistentFields = new Dictionary<string, FieldWrapper>();
            nonPersistentFields = new Dictionary<string, FieldWrapper>();
            wrappedRelations = new Dictionary<string, FieldWrapper>();
            wrappedValueMember = new Dictionary<string, FieldWrapper>();
            wrappedAnonymousRelations = new Dictionary<string, FieldWrapper>();
            wrappedIdentifiedAssociations = new Dictionary<string, FieldWrapper>();

            name = calculateClassName(classToWrap);
            calculateWrappedFields();
        }

        public FieldWrapper GetPrimaryKeyMember()
        {
            if (primaryKey == null)
            {
                foreach (var fieldWrapper in wrappedPersistentFields)
                {
                    if (fieldWrapper.Value.isPrimaryKey)
                    {
                        primaryKey = fieldWrapper.Value;
                        return primaryKey;
                    }
                }
            }

            return primaryKey;
        }

        public List<FieldWrapper> getWrappedFields(bool alsoNonPersistent = false)
        {
            if (alsoNonPersistent)
                return new List<FieldWrapper>(wrappedFields.Values);
            else
                return new List<FieldWrapper>(wrappedPersistentFields.Values);
        }

        public string getName()
        {
            return name;
        }

        private void calculateWrappedFields()
        {
            foreach (PropertyInfo field in classToWrap.GetProperties())
            {

                // wrap all member
                FieldWrapper wrapper = wrappingHandler.CreateFieldWrapper(this, field);
                wrappedFields.Put(field.Name, wrapper);


                // wrap persistent member
                if ((classToWrap.GetCustomAttribute<PersistentAttribute>() != null
                    || field.HasCustomAttribute<PersistentAttribute>() || field.HasCustomAttribute<AssociationAttribute>())
                    && !field.HasCustomAttribute<NonPersistentAttribute>() && !wrapper.isList)
                {
                    wrappedPersistentFields.Put(field.Name, wrapper);

                    // wrap reference member
                    if (typeof(PersistentObject).IsAssignableFrom(field.PropertyType))
                    {

                        // add to all wrappedRelations
                        wrappedRelations.Put(field.Name, wrapper);

                        AssociationAttribute associationAttribute = field.GetCustomAttribute<AssociationAttribute>();

                        // add also to anonymous or identified relations
                        if (associationAttribute == null)
                        {
                            wrappedAnonymousRelations.Put(field.Name, wrapper);
                        }
                        else
                        {
                            wrappedIdentifiedAssociations.Put(associationAttribute.Name, wrapper);
                        }
                    }
                    else
                    { // wrap value Member
                        wrappedValueMember.Put(field.Name, wrapper);
                    }
                }
                else // wrap non persistent member
                {
                    nonPersistentFields.Put(field.Name, wrapper);

                    if (wrapper.isList)
                    {
                        AssociationAttribute associationAttribute = field.GetCustomAttribute<AssociationAttribute>();

                        if (associationAttribute != null)
                        {
                            wrappedRelations.Put(field.Name, wrapper);
                            wrappedIdentifiedAssociations.Put(associationAttribute.Name, wrapper);
                        }
                    }
                }
            }
        }

        public void updateRelations()
        {
            foreach (var entry in wrappedRelations)
            {
                entry.Value.updateAssociation();
            }
        }

        public static string calculateClassName<U>() where U : PersistentObject
        {
            return calculateClassName(typeof(U));
        }

        public static string calculateClassName(Type toWrap)
        {
            string name = "";
            PersistentAttribute persistentAnnotation = toWrap.GetCustomAttribute<PersistentAttribute>();

            if (persistentAnnotation != null)
                name = persistentAnnotation.Name;

            if (name == null || name.Equals(""))
                name = toWrap.Name;

            return name;
        }

        public FieldWrapper getFieldWrapper(string fieldName, bool alsoNonPersistent = false)
        {
            if (alsoNonPersistent)
                return wrappedFields.GetSave(fieldName);
            else
                return wrappedPersistentFields.GetSave(fieldName);
        }

        public FieldWrapper getRelationWrapper(String relationName)
        {
            return wrappedRelations.GetSave(relationName);
        }

        public List<FieldWrapper> getRelationWrapper()
        {
            return new List<FieldWrapper>(wrappedRelations.Values);
        }

        public Type getClassToWrap()
        {
            return classToWrap;
        }

        public List<FieldWrapper> getWrappedValueMemberWrapper()
        {
            return new List<FieldWrapper>(wrappedValueMember.Values);
        }

        public FieldWrapper getWrappedAssociation(string associationName)
        {
            if (wrappedIdentifiedAssociations.ContainsKey(associationName))
                return wrappedIdentifiedAssociations[associationName];

            //if(wrappedAnonymousRelations.ContainsKey(associ)

            return null;
        }

        public override string ToString()
        {
            return "Wrappes: " + classToWrap?.Name;
        }
    }
}
