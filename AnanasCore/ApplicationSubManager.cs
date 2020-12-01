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

        private DatabaseConnection connection;
        private FieldTypeParser currentParser;
        private StatementBuilder statementBuilder;
        private WrappingHandler wrappingHandler;
        private readonly string connectionString;

        public ApplicationSubManager(DependencyConfiguration dependencyConfiguration, string connectionString)
        {
            this.connectionString = connectionString;

            try
            {
                statementBuilder = (StatementBuilder)dependencyConfiguration.resolve(typeof(StatementBuilder));
                connection = (DatabaseConnection)dependencyConfiguration.resolve(typeof(DatabaseConnection));
                currentParser = (FieldTypeParser)dependencyConfiguration.resolve(typeof(FieldTypeParser));
                wrappingHandler = (WrappingHandler)dependencyConfiguration.resolve(typeof(WrappingHandler));
            }
            catch
            {
                throw;
            }
        }

        /**
         * Initializes the database (create schema then update the schema)
         */
        private void initDatabase()
        {
            connection.createSchema();
            connection.updateSchema();
        }

        /**
         * Starts the application, all types have to be registered, the db schema will be
         * updated
         */
        public void start()
        {

            try
            {
                connection.Connect(connectionString);
            }
            catch
            {
                throw;
            }

            wrappingHandler.updateRelations();

            initDatabase();
        }

        /**
         * Creates a new ObjectSpace object for handling data and communicating with the Database
         *
         * @return a newly created ObjectSpace with the programs database connection
         */
        public ObjectSpace createObjectSpace()
        {
            return new ObjectSpace(connection, wrappingHandler, currentParser);
        }

        /**
         * Creates a new ObjectSpace object for handling data and communicating with the Database
         *
         * @param loadOnInit determines if the ObjectSpace should be loaded when created or not
         * @return a newly created ObjectSpace with the programs database connection
         */
        public ObjectSpace createObjectSpace(bool loadOnInit)
        {
            return new ObjectSpace(connection, wrappingHandler, currentParser, loadOnInit);
        }

        /**
         * Registers a subclass from PersistentObject to be able to handle it
         *
         * @param type a subclass from PersistentObject
         */
        public void registerType<T>()
        {
            registerType(typeof(T));
        }

         /**
         * Registers a subclass from PersistentObject to be able to handle it
         *
         * @param type a subclass from PersistentObject
         */
        public void registerType(Type t)
        {
            wrappingHandler.registerType(t);
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

        public WrappingHandler getWrappingHandler()
        {
            return wrappingHandler;
        }

        public StatementBuilder getStatementBuilder()
        {
            return statementBuilder;
        }

        public FieldTypeParser getCurrentFieldTypeParser()
        {
            return currentParser;
        }
    }
}
