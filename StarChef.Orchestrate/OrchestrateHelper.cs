using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fourth.Orchestration.Model.Menus;
using StarChef.Orchestrate.Models;

namespace StarChef.Orchestrate
{
    public static class OrchestrateHelper
    {
        public static Events.CategoryExportType MapCategoryExportType(string exportTypeId)
        {
            return !string.IsNullOrEmpty(exportTypeId)
                                         ? (Events.CategoryExportType)(Convert.ToInt16(exportTypeId))
                                         : Events.CategoryExportType.NONE;
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
