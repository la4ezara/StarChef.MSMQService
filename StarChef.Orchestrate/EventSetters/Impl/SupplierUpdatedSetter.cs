using System;
using Fourth.Orchestration.Model.Menus;
using StarChef.Orchestrate.Models;

namespace StarChef.Orchestrate
{
    public class SupplierUpdatedSetter : IEventSetter<Events.SupplierUpdated.Builder>
    {
        public bool SetForDelete(Events.SupplierUpdated.Builder builder, string entityExternalId, int databaseId)
        {
            if (builder == null) return false;

            var cust = new Customer(databaseId);
            builder
                .SetCustomerId(cust.ExternalId)
                .SetCustomerName(cust.Name)
                .SetExternalId(entityExternalId);

            return true;
        }

        public bool SetForUpdate(Events.SupplierUpdated.Builder builder, string connectionString, int entityId, int databaseId)
        {
            throw new NotImplementedException();
        }
    }
}