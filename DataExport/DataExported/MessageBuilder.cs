using Messaging.MSMQ.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Messaging.MSMQ;
using static DataExported.Constants;
using System.Linq;

namespace DataExported
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
        Document = 28,
        Package = 29,
        IngredientCostPrice = 32,
        DishDetails = 35,
        DishPricing = 36,
        DishAdvancedNutrition = 38,
        Manufacturer = 40,
        Nutrition = 50,
        GroupFilter = 120,
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
        MealPeriodManagement = 253,
        StarChefLiteManagement = 255
    }

    public class MessageBuilder
    {
        private string dbDSN;
        private int databaseId;
        public MessageBuilder()
        {

        }
        public MessageBuilder(string dsn, int databaseId)
        {
            this.dbDSN = dsn;
            this.databaseId = databaseId;
        }

        private IList<IMessage> CreateMessage(
            EntityType entityType, 
            IEnumerable<DataRow> rows
            )
        {
            IList<IMessage> messages = new List<IMessage>();

            foreach (var row in rows)
            {
                var entityId = Convert.ToInt32(row[0]);

                messages.Add(new UpdateMessage(entityId,
                                            this.dbDSN,
                                            (int)Constants.MessageActionType.StarChefEventsUpdated,
                                            this.databaseId,
                                            (int)entityType));

            }

            return messages;
        }

        public IEnumerable<IMessage> GetMessages(
            EntityEnum entity, 
            ConcurrentDictionary<EntityEnum, DataTable> data,
            int skip,
            int take
            )
        {
            IList<IMessage> output = null;

            if(data != null)
            {
                var records = data[entity]
                                    .AsEnumerable()
                                    .Skip(skip)
                                    .Take(take);

                switch (entity)
                {
                    case EntityEnum.Menu:
                        output = CreateMessage(EntityType.Menu, records);
                        break;
                    case EntityEnum.Group:
                        output = CreateMessage(EntityType.Group, records);
                        break;
                    case EntityEnum.Recipe:
                        output = CreateMessage(EntityType.Dish, records);
                        break;
                    case EntityEnum.User:
                        output = CreateMessage(EntityType.User, records);
                        break;
                    case EntityEnum.MealPeriod:
                        output = CreateMessage(EntityType.MealPeriodManagement, records);
                        break;
                }
            }

            return output;
        }
    }
}