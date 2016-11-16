namespace StarChef.Orchestrate.Models.TransferObjects
{
    public class AccountCreatedTransferObject
    {
        public int LoginId { get; set; }
        public string ExternalLoginId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
    }
}