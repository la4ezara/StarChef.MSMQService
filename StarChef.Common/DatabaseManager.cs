using System.Data;
using System.Data.SqlClient;

namespace StarChef.Common
{
    public class DatabaseManager : IDatabaseManager
    {
        public int Execute(
            string connectionString, 
            string spName, 
            int sqlCommandTimeout = 600, 
            params SqlParameter[] parameterValues
            )
        {
            int retval = 0;

            //create & open a SqlConnection, and dispose of it after we are done.
            using (var cn = new SqlConnection(connectionString))
            {
                cn.Open();

                // need a command with sensible timeout value (10 minutes), as some 
                // of these procs may take several minutes to complete
                var cmd = new SqlCommand(spName, cn)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = sqlCommandTimeout
                };

                // add params
                if (parameterValues != null)
                    foreach (var param in parameterValues)
                        cmd.Parameters.Add(param);

                // run proc
                retval = cmd.ExecuteNonQuery();
            }

            return retval;
        }
    }
}