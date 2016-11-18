namespace StarChef.MSMQService.Constants
{
    public enum EntityType
    {
        NotSet = -1,
        User = 10,
        UserGroup = 11,
        Group = 12,
        Ingredient = 20,
        Dish = 21,
        Menu = 22,
        Supplier = 23,
        UserUnit = 24,
        Category = 25,
        SuppliedDish = 26,
        Package = 29,
        IngredientCostPrice = 32,
        DishDetails = 35,
        DishPricing = 36,
        DishAdvancedNutrition = 38,
        Manufacturer = 40,
        Nutrition = 50,
        //			MenuSellPrice = 101,
        //			MenuWastagePercent = 102,
        GroupFilter = 120,
        //			Reports = 130,
        BasicReports = 130,
        StandardReports = 131,
        AdvancedReports = 132,
        AdminReports = 133,
        PriceBand = 135,
        PriceBandOverride = 136,
        UserPreferences = 137,
        MenuCycle = 138,
        UserLogin = 139,
        ListManager = 140,
        ProductSet = 170,
        MenuSet = 171,
        MenuPeriod = 172,
        IngredientCostManagement = 210,
        GlobalSearchReplace = 219,
        GroupManagement = 220,
        DbSettings = 221,
        DataManagement = 222,
        PricingManagement = 223,
        SystemDelete = 224,
        AllowReplaceRemoveRecs = 225,
        PriceBandManagement = 227,
        DlineLookup = 228,
        MasterListExportMangement = 229,
        InterfaceManagerManagement = 230,
        BudgetedCostManagement = 240,
        ProductSpecificationManagement = 250,
        IngredientImport = 251,
        SetManagement = 252,
        StarChefLiteManagement = 255,
        Document = 28,
        MealPeriodManagement = 253
    }
}