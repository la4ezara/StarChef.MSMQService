namespace StarChef.Listener.Types
{
    class PriceBandCustomerDb : CustomerDbFacade
    {
        public PriceBandCustomerDb(string loginDbConnectionString) : base(loginDbConnectionString)
        {
        }

        protected override string SaveStoredProcedureName => "sc_save_product_price_band_list";
    }
}