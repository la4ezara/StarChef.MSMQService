namespace StarChef.Orchestrate.Models.TransferObjects
{
    public class AccountStatusChangedTransferObject
    {
        public string ExternalLoginId { get; set; }
        public bool IsActive { get; set; }
    }
}
