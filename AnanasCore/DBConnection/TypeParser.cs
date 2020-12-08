using System;
using System.Collections.Generic;
using System.Text;

namespace AnanasCore.DBConnection
{
    /// <summary>
    /// Parser to parse values from and for database
    /// </summary>
    public abstract class TypeParser
    {
        public abstract string ParseFieldType(Type type, int size = -1);
        public abstract object CastValue(Type t, object value);
        public abstract object CastValue<T>(object value);
        public abstract string NormalizeValueForInsertStatement(object value);
    }
}
