using Fourth.Orchestration.Model.People;

namespace StarChef.Listener
{
    public interface IEventValidator
    {
        string GetErrors();
        bool IsStarChefEvent(object payload);
        bool IsValidPayload(object payload);
        bool IsEnabled(object payload);
    }
}