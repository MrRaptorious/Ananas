using AnanasCore.Criteria;
using AnanasCore.DBConnection;
using AnanasCore.Wrapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnanasCore
{
    public class ApplicationSubManager
    {

        private DatabaseConnection Connection;
        private FieldTypeParser CurrentParser;
        private StatementBuilder StatementBuilder;
        private WrappingHandler WrappingHandler;
        private readonly string ConnectionString;

        public ApplicationSubManager(DependencyConfiguration dependencyConfiguration, string connectionString)
        {
            ConnectionString = connectionString;

            try
            {
                StatementBuilder = (StatementBuilder)dependencyConfiguration.resolve(typeof(StatementBuilder));
                Connection = (DatabaseConnection)dependencyConfiguration.resolve(typeof(DatabaseConnection));
                CurrentParser = (FieldTypeParser)dependencyConfiguration.resolve(typeof(FieldTypeParser));
                WrappingHandler = (WrappingHandler)dependencyConfiguration.resolve(typeof(WrappingHandler));
            }
            catch
            {
                throw;
            }
        }

        /**
         * Initializes the database (create schema then update the schema)
         */
        private void InitDatabase()
        {
            Connection.CreateSchema();
            Connection.UpdateSchema();
        }

        /**
         * Starts the application, all types have to be registered, the db schema will be
         * updated
         */
        public void Start()
        {

            try
            {
                Connection.Connect(ConnectionString);
            }
            catch
            {
                throw;
            }

            WrappingHandler.UpdateRelations();

            InitDatabase();
        }

        /**
         * Creates a new ObjectSpace object for handling data and communicating with the Database
         *
         * @return a newly created ObjectSpace with the programs database connection
         */
        public ObjectSpace CreateObjectSpace()
        {
            return new ObjectSpace(Connection, WrappingHandler, CurrentParser);
        }

        /**
         * Creates a new ObjectSpace object for handling data and communicating with the Database
         *
         * @param loadOnInit determines if the ObjectSpace should be loaded when created or not
         * @return a newly created ObjectSpace with the programs database connection
         */
        public ObjectSpace CreateObjectSpace(bool loadOnInit)
        {
            return new ObjectSpace(Connection, WrappingHandler, CurrentParser, loadOnInit);
        }

        /**
         * Registers a subclass from PersistentObject to be able to handle it
         *
         * @param type a subclass from PersistentObject
         */
        public void RegisterType<T>()
        {
            RegisterType(typeof(T));
        }

         /**
         * Registers a subclass from PersistentObject to be able to handle it
         *
         * @param type a subclass from PersistentObject
         */
        public void RegisterType(Type t)
        {
            WrappingHandler.RegisterType(t);
        }

        /**
         * Registers a list of subclasses from PersistentObject to be able to handle
         * them
         *
         * @param types a list of subclasses from PersistentObject
         */
        //public void registerTypes(List<Class<? extends PersistentObject>> types)
        //{
        //    if (types != null)
        //    {
        //        for (Class <? extends PersistentObject > type : types)
        //        {
        //            registerType(type);
        //        }
        //    }
        //}

        public WrappingHandler GetWrappingHandler()
        {
            return WrappingHandler;
        }

        public StatementBuilder GetStatementBuilder()
        {
            return StatementBuilder;
        }

        public FieldTypeParser GetCurrentFieldTypeParser()
        {
            return CurrentParser;
        }
    }
}
