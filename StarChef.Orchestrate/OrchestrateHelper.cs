using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fourth.Orchestration.Model.Menus;
using CategoryExportType = Fourth.Orchestration.Model.Menus.Events.CategoryExportType;

namespace StarChef.Orchestrate
{
    public static class OrchestrateHelper
    {
        public static Events.CategoryExportType MapCategoryExportType(string exportTypeId)
        {
            if (string.IsNullOrEmpty(exportTypeId)) return CategoryExportType.NONE;

            int numFromString;
            if (int.TryParse(exportTypeId, out numFromString))
            {
                if (Enum.IsDefined(typeof (CategoryExportType), numFromString))
                    return (CategoryExportType) numFromString;
                return CategoryExportType.NONE;
            }

            // if here --> string name of the enum value
            if (Enum.IsDefined(typeof (CategoryExportType), exportTypeId))
                return (CategoryExportType) Enum.Parse(typeof (CategoryExportType), exportTypeId);
            return CategoryExportType.NONE;
        }

        public static Events.VATRate MapVatType(string vatType)
        {
            Events.VATRate result;

            switch (vatType)
            {
                case "Exempt":
                    result = Events.VATRate.EXEMPT;
                    break;
                case "Zero Rated":
                    result = Events.VATRate.ZERO_ADDED;
                    break;
                default:
                    result = Events.VATRate.STANDARD_VAT;
                    break;
            }
            return result;
        }

        public static Events.RecipeType MapRecipeType(string recipeTypeCodeFromDb)
        {
            Events.RecipeType recipeType;

            switch (recipeTypeCodeFromDb)
            {
                case "Batch":
                    recipeType = Events.RecipeType.BATCH;
                    break;
                case "Choice":
                    recipeType = Events.RecipeType.CHOICE;
                    break;
                case "Option":
                    recipeType = Events.RecipeType.OPTION;
                    break;
                default:
                    recipeType = Events.RecipeType.STANDARD;
                    break;
            }
            return recipeType;
        }
    }
}
