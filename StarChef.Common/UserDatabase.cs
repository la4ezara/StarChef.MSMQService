using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarChef.Common
{
    public class UserDatabase
    {
        public int DatabaseId { get; private set; }
        public string ConnectionString { get; private set; }

        public string ExternalId { get; private set; }
        public HashSet<OrchestrationLookup> OrchestrationLookups { get; set; }

        public UserDatabase(int databaseId, string connectionString, string externalId)
        {
            this.DatabaseId = databaseId;
            this.ConnectionString = connectionString;
            this.ExternalId = externalId;
            this.OrchestrationLookups = new HashSet<OrchestrationLookup>();
        }
    }

    public class OrchestrationLookup
    {
        public int EntityTypeId { get; private set; }
        public bool CanPublish { get; private set; }

        public OrchestrationLookup(int entityTypeId, bool canPublish)
        {
            this.EntityTypeId = entityTypeId;
            this.CanPublish = canPublish;
        }
    }
}
