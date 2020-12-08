using AnanasCore.Criteria;
using AnanasCore.DBConnection;
using AnanasCore.Wrapping;
using System;

namespace AnanasCore
{
    /// <summary>
    /// A class to manage registered types and the syncing with one database
    /// </summary>
    public class ApplicationSubManager
    {
        public TypeParser CurrentParser { get; private set; }
        public StatementBuilder StatementBuilder { get; private set; }
        public WrappingHandler WrappingHandler { get; private set; }

        private DatabaseConnection Connection;
        private readonly string ConnectionString;

        public ApplicationSubManager(DependencyConfiguration dependencyConfiguration, string connectionString)
        {
            ConnectionString = connectionString;

            try
            {
                StatementBuilder = (StatementBuilder)dependencyConfiguration.Resolve(typeof(StatementBuilder));
                Connection = (DatabaseConnection)dependencyConfiguration.Resolve(typeof(DatabaseConnection));
                CurrentParser = (TypeParser)dependencyConfiguration.Resolve(typeof(TypeParser));
                WrappingHandler = (WrappingHandler)dependencyConfiguration.Resolve(typeof(WrappingHandler));
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Initializes the database (create schema then update the schema)
        /// </summary>
        private void InitDatabase()
        {
            Connection.CreateSchema();
            Connection.UpdateSchema();
        }

        /// <summary>
        /// Starts the application, all types have to be registered, the db schema will be updated
        /// </summary>
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

        /// <summary>
        /// Creates a new ObjectSpace object for handling data and communicating with the Database
        /// </summary>
        /// <returns>a newly created ObjectSpace with the programs database connection</returns>
        public ObjectSpace CreateObjectSpace()
        {
            return new ObjectSpace(Connection, WrappingHandler, CurrentParser);
        }

        /// <summary>
        /// Creates a new ObjectSpace object for handling data and communicating with the Database
        /// </summary>
        /// <param name="loadOnInit">determines if the ObjectSpace should be loaded when created or not</param>
        /// <returns>a newly created ObjectSpace with the programs database connection</returns>
        public ObjectSpace CreateObjectSpace(bool loadOnInit)
        {
            return new ObjectSpace(Connection, WrappingHandler, CurrentParser, loadOnInit);
        }

        /// <summary>
        /// Registers a subclass from <see cref="PersistentObject"/> to be able to handle it
        /// </summary>
        /// <typeparam name="T">a subtype from <see cref="PersistentObject"/></typeparam>
        public void RegisterType<T>()
        {
            RegisterType(typeof(T));
        }

        /// <summary>
        /// Registers a subclass from <see cref="PersistentObject"/> to be able to handle it
        /// </summary>
        /// <param name="t">type a subclass from <see cref="PersistentObject"/></param>
        public void RegisterType(Type t)
        {
            WrappingHandler.RegisterType(t);
        }
    }
}
