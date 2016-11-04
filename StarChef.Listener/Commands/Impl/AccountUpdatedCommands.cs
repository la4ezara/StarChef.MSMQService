namespace StarChef.Listener.Commands.Impl
{
    class AccountUpdatedCommands : DatabaseCommands
    {
        public AccountUpdatedCommands(IConnectionStringProvider csProvider) : base(csProvider)
        {
        }

        protected override string SaveStoredProcedureName => "??";
    }
}