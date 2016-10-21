using StarChef.Common;
using System.Configuration;
using System.Data.SqlClient;

namespace StarChef.Orchestrate.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ExternalId { get; set; }

        public Customer(int CustomerId)
        {
            var dbManager = new DatabaseManager();

            Id = CustomerId;
            string connectionString = ConfigurationManager.AppSettings["DSN"];

            var reader = dbManager.ExecuteReader(connectionString,
                                    "sc_get_database_details",
                                    new SqlParameter("@db_database_id", CustomerId));
            if(reader.Read())
            {
                ExternalId = reader[1].ToString();
                Name = reader[2].ToString();
            }
        }

    }
}
