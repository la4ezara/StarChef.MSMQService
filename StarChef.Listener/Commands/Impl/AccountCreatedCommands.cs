namespace StarChef.Listener.Commands.Impl
{
    class AccountCreatedCommands: DatabaseCommands
    {
        public AccountCreatedCommands(IConnectionStringProvider csProvider) : base(csProvider)
        {
        }

        protected override string SaveStoredProcedureName => "??";
    }
}