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
        public string UserExternalLoginId { get; set; }

        public IDatabaseManager dbManager { get; set; }

        public string connectionString { get; set; }
        public Customer(int CustomerId)
        {
            dbManager = new DatabaseManager();
            Id = CustomerId;
            connectionString = ConfigurationManager.AppSettings["DSN"];

            var reader = dbManager.ExecuteReader(connectionString,
                                    "sc_get_database_details",
                                    new SqlParameter("@db_database_id", CustomerId));
            if(reader.Read())
            {
                ExternalId = reader[1].ToString();
                Name = reader[2].ToString();
            }
        }

        public string UserExternalId(int userId)
        {
            var reader = dbManager.ExecuteReader(connectionString,
                                    "sc_get_user_login_details",
                                    new SqlParameter("@db_database_id", Id),
                                    new SqlParameter("@user_id", userId));
            if (reader.Read())
            {
                UserExternalLoginId = reader[1].ToString();
            }

            return UserExternalLoginId;
        }
    }
}