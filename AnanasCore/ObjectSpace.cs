using AnanasCore;
using AnanasCore.Criteria;
using AnanasCore.DBConnection;
using AnanasCore.Extensions;
using AnanasCore.Wrapping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Linq;

namespace AnanasCore
{
    public class ObjectSpace
    {
        private Dictionary<Type, Dictionary<PersistentObject, ChangedObject>> ChangedObjects;
        private Dictionary<Type, List<PersistentObject>> CreatedObjects;
        private ObjectCache ObjectCache;
        public bool IsLoadingObjects { get; private set; }
        private readonly DatabaseConnection connection;
        public WrappingHandler WrappingHandler { get; private set; }
        public FieldTypeParser FieldTypeParser { get; }

        public ObjectSpace(DatabaseConnection connection, WrappingHandler handler, FieldTypeParser parser)
        {
            this.connection = connection;
            WrappingHandler = handler;
            FieldTypeParser = parser;
            InitObjectSpace();
            Refresh();
        }

        public ObjectSpace(DatabaseConnection connection, WrappingHandler handler, FieldTypeParser parser, bool refresh)
        {
            this.connection = connection;
            WrappingHandler = handler;
            FieldTypeParser = parser;
            InitObjectSpace();

            if (refresh)
                Refresh();
        }


        /**
         * Initializes the object and loads it with data
         */
        private void InitObjectSpace()
        {
            IsLoadingObjects = false;
            ChangedObjects = new Dictionary<Type, Dictionary<PersistentObject, ChangedObject>>();
            CreatedObjects = new Dictionary<Type, List<PersistentObject>>();
            ObjectCache = new ObjectCache(WrappingHandler.GetRegisteredTypes());
        }


        /**
         * Marks that there is a object to be created in the database
         *
         * @param obj the newly created object which needs to be saved in the database
         */
        public void AddCreatedObject(PersistentObject obj)
        {
            if (!CreatedObjects.ContainsKey(obj.GetType()))
                CreatedObjects.Put(obj.GetType(), new List<PersistentObject>());

            if (!CreatedObjects[obj.GetType()].Contains(obj))
                CreatedObjects[obj.GetType()].Add(obj);
        }

        /**
    * Creates an object from the requested type
    *
    * @param <T>  the requested type
    * @param type the class of the requested type
    * @return an object from type T
    */
        public T CreateObject<T>()
        {

            T newObject;

            try
            {
                //ConstructorInfo constructor = typeof(T).GetConstructor(new Type[] { typeof(ObjectSpace) });
                newObject = (T)Activator.CreateInstance(typeof(T), this);
            }
            catch
            {
                // TODO
                throw;
            }

            return newObject;
        }

        /**
        * gets one cached object
        *
        * @param cls        the class of the object to load
        * @param id         the uuid of the object to load
        * @param loadFromDB indicates whether  the object should be loaded from the db or not
        * @param <T>        the type of the object to load
        * @return a single cached object of type T with the given uuid
        */
        public T GetObject<T>(Guid id, bool loadFromDB = false) where T : PersistentObject
        {
            return (T)GetObject(typeof(T), id, loadFromDB);
        }

        public PersistentObject GetObject(Type type, Guid id, bool loadFromDB = false)
        {

            object objToReturn = default;

            // check cache
            foreach (PersistentObject obj in ObjectCache.get(type))
            {
                if (obj.ID.Equals(id))
                {
                    objToReturn = obj;
                    break;
                }
            }

            if (objToReturn == null && loadFromDB)
            {

                ClassWrapper clsWrapper = WrappingHandler.GetClassWrapper(type);

                DataRow row = connection.GetObject(clsWrapper, id);

                try
                {

                    objToReturn = LoadObject(clsWrapper, row);
                }
                catch
                {
                    throw;
                }
            }

            return objToReturn as PersistentObject;
        }

        public List<PersistentObject> GetObjects(Type type, WhereClause clause)
        {
            ClassWrapper classWrapper = WrappingHandler.GetClassWrapper(type);
            List<PersistentObject> objectsToReturn = new List<PersistentObject>();

            DataTable table = connection.GetTable(classWrapper, clause);

            try
            {
                foreach (DataRow row in table.Rows)
                {
                    objectsToReturn.Add(LoadObject(classWrapper, row));
                }

                ObjectCache.applyLoadedObjectsToCache();
            }
            catch
            {
                // TODO Auto-generated catch block
                throw;
            }

            return objectsToReturn;
        }

