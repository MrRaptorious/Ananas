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
            if (!os.isLoadingObjects)
            {
                ID = Guid.NewGuid();
                CreationDate = DateTime.Now;
                LastChange = DateTime.Now;
                os.addCreatedObject(this);
            }

            objectSpace = os;
        }

        /**
		 * Gets all fields with values from calling object
		 * 
		 * @return Map of all fields and values
		 */
        public Dictionary<FieldWrapper, object> getPersistentPropertiesWithValues()
        {
            List<FieldWrapper> wrappedFields = objectSpace.wrappingHandler.getClassWrapper(this.GetType())
                    .getWrappedFields();
            Dictionary<FieldWrapper, Object> mapping = new Dictionary<FieldWrapper, object>();

            foreach (FieldWrapper fw in wrappedFields)
            {
                try
                {
                    mapping.Put(fw, fw.getOriginalField().GetValue(this));
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
        public Object getMemberValue(String memberName)
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
        public bool setMemberValue(String memberName, Object value)
        {
            try
            {

                PropertyInfo f = objectSpace.wrappingHandler.getClassWrapper(GetType()).getFieldWrapper(memberName, true)
                .getOriginalField();

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
        public void setPropertyValue(string changedMember, object newValue)
        {
            if (getMemberValue(changedMember) != newValue)
            {
                objectSpace.addChangedObject(this, changedMember, newValue);
                this.setMemberValue(changedMember, newValue);
            }
        }

        /**
         * Sets the member name to the new value and also updates the referenced objects reference
         * 
         * @param memberName the name of the changed to change
         * @param value      the new value
         */
        public void setRelation(string memberName, PersistentObject value)
        {
            setMemberValue(memberName, value);

            AssociationWrapper aw = objectSpace.wrappingHandler.getClassWrapper(GetType())
                    .getFieldWrapper(memberName, true).GetForeignKey();

            if (aw != null && aw.getAssociationPartner() != null && !aw.getAssociationPartner().isList)
            {
                value.setMemberValue(aw.getAssociationPartner().getOriginalField().Name, this);
            }
        }

        /**
         * Gets the referenced List of objects of type T as JormList<T>
         * 
         * @param memberName the name of the JormList<T> member
         */
        protected AnanasList<T> getList<T>(string memberName) where T : PersistentObject
        {
            if (getMemberValue(memberName) == null)
            {
                AssociationAttribute association = objectSpace.wrappingHandler.getClassWrapper(GetType())
                    .getFieldWrapper(memberName, true)
                    .getOriginalField()
                    .GetCustomAttribute<AssociationAttribute>();
                if (association != null)
                {
                    AnanasList<T> list = new AnanasList<T>(objectSpace, this, association.Name);
                    setMemberValue(memberName, list);
                    return list;
                }
            }

            return (AnanasList<T>)getMemberValue(memberName);
        }

        public override string ToString()
        {
            return GetType().Name + "@" + ID;
        }
    }
}
