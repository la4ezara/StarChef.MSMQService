using StarChef.Engine.IntegrationTests.Model;
using System;
using System.Linq;

namespace StarChef.Engine.IntegrationTests.TheoryData
{

    public class IngredientRecalculateTheoryData : TheoryPriceData<PriceRecalculationRequest>
    {
        public IngredientRecalculateTheoryData()
        {
            CustomerDbRepository customerDbRepository = new CustomerDbRepository(TestConfiguration.Instance.ConnectionString, TestConfiguration.Instance.TimeOut);
            var items = customerDbRepository.GetIngredients().ToList();

            var max = TestConfiguration.Instance.MaxTestsAmount;
            if (max != 0 && max < items.Count)
            {
                Random rnd = new Random();
                for (int i = 0; i < max; i++)
                {
                    int index = rnd.Next(items.Count);
                    Add(items[index]);
                }
            }
            else
            {

                foreach (var item in items)
                {
                    Add(item);
                }
            }
        }
    }
}
