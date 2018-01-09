using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Fourth.StarChef.Invariables;
using StarChef.Common.Types;
using System.Runtime.Caching;

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
                var cmd = new SqlCommand(spName, cn)
                {
                    CommandType = CommandType.StoredProcedure
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

        public int Execute(
            string connectionString,
            string spName,
            int timeout,
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
                    CommandTimeout = timeout
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

        public IDataReader ExecuteReaderMultiResultset(
            string connectionString, 
            string spName, 
            params SqlParameter[] parameterValues
            )
        {
            //create & open a SqlConnection, and dispose of it after we are done.
            using (var cn = new SqlConnection(connectionString))
            {
                DataSet dataSet = new DataSet();

                cn.Open();

                // need a command with sensible timeout value (10 minutes), as some 
                // of these procs may take several minutes to complete
                var cmd = new SqlCommand(spName, cn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // add params
                if (parameterValues != null)
                    foreach (var param in parameterValues)
                        cmd.Parameters.Add(param);

                // run proc
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dataSet);
                return dataSet.CreateDataReader();
            }
        }

        public IDataReader ExecuteReader(
           string connectionString,
           string spName,
           params SqlParameter[] parameterValues
           )
        {
            //create & open a SqlConnection, and dispose of it after we are done.
            using (var cn = new SqlConnection(connectionString))
            {
                cn.Open();

                // need a command with sensible timeout value (10 minutes), as some 
                // of these procs may take several minutes to complete
                var cmd = new SqlCommand(spName, cn)
                {
                    CommandType = CommandType.StoredProcedure
                };

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
                var cmd = new SqlCommand("sc_get_orchestration_lookup", cn)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = Constants.TIMEOUT_MSMQ_EXEC_STOREDPROC
                };
                cmd.Parameters.Add(new SqlParameter("@entity_type_id", entityTypeId));
                var rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    if (!rdr.IsDBNull(0)) isEnabled = rdr.GetBoolean(0);
                }
            }
            return isEnabled;
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
    }
}