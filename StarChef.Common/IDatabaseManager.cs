﻿using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using StarChef.Common.Types;

namespace StarChef.Common
{
    public interface IDatabaseManager
    {
        int Execute(string connectionString,string spName,params SqlParameter[] parameterValues);
        int Execute(string connectionString, string spName, bool retry, params SqlParameter[] parameterValues);

        int Execute(string connectionString,string spName,int timeout,params SqlParameter[] parameterValues);

        int Execute(string connectionString, string spName, int timeout,  bool retry, params SqlParameter[] parameterValues);

        IDataReader ExecuteReader(string connectionString,string spName,params SqlParameter[] parameterValues);

        IDataReader ExecuteReader(string connectionString, string spName, bool retry, params SqlParameter[] parameterValues);

        IDataReader ExecuteReaderMultiResultset(
            string connectionString,
            string spName,
            params SqlParameter[] parameterValues
            );

        int ExecuteScalar(
            string connectionString,
            string spName,
            params SqlParameter[] parameterValues
        );

        string GetSetting(string connectionString, string settingName);

        HashSet<UserDatabase> GetUserDatabases(string connectionString);

        IList<int> GetUsersInGroup(string connectionString, int groupId);

        bool IsPublishEnabled(string connectionString, int entityTypeId);
        bool IsSsoEnabled(string connectionString);

        IDictionary<string, ImportTypeSettings> GetImportSettings(string connectionString, int organizationId);

        bool IsSetOrchestrationSentDate(string connectionString, int entityId);
        void UpdateOrchestrationSentDate(string connectionString, int entityId);
        IEnumerable<T> Query<T>(string connectionString, string sql, object param, CommandType commandType);
    }
}