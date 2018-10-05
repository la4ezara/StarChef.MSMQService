using StarChef.Common.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarChef.Engine.Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            var cnStr = "Initial Catalog=SCNET_Tish_Price_Test;Data Source=ie1scqaidb01.northeurope.cloudapp.azure.com;User ID=sl_web_user; Password=reddevil;";
            Common.Repository.PricingRepository pr = new Common.Repository.PricingRepository(cnStr, 360);
            //var dishes = pr.GetDishes();
            //var ingredients = pr.GetIngredients();
            //var products = pr.GetProducts();
            //var productParts = pr.GetProductParts();
            //var groupPrices = pr.GetGroupProductPricesByGroup(0);
            //var groupPrices = pr.GetGroupProductPricesByGroup(107);
            //var groupPrices2 = pr.GetGroupProductPricesByProduct(0, 455751, 0, 0, 0);
            //var prices = pr.GetPrices();
            IPriceEngine engine = new PriceEngine(pr);
            //engine.CalculatePrices(0, 1, 0, 0, 0, DateTime.Now);

            engine.CalculatePrices(0, 0, 0, 0, 0, DateTime.Now);
        }
    }
}
