
namespace StarChef.Orchestrate.Models
{
    public class IngredientSuppliedPackSize
    {
        public bool IsDefault{ get; set; }
        public bool IsPreferred{ get; set; }
        public string DistributorName{ get; set; }
        public string DistributorCode{ get; set; }
        public string DistributorUom{ get; set; }
        public double SupplyQuantityNumber{ get; set; }
        public double SupplyQuantityQuantity{ get; set; }
        public string SupplyQuantityUom{ get; set; }
        public string SupplyQuantityPackDescription{ get; set; }
        public double CostPrice{ get; set; }
        public double CostPricePerItem{ get; set; }
        public string CurrencyIso4217Code{ get; set; }
        public bool IsOrderable{ get; set; }
        public long MaxOrderQuantity{ get; set; }
        public long MinOrderQuantity{ get; set; }
        public string CreatedUserFirstName{ get; set; }
        public string CreatedUserLastName{ get; set; }
        public long CaptureDate { get; set; }
        public string ModifiedUserFirstName{ get; set; }
        public string ModifiedUserLastName{ get; set; }
        public long ModifiedDate{ get; set; }
        public string RetailBarCodeDetails{ get; set; }
        public long RankOrder{ get; set; }
        public int ProductTagId{ get; set; }
        //repeated CategoryType CategoryTypes = 24{ get; set; }
    }
}
