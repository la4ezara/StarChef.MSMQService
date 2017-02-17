using System;
using System.Collections.Generic;
using System.Linq;
using StarChef.Orchestrate.Models;
using Xunit;

using RecipeUpdated = Fourth.Orchestration.Model.Menus.Events.RecipeUpdated;
using RecipeCategoryType = Fourth.Orchestration.Model.Menus.Events.RecipeUpdated.Types.CategoryType;
using RecipeCategory = Fourth.Orchestration.Model.Menus.Events.RecipeUpdated.Types.CategoryType.Types.Category;
using SourceSystem = Fourth.Orchestration.Model.Menus.Events.SourceSystem;
using CategoryExportType = Fourth.Orchestration.Model.Menus.Events.CategoryExportType;
using StarChef.Orchestrate.EventSetters.Impl;

namespace StarChef.Orchestrate.Tests.EventSetters
{
    public class BuilderHelpersTests
    {
        [Fact]
        public void BuildCategoryTypes_should_init_builder_with_category_hierarchy()
        {
            var categoryTypes = new List<CategoryType>();
            categoryTypes.AddRange(new[]
            {
                #region CategoryType
                new CategoryType
                {
                    ProductId = 13768,
                    Id = 1,
                    ExternalId = "83A05879-8C70-4583-A36B-0A31D601F0D9",
                    Name = "Region",
                    CategoryExportType = 1,
                    MainCategories = new List<Category>
                    {
                        #region Category
                        new Category
                        {
                            Id = 51,
                            ProductId = 13768,
                            ExternalId = "1AF82BE6-9E9C-4585-8884-CC3B5C775846",
                            Name = "Scandanavia",
                            ParentExternalId = "83A05879-8C70-4583-A36B-0A31D601F0D9"
                        } 
                        #endregion
                    }
                },

                #endregion
                #region CategoryType
                new CategoryType
                {
                    ProductId = 13768,
                    Id = 4,
                    ExternalId = "AF0FB0B6-34C6-4C9B-BA20-3330FA16A299",
                    Name = "Course123",
                    CategoryExportType = null,
                    MainCategories = new List<Category>
                    {
                        #region Category
                        new Category
                        {
                            Id = 425,
                            ProductId = 13768,
                            ExternalId = "232789EA-841E-4FFF-9FD2-F085495E3FAD",
                            Name = "Starters",
                            ParentExternalId = "AF0FB0B6-34C6-4C9B-BA20-3330FA16A299"
                        },

                        #endregion
                        #region Category
                        new Category
                        {
                            Id = 425,
                            ProductId = 13768,
                            ExternalId = "232789EA-841E-4FFF-9FD2-F085495E3FAD",
                            Name = "Starters",
                            ParentExternalId = "AF0FB0B6-34C6-4C9B-BA20-3330FA16A299",
                            SubCategories = new List<Category>
                            {
                                #region Category
                                new Category
                                {
                                    Id = 442,
                                    ProductId = 13768,
                                    ExternalId = "5666E087-E0BC-4902-932A-2DA74A930469",
                                    Name = "test",
                                    ParentExternalId = "232789EA-841E-4FFF-9FD2-F085495E3FAD"
                                } 
                                #endregion
                            }
                        },

                        #endregion
                        #region Category
                        new Category
                        {
                            Id = 427,
                            ProductId = 13768,
                            ExternalId = "353A88C0-C37D-416A-9C66-FE4AAFBE72CE",
                            Name = "Desserts",
                            ParentExternalId = "AF0FB0B6-34C6-4C9B-BA20-3330FA16A299"
                        },

                        #endregion
                    }
                },

                #endregion
                #region CategoryType
                new CategoryType
                {
                    ProductId = 13768,
                    Id = 5,
                    ExternalId = "576B4721-5913-439F-8453-FC5AD8B6245E",
                    Name = "Ingredient Category",
                    CategoryExportType = null,

                    MainCategories = new List<Category>
                    {
                        #region Category
                        new Category
                        {
                            Id = 382,
                            ProductId = 13768,
                            ExternalId = "2D868A36-9108-4389-B355-E8F86135A534",
                            Name = "Grocery",
                            ParentExternalId = "576B4721-5913-439F-8453-FC5AD8B6245E"
                        },

                        #endregion
                        #region Category
                        new Category
                        {
                            Id = 63,
                            ProductId = 13768,
                            ExternalId = "02C03BD6-1368-4CDB-8D68-105CBE41FDD5",
                            Name = "Bakery",
                            ParentExternalId = "576B4721-5913-439F-8453-FC5AD8B6245E",
                            SubCategories = new List<Category>
                            {
                                #region Category
                                new Category
                                {
                                    Id = 422,
                                    ProductId = 13768,
                                    ExternalId = "7C5D42AA-BED0-4ACC-8919-C291DB92E4DB",
                                    Name = "Italian Breads",
                                    ParentExternalId = "02C03BD6-1368-4CDB-8D68-105CBE41FDD5"
                                } 
                                #endregion
                            }
                        },

                        #endregion
                    }
                } 
                #endregion
            });

            // act
            var builder = RecipeUpdated.CreateBuilder();
            // mandatory fields -> they are not important now
            builder
                .SetExternalId("")
                .SetCustomerId("")
                .SetCustomerName("")
                .SetSequenceNumber(1)
                .SetSource(SourceSystem.STARCHEF);

            Func<dynamic> createCategoryType = () => RecipeCategoryType.CreateBuilder();
            Func<dynamic> createCategory = () => RecipeCategory.CreateBuilder();

            BuilderHelpers.BuildCategoryTypes(builder, createCategoryType, createCategory, categoryTypes);

            #region assert
            var suppliedPackSize = builder.Build();
            Assert.Equal(3, suppliedPackSize.CategoryTypesCount);

            var categoryType = suppliedPackSize.CategoryTypesList.Single(ct => ct.CategoryTypeName == "Region");
            Assert.Equal(CategoryExportType.ING, categoryType.ExportType);
            Assert.Equal("83A05879-8C70-4583-A36B-0A31D601F0D9", categoryType.ExternalId);
            Assert.Equal(1, categoryType.MainCategoriesCount);
            Assert.Equal("Scandanavia", categoryType.MainCategoriesList[0].CategoryName);
            Assert.Equal("1AF82BE6-9E9C-4585-8884-CC3B5C775846", categoryType.MainCategoriesList[0].ExternalId);
            Assert.Equal("83A05879-8C70-4583-A36B-0A31D601F0D9", categoryType.MainCategoriesList[0].ParentExternalId);

            categoryType = suppliedPackSize.CategoryTypesList.Single(ct => ct.CategoryTypeName == "Course123");
            Assert.Equal(CategoryExportType.NONE, categoryType.ExportType);
            Assert.Equal("AF0FB0B6-34C6-4C9B-BA20-3330FA16A299", categoryType.ExternalId);
            Assert.Equal(3, categoryType.MainCategoriesCount);
            Assert.Equal("Starters", categoryType.MainCategoriesList[0].CategoryName);
            Assert.Equal("232789EA-841E-4FFF-9FD2-F085495E3FAD", categoryType.MainCategoriesList[0].ExternalId);
            Assert.Equal("AF0FB0B6-34C6-4C9B-BA20-3330FA16A299", categoryType.MainCategoriesList[0].ParentExternalId);
            Assert.Equal("Starters", categoryType.MainCategoriesList[1].CategoryName);
            Assert.Equal("232789EA-841E-4FFF-9FD2-F085495E3FAD", categoryType.MainCategoriesList[1].ExternalId);
            Assert.Equal("AF0FB0B6-34C6-4C9B-BA20-3330FA16A299", categoryType.MainCategoriesList[1].ParentExternalId);
            Assert.Equal(1, categoryType.MainCategoriesList[1].SubCategoriesCount);
            Assert.Equal("test", categoryType.MainCategoriesList[1].SubCategoriesList[0].CategoryName);
            Assert.Equal("5666E087-E0BC-4902-932A-2DA74A930469", categoryType.MainCategoriesList[1].SubCategoriesList[0].ExternalId);
            Assert.Equal("232789EA-841E-4FFF-9FD2-F085495E3FAD", categoryType.MainCategoriesList[1].SubCategoriesList[0].ParentExternalId);
            Assert.Equal("Desserts", categoryType.MainCategoriesList[2].CategoryName);
            Assert.Equal("353A88C0-C37D-416A-9C66-FE4AAFBE72CE", categoryType.MainCategoriesList[2].ExternalId);
            Assert.Equal("AF0FB0B6-34C6-4C9B-BA20-3330FA16A299", categoryType.MainCategoriesList[2].ParentExternalId);

            categoryType = suppliedPackSize.CategoryTypesList.Single(ct => ct.CategoryTypeName == "Ingredient Category");
            Assert.Equal(CategoryExportType.NONE, categoryType.ExportType);
            Assert.Equal("576B4721-5913-439F-8453-FC5AD8B6245E", categoryType.ExternalId);
            Assert.Equal(2, categoryType.MainCategoriesCount);
            Assert.Equal("Grocery", categoryType.MainCategoriesList[0].CategoryName);
            Assert.Equal("2D868A36-9108-4389-B355-E8F86135A534", categoryType.MainCategoriesList[0].ExternalId);
            Assert.Equal("576B4721-5913-439F-8453-FC5AD8B6245E", categoryType.MainCategoriesList[0].ParentExternalId);
            Assert.Equal("Bakery", categoryType.MainCategoriesList[1].CategoryName);
            Assert.Equal("02C03BD6-1368-4CDB-8D68-105CBE41FDD5", categoryType.MainCategoriesList[1].ExternalId);
            Assert.Equal("576B4721-5913-439F-8453-FC5AD8B6245E", categoryType.MainCategoriesList[1].ParentExternalId);
            Assert.Equal(1, categoryType.MainCategoriesList[1].SubCategoriesCount);
            Assert.Equal("Italian Breads", categoryType.MainCategoriesList[1].SubCategoriesList[0].CategoryName);
            Assert.Equal("7C5D42AA-BED0-4ACC-8919-C291DB92E4DB", categoryType.MainCategoriesList[1].SubCategoriesList[0].ExternalId);
            Assert.Equal("02C03BD6-1368-4CDB-8D68-105CBE41FDD5", categoryType.MainCategoriesList[1].SubCategoriesList[0].ParentExternalId); 
            #endregion

        }
    }
}