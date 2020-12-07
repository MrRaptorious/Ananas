using AnanasCore.Criteria;
using AnanasCore.Wrapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnanasCore
{
    public class AnanasList<T> : List<T>, GenericList where T : PersistentObject
    {
        private readonly ObjectSpace ObjectSpace;
        private readonly PersistentObject Owner;
        private readonly string RelationName;
        private readonly FieldWrapper ListMember;

        public AnanasList(ObjectSpace os, PersistentObject owner, string relationName)
        {
            ObjectSpace = os;
            Owner = owner;
            RelationName = relationName;
            ListMember = ObjectSpace.WrappingHandler.GetClassWrapper(owner.GetType())
                    .GetWrappedAssociation(relationName);

            Load();
        }

        /**
		 * Adds the PersistentObject o to the list
		 * @param o object of type T to add
		 * @return if object is added
		 */
        public new void Add(T objectToAdd)
        {
            string fieldName = ObjectSpace.WrappingHandler.GetClassWrapper(objectToAdd.GetType())
                    .GetWrappedAssociation(RelationName).OriginalField.Name;
            objectToAdd.SetMemberValue(fieldName, Owner);
            base.Add(objectToAdd);
        }

        public new bool Remove(T objectToRemove)
        {

            bool removed = base.Remove(objectToRemove);

            if (removed)
            {
                try
                {
                    // set reference to null
                    string fieldName = ObjectSpace.WrappingHandler.GetClassWrapper(objectToRemove.GetType())
                            .GetWrappedAssociation(RelationName).OriginalField.Name;
                    objectToRemove.SetPropertyValue(fieldName, null);

                    return true;
                }
                catch
                {
                    // TODO Auto-generated catch block
                    //throw;

                    // re-add in case of fail to ensure consistency
                    base.Add(objectToRemove);

                    return false;
                }
            }

            return false;
        }

        public new T RemoveAt(int index)
        {
            T removedObject = default;

            if (base.Count > index)
            {
                removedObject = base[index];
                base.RemoveAt(index);
            }

            if (removedObject != null)
            {
                try
                {
                    ObjectSpace.WrappingHandler.GetClassWrapper(removedObject.GetType())
                            .GetRelationWrapper(RelationName).OriginalField.SetValue(removedObject, null);
                    return removedObject;
                }
                catch
                {
                    // TODO Auto-generated catch block
                    // re-add in case of fail to ensure consistency
                    base.Add(removedObject);

                    return null;
                }
            }

            return null;
        }

        public bool RemoveAll(IEnumerable<T> enumerable)
        {
            bool allRemoved = true;

            foreach (T obj in enumerable)
            {
                allRemoved = Remove(obj) && allRemoved;
            }

            return allRemoved;
        }

        public void Load()
        {
            //var lm = listMember;
            //var fk = lm.GetForeignKey();
            //var rt = fk.getReferencingType();

            Type partnerClass = ListMember.GetForeignKey().AssociationPartnerClass.ClassToWrap;
            WhereClause clause = new WhereClause(ListMember.GetForeignKey().AssociationPartner.Name,
                    Owner.ID, ComparisonOperator.Equal);

            foreach (T obj in ObjectSpace.GetObjects(partnerClass, clause))
            {
                Add(obj);
            }
        }
    }
}
