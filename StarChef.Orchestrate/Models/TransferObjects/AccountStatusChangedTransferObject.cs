namespace StarChef.Orchestrate.Models.TransferObjects
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class AccountStatusChangedTransferObject
    {
        public string ExternalLoginId { get; set; }
        public bool IsActive { get; set; }
    }
}
