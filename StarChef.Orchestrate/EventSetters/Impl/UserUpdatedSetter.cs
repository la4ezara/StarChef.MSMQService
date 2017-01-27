using System;
using System.Data.SqlClient;
using Fourth.Orchestration.Model.Menus;
using StarChef.Common;
using StarChef.Orchestrate.Models;

namespace StarChef.Orchestrate
{
    class UserUpdatedSetter : IEventSetter<Events.UserUpdated.Builder>
    {
        public bool SetBuilder(Events.UserUpdated.Builder builder, string connectionString, int entityId, int databaseId)
        {
            if (builder == null) return false;

            Customer cust = new Customer(databaseId);

            var dbManager = new DatabaseManager();

            var reader = dbManager.ExecuteReaderMultiResultset(connectionString, "sc_event_user_detail", new SqlParameter("@entity_id", entityId));
            if (reader.Read())
            {
                builder.SetCustomerId(cust.ExternalId)
                       .SetCustomerName(cust.Name)
                       .SetExternalId(cust.UserExternalId(entityId))
                       .SetFirstName(reader[3].ToString())
                       .SetLastName(reader[4].ToString())
                       .SetLanguage(reader[9].ToString())
                       .SetCanViewMenuCycle(int.Parse(reader[10].ToString()) == 1)
                       .SetCanCreateMenuCycle(int.Parse(reader[11].ToString()) == 1)
                       .SetCanEditMenuCycle(int.Parse(reader[12].ToString()) == 1)
                       .SetCanDeleteMenuCycle(int.Parse(reader[13].ToString()) == 1)
                       .SetCanViewRecipe(int.Parse(reader[14].ToString()) == 1)
                       .SetCanViewMenu(int.Parse(reader[15].ToString()) == 1);
            }

            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    var userGroupBuilder = Events.UserUpdated.Types.UserGroup.CreateBuilder();

                    userGroupBuilder.SetExternalId(reader[1].ToString())
                                    .SetGroupName(reader[2].ToString());

                    builder.AddGroups(userGroupBuilder);
                }
            }

            return true;
        }
    }
}