using StarChef.Engine.IntegrationTests.Model;
using System;
using System.Linq;

namespace StarChef.Engine.IntegrationTests.TheoryData
{
    public class RecipeRecalculateTheoryData : TheoryPriceData<PriceRecalculationRequest>
    {
        public RecipeRecalculateTheoryData()
        {
            CustomerDbRepository customerDbRepository = new CustomerDbRepository(TestConfiguration.Instance.ConnectionString, TestConfiguration.Instance.TimeOut);
            var recipes = customerDbRepository.GetProducts().ToList();

            var max = TestConfiguration.Instance.MaxTestsAmount;
            if (max != 0 && max < recipes.Count)
            {
                Random rnd = new Random();
                for (int i = 0; i < max; i++)
                {
                    int index = rnd.Next(recipes.Count);
                    Add(recipes[index]);
                }
            }
            else
            {
                foreach (var recipe in recipes)
                {
                    Add(recipe);
                }
            }
        }
    }
}
