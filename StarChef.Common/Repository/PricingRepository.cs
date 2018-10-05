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
using Microsoft.SqlServer.Server;

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

        public IEnumerable<ProductPsetItem> GetPsetProducts(int psetId)
        {
            var param = new
            {
                pset_id = psetId
            };

            var cmd = @"SELECT DISTINCT product_id, 0 as is_choise 
                        FROM product_pset WITH(NOLOCK) 
                        WHERE pset_id = @pset_id";
            using (var connection = GetOpenConnection())
            {
                var result = Query<ProductPsetItem>(connection, cmd, param, CommandType.Text);
                return result;
            }
        }

        public IEnumerable<ProductPsetItem> GetPsetGroupProducts(int pbandId)
        {
            var param = new
            {
                pband_id = pbandId
            };

            var cmd = @"SELECT	DISTINCT pps.product_id, 0 as is_choise 
                        FROM product_pset pps with(nolock)
                        INNER JOIN group_set_link gsl with(nolock) ON pps.pset_id = gsl.set_id
                        WHERE gsl.pband_id = @pband_id";
            using (var connection = GetOpenConnection())
            {
                var result = Query<ProductPsetItem>(connection, cmd, param, CommandType.Text);
                return result;
            }
        }

        public IEnumerable<ProductConvertionRatio> GetProductConvertionRatio(IEnumerable<ProductConvertionRatio> products)
        {
            var param = new
            {
                productConvertion = ToSqlDataRecords(products).AsTableValuedParameter("udt_product_convertion_ratio")
            };

            var cmd = @"CREATE TABLE #ConversionRatio
	                    (
		                    ProductID INT,
		                    SourceUnitID SMALLINT,
		                    TargetUnitID SMALLINT,
		                    Ratio DECIMAL(30,14)
	                    )

	                    INSERT INTO #ConversionRatio(ProductID,SourceUnitID,TargetUnitID)
	                    SELECT DISTINCT 
		                    Product_id, 
		                    Source_unit_id, 
		                    Target_unit_id 
		                    FROM @productConvertion

                        CREATE INDEX IDX_ConverstionRatio ON #ConversionRatio(ProductID)

                        UPDATE #ConversionRatio
		                SET Ratio = dbo.fn_ConversionGetRatioEx(ProductID, SourceUnitID, TargetUnitID)	
                        Select ProductID as Product_id, SourceUnitID as Source_unit_id, TargetUnitID as Target_unit_id, Ratio FROM #ConversionRatio
                        
                        DROP TABLE #ConversionRatio";
            using (var connection = GetOpenConnection())
            {
                var result = Query<ProductConvertionRatio>(connection, cmd, param, CommandType.Text);
                return result;
            }
        }

        public static IEnumerable<SqlDataRecord> ToSqlDataRecords(IEnumerable<ProductConvertionRatio> filters)
        {
            var metaData = new[]
            {
                new SqlMetaData("Product_id", SqlDbType.Int),
                new SqlMetaData("Source_unit_id", SqlDbType.SmallInt),
                new SqlMetaData("Target_unit_id", SqlDbType.SmallInt),
                new SqlMetaData("Ratio", SqlDbType.Decimal, 30, 14)
            };

            foreach (var filter in filters)
            {
                var record = new SqlDataRecord(metaData);
                record.SetInt32(0, filter.ProductId);
                record.SetInt16(1, filter.SourceUnitId);
                record.SetInt16(2, filter.TargetUnitId);
                //record.SetInt16(3, filter.SourceUnitId);
                yield return record;
            }
        }
    }
}