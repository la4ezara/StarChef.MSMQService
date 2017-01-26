using System;
using Fourth.Orchestration.Model.Menus;

namespace StarChef.Orchestrate
{
    public interface IMealPeriodUpdatedSetter
    {
        bool SetBuilder(Events.MealPeriodUpdated.Builder builder, string connectionString, int entityId, int databaseId);
    }
}