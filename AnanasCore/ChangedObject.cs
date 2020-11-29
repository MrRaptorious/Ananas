using AnanasCore.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnanasCore
{
    public class ChangedObject
    {
        private readonly PersistentObject runtimeObject;
        private readonly Dictionary<string, object[]> changedFields;

        public ChangedObject(PersistentObject runtimeObject)
        {
            this.runtimeObject = runtimeObject;
            changedFields = new Dictionary<string, object[]>();
        }

        /**
		 * Adds the the "fieldName" to the list of changed fields
		 * 
		 * @param fieldName The actual name of the changed field
		 * @param newValue     The new value in the changed field
		 * @param oldValue     The old value in the changed field
		 */
        public void addChangedField(string fieldName, object newValue, object oldValue)
        {
            changedFields.Put(fieldName, new object[] { newValue, oldValue });
        }

        /**
		 * Returns all the changed fields of the handled PersistentObject
		 */
        public Dictionary<string, object> getChangedFields()
        {

            Dictionary<string, object> tmpMap = new Dictionary<string, object>();

            foreach (var elem in changedFields)
            {
                tmpMap.Put(elem.Key, elem.Value[0]);
            }

            return tmpMap;
        }

        /**
		 * Returns handled PersistentObject
		 */
        public PersistentObject getRuntimeObject()
        {
            return runtimeObject;
        }

        /**
		 * Rolls the changes on the handled PersistentObject back
		 */
        public void rollback()
        {
            foreach (var elem in changedFields)
            {
                runtimeObject.setMemberValue(elem.Key, elem.Value[1]);
            }
        }
    }
}