        /**
        * Returns a list of objects from the requested type(LOADS POSSIBLY FROM DB)
        *
        * @param<T> the requested type
        * @param cls        the class of the requested type
        * @param loadFromDB determines if only cached or also non cached objects will
        *                   be returned
        * @return a list of objects from the requested type
        */
        public List<T> GetObjects<T>(bool loadFromDB = false) where T : PersistentObject
        {

            if (loadFromDB)
            {
                RefreshType(WrappingHandler.GetClassWrapper(typeof(T)));
                ObjectCache.applyLoadedObjectsToCache();
            }

            List<T> castedList = new List<T>();

            foreach (PersistentObject obj in ObjectCache.get(typeof(T)))
            {
                castedList.Add((T)obj);
            }

            return castedList.Count > 0 ? castedList : null;
        }

        /**
        * Loads always from db TODO create where for memory search
        *
        * @param <T>    type for cls argument
        * @param cls    class to load from db
        * @param clause where clause to restrict the search
        * @return list of t
        */
        public List<T> GetObjects<T>(WhereClause clause) where T : PersistentObject
        {
            ClassWrapper classWrapper = WrappingHandler.GetClassWrapper(typeof(T));
            List<T> objectsToReturn = new List<T>();

            DataTable table = connection.GetTable(classWrapper, clause);

            try
            {
                foreach (DataRow row in table.Rows)
                {
                    objectsToReturn.Add((T)LoadObject(classWrapper, row));
                }

                ObjectCache.applyLoadedObjectsToCache();
            }
            catch
            {
                // TODO Auto-generated catch block
                throw;
            }

            return objectsToReturn;
        }

        /**
         * Reloads all types from the database and updates the cache
         */
        public void Refresh()
        {

            List<ClassWrapper> typeList = WrappingHandler.GetWrapperList();

            foreach (ClassWrapper clsWr in typeList)
                RefreshType(clsWr);

            ObjectCache.applyLoadedObjectsToCache();
        }

        /**
       * Marks a change to an object to update the database
       *
       * @param changedObject the PersistentObject which has changed
       * @param fieldName     the name of the changed member
       * @param newValue      the new value from the changed member
       */
        public void AddChangedObject(PersistentObject changedObject, String fieldName, Object newValue)
        {
            if (IsLoadingObjects)
                return;

            if (!ChangedObjects.ContainsKey(changedObject.GetType()))
            {
                ChangedObjects.Put(changedObject.GetType(), new Dictionary<PersistentObject, ChangedObject>());
            }

            if (!ChangedObjects[changedObject.GetType()].ContainsKey(changedObject))
            {
                ChangedObjects[changedObject.GetType()].Put(changedObject, new ChangedObject(changedObject));
            }

            // perhaps put old value in parameter for performance
            ChangedObjects[changedObject.GetType()][changedObject].addChangedField(fieldName, newValue, changedObject.GetMemberValue(fieldName));
        }

        /**
         * Updates database and commits all changes to all objects
         */
        public void CommitChanges()
        {
            try
            {
                connection.BeginTransaction();

                CreateObjects();
                UpdateObjects();

                connection.CommitTransaction();

            }
            catch
            {
                // TODO Auto-generated catch block
                connection.RollbackTransaction();
                throw;
            }

            CreatedObjects.Clear();
            ChangedObjects.Clear();
        }

        /**
         * Rolls the current changes in the objectSpace back
         */
        public void RollbackChanges()
        {
            foreach (var type in ChangedObjects)
            {
                foreach (var cObject in type.Value)
                {
                    cObject.Value.rollback();
                }
            }

            ChangedObjects.Clear();
        }

        /**
         * Updates database and updates all objects
         */
        private void UpdateObjects()
        {
            foreach (var type in ChangedObjects)
            {
                foreach (var typeObject in type.Value)
                {
                    connection.Update(typeObject.Value);
                }
            }
        }

