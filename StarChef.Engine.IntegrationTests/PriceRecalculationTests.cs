using StarChef.Engine.IntegrationTests.TheoryData;
using Xunit;
using Xunit.Abstractions;

namespace StarChef.Engine.IntegrationTests
{
    public class PriceRecalculationTests : RecalculationTests
    {
        private readonly ITestOutputHelper output;

        public PriceRecalculationTests(ITestOutputHelper output) : base() {
            this.output = output;
        }
            
        //[Fact]
        //public void GlobalPriceRecalculation()
        //{
        //    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        //    sw.Start();
        //    base.GlobalPriceRecalculation(output);
        //    sw.Stop();
        //    output.WriteLine($"Total time: {sw.Elapsed.TotalSeconds}");

        //}

        [Theory]
        [ClassData(typeof(GlobalRecalculateTheoryData))]
        public void PriceRecalculation(string connStr) {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            output.WriteLine($"Conn str : {connStr}");
            var rt = new RecalculationTests(connStr);
            rt.GlobalPriceRecalculation(output);
            sw.Stop();
            output.WriteLine($"Total time: {sw.Elapsed.TotalSeconds}");
            sw.Reset();
        }

        //[Fact]
        //public void AllGlobalPriceRecalculation() {
        //    var cnStr = TestConfiguration.Instance.SlLoginConnectionString;
        //    SlLoginDbRepository repo = new SlLoginDbRepository(cnStr);
        //    var connectionStrings = repo.GetConnectionStrings();

        //    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        //    foreach (var connStr in connectionStrings) {
        //        sw.Start();
        //        output.WriteLine($"Conn str : {connStr}");
        //        var rt = new RecalculationTests(connStr);
        //        rt.GlobalPriceRecalculation();
        //        sw.Stop();
        //        output.WriteLine($"Total time: {sw.Elapsed.TotalSeconds}");
        //        sw.Reset();
        //    }
        //}
    }
}
