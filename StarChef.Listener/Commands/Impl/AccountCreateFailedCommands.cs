namespace StarChef.Listener.Commands.Impl
{
    class AccountCreateFailedCommands : DatabaseCommands
    {
        public AccountCreateFailedCommands(IConnectionStringProvider csProvider) : base(csProvider)
        {
        }

        protected override string SaveStoredProcedureName => "??";
    }
}