        /**
         * Updates database and creates all created objects
         */
        private void CreateObjects()
        {
            foreach (var type in CreatedObjects)
            {
                foreach (PersistentObject createdObject in type.Value)
                {

                    // extract all relations in objects to create and add them to the changed
                    // objects

                    List<FieldWrapper> relationFields = WrappingHandler.GetClassWrapper(createdObject.GetType()).GetRelationWrapper();

                    foreach (FieldWrapper relation in relationFields)
                    {

                        string relationMemberName = relation.OriginalField.Name;

                        object o = createdObject.GetMemberValue(relationMemberName);

                        if (o != null)
                        {
                            AddChangedObject(createdObject, relationMemberName, o);
                           // createdObject.SetMemberValue(relationMemberName, null);
                        }
                    }

                    connection.Create(createdObject);
                }
            }
        }

       // /**
       //* Reloads a type from the database and updates the cache
       //*
       //* @param classWrapper the requested type
       //*/
       // private void RefreshType<T>(ClassWrapper classWrapper) where T : PersistentObject
       // {
       //     DataTable table = connection.GetTable(classWrapper);

       //     try
       //     {
       //         foreach (DataRow row in table.Rows)
       //         {
       //             Guid elementUUID = new Guid((string)row[PersistentObject.KeyPropertyName]);
       //             // only load if not already loaded
       //             //if (objectCache.getTemp(classWrapper.getClassToWrap()).stream().noneMatch(x->x.getID().equals(elementUUID)))
       //             if (ObjectCache.getTemp(typeof(T)).Any(x => x.ID.Equals(elementUUID)))
       //             {
       //                 LoadObject(classWrapper, row);
       //             }
       //         }
       //     }
       //     catch
       //     {
       //         // TODO Auto-generated catch block
       //         throw;
       //     }
       // }

        private void RefreshType(ClassWrapper classWrapper)
        {
            DataTable table = connection.GetTable(classWrapper);

            try
            {
                foreach (DataRow row in table.Rows)
                {
                    Guid elementUUID = new Guid((string)row[PersistentObject.KeyPropertyName]);
                    // only load if not already loaded
                    //if (objectCache.getTemp(classWrapper.getClassToWrap()).stream().noneMatch(x->x.getID().equals(elementUUID)))
                    if (!ObjectCache.getTemp(classWrapper.ClassToWrap).Any(x => x.ID.Equals(elementUUID)))
                    {
                        LoadObject(classWrapper, row);
                    }
                }
            }
            catch
            {
                // TODO Auto-generated catch block
                throw;
            }
        }

        private PersistentObject LoadObject(ClassWrapper classWrapper, DataRow row)
        {
            IsLoadingObjects = true;

            PersistentObject pObject;
            pObject = CreateValueObject(classWrapper, row);

            ObjectCache.addTemp(/*classWrapper.getClassToWrap(),*/ pObject);

            FillReferences(classWrapper, row, pObject);

            IsLoadingObjects = false;

            return pObject;
        }

        /**
         * @param pObject object to fill references
         */
        private void FillReferences(ClassWrapper classWrapper, DataRow row, PersistentObject pObject)
        {
            foreach (FieldWrapper fw in classWrapper.GetRelationWrapper())
            {
                if (fw.IsList)
                    continue;

                string oid = row[fw.Name] as string;

                if (oid != null && !oid.Equals(""))
                {
                    Guid uuidToCompare = new Guid(oid);

                    AssociationWrapper asW = fw.GetForeignKey();
                    ClassWrapper cw = asW.AssociationPartnerClass;
                    Type cl = cw.ClassToWrap;

                    // check if refObj is already loaded
                    PersistentObject refObj = ObjectCache.getTemp(cl).FirstOrDefault(x => x.ID.Equals(uuidToCompare));

                    if (refObj == null)
                        refObj = GetObject(cl, uuidToCompare, true);

                    pObject.SetRelation(fw.OriginalField.Name, refObj);
                }
            }
        }

        private PersistentObject CreateValueObject(ClassWrapper classWrapper, DataRow row)
        {
            PersistentObject pObject;
            try
            {
                pObject = (PersistentObject)Activator.CreateInstance(classWrapper.ClassToWrap, this);

                // set Object fields
                foreach (FieldWrapper fw in classWrapper.GetWrappedValueMemberWrapper())
                {
                    object value = row[fw.Name];
                    pObject.SetMemberValue(fw.OriginalField.Name, FieldTypeParser.CastValue(fw.OriginalField.PropertyType, value));
                }
            }
            catch
            {
                throw;
            }

            return pObject;
        }
    }
}