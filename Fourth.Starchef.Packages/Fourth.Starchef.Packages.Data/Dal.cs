#region usings

using System;
using System.Data;
using System.Data.Common;

#endregion

namespace Fourth.Starchef.Packages.Data
{
    public sealed class Dal : IDisposable
    {
        private readonly string _dbProvider;
        private IDbCommand _cmd;
        private IDbConnection _conn;
        private string _connectionString;
        private bool _disposed;


        public Dal(string userDatabase = null)
        {
            _dbProvider = "System.Data.SqlClient";
            _connectionString = userDatabase;
            CreateConnection();
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        public void CreateConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                _connectionString = "";
            }

            DbProviderFactory factory = DbProviderFactories.GetFactory(_dbProvider);
            _conn = factory.CreateConnection();
            if (_conn != null)
            {
                _conn.ConnectionString = _connectionString;
                _conn.Open();

                _cmd = _conn.CreateCommand();
            }
        }

        public IDbCommand CreateCommand(string sqlText, CommandType commandType)
        {
            return CreateCommand(sqlText, null, commandType);
        }

        public IDbCommand CreateCommand(string sqlText, IDataParameter[] dbParams, CommandType commandType)
        {
            if (_conn.State == ConnectionState.Closed)
                CreateConnection();
            _cmd.CommandText = sqlText;
            _cmd.CommandType = commandType;
            return CreateCommand(_conn, _cmd, dbParams);
        }

        public IDbCommand CreateCommand(IDbConnection conn, IDbCommand cmd, IDataParameter[] cmdParams)
        {
            if (cmdParams != null)
            {
                cmd.Parameters.Clear();
                foreach (IDataParameter param in cmdParams)
                {
                    cmd.Parameters.Add(param);
                }
            }
            cmd.CommandTimeout = 99999999;
            return cmd;
        }

        public IDataParameter GetParameter<T>(string paramName, T paramValue)
        {
            IDataParameter param = _cmd.CreateParameter();
            param.ParameterName = paramName;
            param.Value = paramValue;
            return param;
        }

        public IDataReader GetReader(string sqlText, CommandType commandType)
        {
            return GetReader(sqlText, null, commandType);
        }

        public IDataReader GetReader(string sqlText, IDataParameter[] param, CommandType commandType)
        {
            return GetReader(CreateCommand(sqlText, param, commandType));
        }

        public IDataReader GetReader(IDbCommand cmd)
        {
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public int ExecuteSql(IDbCommand cmd)
        {
            return cmd.ExecuteNonQuery();
        }

        public int ExecuteSql(string sqlText, IDataParameter[] param, CommandType commandType)
        {
            return ExecuteSql(CreateCommand(sqlText, param, commandType));
        }

        public int ExecuteSql(string sqlText, CommandType commandType)
        {
            return ExecuteSql(sqlText, null, commandType);
        }

        public T ExecuteScalar<T>(string sqlText, IDataParameter[] param, CommandType commandType)
        {
            return ExecuteScalar<T>(CreateCommand(sqlText, param, commandType));
        }

        public T ExecuteScalar<T>(IDbCommand cmd)
        {
            return (T) cmd.ExecuteScalar();
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _conn.Dispose();
                }
                _disposed = true;
            }
        }
    }
}