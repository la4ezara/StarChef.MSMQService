namespace StarChef.Listener.Commands.Impl
{
    class PriceBandCommands : DatabaseCommands
    {
        public PriceBandCommands(IConnectionStringProvider csProvider) : base(csProvider)
        {
        }

        protected override string SaveStoredProcedureName => "sc_save_product_price_band_list";
    }
}