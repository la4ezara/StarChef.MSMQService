using System;
using Fourth.Orchestration.Model.Menus;

namespace StarChef.Orchestrate
{
    public interface ISupplierUpdatedSetter
    {
        bool SetBuilder(Events.SupplierUpdated.Builder builder, string connectionString, int entityId, int databaseId);
    }

    class SupplierUpdatedSetter : ISupplierUpdatedSetter
    {
        public bool SetBuilder(Events.SupplierUpdated.Builder builder, string connectionString, int entityId, int databaseId)
        {
            throw new NotImplementedException();
        }
    }
}