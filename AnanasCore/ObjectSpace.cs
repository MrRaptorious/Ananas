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
    /// <summary>
    /// Main object to handle data manipulation from user
    /// </summary>
    public class ObjectSpace
    {
        private Dictionary<Type, Dictionary<PersistentObject, ChangedObject>> ChangedObjects;
        private Dictionary<Type, List<PersistentObject>> CreatedObjects;
        private ObjectCache ObjectCache;
        public bool IsLoadingObjects { get; private set; }
        private readonly DatabaseConnection connection;
        public WrappingHandler WrappingHandler { get; private set; }
        public TypeParser FieldTypeParser { get; }

        public ObjectSpace(DatabaseConnection connection, WrappingHandler handler, TypeParser parser, bool refresh = false)
        {
            this.connection = connection;
            WrappingHandler = handler;
            FieldTypeParser = parser;
            InitObjectSpace();

            if (refresh)
                Refresh();
        }


        /// <summary>
        /// Initializes the object and loads it with data
        /// </summary>
        private void InitObjectSpace()
        {
            IsLoadingObjects = false;
            ChangedObjects = new Dictionary<Type, Dictionary<PersistentObject, ChangedObject>>();
            CreatedObjects = new Dictionary<Type, List<PersistentObject>>();
            ObjectCache = new ObjectCache(WrappingHandler.GetRegisteredTypes());
        }


        /// <summary>
        /// Marks that there is a <see cref="PersistentObject"/> to be created in the database
        /// </summary>
        /// <param name="obj">the newly created <see cref="PersistentObject"/> which needs to be saved in the database</param>
        public void AddCreatedObject(PersistentObject obj)
        {
            if (!CreatedObjects.ContainsKey(obj.GetType()))
                CreatedObjects.Put(obj.GetType(), new List<PersistentObject>());

            if (!CreatedObjects[obj.GetType()].Contains(obj))
                CreatedObjects[obj.GetType()].Add(obj);
        }

        /// <summary>
        /// Creates an object from the requested type
        /// </summary>
        /// <typeparam name="T">requested type</typeparam>
        /// <returns>instance of <typeparamref name="T"/></returns>
        public T CreateObject<T>() where T : PersistentObject
        {
            T newObject;

            try
            {
                newObject = (T)Activator.CreateInstance(typeof(T), this);
            }
            catch
            {
                throw;
            }

            return newObject;
        }

        /// <summary>
        /// Gets one object of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">type of object to get</typeparam>
        /// <param name="id">ID of object</param>
        /// <param name="loadFromDB">determines if object can be loaded from database</param>
        /// <returns>instance of found object of type <typeparamref name="T"/></returns>
        public T GetObject<T>(Guid id, bool loadFromDB = false) where T : PersistentObject
        {
            return (T)GetObject(typeof(T), id, loadFromDB);
        }

        /// <summary>
        /// Gets one object of type <paramref name="type"/>
        /// </summary>
        /// <param name="type">type of object to get</param>
        /// <param name="id">ID of object</param>
        /// <param name="loadFromDB">determines if object can be loaded from database</param>
        /// <returns>instance of found object of type <typeparamref name="T"/></returns>
        public PersistentObject GetObject(Type type, Guid id, bool loadFromDB = false)
        {
            object objToReturn = default;

            // check cache
            foreach (PersistentObject obj in ObjectCache.Get(type))
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

                objToReturn = LoadObject(clsWrapper, row);
            }

            return objToReturn as PersistentObject;
        }

        /// <summary>
        ///  Loads the objects from DB
        /// </summary>
        /// <param name="type">requested type</param>
        /// <param name="clause">where clause to restrict the search</param>
        /// <returns>a list of <see cref="PersistentObject"/>s</returns>
        public List<PersistentObject> GetObjects(Type type, WhereClause clause)
        {
            ClassWrapper classWrapper = WrappingHandler.GetClassWrapper(type);
            List<PersistentObject> objectsToReturn = new List<PersistentObject>();

            DataTable table = connection.GetTable(classWrapper, clause);

            foreach (DataRow row in table.Rows)
            {
                objectsToReturn.Add(LoadObject(classWrapper, row));
            }

            ObjectCache.ApplyLoadedObjectsToCache();

            return objectsToReturn;
        }

        /// <summary>
        /// Loads all objects of a type
        /// </summary>
        /// <typeparam name="T">Type to load</typeparam>
        /// <param name="loadFromDB">determines if object can be loaded from database</param>
        /// <returns>a list of <see cref="PersistentObject"/>s</returns>
        public List<T> GetObjects<T>(bool loadFromDB = false) where T : PersistentObject
        {

            if (loadFromDB)
            {
                RefreshType(WrappingHandler.GetClassWrapper(typeof(T)));
                ObjectCache.ApplyLoadedObjectsToCache();
            }

            List<T> castedList = new List<T>();

            foreach (PersistentObject obj in ObjectCache.Get(typeof(T)))
            {
                castedList.Add((T)obj);
            }

            return castedList.Count > 0 ? castedList : null;
        }

        /// <summary>
        /// Loads all objects of a type (always loads from DB)
        /// </summary>
        /// <typeparam name="T">Type to load</typeparam>
        /// <param name="clause">the clause to apply while loading from DB</param>
        /// <returns>a list of <see cref="PersistentObject"/>s</returns>
        public List<T> GetObjects<T>(WhereClause clause) where T : PersistentObject
        {
            ClassWrapper classWrapper = WrappingHandler.GetClassWrapper(typeof(T));
            List<T> objectsToReturn = new List<T>();

            DataTable table = connection.GetTable(classWrapper, clause);

            foreach (DataRow row in table.Rows)
            {
                objectsToReturn.Add((T)LoadObject(classWrapper, row));
            }

            ObjectCache.ApplyLoadedObjectsToCache();

            return objectsToReturn;
        }

        /// <summary>
        /// Reloads all types from the database and updates the cache
        /// </summary>
        public void Refresh()
        {
            List<ClassWrapper> typeList = WrappingHandler.GetWrapperList();

            foreach (ClassWrapper clsWr in typeList)
                RefreshType(clsWr);

            ObjectCache.ApplyLoadedObjectsToCache();
        }

        /// <summary>
        /// Marks a change to an object to update the database
        /// </summary>
        /// <param name="changedObject">the <see cref="PersistentObject"/> which has changed</param>
        /// <param name="fieldName">the name of the changed member</param>
        /// <param name="newValue">the new value from the changed member</param>
        public void AddChangedObject(PersistentObject changedObject, string fieldName, object newValue)
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

        /// <summary>
        /// Updates database and commits all changes to all objects
        /// </summary>
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
                connection.RollbackTransaction();
                throw;
            }

            CreatedObjects.Clear();
            ChangedObjects.Clear();
        }

        /// <summary>
        /// Rolls the current changes in the objectSpace back
        /// </summary>
        public void RollbackChanges()
        {
            foreach (var type in ChangedObjects)
            {
                foreach (var cObject in type.Value)
                {
                    cObject.Value.Rollback();
                }
            }

            ChangedObjects.Clear();
        }

        /// <summary>
        /// Updates database and updates all objects
        /// </summary>
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

        /// <summary>
        /// Updates database and creates all created objects
        /// </summary>
        private void CreateObjects()
        {
            foreach (var type in CreatedObjects)
            {
                foreach (PersistentObject createdObject in type.Value)
                {

                    // extract all relations in objects to create and add them to the changed
                    // objects

                    List<PropertyWrapper> relationFields = WrappingHandler.GetClassWrapper(createdObject.GetType()).GetRelationWrapper();

                    foreach (PropertyWrapper relation in relationFields)
                    {

                        string relationMemberName = relation.OriginalField.Name;

                        object o = createdObject.GetMemberValue(relationMemberName);

                        if (o != null && !(o is GenericList))
                        {
                            AddChangedObject(createdObject, relationMemberName, o);
                            // createdObject.SetMemberValue(relationMemberName, null);
                        }
                    }

                    connection.Create(createdObject);
                }
            }
        }

        private void RefreshType(ClassWrapper classWrapper)
        {
            DataTable table = connection.GetTable(classWrapper);

            foreach (DataRow row in table.Rows)
            {
                Guid elementUUID = new Guid((string)row[PersistentObject.KeyPropertyName]);
                // only load if not already loaded
                //if (objectCache.getTemp(classWrapper.getClassToWrap()).stream().noneMatch(x->x.getID().equals(elementUUID)))
                if (!ObjectCache.GetTemp(classWrapper.ClassToWrap).Any(x => x.ID.Equals(elementUUID)))
                {
                    LoadObject(classWrapper, row);
                }
            }
        }

        private PersistentObject LoadObject(ClassWrapper classWrapper, DataRow row)
        {
            IsLoadingObjects = true;

            PersistentObject pObject;
            pObject = CreateValueObject(classWrapper, row);

            ObjectCache.AddTemp(pObject);

            FillReferences(classWrapper, row, pObject);

            IsLoadingObjects = false;

            return pObject;
        }

        /// <summary>
        /// Fills the references of a <see cref="PersistentObject"/>
        /// </summary>
        /// <param name="pObject"><see cref="PersistentObject"/> to fill references</param>
        private void FillReferences(ClassWrapper classWrapper, DataRow row, PersistentObject pObject)
        {
            foreach (PropertyWrapper fw in classWrapper.GetRelationWrapper())
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
                    PersistentObject refObj = ObjectCache.GetTemp(cl).FirstOrDefault(x => x.ID.Equals(uuidToCompare));

                    if (refObj == null)
                        refObj = GetObject(cl, uuidToCompare, true);

                    pObject.SetRelation(fw.OriginalField.Name, refObj);
                }
            }
        }

        private PersistentObject CreateValueObject(ClassWrapper classWrapper, DataRow row)
        {
            PersistentObject pObject;

            pObject = (PersistentObject)Activator.CreateInstance(classWrapper.ClassToWrap, this);

            // set Object fields
            foreach (PropertyWrapper fw in classWrapper.GetWrappedValueMemberWrapper())
            {
                object value = row[fw.Name];
                pObject.SetMemberValue(fw.OriginalField.Name, FieldTypeParser.CastValue(fw.OriginalField.PropertyType, value));
            }

            return pObject;
        }
    }
}