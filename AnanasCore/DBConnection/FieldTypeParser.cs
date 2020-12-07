using System;
using System.Collections.Generic;
using System.Text;

namespace AnanasCore.DBConnection
{
    public abstract class FieldTypeParser
    {
        public abstract string ParseFieldType(Type type, int size = -1);
        public abstract object CastValue(Type t, object value);
        public abstract object CastValue<T>(object value);
        public abstract string NormalizeValueForInsertStatement(object value);
    }
}
