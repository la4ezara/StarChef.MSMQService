using System.Data.SqlClient;

namespace StarChef.Common
{
    public interface IDatabaseManager
    {
        int Execute(
                string connectionString,
                string spName,
                int sqlCommandTimeout = 600,
                params SqlParameter[] parameterValues);
    }
}