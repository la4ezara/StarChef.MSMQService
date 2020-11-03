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
using StarChef.Listener.Validators;
using System;
using Fourth.StarChef.Invariables;
using StarChef.Common;
using System.Data.SqlClient;

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
                handler.OnProcessed += SendMessage;
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

        private async Task SendMessage(AccountCreatedEventHandler sender, AccountCreatedTransferObject user, IConfiguration config)
        {
            if (sender == null) return;

            try
            {
                var userDetail = await sender.DbCommands.GetLoginUserIdAndCustomerDb(user.InternalLoginId);

                ThreadContext.Properties["OrganisationId"] = userDetail.Item2;
                var db = new DatabaseManager();
                
                var publishEnabled = db.IsPublishEnabled(userDetail.Item3, (int)Constants.EntityType.User);
                if (publishEnabled) {

                    SqlParameter[] parameters = new SqlParameter[] {
                        new SqlParameter("@EntityId", userDetail.Item1),
                        new SqlParameter("@EntityTypeId", (int)Constants.EntityType.User),
                        new SqlParameter("@RetryCount", 0) { Value = 0, DbType = System.Data.DbType.Int32 },
                        new SqlParameter("@StatusId", 1),
                        new SqlParameter("@DateCreated", DateTime.UtcNow),
                        new SqlParameter("@ExternalId", string.Empty),
                        new SqlParameter("@MessageActionTypeId", (int)Constants.MessageActionType.SalesForceUserCreated) };

                    db.Execute(userDetail.Item3, "sc_calculation_enqueue", Constants.TIMEOUT_MSMQ_EXEC_STOREDPROC, true, parameters);
                }
                _logger.InfoFormat("Enque userId: {0}, databaseId: {1}", userDetail.Item1, userDetail.Item2);
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
