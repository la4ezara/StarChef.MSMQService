using StarChef.Listener.Configuration;
using StarChef.Listener.Tests.Configuration.Handlers;
using Xunit;

namespace StarChef.Listener.Tests.Configuration
{
    public class AzureListenersSectionTests
    {
        [Fact(DisplayName = "Should load configuration with single listener and handler")]
        public void Should_read_configuration_section_with_one_item()
        {
            var config = AzureListenersSection.GetConfiguration("simple");
            Assert.Equal(1, config.Count);
            Assert.Equal(typeof(FirstHandlerType), config["subscription1"][0]);
        }

        [Fact(DisplayName = "Should load configuration with several handlers for the same listener")]
        public void Should_read_configuration_section_with_several_handlers()
        {
            var config = AzureListenersSection.GetConfiguration("twoHandlers");
            Assert.Equal(1, config.Count);
            Assert.Equal(typeof(FirstHandlerType), config["subscription1"][0]);
            Assert.Equal(typeof(SecondHandlerType), config["subscription1"][1]);
        }

        [Fact(DisplayName = "Should load configuration with several listener/handlers")]
        public void Should_read_configuration_section_with_several_subscriptions()
        {
            var config = AzureListenersSection.GetConfiguration("twoSubscriptions");
            Assert.Equal(2, config.Count);
            Assert.Equal(typeof(FirstHandlerType), config["subscription1"][0]);
            Assert.Equal(typeof(SecondHandlerType), config["subscription2"][0]);
        }
    }
}
