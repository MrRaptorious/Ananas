using AnanasCore.Attributes;
using AnanasCore.Extensions;
using AnanasCore.Wrapping;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AnanasCore
{
    /// <summary>
    /// Represents the base class for all objects in the database
    /// </summary>
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

        /// <summary>
        /// Gets all fields with values from calling object
        /// </summary>
        /// <returns>Dictionary of all fields and values</returns>
        public Dictionary<PropertyWrapper, object> GetPersistentPropertiesWithValues()
        {
            List<PropertyWrapper> wrappedFields = objectSpace.WrappingHandler.GetClassWrapper(this.GetType())
                    .GetWrappedFields();
            Dictionary<PropertyWrapper, object> mapping = new Dictionary<PropertyWrapper, object>();

            foreach (PropertyWrapper fw in wrappedFields)
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


        /// <summary>
        /// Gets the value of a member with the given name
        /// </summary>
        /// <param name="memberName">the name of the field from which to get the value</param>
        /// <returns>the value of the given member</returns>
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

        /// <summary>
        /// Sets the value of a member with the given name
        /// </summary>
        /// <param name="memberName">the name of the field to which to set the value to</param>
        /// <param name="value">the value to set</param>
        /// <param name="ignoreCase">determines if the memberName is case sensitive</param>
        /// <returns>the value of the given member</returns>
        public bool SetMemberValue(string memberName, object value, bool ignoreCase = false)
        {
            PropertyInfo f = objectSpace.WrappingHandler.GetClassWrapper(GetType()).GetFieldWrapper(memberName, true, ignoreCase).OriginalField;

            f.SetValue(this, value);

            return true;
        }

        /// <summary>
        /// Sets the value of a member 
        /// </summary>
        /// <param name="changedMember">the name of the changed member</param>
        /// <param name="newValue">the new value of the member</param>
        public void SetPropertyValue(string changedMember, object newValue)
        {
            var oldvalue = GetMemberValue(changedMember);

            if ((newValue != null && !newValue.Equals(oldvalue)) || (newValue is null && oldvalue != null))
            {
                objectSpace.AddChangedObject(this, changedMember, newValue);
                SetMemberValue(changedMember, newValue);
            }
        }

        /// <summary>
        /// Sets the value of a member 
        /// </summary>
        /// <typeparam name="T">type of member to set</typeparam>
        /// <param name="changedMember">the name of the changed member</param>
        /// <param name="newValue">the new value of the member</param>
        /// <param name="capsuledMember">the private encapsulated member</param>
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

        /// <summary>
        /// Sets the member name to the new value and also updates the referenced objects reference
        /// </summary>
        /// <param name="memberName">the name of the changed to change</param>
        /// <param name="value">the new value</param>
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

        /// <summary>
        /// Gets the referenced List of objects of type T as <see cref="AnanasList{T}"/>
        /// </summary>
        /// <param name="memberName">the name of the <see cref="AnanasList{T}"/> member</param>
        /// <param name="prop">the private encapsulated member</param>
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
    }
}
