using System;
using System.Data;

namespace StarChef.Common
{
    public static class DataReaderExtension
    {
        public static T GetValueOrDefault<T>(this IDataReader reader, int colIndex)
        {
            if (reader.IsDBNull(colIndex)) return default(T);

            object item = reader[colIndex];

            return item is T ? (T)item : (T)Convert.ChangeType(item, typeof(T));
        }

        public static T GetValueOrDefault<T>(this IDataReader reader, string colName)
        {
            if (reader.IsDBNull(colName)) return default(T);

            object item = reader[colName];

            return item is T ? (T)item : (T)Convert.ChangeType(item, typeof(T));
        }

        public static T GetValue<T>(this IDataReader reader, string colName)
        {
            var colIndex = reader.GetOrdinal(colName);
            var item = reader[colIndex];
            return item is T ? (T)item : (T)Convert.ChangeType(item, typeof(T));
        }
                
        public static bool IsDBNull(this IDataReader reader, string colName)
        {
            var colIndex = reader.GetOrdinal(colName);
            return reader.IsDBNull(colIndex);
        }
    }
}