using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataExported
{
    public enum EntityEnum
    {
        Menu,
        Group,
        Recipe,
        User,
        MealPeriod
    };

    public partial class Form1 : Form
    {
        private SortedDictionary<EntityEnum, QueryDetails> Entities { get; }

        private ConcurrentDictionary<EntityEnum, DataTable> QueryData { get; set; }

        public Form1()
        {

            this.Entities = this.MapEntities();
            this.QueryData = new ConcurrentDictionary<EntityEnum, DataTable>();

            InitializeComponent();
            var ds = this.MapEntities();
            txtConnectionString.Text =
                @"Initial Catalog=SCNET_abokado;Data Source=10.10.10.109\DEVTEST;User ID=sl_web_user; Password=reddevil;";

            this.btnRetrieve.Click += btnRetrieve_Click;
        }

        
        private async void btnRetrieve_Click(object sender, EventArgs e)
        {
            await Task.WhenAll(this.GetQueries());
            var l = new List<EntityInfo>();
            foreach (var q in this.QueryData)
            {
                l.Add(new EntityInfo
                {
                    Name = q.Key.ToString(),
                    RowCount = q.Value.Rows.Count,
                    Selected = false
                });
            }
            dgEntityInfo.DataSource = new BindingSource(new BindingList<EntityInfo>(l), null);
        }

        private List<Task> GetQueries()
        {
            var tasks = new List<Task>();

            foreach (var e in this.Entities)
            {
                tasks.Add(this.ExecuteQuery(txtConnectionString.Text, e.Value));
            }

            return tasks;
        }

        private SortedDictionary<EntityEnum, QueryDetails> MapEntities()
        {
            var entities = new SortedDictionary<EntityEnum, QueryDetails>
            {
                {
                    EntityEnum.Group, new QueryDetails
                    {
                        Name = EntityEnum.Group,
                        Query = "select * from [Group]"
                    }
                },
                {
                    EntityEnum.Menu, new QueryDetails
                    {
                        Name = EntityEnum.Menu,
                        Query = "select * from [Menu]"
                    }
                },
                {
                    EntityEnum.Recipe, new QueryDetails
                    {
                        Name = EntityEnum.Recipe,
                        Query = "select * from [User]"
                    }
                },
                {
                    EntityEnum.User, new QueryDetails
                    {
                        Name = EntityEnum.User,
                        Query = "select * from [User]"
                    }
                },
                {
                    EntityEnum.MealPeriod, new QueryDetails
                    {
                        Name = EntityEnum.MealPeriod,
                        Query = "select * from [meal_period]"
                    }
                }
            };

            return entities;
        }


        private async Task ExecuteQuery(string connectionString, QueryDetails query)
        {
            using (var cn = new SqlConnection(connectionString))
            {
                cn.Open();

                var cmd = new SqlCommand(query.Query, cn);

                var reader = await cmd.ExecuteReaderAsync();
                var dt = new DataTable();
                dt.Load(reader);
                this.QueryData[query.Name] = dt;
            }
        }

        private void AddMessage()
        {
            
        }

        class EntityInfo
        {
            public string Name { get; set; }
            public long RowCount { get; set; }
            public bool Selected { get; set; }
        }

        class QueryDetails
        {
            public EntityEnum Name { get; set; }
            public string Query { get; set; }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }

}
