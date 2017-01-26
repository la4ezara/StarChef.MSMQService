using System;
using Fourth.Orchestration.Model.Menus;

namespace StarChef.Orchestrate
{
    public interface IGroupUpdatedSetter
    {
        bool SetBuilder(Events.GroupUpdated.Builder builder, string connectionString, int entityId, int databaseId);
    }
}