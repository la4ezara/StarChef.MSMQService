using StarChef.Common;
using System.Collections.Generic;
using System.Data.SqlClient;
using Fourth.Orchestration.Model.Menus;
using System;
using System.Data;

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
            var rand = new Random();
            var builder = Events.GroupUpdated.CreateBuilder();

            var dbManager = new DatabaseManager();
            var dataset = dbManager.ExecuteMultiResultset(connectionString,
                                    "sc_event_group",
                                    new SqlParameter("@entity_id", Id));

            var groupTable = dataset.Tables[0];
            var supplierTable = dataset.Tables[1];
            var ingredientTable = dataset.Tables[2];
            var recipeTable = dataset.Tables[3];
            var menuTable = dataset.Tables[4];

            if (groupTable != null)  //Group Info
            {
                if(groupTable.Rows.Count > 0)
                {
                    var dr = dataset.Tables[0].Rows[0];

                    builder.SetCustomerId(cust.ExternalId)
                    .SetCustomerName(cust.Name)
                    .SetExternalId(dr[1].ToString())
                    .SetGroupName(dr[2].ToString())
                    .SetGroupCode(dr[3].ToString())
                    .SetDescription(dr[4].ToString())
                    .SetCurrencyIso4217Code(dr[5].ToString())
                    .SetLanguageIso6391Code(dr[6].ToString())
                    .SetSource(Events.SourceSystem.STARCHEF)
                    .SetSequenceNumber(rand.Next(1, int.MaxValue));
                }
            }
            
            if (supplierTable != null)  //Supplier
            {
                foreach(DataRow row in supplierTable.Rows)
                {
                    var supBuilder = Events.GroupUpdated.Types.SupplierItem.CreateBuilder();
                    supBuilder.SetExternalId(row[0].ToString())
                        .SetSupplierName(row[1].ToString());
                    builder.AddSuppliers(supBuilder);
                }
            }

            if (ingredientTable != null)  //Ingredient
            {
                foreach (DataRow row in ingredientTable.Rows)
                {
                    var ingBuilder = Events.GroupUpdated.Types.IngredientItem.CreateBuilder();
                    ingBuilder.SetExternalId(row[0].ToString())
                        .SetIngredientName(row[1].ToString());
                    builder.AddIngredients(ingBuilder);
                }
            }

            if (recipeTable != null)  //Recipe
            {
                foreach (DataRow row in recipeTable.Rows)
                {
                    var recBuilder = Events.GroupUpdated.Types.RecipeItem.CreateBuilder();
                    recBuilder.SetExternalId(row[0].ToString())
                        .SetRecipeName(row[1].ToString());
                    builder.AddRecipes(recBuilder);
                }
            }

            if (menuTable != null)  //Recipe
            {
                foreach (DataRow row in menuTable.Rows)
                {
                    var mnuBuilder = Events.GroupUpdated.Types.MenuItem.CreateBuilder();
                    mnuBuilder.SetExternalId(row[0].ToString())
                        .SetMenuName(row[1].ToString());
                    builder.AddMenus(mnuBuilder);
                }
            }

            return builder;
        }
    }
}
