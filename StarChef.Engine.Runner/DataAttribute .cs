using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace StarChef.Engine.Runner
{
    public class ProductDataAttribute : Xunit.Sdk.DataAttribute
    {
        private readonly CustomerDbRepository _customerDbRepository;

        public ProductDataAttribute(string connectionString) {
            _customerDbRepository = new CustomerDbRepository(connectionString);
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (testMethod == null) { throw new ArgumentNullException(nameof(testMethod)); }
            var products = _customerDbRepository.GetProducts().ToList();

            var result = new List<object[]>();
            products.ForEach(p => result.Add(new object[] { p }));
            return result;
        }
    }
}
