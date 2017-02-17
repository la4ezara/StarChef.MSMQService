using StarChef.Listener.Types;
using System;
using StarChef.Listener.Commands;
using PriceBandUpdated = Fourth.Orchestration.Model.Recipes.Events.PriceBandUpdated;

namespace StarChef.Listener.Validators
{
    internal class PriceBandUpdatedValidator : EventValidator, IEventValidator
    {
        private readonly IDatabaseCommands _databaseCommands;

        public PriceBandUpdatedValidator(IDatabaseCommands databaseCommands)
        {
            _databaseCommands = databaseCommands;
        }

        public bool IsEnabled(object payload)
        {
            var e = payload as PriceBandUpdated;
            if (e == null)
                throw new ArgumentException("The type of the payload is not supported");

            var organizationGuid = Guid.Parse(e.CustomerId);
            var isEnabledTask = _databaseCommands.IsEventEnabledForOrganization(typeof(PriceBandUpdated).Name, organizationGuid);
            isEnabledTask.Wait();
            return isEnabledTask.Result;
        }

        public override bool IsStarChefEvent(object payload)
        {
            return true;
        }

        public bool IsValid(object payload)
        {
            if (payload == null) return false;
            if (payload.GetType() != typeof(PriceBandUpdated)) return false;
            var e = (PriceBandUpdated)payload;

            if (!e.HasCustomerId)
            {
                SetLastError("Customer id is missing");
                return false;
            }

            return true;
        }
    }
}
