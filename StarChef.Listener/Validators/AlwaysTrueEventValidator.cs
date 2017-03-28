using System;

namespace StarChef.Listener.Types
{
    internal class AlwaysTrueEventValidator : IEventValidator

    {
        public string GetErrors()
        {
            return string.Empty;
        }

        public bool IsEnabled(object payload)
        {
            return true;
        }

        public bool IsStarChefEvent(object payload)
        {
            return true;
        }

        public bool IsValidPayload(object payload)
        {
            return true;
        }
    }
}