using StarChef.Common;
using System.Data.SqlClient;
using Fourth.Orchestration.Model.Menus;
using System;

namespace StarChef.Orchestrate.Models
{
    public class MealPeriod
    {

        public int Id { get; set; }
       

        public MealPeriod(int MealPeriodId)
        {
            Id = MealPeriodId;
            
        }

        public Events.MealPeriodUpdated.Builder Build(Customer cust, string connectionString)
        {
            var builder = Events.MealPeriodUpdated.CreateBuilder();
            var dbManager = new DatabaseManager();
            
            var reader = dbManager.ExecuteReader(connectionString,
                                    "sc_event_mealperiod",
                                    new SqlParameter("@entity_id", Id));
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
            return builder;
        }
    }
}
