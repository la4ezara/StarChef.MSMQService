using StarChef.Engine.IntegrationTests.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StarChef.Engine.IntegrationTests.TheoryData
{

    public class UnitRecalculateTheoryData : TheoryPriceData<PriceRecalculationRequest>
    {
        public UnitRecalculateTheoryData()
        {
            CustomerDbRepository customerDbRepository = new CustomerDbRepository(TestConfiguration.Instance.ConnectionString, TestConfiguration.Instance.TimeOut);
            var items = customerDbRepository.GetUnits().ToList();
            //List<int> excludeItems = new List<int>() { 1, 110, 236, 149 };
            //List<int> excludeItems = new List<int>();
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
                    //if(!excludeItems.Contains(item.GroupId))
                        Add(item);
                }
            }
        }
    }
}
