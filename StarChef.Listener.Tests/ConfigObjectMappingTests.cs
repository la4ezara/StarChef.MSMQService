using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Xunit;

namespace StarChef.Listener.Tests
{
    public class ConfigObjectMappingTests
    {
        [Fact]
        public void Should_correctly_initialize_mapper()
        {
            ConfigObjectMapping.Init();

            // the assertion method which will fail if the configuration is not correct
            Mapper.AssertConfigurationIsValid();
        }
    }
}
