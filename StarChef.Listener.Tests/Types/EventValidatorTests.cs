using StarChef.Listener.Types;
using Xunit;
using AccountCreated = Fourth.Orchestration.Model.People.Events.AccountCreated;
using SourceSystem = Fourth.Orchestration.Model.People.Events.SourceSystem;

namespace StarChef.Listener.Tests.Types
{
    public class EventValidatorTests
    {
        class TestEventValidator : EventValidator
        {
            
        }

        [Fact]
        public void Should_return_true_if_starchef_event()
        {
            var builder = AccountCreated.CreateBuilder();
            builder
                .SetExternalId("1")
                .SetSource(SourceSystem.STARCHEF);
            var accountCreated = builder.Build();

            var validator = new TestEventValidator();
            var actual = validator.IsStarChefEvent(accountCreated);

            Assert.True(actual);
        }

        [Fact]
        public void Should_return_false_if_nonStarchef_event()
        {
            var builder = AccountCreated.CreateBuilder();
            builder
                .SetExternalId("1")
                .SetSource(SourceSystem.ADACO);
            var accountCreated = builder.Build();

            var validator = new TestEventValidator();
            var actual = validator.IsStarChefEvent(accountCreated);

            Assert.False(actual);
        }



        [Fact]
        public void Should_return_false_if_source_is_not_set_for_event()
        {
            var builder = AccountCreated.CreateBuilder();
            builder
                .SetExternalId("1");
            var accountCreated = builder.Build();

            var validator = new TestEventValidator();
            var actual = validator.IsStarChefEvent(accountCreated);

            Assert.False(actual);
        }
    }
}