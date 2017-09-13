using System;
using System.Collections.Generic;
using System.Linq;
using StarChef.Orchestrate.Models;

namespace StarChef.Orchestrate.EventSetters.Impl
{
    internal static class BuilderHelpers
    {
        internal static void BuildCategoryTypes(dynamic builder, Func<dynamic> createCategoryType, Func<dynamic> createCategory, List<CategoryType> categoryTypes)
        {
            foreach (var catType in categoryTypes)
            {
                var categoryTypeBuilder = createCategoryType();

                var exportType = OrchestrateHelper.MapCategoryExportType(catType.CategoryExportType.ToString());
                categoryTypeBuilder
                    .SetExternalId(catType.ExternalId)
                    .SetCategoryTypeName(catType.Name)
                    .SetExportType(exportType)
                    .SetIsFoodType(catType.IsFood);

                foreach (var category in catType.MainCategories)
                {
                    var mainCategoryBuilder = createCategory();
                    mainCategoryBuilder
                        .SetExternalId(category.ExternalId)
                        .SetCategoryName(category.Name)
                        .SetParentExternalId(category.ParentExternalId);

                    #region build nested items

                    var parentItem = category;
                    var parentBuilder = mainCategoryBuilder;
                    while (parentItem.SubCategories != null)
                    {
                        var nestedItem = category.SubCategories.First();
                        var nestedBuilder = createCategory();
                        nestedBuilder
                            .SetExternalId(nestedItem.ExternalId)
                            .SetCategoryName(nestedItem.Name)
                            .SetParentExternalId(nestedItem.ParentExternalId);
                        parentBuilder.AddSubCategories(nestedBuilder);
                        parentItem = nestedItem;
                        parentBuilder = nestedBuilder;
                    }

                    #endregion

                    categoryTypeBuilder.AddMainCategories(mainCategoryBuilder);
                }
                builder.AddCategoryTypes(categoryTypeBuilder);
            }
        }
    }
}
