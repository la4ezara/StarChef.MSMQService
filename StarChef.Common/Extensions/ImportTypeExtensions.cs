using System.Collections.Generic;
using StarChef.Common.Types;

namespace StarChef.Common.Extensions
{
    public static class ImportTypeExtensions
    {
        #region import types

        public static ImportTypeSettings Ingredient(this IDictionary<string, ImportTypeSettings> dic)
        {
            return GetImportTypeSettingsOrNull(dic, "Ingredient");
        }

        public static ImportTypeSettings IngredientIntolerance(this IDictionary<string, ImportTypeSettings> dic)
        {
            return GetImportTypeSettingsOrNull(dic, "IngredientIntolerance");
        }

        public static ImportTypeSettings IngredientNutrient(this IDictionary<string, ImportTypeSettings> dic)
        {
            return GetImportTypeSettingsOrNull(dic, "IngredientNutrient");
        }

        public static ImportTypeSettings IngredientPriceBand(this IDictionary<string, ImportTypeSettings> dic)
        {
            return GetImportTypeSettingsOrNull(dic, "IngredientPriceBand");
        }

        public static ImportTypeSettings IngredientConversion(this IDictionary<string, ImportTypeSettings> dic)
        {
            return GetImportTypeSettingsOrNull(dic, "IngredientConversion");
        }

        public static ImportTypeSettings IngredientCategory(this IDictionary<string, ImportTypeSettings> dic)
        {
            return GetImportTypeSettingsOrNull(dic, "IngredientCategory");
        }

        public static ImportTypeSettings Users(this IDictionary<string, ImportTypeSettings> dic)
        {
            return GetImportTypeSettingsOrNull(dic, "Users");
        }

        private static ImportTypeSettings GetImportTypeSettingsOrNull(IDictionary<string, ImportTypeSettings> dic, string importTypeName)
        {
            return dic.ContainsKey(importTypeName) ? dic[importTypeName] : null;
        }

        #endregion
    }
}