using Fourth.StarChef.Invariables;
using DeactivateAccountBuilder = Fourth.Orchestration.Model.People.Commands.DeactivateAccount.Builder;

namespace StarChef.Orchestrate.EventSetters.Impl
{
    public class DeactivateAccountSetter : ICommandSetter<DeactivateAccountBuilder>
    {
        public bool Set(DeactivateAccountBuilder builder, UpdateMessage message)
        {
            if (builder == null) return false;

            builder.SetExternalId(message.ExternalId);

            return true;
        }
    }
}
