using Fourth.Orchestration.Model.Menus;

namespace StarChef.Orchestrate
{
    public interface IRecipeUpdatedSetter
    {
        bool SetBuilder(Events.RecipeUpdated.Builder builder, string connectionString, int entityId, int databaseId);
    }
}