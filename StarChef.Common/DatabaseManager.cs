﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Fourth.StarChef.Invariables;
using Fourth.StarChef.Invariables.Extensions;
using StarChef.Common.Types;
using System.Runtime.Caching;
using Dapper;

namespace StarChef.Common
{
    public class DatabaseManager : IDatabaseManager
    {
        public int Execute(
            string connectionString,
            string spName,
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
                var cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = cn;
                cmd.CommandTimeout = cn.ConnectionTimeout;
                cmd.CommandText = spName;

                // add params
                if (parameterValues != null)
                    foreach (var param in parameterValues)
                        cmd.Parameters.Add(param);

                // run proc
                retval = cmd.ExecuteNonQuery();
            }

            return retval;
        }

        public int Execute(string connectionString, string spName, bool retry, params SqlParameter[] parameterValues)
        {
            Func<int> delFunc = () => this.Execute(connectionString, spName, parameterValues);
            if (retry)
            {
                return DeadlockRetryHelper(delFunc, 3);
            }

            return delFunc();
        }

        public int Execute(string connectionString, string spName, int timeout, params SqlParameter[] parameterValues)
        {
            int retval = 0;

            //create & open a SqlConnection, and dispose of it after we are done.
            using (var cn = new SqlConnection(connectionString))
            {
                cn.Open();

                // need a command with sensible timeout value (10 minutes), as some 
                // of these procs may take several minutes to complete
                var cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = timeout;
                cmd.Connection = cn;
                cmd.CommandText = spName;

                // add params
                if (parameterValues != null)
                    foreach (var param in parameterValues)
                        cmd.Parameters.Add(param);

                // run proc
                retval = cmd.ExecuteNonQuery();
            }

            return retval;
        }

        public int Execute(string connectionString, string spName, int timeout, bool retry, params SqlParameter[] parameterValues)
        {
            Func<int> delFunc = () => this.Execute(connectionString, spName, timeout, parameterValues);
            if (retry)
            {
                return DeadlockRetryHelper(delFunc, 3);
            }

            return delFunc();
        }

        public HashSet<UserDatabase> GetUserDatabases(string connectionString)
        {
            var userDatabases = new HashSet<UserDatabase>();
            var reader = ExecuteReader(connectionString, "sp_Get_Users_ConnectionStrings");

            while (reader.Read())
            {
                var dataBaseId = reader.GetValue<int>("db_database_id");
                var databaseConnectionString = reader.GetValue<string>("connString");
                var externalId = reader.GetValue<string>("external_id");
                var db = new UserDatabase(dataBaseId, databaseConnectionString, externalId);
                userDatabases.Add(db);
            }
            return userDatabases;
        }

        public string GetSetting(string connectionString, string settingName)
        {
            var result = string.Empty;
            ObjectCache cache = MemoryCache.Default;
            CacheItem settingNameItem = cache.GetCacheItem(settingName);

            if (settingNameItem == null)
            {
                var value = string.Empty;
                var reader = ExecuteReader(connectionString, "sc_get_db_setting", new SqlParameter("@setting_name", settingName));
                if (reader.Read())
                {
                    value = reader.GetValue(0).ToString();
                    result = value;
                }

                CacheItemPolicy policy = new CacheItemPolicy();
                policy.AbsoluteExpiration = new DateTimeOffset(
                    DateTime.UtcNow.AddHours(1));
                settingNameItem = new CacheItem(settingName, value);
                cache.Set(settingNameItem, policy);
            }
            else
            {
                result = settingNameItem.Value as string;
            }

            return result;
        }

