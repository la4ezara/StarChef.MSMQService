using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarChef.Common.Models
{
    public class MealPeriod
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string ExternalId { get; set; }
        public bool IsEnabled { get; set; }

        public MealPeriod(int MealPeriodId, string connectionString)
        {
            var dbManager = new DatabaseManager();

            Id = MealPeriodId;

            var reader = dbManager.ExecuteReader(connectionString,
                                    "sc_event_mealperiod",
                                    new SqlParameter("@entity_id", MealPeriodId));
            if (reader.Read())
            {
                ExternalId = reader[1].ToString();
                Name = reader[2].ToString();
                IsEnabled = (bool)reader[3];
            }
        }
    }
}
