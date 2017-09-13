using System.Data.SqlClient;
using Fourth.Orchestration.Model.Menus;
using StarChef.Common;
using StarChef.Orchestrate.Models;

namespace StarChef.Orchestrate
{
    public class MenuUpdatedSetter : IEventSetter<Events.MenuUpdated.Builder>
    {
        const string SELL_QUANTITY_NAME = "sell_quantity";
        const string MENU_COUNT_NAME = "menu_count";

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
                    .SetMenuType(reader.GetValue<byte>("menu_type_id") == 1 ? Events.MenuType.ALACARTE : Events.MenuType.BUFFET);

                //This is for Menu Buffet Sales 
                if (!reader.IsDBNull(MENU_COUNT_NAME))
                {
                    builder.SetBuffetMenuSales(reader.GetValue<double>(MENU_COUNT_NAME));
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
                    if (!reader.IsDBNull(SELL_QUANTITY_NAME))
                    {
                        recipeBuilder.SetSellQuantity(reader.GetValue<double>(SELL_QUANTITY_NAME));
                    }

                    builder.AddRecipes(recipeBuilder);
                }
            }

            return true;
        }
    }
}