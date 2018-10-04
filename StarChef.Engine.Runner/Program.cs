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
            var cnStr = "Initial Catalog=SCNET_trg;Data Source=.\\sqlexpress;User ID=sl_web_user; Password=reddevil;";
            Common.Repository.PricingRepository pr = new Common.Repository.PricingRepository(cnStr, 360);
            //var dishes = pr.GetDishes();
            //var ingredients = pr.GetIngredients();
            //var products = pr.GetProducts();
            //var productParts = pr.GetProductParts();
            //var groupPrices = pr.GetGroupProductPricesByGroup(0);
            var groupPrices = pr.GetGroupProductPricesByGroup(107);
            //var groupPrices2 = pr.GetGroupProductPricesByProduct(0, 455751, 0, 0, 0);
            //var prices = pr.GetPrices();

        }
    }
}
