using System.Data.SqlClient;
using Fourth.Orchestration.Model.Menus;
using StarChef.Common;
using StarChef.Orchestrate.Models;

namespace StarChef.Orchestrate
{
    public class SupplierUpdatedSetter : IEventSetter<Events.SupplierUpdated.Builder>
    {
        public bool SetForDelete(Events.SupplierUpdated.Builder builder, string entityExternalId, int databaseId)
        {
            if (builder == null) return false;

            var cust = new Customer(databaseId);
            builder
                .SetCustomerId(cust.ExternalId)
                .SetCustomerName(cust.Name)
                .SetExternalId(entityExternalId);

            return true;
        }

        public bool SetForUpdate(Events.SupplierUpdated.Builder builder, string connectionString, int entityId,
            int databaseId)
        {
            if (builder == null) return false;

            var cust = new Customer(databaseId);
            var dbManager = new DatabaseManager();
            using (
                var reader = dbManager.ExecuteReaderMultiResultset(connectionString, "sc_event_supplier",
                    new SqlParameter("@entity_id", entityId)))
            {
                if (reader.Read())
                {

                    builder
                        .SetCustomerId(cust.ExternalId)
                        .SetCustomerName(cust.Name)
                        .SetExternalId(reader[8].ToString())
                        .SetSupplierName(reader[1].ToString())
                        .SetAccountNumber(reader[2].ToString())
                        .SetAlternateName(reader[3].ToString())
                        .SetCurrencyIso4217Code(reader[4].ToString());
                }

                return true;
            }
        }
    }
}