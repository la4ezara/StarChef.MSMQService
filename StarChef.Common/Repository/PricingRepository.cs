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

        public async Task<IEnumerable<GroupProducts>> GetGroupProductPricesByProduct(int productId) {
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
            IEnumerable<GroupProducts> result = await Task.Run(() =>
            {
                using (var connection = GetOpenConnection())
                {
                    var res = Query<GroupProducts>(connection, cmd, param, CommandType.Text);
                    return res;
                }
            });
            return result;
        }

        public async Task<IEnumerable<GroupProducts>> GetGroupProductPricesByGroup(int groupId)
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
            IEnumerable<GroupProducts> result = await Task.Run(() =>
            {
                using (var connection = GetOpenConnection())
                {
                    var res = Query<GroupProducts>(connection, cmd, param, CommandType.Text);
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

        public async Task<int> CreateMsmqLog(string action, int productId, DateTime logDate) {
            var param = new
            {
                action = action,
                product_id = productId
            };

            var cmd = @"INSERT INTO msmq_update_log (calc_type, group_id, product_id, pset_id, pband_id, unit_id)
                    VALUES (@action, 0, @product_id, 0, 0, 0)
                    SELECT SCOPE_IDENTITY()";
            using (var connection = GetOpenConnection())
            {
                int result = await Task.Run(() => { return ExecuteScalar<int>(connection, cmd, param, CommandType.Text); });
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
                int result = ExecuteScalar<int>(connection, cmd, param, CommandType.StoredProcedure);
                return result == 0 ? true : false;
            }
        }

        public bool UpdatePrices(Dictionary<int, decimal> prices, int? groupId, int logId, DateTime logDate) {
            throw new NotImplementedException();
            if (!prices.Any())
                return true;

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
                int result = ExecuteScalar<int>(connection, cmd, param, CommandType.StoredProcedure);
                return result == 0 ? true : false;
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
    }
}