using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using AnanasCore;
using AnanasCore.Criteria;
using AnanasCore.DBConnection;
using AnanasCore.Wrapping;
using Microsoft.Data.Sqlite;

namespace AnanasSQLite.DBConnection
{
    public class SQLiteConnection : DatabaseConnection
    {

        private SqliteConnection _connection;

        public SQLiteConnection(StatementBuilder builder) : base(builder)
        {
        }

        public override void Connect(string connectionSting)
        {
            base.Connect(connectionSting);
            _connection = new SqliteConnection(connectionSting);
            _connection.Open();
        }

        public override DataRow getObject(ClassWrapper type, Guid id)
        {
            string statement = statementBuilder.createSelect(type, new WhereClause(type.GetPrimaryKeyMember().name, id, ComparisonOperator.Equal));

            DataTable table = new DataTable();

            try
            {
                var command = _connection.CreateCommand();
                command.CommandText = statement;
                table.Load(command.ExecuteReader());
            }
            catch
            {
                throw;
            }

            if (table.Rows.Count > 0)
                return table.Rows[0];
            else
                return null;
        }

        public override DataTable getTable(ClassWrapper type, WhereClause clause = null)
        {
            string result = statementBuilder.createSelect(type, clause);

            DataTable table = new DataTable();

            try
            {
                var command = _connection.CreateCommand();
                command.CommandText = result;
                table.Load(command.ExecuteReader());
            }
            catch
            {
                throw;
            }

            return table;
        }

        public override void update(ChangedObject obj)
        {
            var command = _connection.CreateCommand();
            command.CommandText = statementBuilder.createUpdate(obj);
            command.ExecuteNonQuery();
        }


        public override void delete(PersistentObject obj)
        {
            // TODO Auto-generated method stub
        }

        public override void create(PersistentObject obj)
        {
            string result = statementBuilder.createInsert(obj);
            execute(result);
        }


        public override void execute(string statement)
        {
            var command = _connection.CreateCommand();
            command.CommandText = statement;
            command.ExecuteNonQuery();
        }

        public override void createSchema()
        {
            try
            {
                execute("PRAGMA foreign_keys=off");

                List<string> allStatements = statementBuilder.createAllEntity();

                var command = _connection.CreateCommand();
                command.CommandText = string.Empty;

                foreach (string statementString in allStatements)
                {
                    command.CommandText += statementString + ";" + Environment.NewLine;
                }

                command.ExecuteNonQuery();

                execute("PRAGMA foreign_keys=on");

            }
            catch
            {
                throw;
            }
        }

        public override void updateSchema()
        {
            List<string> updateStatements = new List<string>();

            foreach (ClassWrapper cl in statementBuilder.getAllEntities())
            {

                string getTypeSchemaStatement = "PRAGMA table_info(" + cl.getName() + ")";
                List<string> persistentColumns = new List<string>();

                // collect persistentColumns
                try
                {
                    DataTable table = new DataTable();

                    var command = _connection.CreateCommand();
                    command.CommandText = getTypeSchemaStatement;
                    table.Load(command.ExecuteReader());

                    foreach (DataRow row in table.Rows)
                    {
                        persistentColumns.Add((string)row["name"]);
                    }

                }
                catch
                {
                    // TODO Auto-generated catch block
                    throw;
                }

                foreach (FieldWrapper fieldWrapper in cl.getWrappedFields())
                {
                    if (!persistentColumns.Contains(fieldWrapper.name))
                        updateStatements.Add(statementBuilder.createAddPropertyToEntity(fieldWrapper));
                }
            }

            foreach (string statement in updateStatements)
            {
                try
                {
                    execute(statement);
                }
                catch
                {
                    // TODO Auto-generated catch block
                    throw;
                }
            }
        }


        public override void beginTransaction()
        {
            try
            {
                execute("BEGIN TRANSACTION;");
            }
            catch
            {
                // TODO Auto-generated catch block
                throw;
            }
        }

        public override void commitTransaction()
        {
            try
            {
                execute("COMMIT;");
            }
            catch
            {
                // TODO Auto-generated catch block
                throw;
            }
        }

        public override void rollbackTransaction()
        {
            try
            {
                execute("ROLLBACK;");
            }
            catch
            {
                // TODO Auto-generated catch block
                throw;
            }
        }
    }
}
