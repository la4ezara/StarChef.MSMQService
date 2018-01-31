using Fourth.StarChef.Invariables;
using StarChef.MSMQService;
using StarChef.MSMQService.Configuration;
using StarChef.MSMQService.Configuration.Impl;
using System;
using System.Configuration;
using System.Linq;
using System.Messaging;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace StarChef.MsmqIntegration.WinApp
{
    public partial class Form1 : Form
    {
        private volatile bool _isMonitoring = false;
        private readonly char _semicolon = Convert.ToChar(";");
        private readonly IAppConfiguration _config = new AppConfiguration();
        private volatile int _queueMessageCount = 0;
        private int _processingTimeInterval = 5;
        public Form1()
        {
            InitializeComponent();
            LoadProductTypes();
            LoadMessageTypes();
            lblProcessMessageCount.Text = $"Process message per {_processingTimeInterval} seconds";
            int databaseId = 6;
            txtDatabaseId.Text = databaseId.ToString();
            txtDSN.Text = "Data Source = 10.10.10.109\\devtest; User ID = sl_web_user; Password = reddevil; Initial Catalog = SCNET_demo_qa";
            EnableDisableButtons();
            var processingTime = ConfigurationManager.AppSettings["ProcessingTime"];
            if (!string.IsNullOrEmpty(processingTime))
            {
                int.TryParse(processingTime, out _processingTimeInterval);
            }
        }

        private void EnableDisableButtons()
        {
            btnStartMonitoring.Enabled = !_isMonitoring;
            btnStopMonitoring.Enabled = _isMonitoring;
        }

        protected void ShowQueueSize()
        {
            if (txtStatus.InvokeRequired)
            {
                txtStatus.Invoke(new Action(() => GetMessageCount()));
            }
            else
            {
                GetMessageCount();
            }
        }

        private void Timer_Elapsed()
        {
            while (_isMonitoring)
            {
                Thread.Sleep(1000 * _processingTimeInterval);
                ShowQueueSize();
            }
        }

        private void GetMessageCount()
        {
            try
            {
                using (var queue = new MessageQueue(_config.NormalQueueName))
                {
                    MessagePropertyFilter myFilter = queue.MessageReadPropertyFilter;
                    myFilter.SetDefaults();
                    myFilter.AppSpecific = true;
                    myFilter.Body = false;

                    queue.MessageReadPropertyFilter = myFilter;

                    var messages = queue.GetAllMessages();
                    var count = messages.Count();
                    if (!string.IsNullOrEmpty(lblQueueCountValue.Text))
                    {
                        lblQueueCountValue.Text = count.ToString();
                    }
                    //if (_queueMessageCount != count)
                    //{
                    var diff = _queueMessageCount - count;
                    if (diff > 0)
                    {
                        if (!string.IsNullOrEmpty(txtStatus.Text))
                        {
                            txtStatus.AppendText(Environment.NewLine);
                        }
                        var newMsg = $"{DateTime.Now} - diff: {diff};total: {count}";
                        txtStatus.AppendText(newMsg);
                    }
                    //}
                    _queueMessageCount = count;
                }
            }
            finally
            {

            }
        }

        private void LoadMessageTypes()
        {
            cbMessageType.Items.Clear();
            var names = Enum.GetNames(typeof(Constants.MessageActionType));
            foreach (var name in names)
            {
                cbMessageType.Items.Add(name);
                if (cbMessageType.SelectedItem == null)
                {
                    cbMessageType.SelectedItem = name;
                }
            }
        }

        private void LoadProductTypes()
        {
            cbProductType.Items.Clear();
            var name = Enum.GetName(typeof(Constants.EntityType), Constants.EntityType.Ingredient);
            cbProductType.Items.Add(name);
            name = Enum.GetName(typeof(Constants.EntityType), Constants.EntityType.Dish);
            cbProductType.Items.Add(name);
            cbProductType.SelectedItem = name;
        }

        private void txtCount_KeyPress(object sender, KeyPressEventArgs e)
        {
            //int num;
            //if (!int.TryParse(e.KeyChar.ToString(), out num))
            //{
            //    e.Handled = true;
            //}
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtProductId_KeyPress(object sender, KeyPressEventArgs e)
        {
            //int num;
            //if (!int.TryParse(e.KeyChar.ToString(), out num))
            //{
            //    e.Handled = true;
            //}

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && !_semicolon.Equals(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtCount.Text) && !string.IsNullOrEmpty(txtProductId.Text) && !string.IsNullOrEmpty(txtDatabaseId.Text) && !string.IsNullOrEmpty(txtDSN.Text))
            {
                int productId = 0;
                var productIds = txtProductId.Text.Split(_semicolon);
                var productIdCollection = new List<int>();
                if (productIds.Length > 1)
                {
                    for (int i = 0; i < productIds.Length; i++)
                    {
                        productIdCollection.Add(int.Parse(productIds[i]));
                    }
                }
                else
                {
                    productId = int.Parse(txtProductId.Text);
                }
                
                int count = int.Parse(txtCount.Text);
                Constants.EntityType entityType = (Constants.EntityType)Enum.Parse(typeof(Constants.EntityType),
                    cbProductType.SelectedItem.ToString());
                Constants.MessageActionType messageType = (Constants.MessageActionType)Enum.Parse(typeof(Constants.MessageActionType), cbMessageType.SelectedItem.ToString());
                
                int databaseId = int.Parse(txtDatabaseId.Text);
                string dsn = txtDSN.Text;

                MsmqManager manager = new MsmqManager(_config.NormalQueueName, _config.PoisonQueueName);
                var msg = new UpdateMessage()
                {
                    Action = (int)messageType,
                    DSN = dsn,
                    DatabaseID = databaseId,
                    EntityTypeId = (int)entityType,
                    ExternalId = databaseId.ToString(),
                    GroupID = 1,
                    ProductID = productId
                };

                if (productIdCollection.Any())
                {
                    var json = JsonConvert.SerializeObject(productIdCollection);
                    msg.ExtendedProperties = json;
                    msg.ProductID = default(int);
                }

                for (int i = 0; i < count; i++)
                {
                    manager.mqSend(msg, MessagePriority.High);
                }

            }
        }

        private void btnStopMonitoring_Click(object sender, EventArgs e)
        {
            _isMonitoring = false;
            EnableDisableButtons();
        }

        private void btnStartMonitoring_Click(object sender, EventArgs e)
        {
            _isMonitoring = true;
            EnableDisableButtons();
            ShowQueueSize();
            var t = new Thread(new ThreadStart(Timer_Elapsed));
            t.IsBackground = true;
            t.Start();
        }

        private void btnClearQueue_Click(object sender, EventArgs e)
        {
            using (var queue = new MessageQueue(_config.NormalQueueName))
            {
                queue.Purge();
            }
        }

        private void btnClearConsole_Click(object sender, EventArgs e)
        {
            txtStatus.Text = string.Empty;
        }
    }
}
