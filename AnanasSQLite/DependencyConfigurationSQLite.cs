using AnanasCore;
using AnanasCore.Criteria;
using AnanasCore.DBConnection;
using AnanasCore.Wrapping;
using AnanasSQLite.DBConnection;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnanasSQLite
{
    public class DependencyConfigurationSQLite : DependencyConfiguration
    {
        protected override void ConfigureTypes()
        {
            AddMapping(typeof(DatabaseConnection), typeof(SQLiteConnection));
            AddMapping(typeof(TypeParser), typeof(FieldTypeParserSQLite));
            AddMapping(typeof(StatementBuilder), typeof(SQLiteStatementBuilder));
            AddMapping(typeof(WrappingHandler), typeof(WrappingHandler), true);
        }
    }
}
