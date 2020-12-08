using AnanasCore.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnanasCore
{
    /// <summary>
    /// Represents a changed <see cref="PersistentObject"/>
    /// </summary>
    public class ChangedObject
    {
        public PersistentObject RuntimeObject { get; private set; }
        private readonly Dictionary<string, object[]> changedFields;

        public ChangedObject(PersistentObject runtimeObject)
        {
            RuntimeObject = runtimeObject;
            changedFields = new Dictionary<string, object[]>();
        }

        /// <summary>
        /// Adds the the <paramref name="fieldName"/> to the list of changed fields
        /// </summary>
        /// <param name="fieldName">The actual name of the changed field</param>
        /// <param name="newValue">The new value in the changed field</param>
        /// <param name="oldValue">The old value in the changed field</param>
        public void addChangedField(string fieldName, object newValue, object oldValue)
        {
            changedFields.Put(fieldName, new object[] { newValue, oldValue });
        }

        /// <summary>
        /// Returns all the changed fields of the handled <see cref="PersistentObject"/>
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetChangedFields()
        {
            Dictionary<string, object> tmpMap = new Dictionary<string, object>();

            foreach (var elem in changedFields)
            {
                tmpMap.Put(elem.Key, elem.Value[0]);
            }

            return tmpMap;
        }

        /// <summary>
        /// Rolls the changes on the handled <see cref="PersistentObject"/> back
        /// </summary>
        public void Rollback()
        {
            foreach (var elem in changedFields)
            {
                RuntimeObject.SetMemberValue(elem.Key, elem.Value[1]);
            }
        }
    }
}
