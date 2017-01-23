using StarChef.Common;
using System.Data.SqlClient;
using Fourth.Orchestration.Model.Menus;
using System;

namespace StarChef.Orchestrate.Models
{
    public class Group
    {
        public int Id { get; set; }
     

        public Group(int GroupId)
        {
            Id = GroupId;           
        }


        public Events.GroupUpdated.Builder Build(Customer cust, string connectionString)
        {
            var builder = Events.GroupUpdated.CreateBuilder();

            var dbManager = new DatabaseManager();
            var reader = dbManager.ExecuteReaderMultiResultset(connectionString,
                                    "sc_event_group",
                                    new SqlParameter("@entity_id", Id));

            if (reader.Read())
            {
                builder.SetCustomerId(cust.ExternalId)
                .SetCustomerName(cust.Name)
                .SetExternalId(reader[1].ToString())
                .SetGroupName(reader[2].ToString())
                .SetGroupCode(reader[3].ToString())
                .SetDescription(reader[4].ToString())
                .SetCurrencyIso4217Code(reader[5].ToString())
                .SetLanguageIso6391Code(reader[6].ToString())
                .SetSource(Events.SourceSystem.STARCHEF)
                .SetChangeType(Events.ChangeType.UPDATE)
                .SetSequenceNumber(Fourth.Orchestration.Model.SequenceNumbers.GetNext());

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

            return builder;
        }
    }
}