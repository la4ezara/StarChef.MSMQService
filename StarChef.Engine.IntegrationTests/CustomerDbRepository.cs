using Dapper;
using Fourth.StarChef.Invariables;
using StarChef.Common.Model;
using StarChef.Engine.IntegrationTests.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

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
            var types = Assembly.GetAssembly(this.GetType()).GetTypes().Where(t => t.Namespace == "StarChef.Engine.IntegrationTests.Model").ToList();
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

        public IEnumerable<PriceRecalculationRequest> GetProducts() {
            var cmd = "SELECT product_id FROM Product WITH(NOLOCK) WHERE product_type_id = 2 ORDER BY product_id";
            using (var connection = GetOpenConnection())
            {
                var result = Query<PriceRecalculationRequest>(connection, cmd, null, CommandType.Text);
                return result.Skip(82);
            }
        }

        public IEnumerable<PriceRecalculationRequest> GetIngredients()
        {
            var cmd = "SELECT product_id FROM Product WITH(NOLOCK) WHERE product_type_id = 1 AND master_rank_order = 1 ORDER BY product_id";
            using (var connection = GetOpenConnection())
            {
                var result = Query<PriceRecalculationRequest>(connection, cmd, null, CommandType.Text);
                return result;
            }
        }

        public IEnumerable<PriceRecalculationRequest> GetGroups()
        {
            var cmd = "SELECT group_id FROM [group] WITH(NOLOCK)";
            using (var connection = GetOpenConnection())
            {
                var result = Query<PriceRecalculationRequest>(connection, cmd, null, CommandType.Text);
                return result;
            }
        }

        public IEnumerable<PriceRecalculationRequest> GetPriceBands()
        {
            var cmd = "SELECT pband_id FROM [pband] WITH(NOLOCK)";
            using (var connection = GetOpenConnection())
            {
                var result = Query<PriceRecalculationRequest>(connection, cmd, null, CommandType.Text);
                return result;
            }
        }

        public IEnumerable<PriceRecalculationRequest> GetSets()
        {
            var cmd = "SELECT pset_id FROM [pset] WITH(NOLOCK)";
            using (var connection = GetOpenConnection())
            {
                var result = Query<PriceRecalculationRequest>(connection, cmd, null, CommandType.Text);
                return result;
            }
        }

        public IEnumerable<PriceRecalculationRequest> GetUnits()
        {
            var cmd = "SELECT unit_id FROM [unit] WITH(NOLOCK)";
            using (var connection = GetOpenConnection())
            {
                var result = Query<PriceRecalculationRequest>(connection, cmd, null, CommandType.Text);
                return result;
            }
        }

        //public int ExecutePriceRecalculation(int productId, int groupId, int unitId, int psetId, int pbandId)
        //{
        //    var param = new
        //    {
        //        group_id = groupId,
        //        product_id = productId,
        //        unit_id = unitId,
        //        pset_id = psetId,
        //        pband_id = pbandId,
        //    };

        //    var cmd = "truncate table db_product_calc";
        //    using (var connection = GetOpenConnection())
        //    {
        //        Execute(connection, cmd, param, CommandType.Text);
        //        cmd = "sc_calculate_dish_pricing";
            
        //        var result = Execute(connection, cmd, param, CommandType.StoredProcedure);
        //        return result;
        //    }
        //}

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
    }
}
