using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarChef.Orchestrate.Models.TransferObjects
{
    public class OperationFailedTransferObject
    {
        public string UserId { get; set; }
        public string ErrorCode { get; set; }
        public string Description { get; set; }
    }
}
