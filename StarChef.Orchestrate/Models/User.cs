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
        public User() { }

        public User(int userId)
        {
            Id = userId;
        }

        public Events.UserUpdated.Builder Build(Customer cust, string connectionString)
        {
            var rand = new Random();
            var builder = Events.UserUpdated.CreateBuilder();
            var dbManager = new DatabaseManager();

            var reader = dbManager.ExecuteReaderMultiResultset(connectionString,
                                    "sc_event_user_detail",
                                    new SqlParameter("@entity_id", Id));
            if (reader.Read())
            {
                builder.SetCustomerId(cust.ExternalId)
                       .SetCustomerName(cust.Name)
                       .SetExternalId(cust.UserExternalId(Id))
                       .SetFirstName(reader[3].ToString())
                       .SetLastName(reader[4].ToString())
                       .SetLanguage(reader[9].ToString())
                       .SetCanViewMenuCycle(int.Parse(reader[10].ToString()) == 1)
                       .SetCanCreateMenuCycle(int.Parse(reader[11].ToString()) == 1)
                       .SetCanEditMenuCycle(int.Parse(reader[12].ToString()) == 1)
                       .SetCanDeleteMenuCycle(int.Parse(reader[13].ToString()) == 1)
                       .SetCanViewRecipe(int.Parse(reader[14].ToString()) == 1)
                       .SetCanViewMenu(int.Parse(reader[15].ToString()) == 1)
                       .SetSource(Events.SourceSystem.STARCHEF)
                       .SetChangeType(Events.ChangeType.UPDATE)
                       .SetSequenceNumber(rand.Next(1, int.MaxValue));
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

            return builder;
        }

        /// <summary>
        /// Create command to activate account
        /// </summary>
        public Commands.ActivateAccount.Builder BuildActivateAccount(int userId, int databaseId)
        {
            var dbManager = new DatabaseManager();
            var connectionStringLogin = ConfigurationManager.AppSettings["DSN"];
            SqlParameter[] parameters =
            {
                new SqlParameter { ParameterName = "@user_id", Value = userId },
                new SqlParameter { ParameterName = "@db_database_id", Value = databaseId }
            };
            var readerLogin = dbManager.ExecuteReader(connectionStringLogin, "sc_get_user_login_details", parameters);
            readerLogin.Read();
            var sfAccoutnId = readerLogin.GetString(1);

            var builder = Commands.ActivateAccount.CreateBuilder();

            builder.SetExternalId(sfAccoutnId)
                .SetCommandId(new Random().Next(1, int.MaxValue).ToString())
                .SetSource(Commands.SourceSystem.STARCHEF);
            return builder;
        }

        /// <summary>
        /// Create command to deactivate account
        /// </summary>
        public Commands.DeactivateAccount.Builder BuildDeactivateAccount(int userId, int databaseId)
        {
            var dbManager = new DatabaseManager();
            var connectionStringLogin = ConfigurationManager.AppSettings["DSN"];
            SqlParameter[] parameters =
            {
                new SqlParameter { ParameterName = "@user_id", Value = userId },
                new SqlParameter { ParameterName = "@db_database_id", Value = databaseId }
            };
            var readerLogin = dbManager.ExecuteReader(connectionStringLogin, "sc_get_user_login_details", parameters);
            readerLogin.Read();
            var sfAccoutnId = readerLogin.GetString(1);

            var builder = Commands.DeactivateAccount.CreateBuilder();
            builder.SetExternalId(sfAccoutnId)
                .SetCommandId(new Random().Next(1, int.MaxValue).ToString())
                .SetSource(Commands.SourceSystem.STARCHEF);
            
            return builder;
        }

        public Commands.CreateAccount.Builder BuildCreateAccount(Customer cust, string connectionString)
        {
            var builder = Commands.CreateAccount.CreateBuilder();
            var dbManager = new DatabaseManager();
            var reader = dbManager.ExecuteReader(connectionString,
                                    "sc_event_user",
                                    new SqlParameter("@entity_id", Id));

            var connectionStringLogin = ConfigurationManager.AppSettings["DSN"];
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

            var connectionStringLogin = ConfigurationManager.AppSettings["DSN"];
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
