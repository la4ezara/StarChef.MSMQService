using System.Reflection;
using Fourth.Orchestration.Messaging;
using StarChef.Listener.Commands.Impl;
using StarChef.Listener.Exceptions;
using StarChef.Listener.Handlers;
using StarChef.Listener.Types;
using PriceBandUpdated = Fourth.Orchestration.Model.Recipes.Events.PriceBandUpdated;
using AccountCreated = Fourth.Orchestration.Model.People.Events.AccountCreated;
using AccountCreateFailed = Fourth.Orchestration.Model.People.Events.AccountCreateFailed;
using AccountUpdated = Fourth.Orchestration.Model.People.Events.AccountUpdated;
using AccountUpdateFailed = Fourth.Orchestration.Model.People.Events.AccountUpdateFailed;
using AccountStatusChanged = Fourth.Orchestration.Model.People.Events.AccountStatusChanged;
using AccountStatusChangeFailed = Fourth.Orchestration.Model.People.Events.AccountStatusChangeFailed;
using FileUploadCompleted = Fourth.Orchestration.Model.StarChef.Events.FileUploadCompleted;

using System.Threading.Tasks;
using log4net;
using StarChef.Orchestrate.Models.TransferObjects;
using MSMQHelper = StarChef.MSMQService.MSMQHelper;
using StarChef.Listener.Extensions;
using StarChef.Listener.Validators;
using System;
using Fourth.StarChef.Invariables;
using System.Configuration;

namespace StarChef.Listener
{
    class MessagingHandlersFactory : IMessagingHandlersFactory
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <exception cref="NotSupportedMessageException">Raised when message type is not supported</exception>
        public IMessageHandler CreateHandler<T>()
        {
            var configuration = new AppConfigConfiguration();
            var csProvider = new ConnectionStringProvider();
            var dbCommands = new DatabaseCommands(csProvider, configuration);
            var messagingLogger = new MessagingLogger(dbCommands);

            if (typeof(T) == typeof(PriceBandUpdated))
            {
                var validator = new PriceBandUpdatedValidator(dbCommands);
                return new PriceBandEventHandler(dbCommands, validator, messagingLogger, configuration);
            }

            if (typeof(T) == typeof(AccountCreated))
            {
                var validator = new AccountCreatedValidator(dbCommands);
                var handler = new AccountCreatedEventHandler(dbCommands, validator, configuration, messagingLogger);
                handler.OnProcessed += SendMsmqMessage;
                return handler;
            }

            if (typeof(T) == typeof(AccountCreateFailed))
            {
                var validator = new AccountCreateFailedValidator(dbCommands);
                return new AccountCreateFailedEventHandler(dbCommands, validator, messagingLogger);
            }

            if (typeof(T) == typeof(AccountUpdated))
            {
                var validator = new AccountUpdatedValidator(dbCommands);
                return new AccountUpdatedEventHandler(dbCommands, validator, messagingLogger);
            }

            if (typeof(T) == typeof(AccountUpdateFailed))
            {
                var validator = new AccountUpdateFailedValidator(dbCommands);
                return new AccountUpdateFailedEventHandler(dbCommands, validator, messagingLogger);
            }
            if (typeof (T) == typeof (AccountStatusChanged))
            {
                var validator = new AccountStatusChangedValidator(dbCommands);
                return new AccountStatusChangedEventHandler(dbCommands, validator, messagingLogger);
            }
            if (typeof(T) == typeof(AccountStatusChangeFailed))
            {
                var validator = new AccountStatusChangeFailedValidator(dbCommands);
                return new AccountStatusChangeFailedEventHandler(dbCommands, validator, messagingLogger);
            }
            if (typeof(T) == typeof(FileUploadCompleted))
            {
                var validator = new FileUploadEventValidator(dbCommands);
                return new FileUploadCompletedHandler(dbCommands, validator, messagingLogger);
            }
            
            throw new NotSupportedMessageException(string.Format("Message type {0} is not supported.", typeof(T).Name));
        }

        private async Task SendMsmqMessage(AccountCreatedEventHandler sender, AccountCreatedTransferObject user, IConfiguration config)
        {
            if (sender == null) return;

            try
            {
                var userDetail = await sender.DbCommands.GetLoginUserIdAndCustomerDb(user.LoginId);

                ThreadContext.Properties["OrganisationId"] = userDetail.Item2;

                var msg = new UpdateMessage(productId: userDetail.Item1,
                                            entityTypeId: (int)Constants.EntityType.User,
                                            action: (int)Constants.MessageActionType.SalesForceUserCreated,
                                            dbDSN: userDetail.Item3,
                                            databaseId: userDetail.Item2);
                MSMQHelper.Send(msg, config.NormalQueueName, string.Empty);
                _logger.MessageSent(msg);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
            finally
            {
                ThreadContext.Properties.Remove("OrganisationId");
            }
        }
    }
}
