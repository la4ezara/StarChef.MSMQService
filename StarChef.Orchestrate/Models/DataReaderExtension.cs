using System;
using System.Data;

namespace StarChef.Orchestrate.Models
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
            var colIndex = reader.GetOrdinal(colName);
            if (reader.IsDBNull(colIndex)) return default(T);

            object item = reader[colIndex];

            return item is T ? (T)item : (T)Convert.ChangeType(item, typeof(T));
        }
    }
}