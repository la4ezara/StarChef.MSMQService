using Fourth.StarChef.Invariables;
using Moq;
using Xunit;
using DeactivateAccount = Fourth.Orchestration.Model.People.Commands.DeactivateAccount;
using DeactivateAccountBuilder = Fourth.Orchestration.Model.People.Commands.DeactivateAccount.Builder;
using SourceSystem = Fourth.Orchestration.Model.People.Commands.SourceSystem;

namespace StarChef.Orchestrate.Tests
{
    public class CommandFactoryTests
    {
        [Fact]
        public void Should_create_command_for_DeactivateAccount()
        {
            var message = new UpdateMessage();
            var deactivateAccountSetter = new Mock<ICommandSetter<DeactivateAccountBuilder>>();
            deactivateAccountSetter
                .Setup(m => m.Set(It.IsAny<DeactivateAccountBuilder>(), It.IsAny<UpdateMessage>()))
                .Callback<DeactivateAccountBuilder, UpdateMessage>(SetMandatoryFields);

            var commandFactory = new CommandFactory(deactivateAccountSetter.Object);

            var command = commandFactory.CreateCommand<DeactivateAccount, DeactivateAccountBuilder>(message);

            long cmdId;
            Assert.Equal(SourceSystem.STARCHEF, command.Source);
            Assert.True(long.TryParse(command.CommandId, out cmdId), "CommandId should be initialized with Long");
        }

        private void SetMandatoryFields(dynamic builder, UpdateMessage msg)
        {
            builder.SetExternalId("any");
        }
    }
}
