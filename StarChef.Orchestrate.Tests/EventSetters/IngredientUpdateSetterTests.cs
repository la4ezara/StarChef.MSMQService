using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using StarChef.Orchestrate.Models;
using Xunit;

using SuppliedPackSize = Fourth.Orchestration.Model.Menus.Events.IngredientUpdated.Types.SuppliedPackSize;
using CategoryExportType = Fourth.Orchestration.Model.Menus.Events.CategoryExportType;

namespace StarChef.Orchestrate.Tests.EventSetters
{
    public class IngredientUpdateSetterTests
    {
        [Fact]
        public void ReadCategories_should_read_types_and_categories_from_one_list()
        {
            // create category hierarchy like it should be selected from DB
            var table = new DataTable();
            table.Columns.AddRange(new[]
            {
                new DataColumn("product_id", typeof (int)),
                new DataColumn("tag_id", typeof (int)),
                new DataColumn("tag_name", typeof (string)),
                new DataColumn("tag_parent_id", typeof (int)),
                new DataColumn("tag_export_type_id", typeof (int)),
                new DataColumn("IsFoodType", typeof (bool)),
                new DataColumn("tag_guid", typeof (Guid)),
                new DataColumn("tag_parent_guid", typeof (Guid)),
                new DataColumn("IsSelected", typeof (bool))
            });
            /* FORMATTER FOR VISIBILITY, PLEASE, DON'T CHANGE */
            //            product_id    tag_id   tag_name	            tag_parent_id	tag_export_type_id	IsFoodType	 tag_guid	                                         tag_parent_guid	                      IsSelected
            table.Rows.Add(13768,       1,       "Region",              DBNull.Value,	1,	                0,Guid.Parse("83A05879-8C70-4583-A36B-0A31D601F0D9"), DBNull.Value,	                                      0);
            table.Rows.Add(13768,       4,       "Course123",	        DBNull.Value,	DBNull.Value,	    0,Guid.Parse("AF0FB0B6-34C6-4C9B-BA20-3330FA16A299"), DBNull.Value,	                                      0);
            table.Rows.Add(13768,       5,       "Ingredient Category",	DBNull.Value,	DBNull.Value,	    0,Guid.Parse("576B4721-5913-439F-8453-FC5AD8B6245E"), DBNull.Value,	                                      0);
            table.Rows.Add(13768,       51,      "Scandanavia",	        1,	            DBNull.Value,	    0,Guid.Parse("1AF82BE6-9E9C-4585-8884-CC3B5C775846"), Guid.Parse("83A05879-8C70-4583-A36B-0A31D601F0D9"), 1);
            table.Rows.Add(13768,       63,      "Bakery",	            5,	            DBNull.Value,	    0,Guid.Parse("02C03BD6-1368-4CDB-8D68-105CBE41FDD5"), Guid.Parse("576B4721-5913-439F-8453-FC5AD8B6245E"), 0);
            table.Rows.Add(13768,       382,     "Grocery",	            5,	            DBNull.Value,	    0,Guid.Parse("2D868A36-9108-4389-B355-E8F86135A534"), Guid.Parse("576B4721-5913-439F-8453-FC5AD8B6245E"), 1);
            table.Rows.Add(13768,       422,     "Italian Breads",	    63,	            DBNull.Value,	    0,Guid.Parse("7C5D42AA-BED0-4ACC-8919-C291DB92E4DB"), Guid.Parse("02C03BD6-1368-4CDB-8D68-105CBE41FDD5"), 1);
            table.Rows.Add(13768,       425,     "Starters",	        4,	            DBNull.Value,	    0,Guid.Parse("232789EA-841E-4FFF-9FD2-F085495E3FAD"), Guid.Parse("AF0FB0B6-34C6-4C9B-BA20-3330FA16A299"), 0);
            table.Rows.Add(13768,       425,     "Starters",	        4,	            DBNull.Value,	    0,Guid.Parse("232789EA-841E-4FFF-9FD2-F085495E3FAD"), Guid.Parse("AF0FB0B6-34C6-4C9B-BA20-3330FA16A299"), 1);
            table.Rows.Add(13768,       427,     "Desserts",	        4,	            DBNull.Value,	    0,Guid.Parse("353A88C0-C37D-416A-9C66-FE4AAFBE72CE"), Guid.Parse("AF0FB0B6-34C6-4C9B-BA20-3330FA16A299"), 1);
            table.Rows.Add(13768,       442,     "test",	            425,	        DBNull.Value,	    0,Guid.Parse("5666E087-E0BC-4902-932A-2DA74A930469"), Guid.Parse("232789EA-841E-4FFF-9FD2-F085495E3FAD"), 1);

            /*
             * NOTE
             * [tag_parent_id] - The hierarchy of categories is build using column
             * [IsSelected] - determines if the category actually selected (1), not selected categories are used for building the hierarchy
             * other columns are required to initialize entities
             * 
             * 
             * DB
             * == These categories are assigned to the ingredient: ==
             * Category Type: Course123
             *      Desserts
             *      Starters
             *      test <---- sub cat of Starters
             * Category Type: Ingredient Category
             *      Italian Breads <---- sub cat of Bakery
             *      Grocery
             * Category Type: Region
             *      Scandanavia
             * 
             * RESULT     
             * == These types and categories should be found ==
             * Types:
             *      Course123
             *      Ingredient Category
             *      Region
             * Categories:
             *      Desserts
             *      Starters
             *      Starters > test
             *      Bakery > Italian Breads
             *      Grocery
             *      Scandanavia
             */

            #region act
            var reader = table.CreateDataReader();
            var categoryTypes = new List<IngredientUpdateSetter.CategoryType>();
            var categories = new List<Category>();
            IngredientUpdateSetter.ReadCategories(reader, out categoryTypes, out categories);
            #endregion

            #region assert
            // assert category types
            Assert.Equal(3, categoryTypes.Count);
            Assert.NotNull(categoryTypes.Single(t => t.Name == "Course123"));
            Assert.Equal(3, categoryTypes.Single(t => t.Name == "Course123").MainCategories.Count);

            Assert.NotNull(categoryTypes.Single(t => t.Name == "Ingredient Category"));
            Assert.Equal(2, categoryTypes.Single(t => t.Name == "Ingredient Category").MainCategories.Count);

            Assert.NotNull(categoryTypes.Single(t => t.Name == "Region"));
            Assert.Equal(1, categoryTypes.Single(t => t.Name == "Region").MainCategories.Count);

            // assert categories, some of them should have nested items, which actually were selected
            Assert.Equal(6, categories.Count);
            Assert.NotNull(categories.Single(c => c.Name == "Desserts" && c.SubCategories == null));
            Assert.NotNull(categories.Single(c => c.Name == "Starters" && c.SubCategories == null));

            Assert.NotNull(categories.Single(c => c.Name == "Starters" && c.SubCategories != null));
            Assert.Equal(1, categories.Single(c => c.Name == "Starters" && c.SubCategories != null).SubCategories.Count);
            Assert.Equal("test", categories.Single(c => c.Name == "Starters" && c.SubCategories != null).SubCategories.First().Name);

            Assert.NotNull(categories.Single(c => c.Name == "Bakery" && c.SubCategories != null));
            Assert.Equal(1, categories.Single(c => c.Name == "Bakery" && c.SubCategories != null).SubCategories.Count);
            Assert.Equal("Italian Breads", categories.Single(c => c.Name == "Bakery" && c.SubCategories != null).SubCategories.First().Name);

            Assert.NotNull(categories.Single(c => c.Name == "Grocery" && c.SubCategories == null));
            Assert.NotNull(categories.Single(c => c.Name == "Scandanavia" && c.SubCategories == null)); 
            #endregion
        }
        
        [Fact]
        public void BuildCategoryTypes_should_init_builder_with_category_hierarchy()
        {
            var categoryTypes = new List<IngredientUpdateSetter.CategoryType>();
            categoryTypes.AddRange(new[]
            {
                #region CategoryType
                new IngredientUpdateSetter.CategoryType
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
                new IngredientUpdateSetter.CategoryType
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
                new IngredientUpdateSetter.CategoryType
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
            var builder = SuppliedPackSize.CreateBuilder();
            IngredientUpdateSetter.BuildCategoryTypes(builder, categoryTypes);

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