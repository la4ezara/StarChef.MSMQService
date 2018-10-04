using Dapper;
using Fourth.StarChef.Invariables;
using StarChef.Common.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Linq;

namespace StarChef.Common.Repository
{
    public class PricingRepository : RepositoryBase, IPricingRepository
    {
        readonly string connectionStr;
        public PricingRepository(string connectionString, int timeout) : this(connectionString)
        {
            base.CommandTimeout = timeout;
        }

        public PricingRepository(string connectionString)
        {
            connectionStr = connectionString;
            var types = Assembly.GetAssembly(typeof(DishItem)).GetTypes().Where(t => t.Namespace == "StarChef.Common.Model").ToList();
            foreach (var type in types)
            {
                SetTypeMap(type);
            }
        }

        private void SetTypeMap(Type type)
        {
            var map = new CustomPropertyTypeMap(type,
                (t, columnName) => t.GetProperties().FirstOrDefault(prop => GetDescriptionFromAttribute(prop) == columnName));
            SqlMapper.SetTypeMap(type, map);
        }

        private string GetDescriptionFromAttribute(MemberInfo member)
        {
            if (member == null) return null;
            var attrib = (DescriptionAttribute)Attribute.GetCustomAttribute(member, typeof(DescriptionAttribute), false);
            return attrib?.Description;
        }

        public IEnumerable<DishItem> GetDishes()
        {
            var cmd = "SELECT product_id, recipe_type_id FROM Dish WITH(NOLOCK)";
            using (var connection = GetOpenConnection())
            {
                var result = Query<DishItem>(connection, cmd, null, CommandType.Text);
                return result;
            }
        }

        public IEnumerable<GroupProducts> GetGroupProductPricesByGroup(int groupId)
        {
            var param = new
            {
                group_id = groupId
            };

            var cmd = @"CREATE TABLE #group_products
                (
                    group_id        INT,
                    product_id      INT,
                    price           decimal(25, 13)
                )
                EXEC sc_costing_populate_group_products_full @group_id

                SELECT DISTINCT product_id, group_id, price FROM #group_products
                DROP TABLE #group_products
                ";
            using (var connection = GetOpenConnection())
            {
                var result = Query<GroupProducts>(connection, cmd, param, CommandType.Text);
                return result;
            }
        }

        public IEnumerable<GroupProducts> GetGroupProductPricesByProduct(int groupId, int productId, int psetId, int pbandId, int unitId)
        {
            var param = new
            {
                group_id = groupId,
                include_descendants = 1,
                product_id = productId,
                pset_id = psetId,
                pband_id = pbandId,
                unit_id = unitId
            };
            var cmd = @"CREATE TABLE #group_products
                (
                    group_id        INT,
                    product_id      INT,
                    price           decimal(25, 13)
                )
                exec upfast_sc_GetAvailableProductsWithIngredientPrices2
				@group_id,
				@include_descendants,
				@product_id,
				@pset_id,
				@pband_id,
				@unit_id

                SELECT DISTINCT product_id, group_id, price FROM #group_products
                DROP TABLE #group_products
                ";
            using (var connection = GetOpenConnection())
            {
                var result = Query<GroupProducts>(connection, cmd, param, CommandType.Text);
                return result;
            }
        }

        public IEnumerable<IngredientItem> GetIngredients()
        {
            var cmd = "SELECT product_id, ingredient_id, wastage FROM ingredient WITH(NOLOCK)";
            using (var connection = GetOpenConnection())
            {
                var result = Query<IngredientItem>(connection, cmd, null, CommandType.Text);
                return result;
            }
        }

        public IEnumerable<GroupProductPriceItem> GetPrices()
        {
            var cmd = "SELECT product_id, group_id, product_price FROM db_product_calc WITH(NOLOCK)";
            using (var connection = GetOpenConnection())
            {
                var result = Query<GroupProductPriceItem>(connection, cmd, null, CommandType.Text);
                return result;
            }
        }

        public IEnumerable<ProductPartItem> GetProductParts()
        {
            var cmd = "SELECT product_part_id,portion_type_id,product_id,sub_product_id,quantity,unit_id,is_choice FROM product_part WITH(NOLOCK)";
            using (var connection = GetOpenConnection())
            {
                var result = Query<ProductPartItem>(connection, cmd, null, CommandType.Text);
                return result;
            }
        }

        public IEnumerable<ProductItem> GetProducts()
        {
            var cmd = "SELECT product_id, number, quantity, unit_id, product_type_id, scope_id FROM product WITH(NOLOCK)";
            using (var connection = GetOpenConnection())
            {
                var result = Query<ProductItem>(connection, cmd, null, CommandType.Text);
                return result;
            }
        }

        public bool UpdatePrices(IEnumerable<GroupProductPriceItem> prices)
        {
            throw new NotImplementedException();
        }

        private SqlConnection GetOpenConnection(bool mars = false)
        {
            var cs = connectionStr;
            if (mars)
            {
                var scsb = new SqlConnectionStringBuilder(cs)
                {
                    MultipleActiveResultSets = true
                };
                cs = scsb.ConnectionString;
            }
            var connection = new SqlConnection(cs);
            connection.Open();
            return connection;
        }
    }
}