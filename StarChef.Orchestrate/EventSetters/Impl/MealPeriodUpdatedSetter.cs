using System;
using System.Data.SqlClient;
using Fourth.Orchestration.Model.Menus;
using StarChef.Common;
using StarChef.Orchestrate.Helpers;
using StarChef.Orchestrate.Models;

namespace StarChef.Orchestrate
{
    public class MealPeriodUpdatedSetter : IEventSetter<Events.MealPeriodUpdated.Builder>
    {
        public bool SetForDelete(Events.MealPeriodUpdated.Builder builder, string entityExternalId, int databaseId)
        {
            if (builder == null) return false;

            var cust = new Customer(databaseId);
            builder
                .SetCustomerId(cust.ExternalId)
                .SetCustomerName(cust.Name)
                .SetExternalId(entityExternalId);

            return true;
        }

        public bool SetForUpdate(Events.MealPeriodUpdated.Builder builder, string connectionString, int entityId, int databaseId)
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
                    .SetIsEnabled(reader.GetValueOrDefault<bool>(3));
            }

            return true;
        }
    }
}