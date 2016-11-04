using StarChef.Common;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace StarChef.Orchestrate.Models
{
    public class UserGroup
    {
        public int Id { get; set; }

        private IList<User> Users { get; set; }
        public UserGroup(int ugroupId)
        {
            Id = ugroupId;
            Users = new List<User>();
        }

        public IEnumerable<User> GetUsersInGroup(string connectionString)
        {
            var dbManager = new DatabaseManager();

            var reader = dbManager.ExecuteReader(connectionString,
                                    "sc_event_usergroup",
                                    new SqlParameter("@entity_id", Id));
            while (reader.Read())
            {
                Users.Add(new User(int.Parse(reader[0].ToString())));
            }

            return Users;
        }
    }
}
