using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using StarChef.Orchestrate.Models;

namespace StarChef.Orchestrate.Helpers
{
    public static class DataReaderExtension
    {
        public static T GetValueOrDefault<T>(this IDataReader reader, int colIndex)
        {
            if (reader.IsDBNull(colIndex)) return default(T);

            object item = reader[colIndex];

            return item is T ? (T)item : (T)Convert.ChangeType(item, typeof(T));
        }

        public static T GetValue<T>(this IDataReader reader, string colName)
        {
            var colIndex = reader.GetOrdinal(colName);
            var item = reader[colIndex];
            return item is T ? (T)item : (T)Convert.ChangeType(item, typeof(T));
        }

        public static bool IsDBNull(this IDataReader reader, string colName)
        {
            var colIndex = reader.GetOrdinal(colName);
            return reader.IsDBNull(colIndex);
        }

        internal static void ReadCategories(this IDataReader reader, out List<CategoryType> categoryTypes, out List<Category> categories)
        {
            #region load data from reader
            var records = new List<CategoryRecord>();

            while (reader.Read())
            {
                var cat = new CategoryRecord
                {
                    ProductId = reader.GetValue<int>("product_id"),
                    TagId = reader.GetValue<int>("tag_id"),
                    TagName = reader.GetValue<string>("tag_name"),
                    TagParentId = reader.IsDBNull("tag_parent_id") ? null : (int?)reader.GetValue<int>("tag_parent_id"),
                    TagExportTypeId = reader.IsDBNull("tag_export_type_id") ? null : (int?)reader.GetValue<int>("tag_export_type_id"),
                    IsFoodType = reader.GetValue<bool>("IsFoodType"),
                    TagGuid = reader.GetValue<Guid>("tag_guid"),
                    TagParentGuid = reader.IsDBNull("tag_parent_guid") ? null : (Guid?)reader.GetValue<Guid>("tag_parent_guid"),
                    IsSelected = reader.GetValue<bool>("IsSelected")
                };
                records.Add(cat);
            }
            #endregion
            #region get category types
            // category types don't have a parent
            categoryTypes = records.Where(i => !i.TagParentId.HasValue).Select(i => new CategoryType
            {
                ProductId = i.ProductId,
                Id = i.TagId,
                ExternalId = Convert.ToString(i.TagGuid),
                Name = i.TagName,
                CategoryExportType = i.TagExportTypeId,
                MainCategories = new List<Category>() // will be at least one item in category
            }).ToList();
            #endregion

            #region build category legs for selected ones
            categories = new List<Category>();
            var stack = new Stack<CategoryRecord>();
            records.Where(i => i.IsSelected).ToList().ForEach(c => stack.Push(c));

            // for each category take all parents up to the type (the type is not included)
            while (stack.Count > 0)
            {
                var categoryRecord = stack.Peek();
                if (categoryRecord.TagParentId.HasValue)
                {
                    var parentRecord = records.Single(i => i.TagId == categoryRecord.TagParentId.Value && !i.IsSelected && i.ProductId == categoryRecord.ProductId);
                    stack.Push(parentRecord);
                }
                else // met a category type 
                {
                    // build the leg of items from the top to the selected one
                    var typeRecord = stack.Pop(); // skip the type
                    var categoryType = categoryTypes.Single(t => t.Id == typeRecord.TagId && t.ProductId == typeRecord.ProductId);

                    var currentRecord = stack.Pop();
                    var parentCategory = currentRecord.CreateCategory();
                    categories.Add(parentCategory); // this item maybe the selected one, so that sub items will be created later if needed
                    categoryType.MainCategories.Add(parentCategory); // all category legs should be added to their types respectively
                    while (!currentRecord.IsSelected) // if the current category is not selected, then need to create nested one
                    {
                        // take the next one and create category item
                        currentRecord = stack.Pop();
                        var currentCategory = currentRecord.CreateCategory();

                        // add category to its parent and go down in the hierarchy
                        parentCategory.SubCategories = new List<Category> { currentCategory };
                        parentCategory = currentCategory;
                    }
                }
            }
            #endregion
        }

        internal static void ReadKitchenAreas(this IDataReader reader, out List<KitchenArea> kitchenAreas)
        {
            kitchenAreas = new List<KitchenArea>();
            #region load data from reader
            while (reader.Read())
            {
                var kitchenArea = new KitchenArea
                {
                    ProductPartId = reader.GetValue<int>("ProductPartId"),
                    ExternalId = reader.GetValue<string>("ExternalId"),
                    Name = reader.GetValue<string>("KitchenAreaName"),
                    DisplayOrder = reader.GetValue<int>("DisplayOrder")
                };

                kitchenAreas.Add(kitchenArea);
            }
            #endregion
        }

        [DebuggerDisplay(@"\{{TagName}\}")]
        private struct CategoryRecord
        {
            public int ProductId { get; set; }
            public int TagId { get; set; }
            public string TagName { get; set; }
            public int? TagParentId { get; set; }
            public int? TagExportTypeId { get; set; }
            public bool IsFoodType { get; set; }
            public Guid TagGuid { get; set; }
            public Guid? TagParentGuid { get; set; }
            public bool IsSelected { get; set; }

            internal Category CreateCategory()
            {
                var category = new Category
                {
                    Id = TagId,
                    ProductId = ProductId,
                    ExternalId = Convert.ToString(TagGuid),
                    Name = TagName,
                    ParentExternalId = Convert.ToString(TagParentGuid)
                };
                return category;
            }
        }
    }
}