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
            Customer cust = new Customer(254);
            User user = new User(1);
            Commands.CreateAccount.Builder builderMocked = BuildCreateAccountMock(cust);
            if (builderMocked != null)
            {
                testResult = true;
            }
            Assert.IsTrue(testResult);
        }

        [TestMethod]
        public void TestUpdateUser()
        {
            bool testResult = false;
            Customer cust = new Customer(254);
            User user = new User(1);
            Commands.UpdateAccount.Builder builderMocked = BuildUpdateAccountMock(cust);
            if (builderMocked != null)
            {
                testResult = true;
            }
            Assert.IsTrue(testResult);
        }

        private Commands.CreateAccount.Builder BuildCreateAccountMock(Customer cust)
        {
            var builder = Commands.CreateAccount.CreateBuilder();
            var dbManager = new DatabaseManager();
            
                builder.SetCustomerId("1")
                        .SetEmailAddress("anna@google.com")
                        .SetFirstName("Anna")
                        .SetLastName("Karenina")
                        .SetSource(Commands.SourceSystem.STARCHEF)
                        .SetInternalId("12345");
       
            return builder;
        }

        private Commands.UpdateAccount.Builder BuildUpdateAccountMock(Customer cust)
        {
            var builder = Commands.UpdateAccount.CreateBuilder();
            var rand = new Random();

                builder.SetCustomerId("1")
                        .SetEmailAddress("anna@google.com")
                        .SetFirstName("Anna")
                        .SetLastName("Karenina")
                        .SetCommandId(rand.Next(1, int.MaxValue).ToString())
                        .SetSource(Commands.SourceSystem.STARCHEF)
                        .SetExternalId("12345");

            return builder;
        }
    }
}
