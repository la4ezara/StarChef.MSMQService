namespace StarChef.Orchestrate.Models.TransferObjects
{
    public class AccountUpdateFailedTransferObject : FailedTransferObject
    {
        public string ExternalLoginId { get; set; }
    }
}