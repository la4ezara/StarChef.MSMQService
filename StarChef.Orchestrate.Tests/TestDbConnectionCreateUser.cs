using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StarChef.Common;
using StarChef.Orchestrate.Models;
using Commands = Fourth.Orchestration.Model.People.Commands;
using System.Data.SqlClient;
using Fourth.Orchestration.Model.TeamHoursImport;

namespace StarChef.Orchestrate.Tests
{
    [TestClass]
    public class TestDbConnectionCreateUser
    {
        [TestMethod]
        public void TestCreateUser()
        {
            bool testResult = false;
            string myDBName = "SCNET_demo_qa";
            string connectionString = @"Data Source=.\SQLExpress;Initial Catalog=" + myDBName + ";Persist Security Info=True;User ID=sl_web_user; Password=reddevil";
            Customer cust = new Customer(254);
            User user = new User(1);
            Commands.CreateAccount.Builder builder = user.BuildCreateAccount(cust, connectionString);
            Commands.CreateAccount.Builder builderMocked = BuildCreateAccountMock(cust, connectionString);
            if (builderMocked != null && builder != null)
            {
                if(builderMocked.EmailAddress == builder.EmailAddress && builderMocked.FirstName == builder.FirstName && builderMocked.LastName == builder.LastName && builderMocked.CustomerId == builder.CustomerId)
                testResult = true;
            }
            Assert.IsTrue(testResult);
        }

        [TestMethod]
        public void TestUpdateUser()
        {
            bool testResult = false;
            string myDBName = "SCNET_demo_qa";
            string connectionString = @"Data Source=.\SQLExpress;Initial Catalog=" + myDBName + ";Persist Security Info=True;User ID=sl_web_user; Password=reddevil";
            Customer cust = new Customer(254);
            User user = new User(1);
            Commands.UpdateAccount.Builder builder = user.BuildUpdateAccount(cust, connectionString);
            Commands.UpdateAccount.Builder builderMocked = BuildUpdateAccountMock(cust, connectionString);
            if (builderMocked != null && builder != null)
            {
                if (builderMocked.EmailAddress == builder.EmailAddress && builderMocked.FirstName == builder.FirstName && builderMocked.LastName == builder.LastName && builderMocked.CustomerId == builder.CustomerId)
                    testResult = true;
            }
            Assert.IsTrue(testResult);
        }

        private Commands.CreateAccount.Builder BuildCreateAccountMock(Customer cust, string connectionString)
        {
            var builder = Commands.CreateAccount.CreateBuilder();
            var dbManager = new DatabaseManager();
            var reader = dbManager.ExecuteReader(connectionString,
                                    "sc_event_user",
                                    new SqlParameter("@entity_id", 1));

            var connectionStringLogin = ConfigurationManager.ConnectionStrings["StarchefLogin"];
            SqlParameter[] parameters = { new SqlParameter { ParameterName = "@user_id", Value = 1 }, new SqlParameter { ParameterName = "@db_database_id", Value = cust.Id } };
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

        private Commands.UpdateAccount.Builder BuildUpdateAccountMock(Customer cust, string connectionString)
        {
            var rand = new Random();
            var builder = Commands.UpdateAccount.CreateBuilder();
            var dbManager = new DatabaseManager();

            var reader = dbManager.ExecuteReader(connectionString,
                                    "sc_event_user",
                                    new SqlParameter("@entity_id", 1));

            var connectionStringLogin = ConfigurationManager.ConnectionStrings["StarchefLogin"];
            SqlParameter[] parameters = { new SqlParameter { ParameterName = "@user_id", Value = 1 }, new SqlParameter { ParameterName = "@db_database_id", Value = cust.Id } };
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
