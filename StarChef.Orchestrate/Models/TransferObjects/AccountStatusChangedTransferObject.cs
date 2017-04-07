using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarChef.Orchestrate.Models.TransferObjects
{
    public class AccountStatusChangedTransferObject
    {
        public string ExternalLoginId { get; set; }
        public bool IsActive { get; set; }
    }
}
