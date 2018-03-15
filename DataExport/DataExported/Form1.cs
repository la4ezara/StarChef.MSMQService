using Fourth.StarChef.Invariables.Interfaces;
using Messaging.MSMQ;
using Messaging.MSMQ.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DataExported.Constants;

namespace DataExported
{
    public partial class Form1 : Form
    {
        private SortedDictionary<EntityEnum, QueryDetails> Entities { get; }
        private ConcurrentDictionary<EntityEnum, DataTable> QueryData { get;}
        private IMessagingFactory MessagingFactory { get;}
        private MessageBuilder MessageBuilder { get; set; }

        public Form1()
        {
            InitializeComponent();

            this.Entities = this.MapEntities();
            this.QueryData = new ConcurrentDictionary<EntityEnum, DataTable>();
            this.MapEntities();

            txtConnectionString.Text = @"Data Source=10.10.10.109\devtest;User ID=sl_web_user; Password=reddevil; Initial Catalog=sl_login;";

            this.MessagingFactory = new MsmqMessagingFactory();

            //txtDatabaseId.Text = "182";

            //MessageBuilder = new MessageBuilder(txtConnectionString.Text, Convert.ToInt32(txtDatabaseId.Text));

            this.btnRetrieve.Click += btnRetrieve_Click;
            this.bntExport.Click += BntExport_Click;
            this.btnRetrieveExportDetails.Click += BtnRetrieveExportDetails_Click;
            txtSkip.Text = "0";
            txtTake.Text = "100";
        }

        private async void BtnRetrieveExportDetails_Click(object sender, EventArgs e)
        {
            try
            {
                txtLog.Text = string.Empty;
                var selectedCustomer = Convert.ToInt16(ddlCustomers.SelectedValue);
                var constring = GetCustomerDbConnectionString(selectedCustomer);
                MessageBuilder = new MessageBuilder(constring, selectedCustomer);
                await Task.WhenAll(this.GetQueries(constring));
                var l = new List<EntityInfo>();
                foreach (var q in this.QueryData.Where(x => x.Key != EntityEnum.Login))
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
            catch (Exception ex)
            {
                txtLog.Text = ex.Message;
            }
        }

        private void BntExport_Click(object sender, EventArgs e)
        {
            try
            {
                txtLog.Text = string.Empty;
                var skip = int.Parse(txtSkip.Text);
                var take = int.Parse(txtTake.Text);
                var rows = dgEntityInfo.Rows.Cast<DataGridViewRow>().Where(x => Convert.ToBoolean(x.Cells[2].Value));
                foreach (var r in rows)
                {
                    var entity = (EntityEnum)Enum.Parse(typeof(EntityEnum), r.Cells[0].Value.ToString(), true);

                    //Create entity based message
                    var payloadMessages = MessageBuilder.GetMessages(entity, this.QueryData, skip, take);

                    //Send message to MSMQ
                    SendMessageToMsmq(payloadMessages);
                }
                MessageBox.Show("Data exported successfully");
            }
            catch (Exception ex)
            {
                txtLog.Text = ex.Message;

            }
        }

        private async void btnRetrieve_Click(object sender, EventArgs e)
        {
            try
            {
                txtLog.Text = string.Empty;
                await this.ExecuteQuery(txtConnectionString.Text, this.Entities[EntityEnum.Login]);
                var customers = this.QueryData[EntityEnum.Login];
                this.ddlCustomers.DataSource = customers;
                this.ddlCustomers.DisplayMember = "database_desc";
                this.ddlCustomers.ValueMember = "db_database_id";
            }
            catch (Exception ex)
            {
                txtLog.Text = ex.Message;
            }
        }

        private string GetCustomerDbConnectionString(short dbId)
        {
            var row = (from r in this.QueryData[EntityEnum.Login].AsEnumerable()
                where r.Field<short>("db_database_id") == dbId
                select r).FirstOrDefault();

            return $"{row[1]};{row[2]}";
        }

        private void SendMessageToMsmq(IEnumerable<IMessage> messages)
        {
            IMessageBus bus = this.MessagingFactory.CreateMessageBus();

            foreach(var message in messages)
            {
                bus.SendMessage(message);
            }
        }

        private List<Task> GetQueries(string customerConnectionString)
        {
            var tasks = new List<Task>();

            foreach (var e in this.Entities.Where(x=>!x.Value.IsInternal))
            {
                tasks.Add(this.ExecuteQuery(customerConnectionString, e.Value));
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
                        Query = "select group_id from [Group]"
                    }
                },
                {
                    EntityEnum.Menu, new QueryDetails
                    {
                        Name = EntityEnum.Menu,
                        Query = "select menu_id from [Menu]"
                    }
                },
                {
                    EntityEnum.Recipe, new QueryDetails
                    {
                        Name = EntityEnum.Recipe,
                        Query = "select product_id from [Dish]"
                    }
                },
                {
                    EntityEnum.User, new QueryDetails
                    {
                        Name = EntityEnum.User,
                        Query = "select user_id from [User]"
                    }
                },
                {
                    EntityEnum.MealPeriod, new QueryDetails
                    {
                        Name = EntityEnum.MealPeriod,
                        Query = "select meal_period_id from [meal_period]"
                    }
                },
                {
                    EntityEnum.Login, new QueryDetails
                    {
                        Name = EntityEnum.Login,
                        Query = "select * from db_database order by database_desc",
                        IsInternal = true
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
            public bool IsInternal { get; set; }
        }

        private void bntExport_Click_1(object sender, EventArgs e)
        {

        }
    }

}
