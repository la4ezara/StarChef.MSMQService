using System.Data.SqlClient;
using Fourth.Orchestration.Model.Menus;
using StarChef.Common;
using StarChef.Orchestrate.Models;

namespace StarChef.Orchestrate
{
    class MealPeriodUpdatedSetter : IMealPeriodUpdatedSetter
    {
        public bool SetBuilder(Events.MealPeriodUpdated.Builder builder, string connectionString, int entityId, int databaseId)
        {
            if (builder == null) return false;

            Customer cust = new Customer(databaseId);

            var dbManager = new DatabaseManager();

            var reader = dbManager.ExecuteReader(connectionString,
                "sc_event_mealperiod",
                new SqlParameter("@entity_id", entityId));
            if (reader.Read())
            {
                builder.SetCustomerId(cust.ExternalId)
                    .SetCustomerName(cust.Name)
                    .SetExternalId(reader[1].ToString())
                    .SetMealPeriodName(reader[2].ToString())
                    .SetSource(Events.SourceSystem.STARCHEF)
                    .SetChangeType(Events.ChangeType.UPDATE)
                    .SetSequenceNumber(Fourth.Orchestration.Model.SequenceNumbers.GetNext());
            }

            return true;
        }
    }
}