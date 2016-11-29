namespace DataExported
{
    public class Constants
    {
        public enum EntityEnum
        {
            Menu,
            Group,
            Recipe,
            User,
            MealPeriod,
            Login
        };

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
            UserCreated = 13,
            UserUpdated = 14,
            UserActivated = 15,
            UserDeActivated = 16,
            SalesForceUserCreated = 17
        }
    }
}
