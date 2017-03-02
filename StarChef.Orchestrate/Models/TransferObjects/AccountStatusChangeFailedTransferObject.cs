using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarChef.Orchestrate.Models.TransferObjects
{
    public class AccountStatusChangeFailedTransferObject : FailedTransferObject
    {
        public string ExternalLoginId { get; set; }
    }
}
