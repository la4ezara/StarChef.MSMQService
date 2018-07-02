#region usings

using System;
using System.Data;

#endregion

namespace Fourth.Starchef.Packages.Data
{
    public static class DataReaderExtensions
    {
        public static T GetDrValue<T>(this IDataReader dr, int index)
        {
            if (dr.GetValue(index) == DBNull.Value)
                return default(T);
            return (T) dr.GetValue(index);
        }
    }
}