using System;
using System.Data.SqlClient;
using Fourth.Orchestration.Model.Menus;
using StarChef.Common;
using StarChef.Orchestrate.Models;

namespace StarChef.Orchestrate
{
    public class GroupUpdatedSetter : IEventSetter<Events.GroupUpdated.Builder>
    {
        public bool SetForDelete(Events.GroupUpdated.Builder builder, string entityExternalId, int databaseId)
        {
            if (builder == null) return false;

            var cust = new Customer(databaseId);
            builder
                .SetCustomerId(cust.ExternalId)
                .SetCustomerName(cust.Name)
                .SetExternalId(entityExternalId);

            return true;
        }

        public bool SetForUpdate(Events.GroupUpdated.Builder builder, string connectionString, int entityId, int databaseId)
        {
            if (builder == null) return false;

            Customer cust = new Customer(databaseId);

            var dbManager = new DatabaseManager();
            var reader = dbManager.ExecuteReaderMultiResultset(connectionString, "sc_event_group", new SqlParameter("@entity_id", entityId));

            if (reader.Read())
            {
                builder.SetCustomerId(cust.ExternalId)
                    .SetCustomerName(cust.Name)
                    .SetExternalId(reader[1].ToString())
                    .SetGroupName(reader[2].ToString())
                    .SetGroupCode(reader[3].ToString())
                    .SetDescription(reader[4].ToString())
                    .SetCurrencyIso4217Code(reader[5].ToString())
                    .SetLanguageIso6391Code(reader[6].ToString());

            }

            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    var supBuilder = Events.GroupUpdated.Types.SupplierItem.CreateBuilder();
                    supBuilder.SetExternalId(reader[0].ToString())
                        .SetSupplierName(reader[1].ToString());
                    builder.AddSuppliers(supBuilder);
                }
            }

            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    var ingBuilder = Events.GroupUpdated.Types.IngredientItem.CreateBuilder();
                    ingBuilder.SetExternalId(reader[0].ToString())
                        .SetIngredientName(reader[1].ToString());
                    builder.AddIngredients(ingBuilder);
                }
            }

            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    var recBuilder = Events.GroupUpdated.Types.RecipeItem.CreateBuilder();
                    recBuilder.SetExternalId(reader[0].ToString())
                        .SetRecipeName(reader[1].ToString());
                    builder.AddRecipes(recBuilder);
                }
            }

            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    var mnuBuilder = Events.GroupUpdated.Types.MenuItem.CreateBuilder();
                    mnuBuilder.SetExternalId(reader[0].ToString())
                        .SetMenuName(reader[1].ToString());
                    builder.AddMenus(mnuBuilder);
                }
            }

            return true;
        }
    }
}