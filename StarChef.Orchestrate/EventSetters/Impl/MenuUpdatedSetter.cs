using System.Data.SqlClient;
using Fourth.Orchestration.Model.Menus;
using StarChef.Common;
using StarChef.Orchestrate.Models;

namespace StarChef.Orchestrate
{
    class MenuUpdatedSetter : IMenuUpdatedSetter
    {
        public bool SetBuilder(Events.MenuUpdated.Builder builder, string connectionString, int entityId, int databaseId)
        {
            if (builder == null) return false;

            Customer cust = new Customer(databaseId);

            var dbManager = new DatabaseManager();
            var reader = dbManager.ExecuteReaderMultiResultset(connectionString,
                "sc_event_menu",
                new SqlParameter("@entity_id", entityId));

            if (reader.Read())
            {
                builder.SetCustomerId(cust.ExternalId)
                    .SetCustomerName(cust.Name)
                    .SetExternalId(reader[1].ToString())
                    .SetMenuName(reader[2].ToString())
                    .SetMenuType((byte)reader[3] == 1 ? Events.MenuType.ALACARTE : Events.MenuType.BUFFET)
                    .SetSource(Events.SourceSystem.STARCHEF)
                    .SetSequenceNumber(Fourth.Orchestration.Model.SequenceNumbers.GetNext());
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
                    builder.AddRecipes(recipeBuilder);
                }
            }

            return true;
        }
    }
}