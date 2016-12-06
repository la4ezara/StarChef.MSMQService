﻿using System.Reflection;
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
using System.Threading.Tasks;
using log4net;
using StarChef.Data;
using StarChef.Orchestrate.Models.TransferObjects;
using MSMQHelper = StarChef.MSMQService.MSMQHelper;
using UpdateMessage = StarChef.MSMQService.UpdateMessage;
using StarChef.Listener.Extensions;
using StarChef.Listener.Validators;
using System;

namespace StarChef.Listener
{
    class MessagingHandlersFactory : IMessagingHandlersFactory
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <exception cref="NotSupportedMessageException">Raised when message type is not supported</exception>
        public IMessageHandler CreateHandler<T>()
        {
            var csProvider = new ConnectionStringProvider();
            var dbCommands = new DatabaseCommands(csProvider);
            var messagingLogger = new MessagingLogger(dbCommands);

            if (typeof(T) == typeof(PriceBandUpdated))
            {
                var validator = new AlwaysTrueEventValidator();
                return new PriceBandEventHandler(dbCommands, validator, messagingLogger);
            }

            if (typeof(T) == typeof(AccountCreated))
            {
                var validator = new AccountCreatedValidator();
                var handler = new AccountCreatedEventHandler(dbCommands, validator, messagingLogger);
                handler.OnProcessed += SendMsmqMessage;
                return handler;
            }

            if (typeof(T) == typeof(AccountCreateFailed))
            {
                var validator = new AccountCreateFailedValidator();
                return new AccountCreateFailedEventHandler(dbCommands, validator, messagingLogger);
            }

            if (typeof(T) == typeof(AccountUpdated))
            {
                var validator = new AccountUpdatedValidator();
                return new AccountUpdatedEventHandler(dbCommands, validator, messagingLogger);
            }

            if (typeof(T) == typeof(AccountUpdateFailed))
            {
                var validator = new AccountUpdateFailedValidator();
                return new AccountUpdateFailedEventHandler(dbCommands, validator, messagingLogger);
            }
            if (typeof (T) == typeof (AccountStatusChanged))
            {
                var validator = new AccountStatusChangedValidator();
                return new AccountStatusChangedEventHandler(dbCommands, validator, messagingLogger);
            }
            if (typeof(T) == typeof(AccountStatusChangeFailed))
            {
                var validator = new AccountStatusChangeFailedValidator();
                return new AccountStatusChangeFailedEventHandler(dbCommands, validator, messagingLogger);
            }

            throw new NotSupportedMessageException(string.Format("Message type {0} is not supported.", typeof(T).Name));
        }

        private async Task SendMsmqMessage(AccountCreatedEventHandler sender, AccountCreatedTransferObject user)
        {
            if (sender == null) return;

            try
            {
                var userDetail = await sender.DbCommands.GetLoginUserIdAndCustomerDb(user.LoginId);

                var msg = new UpdateMessage(productId: userDetail.Item1,
                                            entityTypeId: (int)Constants.EntityType.User,
                                            action: (int)Constants.MessageActionType.SalesForceUserCreated,
                                            dbDSN: userDetail.Item3,
                                            databaseId: userDetail.Item2);
                MSMQHelper.Send(msg);
                _logger.MessageSent(msg);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
        }
    }
}