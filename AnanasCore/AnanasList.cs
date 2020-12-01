using AnanasCore.Criteria;
using AnanasCore.Wrapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnanasCore
{
    public class AnanasList<T> : List<T>, GenericList where T : PersistentObject
    {
        ObjectSpace objectSpace;
        PersistentObject owner;
        string relationName;
        FieldWrapper listMember;

        public AnanasList(ObjectSpace os, PersistentObject owner, string relationName)
        {
            this.objectSpace = os;
            this.owner = owner;
            this.relationName = relationName;
            listMember = objectSpace.wrappingHandler.getClassWrapper(owner.GetType())
                    .getWrappedAssociation(relationName);

            load();
        }

        /**
		 * Adds the PersistentObject o to the list
		 * @param o object of type T to add
		 * @return if object is added
		 */
        public new void Add(T objectToAdd)
        {
            string fieldName = objectSpace.wrappingHandler.getClassWrapper(objectToAdd.GetType())
                    .getWrappedAssociation(relationName).getOriginalField().Name;
            objectToAdd.setPropertyValue(fieldName, owner);
            base.Add(objectToAdd);
        }

        public bool remove(T objectToRemove)
        {

            bool removed = base.Remove(objectToRemove);

            if (removed)
            {
                try
                {
                    // set reference to null
                    string fieldName = objectSpace.wrappingHandler.getClassWrapper(objectToRemove.GetType())
                            .getWrappedAssociation(relationName).getOriginalField().Name;
                    objectToRemove.setPropertyValue(fieldName, null);

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

        public T removeAt(int index)
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
                    objectSpace.wrappingHandler.getClassWrapper(removedObject.GetType())
                            .getRelationWrapper(relationName).getOriginalField().SetValue(removedObject, null);
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

        public bool removeAll(IEnumerable<T> enumerable)
        {
            bool allRemoved = true;

            foreach (T obj in enumerable)
            {
                allRemoved = remove(obj) && allRemoved;
            }

            return allRemoved;
        }

        //public int removeAll(Predicate<T> filter)
        //{
        //    return base.RemoveAll(filter);
        //}

        public void load()
        {

            var lm = listMember;
            var fk = lm.GetForeignKey();
            //var rt = fk.getReferencingType();

            Type partnerClass = listMember.GetForeignKey().getReferencingType().getClassToWrap();
            WhereClause clause = new WhereClause(listMember.GetForeignKey().getAssociationPartner().name,
                    owner.ID, ComparisonOperator.Equal);

            foreach (T obj in objectSpace.getObjects(partnerClass, clause))
            {
                Add(obj);
            }
        }
    }
}