        public IDataReader ExecuteReaderMultiResultset(string connectionString, string spName, params SqlParameter[] parameterValues)
        {
            //create & open a SqlConnection, and dispose of it after we are done.
            using (var cn = new SqlConnection(connectionString))
            {
                DataSet dataSet = new DataSet();

                cn.Open();

                // need a command with sensible timeout value (10 minutes), as some 
                // of these procs may take several minutes to complete
                var cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = cn;
                cmd.CommandTimeout = cn.ConnectionTimeout;
                cmd.CommandText = spName;

                // add params
                if (parameterValues != null)
                {
                    foreach (var param in parameterValues)
                    {
                        cmd.Parameters.Add(param);
                    }
                }

                // run proc
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dataSet);
                return dataSet.CreateDataReader();
            }
        }

        public int ExecuteScalar(string connectionString,string spName, params SqlParameter[] parameterValues)
        { 
            int result;
            using (var cn = new SqlConnection(connectionString))
            {
                cn.Open();

                // need a command with sensible timeout value (10 minutes), as some 
                // of these procs may take several minutes to complete
                var cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = cn;
                cmd.CommandTimeout = cn.ConnectionTimeout;
                cmd.CommandText = spName;

                // add params
                if (parameterValues != null)
                {
                    foreach (var param in parameterValues)
                    {
                        cmd.Parameters.Add(param);
                    }
                }

                result = (int)cmd.ExecuteScalar();
            }
            return result;
        }

        public int ExecuteScalar(string connectionString, string spName, bool retry, params SqlParameter[] parameterValues)
        {
            Func<int> delFunc = () => this.ExecuteScalar(connectionString, spName, parameterValues);
            if (retry)
            {
                return DeadlockRetryHelper(delFunc, 3);
            }

            return delFunc();
        }

        public IDataReader ExecuteReader(string connectionString,string spName,params SqlParameter[] parameterValues)
        {
            //create & open a SqlConnection, and dispose of it after we are done.
            using (var cn = new SqlConnection(connectionString))
            {
                cn.Open();

                // need a command with sensible timeout value (10 minutes), as some 
                // of these procs may take several minutes to complete
                var cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = cn;
                cmd.CommandText = spName;

                // add params
                if (parameterValues != null)
                    foreach (var param in parameterValues)
                        cmd.Parameters.Add(param);

                // run proc
                var reader = cmd.ExecuteReader();
                var dt = new DataTable();
                dt.Load(reader);
                return dt.CreateDataReader();
            }
        }

        public IDataReader ExecuteReader(string connectionString,string spName, bool retry, params SqlParameter[] parameterValues)
        {
            Func<IDataReader> delFunc = () => this.ExecuteReader(connectionString, spName, parameterValues);
            if (retry)
            {
                return DeadlockRetryHelper(delFunc, 3);
            }

            return delFunc();
        }

        public IList<int> GetUsersInGroup(string connectionString, int groupId)
        {
            var result = new List<int>();
            var reader = ExecuteReader(connectionString, "sc_event_usergroup", new SqlParameter("@entity_id", groupId));
            while (reader.Read())
            {
                var id = int.Parse(reader[0].ToString());
                result.Add(id);
            }
            return result;
        }

        public bool IsPublishEnabled(string connectionString, int entityTypeId)
        {
            bool isEnabled = false;

            //create & open a SqlConnection, and dispose of it after we are done.
            using (var cn = new SqlConnection(connectionString))
            {
                cn.Open();

                // need a command with sensible timeout value (10 minutes), as some 
                // of these procs may take several minutes to complete
                var cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = Constants.TIMEOUT_MSMQ_EXEC_STOREDPROC;
                cmd.Connection = cn;
                cmd.CommandText = "sc_get_orchestration_lookup";
                cmd.Parameters.Add(new SqlParameter("@entity_type_id", entityTypeId));
                var rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    if (!rdr.IsDBNull(0)) isEnabled = rdr.GetBoolean(0);
                }
            }
            return isEnabled;
        }

        public bool IsSetOrchestrationSentDate(string connectionString, int entityId)
        {
            var result = false;

            var reader = ExecuteReader(connectionString, "sc_get_product_orchestration_sent_date",
                new SqlParameter("@product_id", entityId));

            if (reader.Read() && !reader.IsDBNull(0))
            {
                result = true;
            }

            return result;
        }

        public void UpdateOrchestrationSentDate(string connectionString, int entityId)
        {
            Execute(connectionString, "sc_update_product_orchestration_sent_date",
                new SqlParameter("@product_id", entityId),
                new SqlParameter("@orchestration_sent_date", DateTime.UtcNow));
        }


        public bool IsSsoEnabled(string connectionString)
        {
            var value = GetSetting(connectionString, Constants.CONFIG_ALLOW_SINGLE_SIGN_ON);
            return value == "1" || value.ToUpperInvariant() == "TRUE";
        }

        public IDictionary<string, ImportTypeSettings> GetImportSettings(string connectionString, int organizationId)
        {
            var result = new Dictionary<string, ImportTypeSettings>();
            var reader = ExecuteReader(connectionString, "sc_supplier_import_get_settings", new SqlParameter("@organisation_id", organizationId));
            while (reader.Read())
            {
                var settings = new ImportTypeSettings
                {
                    Id = reader.GetValue<int>("import_type_id"),
                    Name = reader.GetValue<string>("import_type_name"),
                    AutoCalculateCost = reader.GetValue<bool>("auto_calc_cost_real_time"),
                    AutoCalculateIntolerance = reader.GetValue<bool>("autp_calc_intol_real_time")
                };
                result.Add(settings.Name, settings);
            }
            return result;
        }

        public IEnumerable<T> Query<T>(string connectionString, string sql, object param, CommandType commandType)
        {

            var scsb = new SqlConnectionStringBuilder(connectionString)
            {
                MultipleActiveResultSets = true
            };

            connectionString = scsb.ConnectionString;

            var connection = new SqlConnection(connectionString);
            connection.Open();

            var result = connection.Query<T>(
                    sql,
                    param,
                    commandType: commandType,
                    commandTimeout: 120);

            connection.Close();

            return result;
        }

        protected T DeadlockRetryHelper<T>(Func<T> repositoryMethod, int maxRetries)
        {
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    return repositoryMethod();
                }
                catch (SqlException e) // This example is for SQL Server, change the exception type/logic if you're using another DBMS
                {
                    if (e.Number == 1205)  // SQL Server error code for deadlock
                    {
                        retryCount++;
                    }
                    else
                    {
                        throw;  // Not a deadlock so throw the exception
                    }
                    // Add some code to do whatever you want with the exception once you've exceeded the max. retries
                }
            }

            return default(T);
        }
    }
}