namespace StarChef.Listener.Types
{
    internal class AlwaysTrueEventValidator : IEventValidator

    {
        public string GetErrors()
        {
            return string.Empty;
        }

        public bool IsStarChefEvent(object payload)
        {
            return true;
        }

        public bool IsValid(object payload)
        {
            return true;
        }
    }
}