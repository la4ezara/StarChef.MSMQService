using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using StarChef.Listener.Commands;
using StarChef.Listener.Types;
using StarChef.Orchestrate.Models.TransferObjects;
using Xunit;

namespace StarChef.Listener.Tests.Types
{
    public class MessagingLoggerTests
    {
        [Fact(DisplayName = "Should set default error code")]
        public void Should_set_default_failed_code_when_error_code_null()
        {
            var dbCommands = new Mock<IDatabaseCommands>();
            var logger = new MessagingLogger(dbCommands.Object);

            var failedMessage = new AccountStatusChangeFailedTransferObject
            {
                Description = "any",
                ErrorCode = null,
                ExternalLoginId = "any"
            };
            logger.ReceivedFailedMessage(failedMessage, "1").Wait();

            dbCommands.Verify(m => m.RecordMessagingEvent(
                It.Is<string>(v => v == "1"),
                It.Is<bool>(v => v == true),
                It.Is<string>(v => v == "FAILED_MESSAGE"),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        }
    }
}
