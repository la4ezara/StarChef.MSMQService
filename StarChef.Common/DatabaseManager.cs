using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

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

        public string GetSetting(string connectionString, string settingName)
        {
            var reader = ExecuteReader(connectionString, "sc_get_db_setting", new SqlParameter("@setting_name", settingName));
            if (reader.Read())
                return reader.GetValue(0).ToString();
            return null;
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
    }
}