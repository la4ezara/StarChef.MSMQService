using Fourth.StarChef.Invariables;
using StarChef.Common.Model;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace StarChef.Engine.IntegrationTests
{

    public class CustomerDbRepository : RepositoryBase
    {
        readonly string connectionStr;
        public CustomerDbRepository(string connectionString, int timeout) : this(connectionString)
        {
            base.CommandTimeout = timeout;
        }

        public CustomerDbRepository(string connectionString)
        {
            connectionStr = connectionString;
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

        public IEnumerable<DbPrice> ExecutePriceRecalculation(int productId, int groupId, int unitId, int psetId, int pbandId)
        {
            var param = new
            {
                group_id = groupId,
                product_id = productId,
                unit_id = unitId,
                pset_id = psetId,
                pband_id = pbandId,
            };

            using (var connection = GetOpenConnection())
            {
                var cmd = "sc_calculate_dish_pricing_pure";

                var result = Query<DbPrice>(connection, cmd, param, CommandType.StoredProcedure);
                return result;
            }
        }

        public void ClearPrices()
        {
            using (var connection = GetOpenConnection())
            {
                var cmd = "truncate table db_product_calc";

                var result = base.Execute(connection, cmd, null, CommandType.Text);
            }
        }

        public IEnumerable<int> GetProducts() {
            var cmd = @"select TOP 5 p.product_id from product as p
                join product_part as pp on p.product_id = pp.product_id
                join dish as d on p.product_id = d.product_id
                ORDER BY NEWID()";
            using (var connection = GetOpenConnection())
            {
                var result = Query<int>(connection, cmd, null, CommandType.Text);
                return result;
            }
        }
    }
}
