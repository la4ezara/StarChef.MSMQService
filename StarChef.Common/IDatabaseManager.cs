using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using StarChef.Common.Types;

namespace StarChef.Common
{
    public interface IDatabaseManager
    {
        int Execute(
                string connectionString,
                string spName,
                params SqlParameter[] parameterValues);

        int Execute(
                string connectionString,
                string spName,
                int timeout,
                params SqlParameter[] parameterValues);

        IDataReader ExecuteReader(
            string connectionString,
            string spName,
            params SqlParameter[] parameterValues
            );

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

        bool IsSetOrchestrationSendDate(string connectionString, int entityId);
        void UpdateOrchestrationSendDate(string connectionString, int entityId);
    }
}