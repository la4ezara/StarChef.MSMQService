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

        string GetSetting(string connectionString, string settingName);

        IList<int> GetUsersInGroup(string connectionString, int groupId);

        bool IsPublishEnabled(string connectionString, int entityTypeId);
        bool IsSsoEnabled(string connectionString);

        IDictionary<string, ImportTypeSettings> GetImportSettings(string connectionString, int organizationId);
    }
}