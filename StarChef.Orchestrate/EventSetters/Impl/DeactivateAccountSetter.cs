using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fourth.Orchestration.Model.People;
using StarChef.Common;
using DeactivateAccountBuilder = Fourth.Orchestration.Model.People.Commands.DeactivateAccount.Builder;
using StarChef.MSMQService;

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
