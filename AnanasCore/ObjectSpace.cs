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
        private Dictionary<Type, Dictionary<PersistentObject, ChangedObject>> changedObjects;
        private Dictionary<Type, List<PersistentObject>> createdObjects;
        private ObjectCache objectCache;
        public bool isLoadingObjects { get; private set; }
        private readonly DatabaseConnection connection;
        public WrappingHandler wrappingHandler { get; private set; }
        FieldTypeParser fieldTypeParser;

        public ObjectSpace(DatabaseConnection connection, WrappingHandler handler, FieldTypeParser parser)
        {
            this.connection = connection;
            wrappingHandler = handler;
            fieldTypeParser = parser;
            initObjectSpace();
            Refresh();
        }

        public ObjectSpace(DatabaseConnection connection, WrappingHandler handler, FieldTypeParser parser, bool refresh)
        {
            this.connection = connection;
            wrappingHandler = handler;
            fieldTypeParser = parser;
            initObjectSpace();

            if (refresh)
                Refresh();
        }


        /**
         * Initializes the object and loads it with data
         */
        private void initObjectSpace()
        {
            isLoadingObjects = false;
            changedObjects = new Dictionary<Type, Dictionary<PersistentObject, ChangedObject>>();
            createdObjects = new Dictionary<Type, List<PersistentObject>>();
            objectCache = new ObjectCache(wrappingHandler.getRegisteredTypes());
        }


        /**
         * Marks that there is a object to be created in the database
         *
         * @param obj the newly created object which needs to be saved in the database
         */
        public void addCreatedObject(PersistentObject obj)
        {
            if (!createdObjects.ContainsKey(obj.GetType()))
                createdObjects.Put(obj.GetType(), new List<PersistentObject>());

            if (!createdObjects[obj.GetType()].Contains(obj))
                createdObjects[obj.GetType()].Add(obj);
        }

        /**
    * Creates an object from the requested type
    *
    * @param <T>  the requested type
    * @param type the class of the requested type
    * @return an object from type T
    */
        public T createObject<T>()
        {

            T newObject = default;

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
        public T getObject<T>(Guid id, bool loadFromDB = false) where T : PersistentObject
        {
            return (T)getObject(typeof(T), id, loadFromDB);
        }

        public PersistentObject getObject(Type type, Guid id, bool loadFromDB = false)
        {

            object objToReturn = default;

            // check cache
            foreach (PersistentObject obj in objectCache.get(type))
            {
                if (obj.ID.Equals(id))
                {
                    objToReturn = obj;
                    break;
                }
            }

            if (objToReturn == null && loadFromDB)
            {

                ClassWrapper clsWrapper = wrappingHandler.getClassWrapper(type);

                DataRow row = connection.getObject(clsWrapper, id);

                try
                {

                    objToReturn = loadObject(clsWrapper, row);
                }
                catch
                {
                    throw;
                }
            }

            return objToReturn as PersistentObject;
        }

        public List<PersistentObject> getObjects(Type type, WhereClause clause)
        {
            ClassWrapper classWrapper = wrappingHandler.getClassWrapper(type);
            List<PersistentObject> objectsToReturn = new List<PersistentObject>();

            DataTable table = connection.getTable(classWrapper, clause);

            try
            {
                foreach (DataRow row in table.Rows)
                {
                    objectsToReturn.Add(loadObject(classWrapper, row));
                }

                objectCache.applyLoadedObjectsToCache();
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
        public List<T> getObjects<T>(bool loadFromDB = false) where T : PersistentObject
        {

            if (loadFromDB)
            {
                refreshType(wrappingHandler.getClassWrapper(typeof(T)));
                objectCache.applyLoadedObjectsToCache();
            }

            List<T> castedList = new List<T>();

            foreach (PersistentObject obj in objectCache.get(typeof(T)))
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
        public List<T> getObjects<T>(WhereClause clause) where T : PersistentObject
        {
            ClassWrapper classWrapper = wrappingHandler.getClassWrapper(typeof(T));
            List<T> objectsToReturn = new List<T>();

            DataTable table = connection.getTable(classWrapper, clause);

            try
            {
                foreach (DataRow row in table.Rows)
                {
                    objectsToReturn.Add((T)loadObject(classWrapper, row));
                }

                objectCache.applyLoadedObjectsToCache();
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

            List<ClassWrapper> typeList = wrappingHandler.getWrapperList();

            foreach (ClassWrapper clsWr in typeList)
                refreshType(clsWr);

            objectCache.applyLoadedObjectsToCache();
        }

        /**
       * Marks a change to an object to update the database
       *
       * @param changedObject the PersistentObject which has changed
       * @param fieldName     the name of the changed member
       * @param newValue      the new value from the changed member
       */
        public void addChangedObject(PersistentObject changedObject, String fieldName, Object newValue)
        {
            if (!changedObjects.ContainsKey(changedObject.GetType()))
            {
                changedObjects.Put(changedObject.GetType(), new Dictionary<PersistentObject, ChangedObject>());
            }

            if (!changedObjects[changedObject.GetType()].ContainsKey(changedObject))
            {
                changedObjects[changedObject.GetType()].Put(changedObject, new ChangedObject(changedObject));
            }

            // perhaps put old value in parameter for performance
            changedObjects[changedObject.GetType()][changedObject].addChangedField(fieldName, newValue, changedObject.getMemberValue(fieldName));
        }

        /**
         * Updates database and commits all changes to all objects
         */
        public void commitChanges()
        {
            try
            {
                connection.beginTransaction();

                createObjects();
                updateObjects();

                connection.commitTransaction();

            }
            catch
            {
                // TODO Auto-generated catch block
                connection.rollbackTransaction();
                throw;
            }

            createdObjects.Clear();
            changedObjects.Clear();
        }

        /**
         * Rolls the current changes in the objectSpace back
         */
        public void rollbackChanges()
        {
            foreach (var type in changedObjects)
            {
                foreach (var cObject in type.Value)
                {
                    cObject.Value.rollback();
                }
            }

            changedObjects.Clear();
        }

        private T castToT<T>(PersistentObject po) where T : PersistentObject
        {
            return (T)po;
        }

        /**
         * Updates database and updates all objects
         */
        private void updateObjects()
        {
            foreach (var type in changedObjects)
            {
                foreach (var typeObject in type.Value)
                {
                    connection.update(typeObject.Value);
                }
            }
        }

        /**
         * Updates database and creates all created objects
         */
        private void createObjects()
        {
            foreach (var type in createdObjects)
            {
                foreach (PersistentObject createdObject in type.Value)
                {

                    // extract all relations in objects to create and add them to the changed
                    // objects

                    List<FieldWrapper> relationFields = wrappingHandler.getClassWrapper(createdObject.GetType()).getRelationWrapper();

                    foreach (FieldWrapper relation in relationFields)
                    {

                        string relationMemberName = relation.getOriginalField().Name;

                        object o = createdObject.getMemberValue(relationMemberName);

                        if (o != null)
                        {
                            addChangedObject(createdObject, relationMemberName, o);
                            createdObject.setMemberValue(relationMemberName, null);
                        }
                    }

                    connection.create(createdObject);
                }
            }
        }

        /**
       * Reloads a type from the database and updates the cache
       *
       * @param classWrapper the requested type
       */
        private void refreshType<T>(ClassWrapper classWrapper) where T : PersistentObject
        {
            DataTable table = connection.getTable(classWrapper);

            try
            {
                foreach (DataRow row in table.Rows)
                {
                    Guid elementUUID = new Guid((string)row[PersistentObject.KeyPropertyName]);
                    // only load if not already loaded
                    //if (objectCache.getTemp(classWrapper.getClassToWrap()).stream().noneMatch(x->x.getID().equals(elementUUID)))
                    if (objectCache.getTemp(typeof(T)).Any(x => x.ID.Equals(elementUUID)))
                    {
                        loadObject(classWrapper, row);
                    }
                }
            }
            catch
            {
                // TODO Auto-generated catch block
                throw;
            }
        }

        private void refreshType(ClassWrapper classWrapper)
        {
            DataTable table = connection.getTable(classWrapper);

            try
            {
                foreach (DataRow row in table.Rows)
                {
                    Guid elementUUID = new Guid((string)row[PersistentObject.KeyPropertyName]);
                    // only load if not already loaded
                    //if (objectCache.getTemp(classWrapper.getClassToWrap()).stream().noneMatch(x->x.getID().equals(elementUUID)))
                    if (!objectCache.getTemp(classWrapper.getClassToWrap()).Any(x => x.ID.Equals(elementUUID)))
                    {
                        loadObject(classWrapper, row);
                    }
                }
            }
            catch
            {
                // TODO Auto-generated catch block
                throw;
            }
        }

        private PersistentObject loadObject(ClassWrapper classWrapper, DataRow row)
        {
            isLoadingObjects = true;

            PersistentObject pObject;
            pObject = createValueObject(classWrapper, row);

            objectCache.addTemp(/*classWrapper.getClassToWrap(),*/ pObject);

            fillReferences(classWrapper, row, pObject);

            isLoadingObjects = false;

            return pObject;
        }

        /**
         * @param pObject object to fill references
         */
        private void fillReferences(ClassWrapper classWrapper, DataRow row, PersistentObject pObject)
        {
            foreach (FieldWrapper fw in classWrapper.getRelationWrapper())
            {
                if (fw.isList)
                    continue;

                string oid = (string)row[fw.name];

                if (oid != null && !oid.Equals(""))
                {
                    Guid uuidToCompare = new Guid(oid);

                    AssociationWrapper asW = fw.GetForeignKey();
                    ClassWrapper cw = asW.getReferencingType();
                    Type cl = cw.getClassToWrap();

                    // check if refObj is already loaded
                    PersistentObject refObj = objectCache.getTemp(cl).FirstOrDefault(x => x.ID.Equals(uuidToCompare));

                    if (refObj == null)
                        refObj = getObject(cl, uuidToCompare, true);

                    pObject.setRelation(fw.getOriginalField().Name, refObj);
                }
            }
        }

        private PersistentObject createValueObject(ClassWrapper classWrapper, DataRow row)
        {
            PersistentObject pObject = null;

            try
            {
                pObject = (PersistentObject)Activator.CreateInstance(classWrapper.getClassToWrap(), this);

                // set Object fields
                foreach (FieldWrapper fw in classWrapper.getWrappedValueMemberWrapper())
                {
                    object value = row[fw.name];
                    pObject.setMemberValue(fw.getOriginalField().Name, fieldTypeParser.CastValue(fw.getOriginalField().PropertyType, value));
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