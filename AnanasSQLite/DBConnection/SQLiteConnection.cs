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

        public override DataRow GetObject(ClassWrapper type, Guid id)
        {
            string statement = statementBuilder.CreateSelect(type, new WhereClause(type.GetPrimaryKeyMember().Name, id, ComparisonOperator.Equal));

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

        public override DataTable GetTable(ClassWrapper type, WhereClause clause = null)
        {
            string result = statementBuilder.CreateSelect(type, clause);

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

        public override void Update(ChangedObject obj)
        {
            var command = _connection.CreateCommand();
            command.CommandText = statementBuilder.CreateUpdate(obj);
            command.ExecuteNonQuery();
        }


        public override void Delete(PersistentObject obj)
        {
            // TODO Auto-generated method stub
        }

        public override void Create(PersistentObject obj)
        {
            string result = statementBuilder.CreateInsert(obj);
            Execute(result);
        }


        public override void Execute(string statement)
        {
            var command = _connection.CreateCommand();
            command.CommandText = statement;
            command.ExecuteNonQuery();
        }

        public override void CreateSchema()
        {
            try
            {
                Execute("PRAGMA foreign_keys=off");

                List<string> allStatements = statementBuilder.CreateAllEntity();

                var command = _connection.CreateCommand();
                command.CommandText = string.Empty;

                foreach (string statementString in allStatements)
                {
                    command.CommandText += statementString + ";" + Environment.NewLine;
                }

                command.ExecuteNonQuery();

                Execute("PRAGMA foreign_keys=on");

            }
            catch
            {
                throw;
            }
        }

        public override void UpdateSchema()
        {
            List<string> updateStatements = new List<string>();

            foreach (ClassWrapper cl in statementBuilder.GetAllEntities())
            {

                string getTypeSchemaStatement = "PRAGMA table_info(" + cl.Name + ")";
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

                foreach (FieldWrapper fieldWrapper in cl.GetWrappedFields())
                {
                    if (!persistentColumns.Contains(fieldWrapper.Name))
                        updateStatements.Add(statementBuilder.CreateAddPropertyToEntity(fieldWrapper));
                }
            }

            foreach (string statement in updateStatements)
            {
                try
                {
                    Execute(statement);
                }
                catch
                {
                    // TODO Auto-generated catch block
                    throw;
                }
            }
        }


        public override void BeginTransaction()
        {
            try
            {
                Execute("BEGIN TRANSACTION;");
            }
            catch
            {
                // TODO Auto-generated catch block
                throw;
            }
        }

        public override void CommitTransaction()
        {
            try
            {
                Execute("COMMIT;");
            }
            catch
            {
                // TODO Auto-generated catch block
                throw;
            }
        }

        public override void RollbackTransaction()
        {
            try
            {
                Execute("ROLLBACK;");
            }
            catch
            {
                // TODO Auto-generated catch block
                throw;
            }
        }
    }
}
