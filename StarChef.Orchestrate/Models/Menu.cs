namespace StarChef.Orchestrate.Models
{
    using Fourth.Orchestration.Model.Menus;
    using StarChef.Common;
    using System;
    using System.Data.SqlClient;

    public class Menu
    {
        public int Id { get; }


        public Menu(int menuId)
        {
            Id = menuId;
        }


        public Events.MenuUpdated.Builder Build(Customer cust, string connectionString)
        {
            var rand = new Random();
            var builder = Events.MenuUpdated.CreateBuilder();

            var dbManager = new DatabaseManager();
            var reader = dbManager.ExecuteReaderMultiResultset(connectionString,
                                    "sc_event_menu",
                                    new SqlParameter("@entity_id", Id));

            if (reader.Read())
            {
                builder.SetCustomerId(cust.ExternalId)
                .SetCustomerName(cust.Name)
                .SetExternalId(reader[1].ToString())
                .SetMenuName(reader[2].ToString())
                .SetMenuType((int)reader[3] == 1 ? Events.MenuType.ALACARTE : Events.MenuType.BUFFET)
                .SetSource(Events.SourceSystem.STARCHEF)
                .SetSequenceNumber(rand.Next(1, int.MaxValue));
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
                        .SetCourseName(reader[2].ToString());
                    builder.AddRecipes(recipeBuilder);
                }
            }

            return builder;
        }
    }
}
