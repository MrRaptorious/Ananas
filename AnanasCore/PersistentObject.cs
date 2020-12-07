using AnanasCore.Attributes;
using AnanasCore.Extensions;
using AnanasCore.Wrapping;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AnanasCore
{
    public class PersistentObject
    {
        public const string KeyPropertyName = "ID";

        private readonly ObjectSpace objectSpace;

        [PrimaryKey]
        [Persistent(KeyPropertyName)]
        public Guid ID { get; set; }

        [Persistent("CREATIONDATE")]
        public DateTime CreationDate { get; set; }

        [Persistent("LASTCHANGE")]
        public DateTime LastChange { get; set; }

        [Persistent("DELETED")]
        public bool IsDeleted { get; set; }

        public PersistentObject(ObjectSpace os)
        {
            if (os != null && !os.IsLoadingObjects)
            {
                ID = Guid.NewGuid();
                CreationDate = DateTime.Now;
                LastChange = DateTime.Now;
                os.AddCreatedObject(this);
            }

            objectSpace = os;
        }

        public void Delete()
        {
            SetPropertyValue(nameof(IsDeleted), true);
        }

        /**
		 * Gets all fields with values from calling object
		 * 
		 * @return Map of all fields and values
		 */
        public Dictionary<FieldWrapper, object> GetPersistentPropertiesWithValues()
        {
            List<FieldWrapper> wrappedFields = objectSpace.WrappingHandler.GetClassWrapper(this.GetType())
                    .GetWrappedFields();
            Dictionary<FieldWrapper, object> mapping = new Dictionary<FieldWrapper, object>();

            foreach (FieldWrapper fw in wrappedFields)
            {
                try
                {
                    mapping.Put(fw, fw.OriginalField.GetValue(this));
                }
                catch
                {
                    throw;
                }
            }
            return mapping;
        }


        /**
         * Gets the value of a member with the given name
         * 
         * @param memberName the name of the field from which to get the value
         * @return the value of the given member
         */
        public object GetMemberValue(string memberName)
        {
            try
            {
                PropertyInfo f = this.GetType().GetProperty(memberName);
                return f.GetValue(this);
            }
            catch
            {
                throw;
            }
        }

        /**
         * Sets the value of a member with the given name (WILL NOT RESULT IN DB-UPDATE)
         * 
         * @param memberName the name of the field to which to set the value to
         * @param value      the value to set
         * @return the value of the given member
         */
        public bool SetMemberValue(string memberName, object value, bool ignoreCase = false)
        {
            try
            {
                PropertyInfo f = objectSpace.WrappingHandler.GetClassWrapper(GetType()).GetFieldWrapper(memberName, true, ignoreCase).OriginalField;

                f.SetValue(this, value);

                return true;
            }
            catch
            {
                throw;
            }
        }

        /**
         * Sets the value of a member AND marks change to update in database
         * 
         * @param changedMember the name of the changed member
         * @param newValue      the new value of the member
         */
        public void SetPropertyValue(string changedMember, object newValue)
        {
            var oldvalue = GetMemberValue(changedMember);

            if ((newValue != null && !newValue.Equals(oldvalue)) || (newValue is null && oldvalue != null))
            {
                objectSpace.AddChangedObject(this, changedMember, newValue);
                SetMemberValue(changedMember, newValue);
            }
        }

        public void SetPropertyValue<T>(string changedMember, object newValue, ref T capsuledMember)
        {
            // only track when objectspace is not null
            if (objectSpace is null)
            {
                capsuledMember = (T)newValue;
            }
            else
            {
                var oldvalue = GetMemberValue(changedMember);

                if ((newValue != null && !newValue.Equals(oldvalue)) || (newValue is null && oldvalue != null))
                {
                    objectSpace.AddChangedObject(this, changedMember, newValue);
                    capsuledMember = (T)newValue;
                }
            }
        }

        /**
         * Sets the member name to the new value and also updates the referenced objects reference
         * 
         * @param memberName the name of the changed to change
         * @param value      the new value
         */
        public void SetRelation(string memberName, PersistentObject value)
        {
            SetMemberValue(memberName, value);

            AssociationWrapper aw = objectSpace.WrappingHandler.GetClassWrapper(GetType())
                    .GetFieldWrapper(memberName, true).GetForeignKey();

            if (aw != null && aw.AssociationPartner != null && !aw.AssociationPartner.IsList)
            {
                value.SetMemberValue(aw.AssociationPartner.OriginalField.Name, this);
            }
        }

        /**
         * Gets the referenced List of objects of type T as JormList<T>
         * 
         * @param memberName the name of the JormList<T> member
         */
        protected AnanasList<T> GetList<T>(string memberName, ref AnanasList<T> prop) where T : PersistentObject
        {
            if (prop is null)
            {
                AssociationAttribute association = objectSpace.WrappingHandler.GetClassWrapper(GetType())
                    .GetFieldWrapper(memberName, true)
                    .OriginalField
                    .GetCustomAttribute<AssociationAttribute>();
                if (association != null)
                {
                    AnanasList<T> list = new AnanasList<T>(objectSpace, this, association.Name);
                    prop = list;
                    return list;
                }
            }

            return prop;
        }

        public override string ToString()
        {
            return GetType().Name + "@" + ID;
        }
    }
}
