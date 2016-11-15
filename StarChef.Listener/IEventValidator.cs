using Fourth.Orchestration.Model.People;

namespace StarChef.Listener
{
    public interface IEventValidator
    {
        string GetErrors();
        bool IsStarChefEvent(object payload);
        bool IsValid(object payload);
    }
}