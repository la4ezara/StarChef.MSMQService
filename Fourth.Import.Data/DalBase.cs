using System;
using System.Data;
using System.Data.Common;

namespace Fourth.Import.Data
{
    public class DalBase : IDisposable
    {
        private bool _disposed;
        private readonly string _dbProvider;
        private string _connectionString;

        public DalBase(string dbProvider, string connectionString)
        {
            _dbProvider = dbProvider;
            _connectionString = connectionString;
        }

        protected IDbConnection GetConnection() {
            if (string.IsNullOrEmpty(_connectionString))
            {
                _connectionString = string.Empty;
            }

            DbProviderFactory factory = DbProviderFactories.GetFactory(_dbProvider);
            var conn = factory.CreateConnection();
            conn.ConnectionString = _connectionString;
            return conn;
        }

        protected IDbCommand CreateCommand(IDbConnection conn, IDbCommand cmd, IDataParameter[] cmdParams)
        {
            if (cmdParams != null)
            {
                cmd.Parameters.Clear();
                foreach (IDataParameter param in cmdParams)
                {
                    cmd.Parameters.Add(param);
                }
            }
            cmd.CommandTimeout = 0;
            return cmd;
        }

        protected IDataParameter GetParameter<T>(string paramName, T paramValue)
        {
            IDataParameter res = null;
            if (this._dbProvider == ProviderType.Sql)
            {
                res = new System.Data.SqlClient.SqlParameter(paramName, paramValue);
            }
            else if (this._dbProvider == ProviderType.OleDb)
            {
                res = new System.Data.OleDb.OleDbParameter(paramName, paramValue);
            }
            return res;
        }

        protected IDataReader GetReader(IDbConnection conn, string sqlText, CommandType commandType)
        {
            return GetReader(conn, sqlText, null, commandType);
        }

        protected IDataReader GetReader(IDbConnection conn, string sqlText, IDataParameter[] param, CommandType commandType)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = sqlText;
            cmd.CommandType = commandType;
            CreateCommand(conn, cmd, param);
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        protected T ExecuteSql<T>(IDbConnection conn, string sqlText, IDataParameter[] param, CommandType commandType)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = sqlText;
            cmd.CommandType = commandType;
            CreateCommand(conn, cmd, param);
            var result = cmd.ExecuteScalar();
            if (result == null || result == DBNull.Value) {
                return default(T);
            }
            return (T)Convert.ChangeType(result, typeof(T));
        }

        protected int ExecuteSql(string sqlText, IDataParameter[] param, CommandType commandType)
        {
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = sqlText;
                cmd.CommandType = commandType;
                CreateCommand(conn, cmd, param);
                return cmd.ExecuteNonQuery();
            }
        }

        protected int ExecuteSql(string sqlText, CommandType commandType)
        {
            return ExecuteSql(sqlText, null, commandType);
        }

        public void ExecuteSql(string sqlText)
        {

            ExecuteSql(sqlText, CommandType.Text);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }
                _disposed = true;
            }
        }
    }
}
