﻿using Dapper;
using Fourth.StarChef.Invariables;
using Microsoft.SqlServer.Server;
using StarChef.Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
            var types = Assembly.GetAssembly(typeof(DbPrice)).GetTypes().Where(t => t.Namespace == "StarChef.Common.Model").ToList();
            foreach (var type in types)
            {
                SetTypeMap(type);
            }
        }

        public async Task<IEnumerable<ProductGroupPrice>> GetGroupProductPricesByProduct(int productId) {
            var param = new
            {
                product_id = productId
            };

            var cmd = @"CREATE TABLE #group_products
                (
                    group_id        INT,
                    product_id      INT,
                    price           decimal(25, 13)
                )
                EXEC upfast_sc_GetAvailableProductsWithIngredientPrices2 0, 1, @product_id, 0, 0, 0

                SELECT DISTINCT product_id, group_id, price FROM #group_products
                DROP TABLE #group_products";
            IEnumerable<ProductGroupPrice> result = await Task.Run(() =>
            {
                using (var connection = GetOpenConnection())
                {
                    var res = Query<ProductGroupPrice>(connection, cmd, param, CommandType.Text);
                    return res;
                }
            });
            return result;
        }

        public async Task<IEnumerable<ProductGroupPrice>> GetGroupProductPricesByGroup(int groupId)
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
            IEnumerable<ProductGroupPrice> result = await Task.Run(() =>
            {
                using (var connection = GetOpenConnection())
                {
                    var res = Query<ProductGroupPrice>(connection, cmd, param, CommandType.Text);
                    return res;
                }
            });
            return result;
        }

        public async Task<IEnumerable<DbPrice>> GetPrices()
        {
            var cmd = "SELECT product_id, group_id, product_price FROM db_product_calc WITH(NOLOCK)";
            using (var connection = GetOpenConnection())
            {
                IEnumerable<DbPrice> result = await Task.Run(() =>
                {
                    var res = Query<DbPrice>(connection, cmd, null, CommandType.Text);
                    return res;
                });
                return result;
            }
        }

        public async Task<int> GetPricesCount()
        {
            var cmd = "SELECT COUNT(*) FROM db_product_calc WITH(NOLOCK)";
            using (var connection = GetOpenConnection())
            {
                int result = await Task.Run(() =>
                {
                    var res = ExecuteScalar<int>(connection, cmd, null, CommandType.Text);
                    return res;
                });
                return result;
            }
        }

        public async Task<IEnumerable<DbPrice>> GetPrices(int groupId)
        {
            var param = new
            {
                group_id = groupId
            };

            var cmd = "SELECT product_id, group_id, product_price FROM db_product_calc WITH(NOLOCK) where group_id = @group_id";
            using (var connection = GetOpenConnection())
            {
                IEnumerable<DbPrice> result = await Task.Run(() =>
                {
                    var res = Query<DbPrice>(connection, cmd, param, CommandType.Text);
                    return res;
                });
                return result;
            }
        }

        public async Task<IEnumerable<ProductPart>> GetProductParts()
        {
            var cmd = @"
                CREATE TABLE #convetion(product_id INT, source_unit_id INT, target_unit_id INT, ratio DECIMAL(30,14))

                INSERT INTO #convetion(product_id, source_unit_id, target_unit_id)
                select DISTINCT pp.sub_product_id, p.unit_id, pp.unit_id
                from product_part as pp WITH (NOLOCK)
                JOIN product as p ON pp.sub_product_id = p.product_id

                UPDATE #convetion
                SET ratio = dbo.fn_ConversionGetRatioEx(product_id, source_unit_id, target_unit_id) 
                
                SELECT DISTINCT pp.product_part_id,pp.portion_type_id,pp.product_id,pp.sub_product_id,pp.quantity,pp.unit_id,pp.is_choice, p.product_type_id,
                c.ratio as ratio 
                FROM product_part as pp WITH(NOLOCK)
                JOIN product as p WITH(NOLOCK) ON pp.sub_product_id = p.product_id
                JOIN #convetion as c ON c.product_id = pp.sub_product_id AND ISNULL(c.source_unit_id, 0) = ISNULL(p.unit_id,0) AND ISNULL(c.target_unit_id, 0) = ISNULL(pp.unit_id,0)
                DROP TABLE #convetion";
            using (var connection = GetOpenConnection())
            {
                IEnumerable<ProductPart> result = await Task.Run(()=>{ return Query<ProductPart>(connection, cmd, null, CommandType.Text); });
                return result;
            }
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            var cmd = @"SELECT p.product_id, p.number, p.quantity, p.unit_id, p.product_type_id, p.scope_id, i.wastage, d.recipe_type_id 
                    FROM product as p WITH(NOLOCK)
                    LEFT JOIN ingredient as i WITH(NOLOCK) on i.product_id = p.product_id
                    LEFT JOIN dish as d WITH(NOLOCK) on d.product_id = p.product_id";
            using (var connection = GetOpenConnection())
            {
                IEnumerable<Product> result = await Task.Run(()=> { return Query<Product>(connection, cmd, null, CommandType.Text); });
                return result;
            }
        }

        public async Task<Tuple<IEnumerable<Product>, IEnumerable<ProductPart>>> GetProductsAndParts(int productId)
        {
            //get top level recipes and build cuts
            var cmd = @"DECLARE @topRecipes TABLE(product_id INT);

                     WITH rtop(product_part_id,product_id,sub_product_id)
                    AS (
                    SELECT product_part_id, product_id, sub_product_id from product_part where sub_product_id = @product_id
                    UNION ALL

                    SELECT pp.product_part_id, pp.product_id, pp.sub_product_id from product_part as pp
                    JOIN rtop ON rtop.product_id = pp.sub_product_id
                    )
                    --get top level recipes which are not nested in any other recipes. AKA forest top cuts
                    INSERT INTO @topRecipes(product_id)
                    SELECT DISTINCT rtop.product_id FROM rtop
                    LEFT JOIN product_part as pp on rtop.product_id = pp.sub_product_id
                    WHERE pp.sub_product_id IS NULL;

                    IF(NOT EXISTS(SELECT 1 FROM @topRecipes WHERE product_id = @product_id))
                    BEGIN
	                    INSERT INTO @topRecipes(product_id) VALUES(@product_id);
                    END

                    DECLARE @tmP_productParts TABLE (productPartId INT , productId INT , sub_product_id INT);

                    DECLARE @topRecipeId INT
	
                    DECLARE cur_cut CURSOR FOR 
                    SELECT  [product_id] FROM @topRecipes
                    OPEN cur_cut
	                    FETCH NEXT FROM cur_cut
	                    INTO @topRecipeId

                    WHILE @@FETCH_STATUS = 0
                    BEGIN

                    WITH rcut(product_part_id,product_id,sub_product_id)
                    AS (
                    SELECT product_part_id, product_id, sub_product_id from product_part where product_id = @topRecipeId
                    UNION ALL

                    SELECT pp.product_part_id, pp.product_id, pp.sub_product_id from product_part as pp
                    JOIN rcut ON rcut.sub_product_id = pp.product_id
                    )
                    INSERT INTO @tmP_productParts(productPartId, productId, sub_product_id)
                    SELECT product_part_id,product_id,sub_product_id FROM rcut;

	                    FETCH NEXT FROM cur_cut INTO @topRecipeId
                    END
                    CLOSE cur_cut
                    DEALLOCATE cur_cut

                    CREATE TABLE #convertion(product_id INT, source_unit_id INT, target_unit_id INT, ratio DECIMAL(30,14))

                    IF(EXISTS(SELECT 1 FROM @tmP_productParts))
                    BEGIN
	                    INSERT INTO #convertion(product_id, source_unit_id, target_unit_id)
	                    select DISTINCT pp.sub_product_id, p.unit_id, pp.unit_id
	                    from product_part as pp 
	                    JOIN product as p ON pp.sub_product_id = p.product_id
	                    JOIN @tmP_productParts ON productPartId = pp.product_part_id
                    END
                    ELSE
                    BEGIN
	                    INSERT INTO #convertion(product_id, source_unit_id, target_unit_id)
	                    select DISTINCT p.product_id, p.unit_id, p.unit_id
	                    FROM product as p
	                    WHERE p.product_id = @product_id
                    END

                    UPDATE #convertion
                    SET ratio = dbo.fn_ConversionGetRatioEx(product_id, source_unit_id, target_unit_id) 
                
                    SELECT DISTINCT pp.product_part_id,pp.portion_type_id,pp.product_id,pp.sub_product_id,pp.quantity,pp.unit_id,pp.is_choice, p.product_type_id,
                    c.ratio as ratio 
                    FROM product_part as pp 
                    JOIN product as p ON pp.sub_product_id = p.product_id
                    JOIN #convertion as c ON c.product_id = pp.sub_product_id AND ISNULL(c.source_unit_id, 0) = ISNULL(p.unit_id,0) AND ISNULL(c.target_unit_id, 0) = ISNULL(pp.unit_id,0)
                    JOIN @tmP_productParts ON productPartId = pp.product_part_id
                    DROP TABLE #convertion

                    DECLARE @AlternateProductId TABLE(product_Id INT)
					INSERT INTO @AlternateProductId(product_Id)
					SELECT DISTINCT pa.product_id AS alternate_product_id
					FROM product AS p 
					JOIN product as pa ON p.master_ref_no = pa.master_ref_no
					WHERE p.product_type_id = 1 AND pa.product_type_id = 1 
					AND p.product_id <> pa.product_id AND pa.master_rank_order > 0 
					AND p.master_rank_order IN (0,1) 
					AND p.product_id IN (SELECT DISTINCT sub_product_id FROM @tmP_productParts)

                    SELECT p.product_id, p.number, p.quantity, p.unit_id, p.product_type_id, p.scope_id, i.wastage, d.recipe_type_id 
                    FROM product as p 
                    LEFT JOIN ingredient as i on i.product_id = p.product_id
                    LEFT JOIN dish as d on d.product_id = p.product_id
                    WHERE p.product_id IN (SELECT DISTINCT productId FROM @tmP_productParts)
                    OR p.product_id IN (SELECT DISTINCT sub_product_id FROM @tmP_productParts)
                    OR p.product_id = @product_id
                    OR p.product_id IN (SELECT DISTINCT product_id FROM @AlternateProductId)";

            var param = new
            {
                product_id = productId
            };

            using (var connection = GetOpenConnection())
            {
                var multi = await Task.Run(() => { return QueryMultiple(connection, cmd, param, CommandType.Text); });
                IEnumerable<ProductPart> parts = multi.Read<ProductPart>();
                IEnumerable<Product> products = multi.Read<Product>();
                return new Tuple<IEnumerable<Product>, IEnumerable<ProductPart>>(products, parts);
            }
        }

        #region Msmqlog

        public async Task<int> CreateMsmqLog(string action, int productId, int groupId, int pbandId, int setId, int unitId, DateTime logDate) {
            var param = new
            {
                action = action,
                product_id = productId,
                updateDate = logDate,
                group_id = groupId,
                pset_id = setId,
                pband_id = pbandId,
                unit_id = unitId
            };

            var cmd = @"INSERT INTO msmq_update_log (calc_type, group_id, product_id, pset_id, pband_id, unit_id,update_start_time)
                    VALUES (@action, @group_id, @product_id, @pset_id, @pband_id, @unit_id, @updateDate)
                    SELECT SCOPE_IDENTITY()";
            using (var connection = GetOpenConnection())
            {
                int result = await Task.Run(() => { return ExecuteScalar<int>(connection, cmd, param, CommandType.Text); });
                return result;
            }
        }

        public async Task<MsmqLog> GetLastMsmqStartTime(int productId)
        {
            var param = new
            {
                product_id = productId
            };
            var cmd = @"SELECT top 1 * FROM msmq_update_log
                        where calc_type like 'Dish Pricing Calculation%'  and calc_type not like 'Dish Pricing Calculation Skipped'
                        and (product_id is null or product_id = @product_id)
                        order by log_id desc";
            using (var connection = GetOpenConnection())
            {
                var result = await Task.Run(() => { return QuerySingleOrDefault<MsmqLog>(connection, cmd, param, CommandType.Text); });
                return result;
            }
        }

        public async Task<int> UpdateMsmqLog(DateTime logDate, int logId, bool isSuccess)
        {
            var param = new
            {
                updateTime = logDate,
                logid = logId,
                return_value = isSuccess ? 0 : 1
            };

            var cmd = @"UPDATE msmq_update_log
                        SET [update_end_time] = @updateTime,
                        return_value = @return_value
                        WHERE log_id = @logid";
            using (var connection = GetOpenConnection())
            {
                int result = await Task.Run(() => { return Execute(connection, cmd, param, CommandType.Text); });
                return result;
            }
        }

        #endregion

        #region Prices

        public async Task ClearPrices()
        {
            using (var connection = GetOpenConnection())
            {
                var cmd = "truncate table db_product_calc";

                await Task.Run(() => { base.Execute(connection, cmd, null, CommandType.Text); });
            }
        }

        public bool InsertPrices(Dictionary<int, decimal> prices, int? groupId, int logId, DateTime logDate)
        {
            if (!prices.Any())
                return true;

            bool result = false;

            var param = new
            {
                udt_prices = ToSqlDataRecords(prices).AsTableValuedParameter("udt_product_price"),
                group_id = groupId,
                logid = logId,
                updateDate = logDate
            };

            var cmd = @"sc_insert_prices";
            using (var connection = GetOpenConnection())
            {
                int res = ExecuteScalar<int>(connection, cmd, param, CommandType.StoredProcedure, true);
                result = res == 0;
            }

            return result;
        }

        public bool UpdatePrices(Dictionary<int, decimal> prices, int? groupId, int logId, DateTime logDate)
        {
            if (!prices.Any())
            {
                return true;
            }

            bool result = false;

            var param = new
            {
                udt_prices = ToSqlDataRecords(prices).AsTableValuedParameter("udt_product_price"),
                group_id = groupId,
                logid = logId,
                updateDate = logDate
            };

            var cmd = @"sc_update_prices";
            using (var connection = GetOpenConnection())
            {
                int res = ExecuteScalar<int>(connection, cmd, param, CommandType.StoredProcedure, true);
                result = res == 0;
                
            }

            return result;
        }

        #endregion

        public async Task<IEnumerable<GroupSets>> GetGroupSets(int groupId, int includeDescendants) {
            var param = new
            {
                group_id = groupId,
                include_descendants = includeDescendants
            };

            var cmd = @"sc_GetAvailableSetsForGroup";
            using (var connection = GetOpenConnection())
            {
                var result = await Task.Run(() => { return Query<GroupSets>(connection, cmd, param, CommandType.StoredProcedure); });
                return result;
            }
        }

        public async Task<IEnumerable<ProductPset>> GetProductPsets() {
            var cmd = @"SELECT DISTINCT pps.product_id, pps.pset_id FROM product_pset as pps
                        JOIN product as p ON pps.product_id = p.product_id
                        WHERE p.status_id = 1";
            using (var connection = GetOpenConnection())
            {
                var result = await Task.Run(() => { return Query<ProductPset>(connection, cmd, null, CommandType.StoredProcedure); });
                return result;
            }
        }

        public async Task<string> GetDbSetting(string settingName) {
            var param = new
            {
                setting_name = settingName
            };

            var cmd = @"select TOP 1 db_setting_value from db_setting where
                is_deleted = 0 AND db_setting_name like @setting_name";
            using (var connection = GetOpenConnection())
            {
                string result = await Task.Run(() => { return ExecuteScalar<string>(connection, cmd, param, CommandType.Text); });
                return result;
            }
        }

        public async Task<bool> IsIngredientAccess()
        {
            var cmd = @"select TOP 1 restrict_ingredient_access_by_vendor from ingredient_access_db_setting";
            using (var connection = GetOpenConnection())
            {
                bool result = await Task.Run(() => { return ExecuteScalar<bool>(connection, cmd, null, CommandType.Text); });
                return result;
            }
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

        public static IEnumerable<SqlDataRecord> ToSqlDataRecords(Dictionary<int, decimal> filters)
        {
            var metaData = new[]
            {
                new SqlMetaData("product_id", SqlDbType.Int),
                new SqlMetaData("price", SqlDbType.Decimal, 18, 9)
            };

            foreach (var filter in filters)
            {
                var record = new SqlDataRecord(metaData);
                record.SetInt32(0, filter.Key);
                record.SetDecimal(1, filter.Value);
                yield return record;
            }
        }

        public static IEnumerable<SqlDataRecord> ToSqlDataRecords(List<int> filters)
        {
            var metaData = new[]
            {
                new SqlMetaData("product_id", SqlDbType.Int),
                new SqlMetaData("price", SqlDbType.Decimal, 18, 9)
            };

            foreach (var filter in filters)
            {
                var record = new SqlDataRecord(metaData);
                record.SetInt32(0, filter);
                record.SetDecimal(1, 0);
                yield return record;
            }
        }

        /// <summary>
        /// Delete specified product prices
        /// </summary>
        /// <param name="prices">products which prices will be deleted</param>
        /// <param name="groupId">Group scope of delete operation</param>
        /// <returns></returns>
        public async Task ClearPrices(List<int> prices, int? groupId)
        {
            if (groupId.HasValue)
            {
                var cmd = "DELETE FROM db_product_calc WHERE group_Id = @group_id AND product_id IN (SELECT product_id FROM @udt_prices)";

                var param = new
                {
                    udt_prices = ToSqlDataRecords(prices).AsTableValuedParameter("udt_product_price"),
                    group_id = groupId
                };

                using (var connection = GetOpenConnection())
                {
                    await Task.Run(() => { Execute(connection, cmd, param, CommandType.Text, true); });
                }
            }
            else
            {
                var param = new
                {
                    udt_prices = ToSqlDataRecords(prices).AsTableValuedParameter("udt_product_price")
                };
                var cmd = "DELETE FROM db_product_calc WHERE group_Id IS NULL AND product_id IN (SELECT product_id FROM @udt_prices)";
                using (var connection = GetOpenConnection())
                {
                    await Task.Run(() => { Execute(connection, cmd, param, CommandType.Text, true); });
                }
            }
        }

        /// <summary>
        /// Delete product prices
        /// </summary>
        /// <param name="groupId">Group scope of delete operation</param>
        /// <returns></returns>
        public async Task ClearPrices(int? groupId)
        {
            if (groupId.HasValue)
            {
                var cmd = "DELETE FROM db_product_calc WHERE group_Id = @group_id";

                var param = new
                {
                    group_id = groupId,
                };

                using (var connection = GetOpenConnection())
                {
                    await Task.Run(() => { Execute(connection, cmd, param, CommandType.Text, true); });
                }
            }
            else
            {
                var cmd = "DELETE FROM db_product_calc WHERE group_Id IS NULL";
                using (var connection = GetOpenConnection())
                {
                    await Task.Run(() => { Execute(connection, cmd, null, CommandType.Text, true); });
                }
            }
        }

        public async Task<IEnumerable<IngredientAlternate>> GetIngredientAlternates()
        {
            return await GetIngredientAlternates(null);
        }

        public async Task<IEnumerable<IngredientAlternate>> GetIngredientAlternates(IEnumerable<int> ingredients)
        {
            var cmd = @"CREATE TABLE #convetion(product_id INT, target_product_id INT, source_unit_id INT, target_unit_id INT, ratio DECIMAL(30,14))

                INSERT INTO #convetion(product_id,target_product_id, source_unit_id, target_unit_id)

                SELECT p.product_id, pa.product_id AS alternate_product_id, pa.unit_id, p.unit_id
                FROM product AS p 
                JOIN product as pa ON p.master_ref_no = pa.master_ref_no
                WHERE p.product_type_id = 1 AND pa.product_type_id = 1 
                AND p.product_id <> pa.product_id AND pa.master_rank_order > 0 
                AND p.master_rank_order IN (0,1) 

                UPDATE #convetion
                SET ratio = dbo.fn_ConversionGetRatioEx(target_product_id, source_unit_id, target_unit_id) 

                SELECT p.product_id, pa.product_id AS alternate_product_id, pa.master_rank_order AS alternate_rank, c.ratio
                FROM product AS p 
                JOIN product as pa ON p.master_ref_no = pa.master_ref_no
				JOIN #convetion as c ON pa.product_id = c.target_product_id AND p.product_id = c.product_id
                WHERE p.product_type_id = 1 AND pa.product_type_id = 1 
                AND p.product_id <> pa.product_id AND pa.master_rank_order > 0 
                AND p.master_rank_order IN (0,1) AND p.master_ref_rev = 0
                ORDER BY p.product_id, pa.master_rank_order

                DROP TABLE #convetion";
            using (var connection = GetOpenConnection())
            {
                var result = await Task.Run(() => { return Query<IngredientAlternate>(connection, cmd, null, CommandType.Text); });
                return result;
            }
        }
    }
}