using Google.ProtocolBuffers;
using StarChef.MSMQService;

namespace StarChef.Orchestrate
{
    public interface ICommandFactory
    {
        TCommand CreateCommand<TCommand, TBuilder>(UpdateMessage message)
            where TCommand : GeneratedMessage<TCommand, TBuilder>
            where TBuilder : GeneratedBuilder<TCommand, TBuilder>, new();
    }
}