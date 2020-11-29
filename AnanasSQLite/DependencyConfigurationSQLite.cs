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
        protected override void configureTypes()
        {
            addMapping(typeof(DatabaseConnection), typeof(SQLiteConnection));
            addMapping(typeof(FieldTypeParser), typeof(FieldTypeParserSQLite));
            addMapping(typeof(StatementBuilder), typeof(SQLiteStatementBuilder));
            addMapping(typeof(WrappingHandler), typeof(WrappingHandler), true);
        }
    }
}
