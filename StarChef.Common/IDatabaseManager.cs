using System.Data;
using System.Data.SqlClient;

namespace StarChef.Common
{
    public interface IDatabaseManager
    {
        int Execute(
                string connectionString,
                string spName,
                params SqlParameter[] parameterValues);

        IDataReader ExecuteReader(
           string connectionString,
           string spName,
           params SqlParameter[] parameterValues
           );

        DataSet ExecuteMultiResultset(
           string connectionString,
           string spName,
           params SqlParameter[] parameterValues
           );
    }
}