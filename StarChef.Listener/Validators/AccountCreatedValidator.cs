﻿using System;
using StarChef.Listener.Commands;
using StarChef.Listener.Types;
using AccountCreated = Fourth.Orchestration.Model.People.Events.AccountCreated;

namespace StarChef.Listener.Validators
{
    class AccountCreatedValidator : EventValidator, IEventValidator
    {
        public AccountCreatedValidator(IDatabaseCommands databaseCommands) : base(databaseCommands)
        {
        }

        public bool IsValid(object payload)
        {
            if (payload == null) return false;
            if (payload.GetType() != typeof(AccountCreated)) return false;
            var e = (AccountCreated) payload;

            if (!e.HasInternalId)
            {
                SetLastError("InternalId is missing");
                return false;
            }
            if (!e.HasExternalId)
            {
                SetLastError("ExternalId is missing");
                return false;
            }
            if (!e.HasFirstName)
            {
                SetLastError("FirstName is missing");
                return false;
            }
            if (!e.HasLastName)
            {
                SetLastError("LastName is missing");
                return false;
            }
            if (!e.HasEmailAddress)
            {
                SetLastError("EmailAddress is missing");
                return false;
            }
            return true;
        }
    }
}