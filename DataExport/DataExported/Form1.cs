using Messaging.MSMQ;
using Messaging.MSMQ.Interface;
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

        private IMessagingFactory MessagingFactory;
        private MessageBuilder MessageBuilder;

        public Form1()
        {

            this.Entities = this.MapEntities();
            this.QueryData = new ConcurrentDictionary<EntityEnum, DataTable>();

            InitializeComponent();
            var ds = this.MapEntities();
            txtConnectionString.Text =
                @"Initial Catalog=SCNET_abokado;Data Source=10.10.10.109\DEVTEST;User ID=sl_web_user; Password=reddevil;";
            txtDatabaseId.Text = "182";

            MessageBuilder = new MessageBuilder(txtConnectionString.Text, Convert.ToInt32(txtDatabaseId.Text));

            this.btnRetrieve.Click += btnRetrieve_Click;
            this.bntExport.Click += BntExport_Click;
        }

        private void BntExport_Click(object sender, EventArgs e)
        {
            this.MessagingFactory = new MsmqMessagingFactory();
            var rows = dgEntityInfo.Rows.Cast<DataGridViewRow>().Where(x => Convert.ToBoolean(x.Cells[2].Value));
            foreach (var r in rows)
            {
                var entity =  (EntityEnum)Enum.Parse(typeof(EntityEnum), r.Cells[0].Value.ToString(), true);

                //Create entity based message
                var payloadMessages = MessageBuilder.GetMessages(entity,this.QueryData);

                //Send message to MSMQ
                SendMessageToMSMQ(payloadMessages);
            }
        }

        private void SendMessageToMSMQ(IEnumerable<IMessage> messages)
        {
            IMessageBus bus = this.MessagingFactory.CreateMessageBus();

            foreach(var message in messages)
            {
                bus.SendMessage(message);
            }
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
        }

        private void bntExport_Click_1(object sender, EventArgs e)
        {

        }
    }

}
