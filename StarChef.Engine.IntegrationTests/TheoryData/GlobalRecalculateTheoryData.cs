using System;
using System.Linq;

namespace StarChef.Engine.IntegrationTests.TheoryData
{

    public class GlobalRecalculateTheoryData : TheoryPriceData<string>
    {
        public GlobalRecalculateTheoryData()
        {
            SlLoginDbRepository repo = new SlLoginDbRepository(TestConfiguration.Instance.SlLoginConnectionString);
            var items = repo.GetConnectionStrings().OrderBy(c=> c).ToList();

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
