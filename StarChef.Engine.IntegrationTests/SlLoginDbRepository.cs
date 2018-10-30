using Fourth.StarChef.Invariables;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace StarChef.Engine.IntegrationTests
{
    class SlLoginDbRepository : RepositoryBase
    {
        readonly string connectionStr;
        public SlLoginDbRepository(string connectionString, int timeout) : this(connectionString)
        {
            base.CommandTimeout = timeout;
        }

        public SlLoginDbRepository(string connectionString)
        {
            connectionStr = connectionString;
        }

        public IEnumerable<string> GetConnectionStrings()
        {
            var cmd = @"SELECT [database_name] + ';' + [server_name]
                    FROM [dbo].[db_database] where is_online = 1";
            using (var connection = GetOpenConnection())
            {
                var result = Query<string>(connection, cmd, null, CommandType.Text);
                return result;
            }
        }

        private SqlConnection GetOpenConnection(bool mars = false)
        {
            var cs = connectionStr;
            if (mars)
            {
                var scsb = new SqlConnectionStringBuilder(cs)
                {
                    MultipleActiveResultSets = true
                };
                cs = scsb.ConnectionString;
            }
            var connection = new SqlConnection(cs);
            connection.Open();
            return connection;
        }
    }
}
