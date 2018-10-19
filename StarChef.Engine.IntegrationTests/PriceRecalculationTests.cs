using StarChef.Common.Engine;
using StarChef.Common.Repository;
using StarChef.Engine.IntegrationTests.Model;
using Xunit;
using System.Linq;
using Xunit.Abstractions;
using System.Collections.Generic;
using StarChef.Engine.IntegrationTests.TheoryData;

namespace StarChef.Engine.IntegrationTests
{
    

    


    


    public class PriceRecalculationTests : RecalculationTests
    {
        private readonly ITestOutputHelper output;

        public PriceRecalculationTests(ITestOutputHelper output) : base("Initial Catalog=SCNET_trg;Data Source=.\\sqlexpress;User ID=sl_web_user; Password=reddevil;") {
            this.output = output;
        }
            
        [Fact]
        public override void GlobalPriceRecalculation()
        {
            base.GlobalPriceRecalculation();
        }

        [Theory]
        [ClassData(typeof(RecipeRecalculateTheoryData))]
        public void RecipePriceRecalculationsTests(PriceRecalculationRequest request)
        {
            output.WriteLine(request.ToString());
            base.SinglePriceRecalculation(request);
        }

        [Theory]
        [ClassData(typeof(IngredientRecalculateTheoryData))]
        public void IngredientsPriceRecalculationsTests(PriceRecalculationRequest request)
        {
            output.WriteLine(request.ToString());
            base.SinglePriceRecalculation(request);
        }

        [Theory]
        [ClassData(typeof(GroupRecalculateTheoryData))]
        public void GroupPriceRecalculationsTests(PriceRecalculationRequest request)
        {
            output.WriteLine(request.ToString());
            base.SinglePriceRecalculation(request);
        }

        [Theory]
        [ClassData(typeof(SetRecalculateTheoryData))]
        public void SetPriceRecalculationsTests(PriceRecalculationRequest request)
        {
            output.WriteLine(request.ToString());
            base.SinglePriceRecalculation(request);
        }

        [Theory]
        [ClassData(typeof(PriceBandRecalculateTheoryData))]
        public void PriceBandPriceRecalculationsTests(PriceRecalculationRequest request)
        {
            output.WriteLine(request.ToString());
            base.SinglePriceRecalculation(request);
        }

        [Theory]
        [ClassData(typeof(UnitRecalculateTheoryData))]
        public void UnitPriceRecalculationsTests(PriceRecalculationRequest request)
        {
            output.WriteLine(request.ToString());
            base.SinglePriceRecalculation(request);
        }
    }
}
