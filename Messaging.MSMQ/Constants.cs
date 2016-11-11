namespace Messaging.MSMQ
{
    public class Constants
    {
        public enum MessageActionType
        {
            NoMessage = -1,
            UpdatedUserDefinedUnit = 0,
            UpdatedProductSet = 1,
            UpdatedPriceBand = 2,
            UpdatedGroup = 3,
            UpdatedProductCost = 4,
            GlobalUpdate = 5,
            UpdatedProductNutrient = 6,
            UpdatedProductIntolerance = 7,
            UpdatedProductNutrientInclusive = 8,
            GlobalUpdateBudgeted = 9,
            UpdateAlternateIngredients = 10,
            CreatePackage = 11,
            StarChefEventsUpdated = 12,
            UpdatedUser = 13
        }
    }
}
