using StarChef.Common.Engine;
using StarChef.Engine.IntegrationTests;
using System.Linq;

namespace StarChef.Engine.Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            //var cnStr = "Initial Catalog=SCNET_Tish_Price_Test;Data Source=ie1scqaidb01.northeurope.cloudapp.azure.com;User ID=sl_web_user; Password=reddevil;";
            //var cnStr = "Initial Catalog=SCNET_trg;Data Source=ie1scqaidb01.northeurope.cloudapp.azure.com;User ID=sl_web_user; Password=reddevil;";
            //var cnStr = "Initial Catalog=SCNET_marstons;Data Source=ie1scqaidb01.northeurope.cloudapp.azure.com;User ID=sl_web_user; Password=reddevil;";
            //var cnStr = "Initial Catalog=SCNET_trg;Data Source=.\\sqlexpress;User ID=sl_web_user; Password=reddevil;";
            var cnStr = "Initial Catalog=SCNET_marstons;Data Source=.\\sqlexpress;User ID=sl_web_user; Password=reddevil;";

            //var t = new RecalculationTests(cnStr);
            //t.RecipePriceRecalculations();
            //return;

            Common.Repository.PricingRepository pr = new Common.Repository.PricingRepository(cnStr, 360);
            
            //var groupPrices2 = pr.GetGroupProductPricesByProduct(0, 455751, 0, 0, 0);
            //var prices = pr.GetPrices();
            IPriceEngine engine = new PriceEngine(pr);
            //var prices = engine.CalculatePrices(0, 152596, 0, 0, 0);
            //var prices = engine.CalculatePrices(0, 0, 0, 1, 0);
            //var prices = engine.CalculatePrices(0, 0, 0, 0, 369);
            var prices = engine.GlobalRecalculation(false, System.DateTime.UtcNow).Result;

            //var prices = engine.CalculatePrices(0, 0, 0, 0, 0).ToList();
            var taskPrices = pr.GetPrices().Result;
            var dbPrices = taskPrices.OrderBy(x => x.ProductId).ToList();
            
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var priceToUpdate = engine.ComparePrices(dbPrices, prices).ToList();
            var pricesToDelete = engine.ComparePrices(prices, dbPrices).ToList();

            sw.Stop();
            System.Diagnostics.Trace.WriteLine($"prices to update - {priceToUpdate.Count}");
            System.Diagnostics.Trace.WriteLine($"prices to delete - {pricesToDelete.Count}");
            System.Diagnostics.Trace.WriteLine($"Total Comparison time - {sw.Elapsed.TotalSeconds}");
        }
    }
}
