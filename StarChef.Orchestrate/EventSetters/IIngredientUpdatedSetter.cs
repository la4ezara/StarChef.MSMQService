using Fourth.Orchestration.Model.Menus;

namespace StarChef.Orchestrate
{
    public interface IIngredientUpdatedSetter
    {
        bool SetBuilder(Events.IngredientUpdated.Builder builder, string connectionString, int entityId, int databaseId);
    }
}