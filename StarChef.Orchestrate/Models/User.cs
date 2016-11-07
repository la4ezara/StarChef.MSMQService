using StarChef.Common;
using System.Data.SqlClient;
using System;
using System.Configuration;
using Events = Fourth.Orchestration.Model.Menus.Events;
using Commands = Fourth.Orchestration.Model.People.Commands;

namespace StarChef.Orchestrate.Models
{
    public class User
    {
        public int Id { get; set; }


        public User(int userId)
        {
            Id = userId;
        }

        public Events.UserUpdated.Builder Build(Customer cust, string connectionString)
        {
            var rand = new Random();
            var builder = Events.UserUpdated.CreateBuilder();
            var dbManager = new DatabaseManager();

            var reader = dbManager.ExecuteReader(connectionString,
                                    "sc_event_user",
                                    new SqlParameter("@entity_id", Id));
            if (reader.Read())
            {
                builder.SetCustomerId(cust.ExternalId)
                .SetCustomerName(cust.Name)
                .SetExternalId(reader[1].ToString())
                .SetFirstName(reader[4].ToString())
                .SetLastName(reader[5].ToString())
                .SetLanguage(reader[10].ToString())
                .SetSource(Events.SourceSystem.STARCHEF)
                .SetSequenceNumber(rand.Next(1, int.MaxValue));
            }
            return builder;
        }

        public Commands.ActivateAccount.Builder BuildActivateAccount(Customer cust, string connectionString)
        {
            var rand = new Random();
            var builder = Commands.ActivateAccount.CreateBuilder();
            var dbManager = new DatabaseManager();

            var reader = dbManager.ExecuteReader(connectionString,
                                    "sc_event_user",
                                    new SqlParameter("@entity_id", Id));
            if (reader.Read())
            {
                builder.SetExternalId(cust.ExternalId)
                        .SetCommandId(rand.Next(1, int.MaxValue).ToString())
                        .SetSource(Commands.SourceSystem.STARCHEF);
            }
            return builder;
        }

        public Commands.DeactivateAccount.Builder BuildDeactivateAccount(Customer cust, string connectionString)
        {
            var rand = new Random();
            var builder = Commands.DeactivateAccount.CreateBuilder();
            var dbManager = new DatabaseManager();

            var reader = dbManager.ExecuteReader(connectionString,
                                    "sc_event_user",
                                    new SqlParameter("@entity_id", Id));
            if (reader.Read())
            {
                builder.SetExternalId(cust.ExternalId)
                        .SetCommandId(rand.Next(1, int.MaxValue).ToString())
                        .SetSource(Commands.SourceSystem.STARCHEF);
            }
            return builder;
        }

        public Commands.CreateAccount.Builder BuildCreateAccount(Customer cust, string connectionString)
        {
            var builder = Commands.CreateAccount.CreateBuilder();
            var dbManager = new DatabaseManager();
            var reader = dbManager.ExecuteReader(connectionString,
                                    "sc_event_user",
                                    new SqlParameter("@entity_id", Id));

            var connectionStringLogin = ConfigurationManager.ConnectionStrings["StarchefLogin"];
            SqlParameter[] parameters = { new SqlParameter { ParameterName = "@user_id", Value = Id}, new SqlParameter { ParameterName = "@db_database_id", Value = cust.Id } };
            var readerLogin = dbManager.ExecuteReader(connectionStringLogin.ToString(),
                                   "sc_get_user_login_details", parameters);

            if (reader.Read())
            {
                builder.SetCustomerId(cust.ExternalId)
                        .SetEmailAddress(reader[1].ToString())
                        .SetFirstName(reader[3].ToString())
                        .SetLastName(reader[4].ToString())
                        .SetSource(Commands.SourceSystem.STARCHEF);

                if (readerLogin.Read())
                {
                    builder.SetInternalId(readerLogin[2].ToString());
                }
            }
            return builder;
        }

        public Commands.UpdateAccount.Builder BuildUpdateAccount(Customer cust, string connectionString)
        {
            var rand = new Random();
            var builder = Commands.UpdateAccount.CreateBuilder();
            var dbManager = new DatabaseManager();

            var reader = dbManager.ExecuteReader(connectionString,
                                    "sc_event_user",
                                    new SqlParameter("@entity_id", Id));

            var connectionStringLogin = ConfigurationManager.ConnectionStrings["StarchefLogin"];
            SqlParameter[] parameters = { new SqlParameter { ParameterName = "@user_id", Value = Id }, new SqlParameter { ParameterName = "@db_database_id", Value = cust.Id } };
            var readerLogin = dbManager.ExecuteReader(connectionStringLogin.ToString(),
                                   "sc_get_user_login_details", parameters);

            if (reader.Read())
            {
                builder.SetCustomerId(cust.ExternalId)
                        .SetEmailAddress(reader[1].ToString())
                        .SetFirstName(reader[3].ToString())
                        .SetLastName(reader[4].ToString())
                        .SetCommandId(rand.Next(1, int.MaxValue).ToString())
                        .SetSource(Commands.SourceSystem.STARCHEF);

                if (readerLogin.Read())
                {
                    builder.SetExternalId(readerLogin[1].ToString());
                }
            }
            return builder;
        }
    }
}
