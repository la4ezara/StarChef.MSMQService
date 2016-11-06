using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarChef.Orchestrate.Models.TransferObjects
{
    public class UserTransferObject
    {
        public int Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
    }
}
