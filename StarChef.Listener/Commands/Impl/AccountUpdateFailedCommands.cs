namespace StarChef.Listener.Commands.Impl
{
    class AccountUpdateFailedCommands : DatabaseCommands
    {
        public AccountUpdateFailedCommands(IConnectionStringProvider csProvider) : base(csProvider)
        {
        }

        protected override string SaveStoredProcedureName => "??";
    }
}