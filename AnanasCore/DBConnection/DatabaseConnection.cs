using System;
using System.Collections.Generic;
using System.Data;
using AnanasCore.Criteria;
using AnanasCore.Wrapping;

namespace AnanasCore.DBConnection
{
    /// <summary>
    /// Abstract base class to represent a generic relational database connection
    /// </summary>
    public abstract class DatabaseConnection
    {
        protected string connectionString;
        protected StatementBuilder statementBuilder;

        public DatabaseConnection(StatementBuilder builder)
        {
            statementBuilder = builder;
        }

        /// <summary>
        /// Opens the connection to the database
        /// </summary>
        /// <param name="connectionString">the configured connectionString for the database</param>
        public virtual void Connect(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Initializes database (create then update schema)
        /// </summary>
        protected void InitDatabase()
        {

            List<string> createStatements = new List<string>();

            foreach (var clsWrapper in statementBuilder.GetAllEntities())
            {
                string createStatement = statementBuilder.CreateEntity(clsWrapper);

                if (createStatement != null)
                    createStatements.Add(createStatement);
            }

            foreach (string statement in createStatements)
            {
                Execute(statement);
            }

            UpdateSchema();
        }

        public abstract DataTable GetTable(ClassWrapper t, WhereClause clause = null);

        public abstract DataRow GetObject(ClassWrapper t, Guid id);

        public abstract void BeginTransaction();

        public abstract void CommitTransaction();

        public abstract void RollbackTransaction();

        public abstract void Update(ChangedObject obj);

        public abstract void Delete(PersistentObject obj);

        public abstract void Create(PersistentObject obj);

        public abstract void Execute(string statement);

        public abstract void CreateSchema();

        public abstract void UpdateSchema();
    }
}
