using StarChef.Common;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace StarChef.Orchestrate.Models
{
    public class Group
    {
        public string ExternalId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string CurrencyCode { get; set; }
        public string LanguageCode { get; set; }
        Dictionary<string,string> Suppliers { get; set; }
        Dictionary<string, string> Ingredients { get; set; }
        Dictionary<string, string> Recipes { get; set; }
        Dictionary<string, string> Menus { get; set; }

        public Group(int GroupId, string connectionString)
        {
            var dbManager = new DatabaseManager();

            Id = GroupId;

            var reader = dbManager.ExecuteReader(connectionString,
                                    "sc_event_group",
                                    new SqlParameter("@entity_id", GroupId));
            if (reader.Read())
            {
                ExternalId = reader[1].ToString();
                Name = reader[2].ToString();
                Code = reader[3].ToString();
                Description = reader[4].ToString();
                CurrencyCode = reader[5].ToString();
                LanguageCode = reader[6].ToString();
            }

            reader.NextResult();
            Suppliers = new Dictionary<string, string>();
            while(reader.Read())
            {
                Suppliers.Add(reader[0].ToString(), reader[1].ToString());
            }

            reader.NextResult();
            Ingredients = new Dictionary<string, string>();
            while (reader.Read())
            {
                Ingredients.Add(reader[0].ToString(), reader[1].ToString());
            }

            reader.NextResult();
            Recipes = new Dictionary<string, string>();
            while (reader.Read())
            {
                Recipes.Add(reader[0].ToString(), reader[1].ToString());
            }

            reader.NextResult();
            Menus = new Dictionary<string, string>();
            while (reader.Read())
            {
                Menus.Add(reader[0].ToString(), reader[1].ToString());
            }
        }
    }
}
