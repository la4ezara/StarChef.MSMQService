using System;
using System.Data.SqlClient;
using Fourth.Orchestration.Model.Menus;
using StarChef.Common;
using StarChef.Orchestrate.Models;

namespace StarChef.Orchestrate
{
    public class MenuUpdatedSetter : IEventSetter<Events.MenuUpdated.Builder>
    {
        public bool SetForDelete(Events.MenuUpdated.Builder builder, string entityExternalId, int databaseId)
        {
            if (builder == null) return false;

            var cust = new Customer(databaseId);
            builder
                .SetCustomerId(cust.ExternalId)
                .SetCustomerName(cust.Name)
                .SetExternalId(entityExternalId);

            return true;
        }

        public bool SetForUpdate(Events.MenuUpdated.Builder builder, string connectionString, int entityId, int databaseId)
        {
            if (builder == null) return false;

            Customer cust = new Customer(databaseId);

            var dbManager = new DatabaseManager();
            var reader = dbManager.ExecuteReaderMultiResultset(connectionString, "sc_event_menu", new SqlParameter("@entity_id", entityId));

            if (reader.Read())
            {
                builder.SetCustomerId(cust.ExternalId)
                    .SetCustomerName(cust.Name)
                    .SetExternalId(reader[1].ToString())
                    .SetMenuName(reader[2].ToString())
                    .SetMenuType((byte)reader[3] == 1 ? Events.MenuType.ALACARTE : Events.MenuType.BUFFET);

                //This is for Menu Buffet Sales 
                var menuCountName = "menu_count";
                if (!reader.IsDBNull(menuCountName))
                {
                    builder.SetBuffetMenuSales(reader.GetValue<double>(menuCountName));
                }
            }

            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    var groupBuilder = Events.MenuUpdated.Types.MenuGroup.CreateBuilder();
                    groupBuilder.SetExternalId(reader[0].ToString())
                        .SetGroupName(reader[1].ToString());
                    builder.AddGroups(groupBuilder);
                }
            }

            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    var recipeBuilder = Events.MenuUpdated.Types.MenuRecipe.CreateBuilder();
                    recipeBuilder.SetExternalId(reader[0].ToString())
                        .SetRecipeName(reader[1].ToString())
                        .SetCourseName(reader[2].ToString())
                        .SetDisplayOrder((int)reader[3]);

                    //This is for menu buffet Sales Mix
                    var sellQuantityName = "sell_quantity";
                    if (!reader.IsDBNull(sellQuantityName))
                    {
                        recipeBuilder.SetSellQuantity(reader.GetValue<double>(sellQuantityName));
                    }

                    builder.AddRecipes(recipeBuilder);
                }
            }

            return true;
        }
    }
}