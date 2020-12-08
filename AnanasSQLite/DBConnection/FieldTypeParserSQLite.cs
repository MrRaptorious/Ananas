using AnanasCore;
using AnanasCore.DBConnection;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnanasSQLite.DBConnection
{
    public class FieldTypeParserSQLite : TypeParser
    {
        private string NormalizeValueForInsertStatement(Type type, object value)
        {
            if (type == typeof(string))
                return "'" + value + "'";

            if (typeof(PersistentObject).IsAssignableFrom(type))
                return "'" + ((PersistentObject)value).ID + "'";

            if (type == typeof(DateTime))
                return "" + (long)((DateTime)value).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            if (type == typeof(Guid))
                return "'" + value.ToString() + "'";

            if (type == typeof(bool))
                return (bool)value ? "1" : "0";

            return value.ToString();
        }

        public override string ParseFieldType(Type type, int Size)
        {
            if (type == typeof(string) || type == typeof(char) || type == typeof(Guid)
                || typeof(PersistentObject).IsAssignableFrom(type))
                return "TEXT";

            if (type == typeof(int) || type == typeof(DateTime) || type == typeof(bool))
                return "INTEGER";

            return "TEXT";
        }

        public override object CastValue(Type type, object value)
        {
            if (value == null)
                return null;

            if (type == typeof(string))
                return value.ToString();

            if (type == typeof(int))
                return Int32.Parse(value.ToString());

            if (type == typeof(DateTime))
                return DateTimeFromUnixTimeStamp(double.Parse(value.ToString()));

            if (type == typeof(bool))
                return !value.ToString().Equals("0");

            if (type == typeof(Guid))
                return new Guid(value.ToString());

            if (type == typeof(float))
            {
                if (value is null || value.Equals(""))
                    return 0;
                else
                    return float.Parse(value.ToString());
            }

            if (type == typeof(double))
            {
                if (value is null || value.Equals(""))
                    return 0;
                else
                    return double.Parse(value.ToString());
            }

            return null;
        }

        public override string NormalizeValueForInsertStatement(object value)
        {
            if (value == null)
                return "NULL";

            return NormalizeValueForInsertStatement(value.GetType(), value);
        }

        private DateTime DateTimeFromUnixTimeStamp(double unixTime)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTime).ToLocalTime();
            return dtDateTime;
        }

        public override object CastValue<T>(object value)
        {
            return CastValue(typeof(T), value);
        }
    }
}
