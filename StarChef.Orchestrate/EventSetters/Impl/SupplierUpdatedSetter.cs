using System;
using Fourth.Orchestration.Model.Menus;

namespace StarChef.Orchestrate
{
    class SupplierUpdatedSetter : IEventSetter<Events.SupplierUpdated.Builder>
    {
        public bool SetBuilder(Events.SupplierUpdated.Builder builder, string connectionString, int entityId, int databaseId)
        {
            throw new NotImplementedException();
        }
    }
}