namespace StarChef.Orchestrate.Models.TransferObjects
{
    public class AccountStatusChangeFailedTransferObject : FailedTransferObject
    {
        public string ExternalLoginId { get; set; }
    }
}
