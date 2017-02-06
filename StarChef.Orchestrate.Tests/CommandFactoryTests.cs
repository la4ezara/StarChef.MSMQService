using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using StarChef.MSMQService;
using Xunit;

using SourceSystem = Fourth.Orchestration.Model.People.Commands.SourceSystem;

using CreateAccount = Fourth.Orchestration.Model.People.Commands.CreateAccount;
using UpdateAccount = Fourth.Orchestration.Model.People.Commands.UpdateAccount;
using ActivateAccount = Fourth.Orchestration.Model.People.Commands.ActivateAccount;
using DeactivateAccount = Fourth.Orchestration.Model.People.Commands.DeactivateAccount;

using CreateAccountBuilder = Fourth.Orchestration.Model.People.Commands.CreateAccount.Builder;
using UpdateAccountBuilder = Fourth.Orchestration.Model.People.Commands.UpdateAccount.Builder;
using ActivateAccountBuilder = Fourth.Orchestration.Model.People.Commands.ActivateAccount.Builder;
using DeactivateAccountBuilder = Fourth.Orchestration.Model.People.Commands.DeactivateAccount.Builder;

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
