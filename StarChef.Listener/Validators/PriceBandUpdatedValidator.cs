﻿using StarChef.Listener.Types;
using System;
using System.Reflection;
using log4net;
using StarChef.Listener.Commands;
using PriceBandUpdated = Fourth.Orchestration.Model.Recipes.Events.PriceBandUpdated;

namespace StarChef.Listener.Validators
{
    internal class PriceBandUpdatedValidator : EventValidator, IEventValidator
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public PriceBandUpdatedValidator(IDatabaseCommands databaseCommands) : base(databaseCommands)
        {
        }

        public override bool IsEnabled(object payload)
        {
            var e = payload as PriceBandUpdated;
            if (e == null)
                throw new ArgumentException("The type of the payload is not supported");

            return GetFromDbConfiguration(Guid.Parse(e.CustomerId), typeof (PriceBandUpdated).Name);
        }

        public bool IsValidPayload(object payload)
        {
            _logger.Info("Validating the payload");

            if (payload == null) return false;
            if (payload.GetType() != typeof(PriceBandUpdated)) return false;
            var e = (PriceBandUpdated)payload;

            if (!e.HasCustomerId)
            {
                SetLastError("Customer id is missing");
                return false;
            }

            _logger.Info("Payload is valid");
            return true;
        }
    }
}
