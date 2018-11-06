﻿using StarChef.Engine.IntegrationTests.TheoryData;
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

        [Theory]
        [ClassData(typeof(GlobalRecalculateTheoryData))]
        public void PriceRecalculationStorage(string connStr)
        {
            output.WriteLine($"Conn str : {connStr}");
            var rt = new RecalculationTests(connStr);
            rt.GlobalPriceRecalculationStorage(output);
        }

        [Theory]
        [ClassData(typeof(GlobalRecalculateTheoryData))]
        public void PriceRecalculationNoStorage(string connStr)
        {
            output.WriteLine($"Conn str : {connStr}");
            var rt = new RecalculationTests(connStr);
            rt.GlobalPriceRecalculationNoStorage(output);
        }
    }
}
