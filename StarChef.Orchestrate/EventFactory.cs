using System;
using Fourth.Orchestration.Model.Recipes;
using EventModel = Fourth.Orchestration.Model;
using CategoryModel = Fourth.Orchestration.Model.Recipes.Events.RecipeUpdated.Types.Category;

namespace StarChef.Orchestrate
{
    public class EventFactory
    {
        public static Events.RecipeUpdated CreateRecipeEvent()
        {
            var rand = new Random();

            // Create a builder for the event
            var categoryBuilder = GetCategoryBuilder();

            var builder = Events.RecipeUpdated.CreateBuilder();
            builder.SetId(Guid.NewGuid().ToString())
                   .SetName(new string('N', rand.Next(10, 30)))
                   .SetCreatedByFirstName(new string('F', rand.Next(10, 30)))
                   .SetOrganisationId(new string('O', rand.Next(10, 30)))
                   .SetSequenceNumber(rand.Next(1, int.MaxValue))
                   .SetSourceSystem(Events.SourceSystem.STARCHEF);

            // Build the immutable data object
            var eventObj = builder.Build();

            return eventObj;
        }

        private static CategoryModel.Builder GetCategoryBuilder()
        {
            var categoryBuilder = CategoryModel.CreateBuilder();

            categoryBuilder.SetName("CategoryName");

            return categoryBuilder;
        }
    }
}