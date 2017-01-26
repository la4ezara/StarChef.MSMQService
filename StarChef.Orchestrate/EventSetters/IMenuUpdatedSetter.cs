using System;
using Fourth.Orchestration.Model.Menus;

namespace StarChef.Orchestrate
{
    public interface IMenuUpdatedSetter
    {
        bool SetBuilder(Events.MenuUpdated.Builder builder, string connectionString, int entityId, int databaseId);
    }
}