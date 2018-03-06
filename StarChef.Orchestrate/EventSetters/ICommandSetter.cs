using Fourth.StarChef.Invariables;

namespace StarChef.Orchestrate
{
    public interface ICommandSetter<T>
    {
        bool Set(T builder, UpdateMessage message);
    }
}