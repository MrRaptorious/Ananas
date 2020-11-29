using System;
using System.Collections.Generic;
using System.Data;
using AnanasCore.Criteria;
using AnanasCore.Wrapping;

namespace AnanasCore.DBConnection
{
    public abstract class DatabaseConnection
    {
        protected string connectionString;
        protected StatementBuilder statementBuilder;

        public DatabaseConnection(StatementBuilder builder)
        {
            statementBuilder = builder;
        }

        /**
		 * Opens the connection to the database
		 * @param connectionString the configured connectionString for the database
		 */
        public virtual void Connect(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /**
		 * Initializes database (create then update schema)
		 */
        protected void InitDatabase()
        {

            List<string> createStatements = new List<string>();

            foreach (var clsWrapper in statementBuilder.getAllEntities())
            {
                string createStatement = statementBuilder.createEntity(clsWrapper);

                if (createStatement != null)
                    createStatements.Add(createStatement);
            }

            foreach (string statement in createStatements)
            {
                execute(statement);
            }

            updateSchema();
        }

        public abstract DataTable getTable(ClassWrapper t, WhereClause clause = null);

        public abstract DataRow getObject(ClassWrapper t, Guid id);

        public abstract void beginTransaction();

        public abstract void commitTransaction();

        public abstract void rollbackTransaction();

        public abstract void update(ChangedObject obj);

        public abstract void delete(PersistentObject obj);

        public abstract void create(PersistentObject obj);

        public abstract void execute(string statement);

        public abstract void createSchema();

        public abstract void updateSchema();
    }
}
