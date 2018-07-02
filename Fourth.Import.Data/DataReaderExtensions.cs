using System;
using System.Data;

namespace Fourth.Import.Data
{
    public static class DataReaderExtensions
    {
        public static T GetValue<T>(this IDataReader reader, string colName)
        {
            var colIndex = reader.GetOrdinal(colName);

            if (reader.IsDBNull(colIndex))
            {
                return default(T);
            }

            var item = reader[colIndex];
            return item is T ? (T)item : (T)Convert.ChangeType(item, typeof(T));
        }

        public static T GetValue<T>(this IDataReader reader, int colIndex)
        {
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