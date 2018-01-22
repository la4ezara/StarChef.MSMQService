namespace StarChef.Common
{
    public class EnumHelper
    {
        public enum EntityTypeWrapper
        {
            Recipe = 0,
            Menu,
            Ingredient,
            User,
            UserGroup,
            MealPeriod,
            Group,
            UserCreated,
            SendUserUpdatedEventAndCommand,
            UserActivated,
            UserDeactivated,
            SendUserUpdatedEvent,
            SendSupplierUpdatedEvent,
            ProductSet
        }
    }
}