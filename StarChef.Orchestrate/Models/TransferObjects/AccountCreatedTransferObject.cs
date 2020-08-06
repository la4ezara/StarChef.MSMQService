using System.Collections.Generic;

namespace StarChef.Orchestrate.Models.TransferObjects
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class AccountCreatedTransferObject
    {
        public int InternalLoginId { get; set; }
        public string ExternalLoginId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string ExternalCustomerId { get; set; }
        public IEnumerable<string> PermissionSets { get; set; }
        public string CustomerCanonicallId { get; set; }
        public string InternalId { get; set; }
    }
}